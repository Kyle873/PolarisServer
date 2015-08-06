﻿using PolarisServer.Packets.PSOPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0xB, 0x30)]
    class QuestCounterHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            // Not sure what this does yet
            byte[] allTheQuests = new byte[408];
            for (int i = 0; i < allTheQuests.Length; i++)
                allTheQuests[i] = 0xFF;
            context.SendPacket(0xB, 0x22, 0x0, allTheQuests);
        }
    }

    [PacketHandlerAttr(0xB, 0x15)]
    class QuestCounterAckHander : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketWriter writer = new PacketWriter();

            // Set the amount of every quest type to 3 (all 36 of em!)
            for (int i = 0; i < 36; i++)
            {
                writer.Write((UInt16)3);
            }

            // Only set Extreme Quests as unplayed (Bitfield matches the one below)
            writer.Write((UInt64)2);
            // Turn on all the quests (This is a 36-bit bitfield! TODO: Make an enum)
            writer.Write((Int64)~0);

            // Set both of these to one for testing
            writer.Write(1); // ARKS Ship Grand Prix flag
            writer.Write(1337); // ARKS Ship Grand Prix Boost level

            context.SendPacket(0xB, 0x16, 0x0, writer.ToArray());
        }
    }

    [PacketHandlerAttr(0xB, 0x17)]
    class QuestListRequestHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            // What am I doing
            PSOPackets.QuestListPacket.QuestDefiniton[] defs = new PSOPackets.QuestListPacket.QuestDefiniton[1];
            for (int i = 0; i < defs.Length; i++)
            {
                defs[i].dateOrSomething = "2013/01/25";
                defs[i].questNameString = 20070;
                defs[i].needsToBeNonzero = 0x36;
                defs[i].getsSetToWord = 0xFFFF;
            }

            context.SendPacket(new QuestListPacket(defs));
            context.SendPacket(new NoPayloadPacket(0xB, 0x1B));
        }
    }

    [PacketHandlerAttr(0xB, 0x19)]
    class QuestDifficultyRequestHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            // What am I doing
            PSOPackets.QuestDifficultyPacket.QuestDifficulty[] diffs = new QuestDifficultyPacket.QuestDifficulty[1];
            for (int i = 0; i < diffs.Length; i++)
            {
                diffs[i].dateOrSomething = "2013/01/25";
                diffs[i].something = 0x20;
                diffs[i].something2 = 0x0B;
                diffs[i].something3 = 0x753A;

                // These are likely bitfields
                diffs[i].something4 = 0x00030301;
                diffs[i].something5 = 0x0101;
            }

            context.SendPacket(new QuestDifficultyPacket(diffs));

            // [K873] I believe this is the correct packet, but it causes an infinite send/recieve loop, we're probably just missing something else
            // context.SendPacket(new NoPayloadPacket(0xB, 0x1C));
        }
    }
}