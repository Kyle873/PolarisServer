using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SystemMessagePacket : Packet
    {
        public enum MessageType
        {
            GoldenTicker = 0,
            AdminMessage,
            AdminMessageInstant,
            SystemMessage,
            GenericMessage
        }

        private readonly string message;
        private readonly MessageType type;

        public SystemMessagePacket(string message, MessageType type)
        {
            this.message = message;
            this.type = type;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();
            writer.WriteUtf16(message, 0x78F7, 0xA2);
            writer.Write((UInt32) type);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x19, 0x01, PacketFlags.Packed);
        }
    }
}
