using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SetMesetaPacket : Packet
    {
        public Int64 NewAmount { get; } = 0;

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(NewAmount);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x0F, 0x14);
        }
    }
}
