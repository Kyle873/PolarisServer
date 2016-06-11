using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PolarisServer.Database;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SetQuestPacket : Packet
    {
        QuestDefiniton questDef;
        Player player;

        public SetQuestPacket(QuestDefiniton questdef, Database.Player player)
        {
            this.questDef = questdef;
            this.player = player;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.Write(questDef.questNameString);
            writer.Write(0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.WriteStruct(new ObjectHeader((uint)player.PlayerID, EntityType.Player));
            writer.Write(0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(0);
            writer.Write(0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xE, 0x25);
        }
    }
}
