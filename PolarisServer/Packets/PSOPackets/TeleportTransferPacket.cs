using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class TeleportTransferPacket : Packet
    {
        private PSOObject src;
        private PSOLocation dest;

        public TeleportTransferPacket(PSOObject srcTeleporter, PSOLocation destination)
        {
            src = srcTeleporter;
            dest = destination;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.Write(new byte[12]);
            writer.WriteStruct(src.Header);
            writer.WritePosition(dest);
            writer.Write(new byte[2]);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x4, 0x2, PacketFlags.ObjectRelated);
        }
    }
}
