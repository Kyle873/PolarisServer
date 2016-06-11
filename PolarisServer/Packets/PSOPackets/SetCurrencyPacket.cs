using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SetCurrencyPacket : Packet
    {
        public int NewACAmount { get; } = 0;
        public int NewFUNAmount { get; } = 0;

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            // AC
            writer.Write(NewACAmount);

            // Padding?
            for (var i = 0; i < 20; i++)
                writer.Write((byte)0);

            // FUN
            writer.Write(NewFUNAmount);

            // Padding?
            for (var i = 0; i < 4; i++)
                writer.Write((byte)0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x11, 0x1C);
        }
    }
}
