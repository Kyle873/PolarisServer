using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class NoPayloadPacket : Packet
    {
        private readonly byte type;
        private readonly byte subType;

        public NoPayloadPacket(byte type, byte subtype)
        {
            this.type = type;
            this.subType = subtype;
        }

        public override byte[] Build()
        {
            return new byte[0];
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(type, subType);
        }
    }
}
