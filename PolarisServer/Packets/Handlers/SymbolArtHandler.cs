using System;

using PolarisServer.Packets.PSOPackets;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x2F, 0x6)]
    class SymbolArtListHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            context.SendPacket(new SymbolArtList(new Models.ObjectHeader((uint)context.User.PlayerID, Models.EntityType.Player)));
        }
    }
}
