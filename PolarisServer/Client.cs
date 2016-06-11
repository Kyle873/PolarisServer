using System;
using System.IO;
using System.Security.Cryptography;

using PolarisServer.Database;
using PolarisServer.Models;
using PolarisServer.Network;
using PolarisServer.Packets;
using PolarisServer.Packets.Handlers;
using PolarisServer.Zone;

namespace PolarisServer
{
    public class Client
    {
        public bool IsClosed { get; private set; }
        public SocketClient Socket { get; private set; }

        // Game properties, TODO Consider moving these somewhere else
        public Player User { get; set; }
        public Character Character { get; set; }
        // public Zone.Zone CurrentZone { get; set; }
        public Map CurrentZone;
        public uint MovementTimestamp { get; internal set; }
        public Party.Party currentParty;

        public PSOLocation CurrentLocation;
        public PSOLocation LastLocation;

        internal static RSACryptoServiceProvider RsaCsp = null;
        internal ICryptoTransform InputArc4, OutputArc4;

        private readonly byte[] readBuffer;
        private readonly Server server;

        private int packetID;
        private uint readBufferSize;

        public Client(Server server, SocketClient socket)
        {
            IsClosed = false;
            this.server = server;
            Socket = socket;

            socket.DataReceived += HandleDataReceived;
            socket.ConnectionLost += HandleConnectionLost;

            readBuffer = new byte[1024 * 64];
            readBufferSize = 0;

            InputArc4 = null;
            OutputArc4 = null;

            var welcome = new PacketWriter();
            welcome.Write((ushort)3);
            welcome.Write((ushort)201);
            welcome.Write((ushort)0);
            welcome.Write((ushort)0);
            SendPacket(0x03, 0x08, 0, welcome.ToArray());
        }

        private void HandleDataReceived(byte[] data, int size)
        {
            Logger.Write("[<--] Recieved {0} bytes", size);

            if ((readBufferSize + size) > readBuffer.Length)
            {
                // Buffer overrun
                // TODO: Drop the connection when this occurs?
                return;
            }

            Array.Copy(data, 0, readBuffer, readBufferSize, size);

            if (InputArc4 != null)
                InputArc4.TransformBlock(readBuffer, (int)readBufferSize, size, readBuffer, (int)readBufferSize);

            readBufferSize += (uint)size;

            // Process ALL the packets
            uint position = 0;

            while ((position + 8) <= readBufferSize)
            {
                var packetSize =
                    readBuffer[position] |
                    ((uint)readBuffer[position + 1] << 8) |
                    ((uint)readBuffer[position + 2] << 16) |
                    ((uint)readBuffer[position + 3] << 24);

                // Minimum size, just to avoid possible infinite loops etc
                if (packetSize < 8)
                    packetSize = 8;

                // If we don't have enough data for this one...
                if (packetSize > 0x1000000 || (packetSize + position) > readBufferSize)
                    break;

                // Now handle this one
                HandlePacket(
                    readBuffer[position + 4], readBuffer[position + 5],
                    readBuffer[position + 6], readBuffer[position + 7],
                    readBuffer, position + 8, packetSize - 8);

                // If the connection was closed, we have no more business here
                if (IsClosed)
                    break;

                position += packetSize;
            }

            // Wherever 'position' is up to, is what was successfully processed
            if (position > 0)
            {
                if (position >= readBufferSize)
                    readBufferSize = 0;
                else
                {
                    Array.Copy(readBuffer, position, readBuffer, 0, readBufferSize - position);
                    readBufferSize -= position;
                }
            }
        }

        private void HandleConnectionLost()
        {
            // :(
            Logger.Write("[BYE] Connection lost. :(");

            if (CurrentZone != null)
                CurrentZone.RemoveClient(this);

            IsClosed = true;
        }

        public void SendPacket(byte[] blob)
        {
            var typeA = blob[4];
            var typeB = blob[5];
            var flags1 = blob[6];
            var flags2 = blob[7];

            Logger.Write("[<--] Packet {0:X}-{1:X} (flags {2}) ({3} bytes)", typeA, typeB, (PacketFlags)flags1,
                blob.Length);
            LogPacket(false, typeA, typeB, flags1, flags2, blob);

            if (Logger.VerbosePackets)
            {
                var info = string.Format("[<--] {0:X}-{1:X} Data:", typeA, typeB);
                Logger.WriteHex(info, blob);
            }

            if (OutputArc4 != null)
                OutputArc4.TransformBlock(blob, 0, blob.Length, blob, 0);

            try
            {
                Socket.Write(blob);
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error sending packet", ex);
            }
        }

        public void SendPacket(byte typeA, byte typeB, byte flags, byte[] data)
        {
            var packet = new byte[8 + data.Length];

            // TODO: Use BinaryWriter here maybe?
            var dataLen = (uint)data.Length + 8;
            packet[0] = (byte)(dataLen & 0xFF);
            packet[1] = (byte)((dataLen >> 8) & 0xFF);
            packet[2] = (byte)((dataLen >> 16) & 0xFF);
            packet[3] = (byte)((dataLen >> 24) & 0xFF);
            packet[4] = typeA;
            packet[5] = typeB;
            packet[6] = flags;
            packet[7] = 0;

            Array.Copy(data, 0, packet, 8, data.Length);

            SendPacket(packet);
        }

        public void SendPacket(Packet packet)
        {
            var h = packet.GetHeader();
            SendPacket(h.Type, h.Subtype, h.Flags1, packet.Build());
        }

        private void HandlePacket(byte typeA, byte typeB, byte flags1, byte flags2, byte[] data, uint position, uint size)
        {
            Logger.Write("[-->] Packet {0:X}-{1:X} (flags {2}) ({3} bytes)", typeA, typeB, (PacketFlags)flags1, size + 8);

            if (Logger.VerbosePackets && size > 0) // TODO: This is trimming too far?
            {
                var dataTrimmed = new byte[size];
                for (var i = 0; i < size; i++)
                    dataTrimmed[i] = data[i];

                var info = string.Format("[-->] {0:X}-{1:X} Data:", typeA, typeB);
                Logger.WriteHex(info, dataTrimmed);
            }

            var packet = new byte[size];
            Array.Copy(data, position, packet, 0, size);

            LogPacket(true, typeA, typeB, flags1, flags2, packet);

            var handler = PacketHandlers.GetHandlerFor(typeA, typeB);
            if (handler != null)
                handler.HandlePacket(this, flags1, packet, 0, size);
            else
                Logger.WriteWarning("[!!!] UNIMPLEMENTED PACKET {0:X}-{1:X} - (Flags {2}) ({3} bytes)", typeA, typeB, (PacketFlags)flags1, size);
        }

        private void LogPacket(bool fromClient, byte typeA, byte typeB, byte flags1, byte flags2, byte[] packet)
        {
            // Check for and create packets directory if it doesn't exist
            var packetPath = "packets/" + server.StartTime.ToShortDateString().Replace('/', '-') + "-" +
                             server.StartTime.ToShortTimeString().Replace('/', '-').Replace(':', '-');

            if (!Directory.Exists(packetPath))
                Directory.CreateDirectory(packetPath);

            var filename = string.Format("{0}/{1}.{2:X}.{3:X}.{4}.bin", packetPath, packetID++, typeA, typeB,
                fromClient ? "C" : "S");

            using (var stream = File.OpenWrite(filename))
            {
                if (fromClient)
                {
                    stream.WriteByte(typeA);
                    stream.WriteByte(typeB);
                    stream.WriteByte(flags1);
                    stream.WriteByte(flags2);
                }

                stream.Write(packet, 0, packet.Length);
            }
        }
    }
}
