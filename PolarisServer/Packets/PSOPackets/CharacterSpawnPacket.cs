using System;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class CharacterSpawnPacket : Packet
    {
        public bool IsItMe = true;
        public PSOLocation Position;

        private readonly Character character;

        public CharacterSpawnPacket(Character character, PSOLocation position)
        {
            this.character = character;

            Position = position;
        }

        public CharacterSpawnPacket(Character character, PSOLocation position, bool isme)
        {
            this.character = character;

            IsItMe = isme;
            Position = position;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            // Player header
            writer.WritePlayerHeader((uint)character.Player.PlayerID);

            // Spawn position
            writer.Write(Position);

            writer.Write((ushort)0); // padding?
            writer.WriteFixedLengthASCII("Character", 32);
            writer.Write((ushort)1); // 0x44
            writer.Write((ushort)0); // 0x46
            writer.Write((uint)602); // 0x48
            writer.Write((uint)1); // 0x4C
            writer.Write((uint)53); // 0x50
            writer.Write((uint)0); // 0x54
            writer.Write((uint)(IsItMe ? 47 : 39)); // 0x58
            writer.Write((ushort)559); // 0x5C
            writer.Write((ushort)306); // 0x5E
            writer.Write((uint)character.Player.PlayerID); // player ID copy
            writer.Write((uint)0); // "char array ugggghhhhh" according to PolarisLegacy
            writer.Write((uint)0); // "voiceParam_unknown4"
            writer.Write((uint)0); // "voiceParam_unknown8"
            writer.WriteFixedLengthUtf16(character.Name, 16);
            writer.Write((uint)0); // 0x90
            writer.WriteStruct(character.Looks);
            writer.WriteStruct(character.Jobs);
            writer.WriteFixedLengthUtf16("", 32); // title?
            writer.Write((uint)0); // 0x204
            writer.Write((uint)0); // gmflag?
            writer.WriteFixedLengthUtf16(character.Player.Nickname, 16); // nickname, maybe not 16 chars?

            for (var i = 0; i < 64; i++)
                writer.Write((byte)0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x08, 0x04);
        }
    }
}
