using System;
using System.Runtime.InteropServices;

using PolarisServer.Models;
using PolarisServer.Packets.PSOPackets;

namespace PolarisServer.Packets.Handlers
{
    [Flags]
    public enum PackedData : Int32
    {
        Ent1ID = 0x01,
        Ent1Type = 0x02,
        Ent1A = 0x04,
        Ent2ID = 0x08,
        Ent2Type = 0x10,
        Ent2A = 0x20,
        Timestamp = 0x40,
        RotX = 0x80,
        RotY = 0x100,
        RoyZ = 0x200,
        RoyW = 0x400,
        CurX = 0x800,
        CurY = 0x1000,
        CurZ = 0x2000,
        Unknown1 = 0x4000,
        UnkX = 0x8000,
        UnkY = 0x10000,
        UnkZ = 0x20000,
        Unknown2 = 0x40000,
        Unknown3 = 0x80000,
        Unknown4 = 0x100000
    }

    public struct PackedVec4
    {
        public UInt16 x, y, z, w;

        public PackedVec4(PSOLocation location)
        {
            this.x = Helper.FloatToHalfPrecision(location.RotX);
            this.y = Helper.FloatToHalfPrecision(location.RotY);
            this.z = Helper.FloatToHalfPrecision(location.RotZ);
            this.w = Helper.FloatToHalfPrecision(location.RotW);
        }
    }

    public struct PackedVec3
    {
        public UInt16 x, y, z;

        public PackedVec3(PSOLocation location)
        {
            this.x = Helper.FloatToHalfPrecision(location.PosX);
            this.y = Helper.FloatToHalfPrecision(location.PosY);
            this.z = Helper.FloatToHalfPrecision(location.PosZ);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public struct FullMovementData
    {
        [FieldOffset(0x0)]
        public ObjectHeader entity1;
        [FieldOffset(0xC)]
        public ObjectHeader entity2;
        [FieldOffset(0x18)]
        public UInt32 timestamp;
        [FieldOffset(0x1C)]
        public PackedVec4 rotation;
        [FieldOffset(0x24)]
        public PackedVec3 currentPos;
        [FieldOffset(0x2A)]
        public UInt16 Unknown2; // This MAY be part of lastPos, as lastPos may be a Vec4?
        [FieldOffset(0x2C)]
        public PackedVec3 unknownPos;
        [FieldOffset(0x32)]
        public UInt16 Unknown3; // This MAY be part of currentPos, as lastPos may be a Vec4?
        [FieldOffset(0x34)]
        public UInt32 Unknown4;
    }

    [PacketHandlerAttr(0x04, 0x07)]
    public class MovementHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            // This packet is "Compressed" basically.
            reader.ReadBytes(6); // Get past the junk

            // For simplicity's sake, read the 3 flag bytes into a big int
            byte[] flagBytes = reader.ReadBytes(3);
            uint dataFlags = flagBytes[0];
            dataFlags |= (uint)(flagBytes[1] << 8);
            dataFlags |= (uint)(flagBytes[2] << 16);

            PackedData theFlags = (PackedData)dataFlags;

            // Debug
            Logger.WriteInternal("[MOV] Movement packet from {0} contains {1} data.", context.Character.Name, theFlags);

            // TODO: Maybe do this better someday
            FullMovementData dstData = new FullMovementData();

            if (theFlags.HasFlag(PackedData.Ent1ID))
            {
                dstData.entity1.ID = (uint)reader.ReadUInt64();
            }
            if (theFlags.HasFlag(PackedData.Ent1Type))
            {
                dstData.entity1.EntityType = (EntityType)reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.Ent1A))
            {
                dstData.entity1.Unknown_A = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.Ent2ID))
            {
                dstData.entity1.ID = (uint)reader.ReadUInt64();
            }
            if (theFlags.HasFlag(PackedData.Ent2Type))
            {
                dstData.entity1.EntityType = (EntityType)reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.Ent2A))
            {
                dstData.entity1.Unknown_A = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.Timestamp))
            {
                dstData.timestamp = reader.ReadUInt32();
                context.MovementTimestamp = dstData.timestamp;
            }
            if (theFlags.HasFlag(PackedData.RotX))
            {
                dstData.rotation.x = reader.ReadUInt16();
                context.CurrentLocation.RotX = Helper.FloatFromHalfPrecision(dstData.rotation.x);
            }
            if (theFlags.HasFlag(PackedData.RotY))
            {
                dstData.rotation.y = reader.ReadUInt16();
                context.CurrentLocation.RotY = Helper.FloatFromHalfPrecision(dstData.rotation.y);
            }
            if (theFlags.HasFlag(PackedData.RoyZ))
            {
                dstData.rotation.z = reader.ReadUInt16();
                context.CurrentLocation.RotZ = Helper.FloatFromHalfPrecision(dstData.rotation.z);
            }
            if (theFlags.HasFlag(PackedData.RoyW))
            {
                dstData.rotation.w = reader.ReadUInt16();
                context.CurrentLocation.RotW = Helper.FloatFromHalfPrecision(dstData.rotation.w);
            }
            if (theFlags.HasFlag(PackedData.CurX))
            {
                dstData.currentPos.x = reader.ReadUInt16();
                context.CurrentLocation.PosX = Helper.FloatFromHalfPrecision(dstData.currentPos.x);
            }
            if (theFlags.HasFlag(PackedData.CurY))
            {
                dstData.currentPos.y = reader.ReadUInt16();
                context.CurrentLocation.PosY = Helper.FloatFromHalfPrecision(dstData.currentPos.y);
            }
            if (theFlags.HasFlag(PackedData.CurZ))
            {
                dstData.currentPos.z = reader.ReadUInt16();
                context.CurrentLocation.PosZ = Helper.FloatFromHalfPrecision(dstData.currentPos.z);
            }
            if (theFlags.HasFlag(PackedData.Unknown1))
            {
                dstData.Unknown2 = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.UnkX))
            {
                dstData.unknownPos.x = reader.ReadUInt16();
                context.LastLocation.PosX = Helper.FloatFromHalfPrecision(dstData.unknownPos.x);
            }
            if (theFlags.HasFlag(PackedData.UnkY))
            {
                dstData.unknownPos.y = reader.ReadUInt16();
                context.LastLocation.PosY = Helper.FloatFromHalfPrecision(dstData.unknownPos.y);
            }
            if (theFlags.HasFlag(PackedData.UnkZ))
            {
                dstData.unknownPos.z = reader.ReadUInt16();
                context.LastLocation.PosZ = Helper.FloatFromHalfPrecision(dstData.unknownPos.z);
            }
            if (theFlags.HasFlag(PackedData.Unknown2))
            {
                dstData.Unknown3 = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.Unknown3))
            {
                if (theFlags.HasFlag(PackedData.Unknown4))
                {
                    dstData.Unknown4 = reader.ReadByte();
                }
                else
                {
                    dstData.Unknown4 = reader.ReadUInt32();
                }
            }

            Logger.WriteInternal("[MOV] Player moving! {0} -> ({1}, {2}, {3})",
                                  context.Character.Name, context.CurrentLocation.PosX, context.CurrentLocation.PosY, context.CurrentLocation.PosZ);

            foreach (var c in Server.Instance.Clients)
            {
                if (c.Character == null || c == context || c.CurrentZone != context.CurrentZone)
                    continue;

                c.SendPacket(0x4, 0x7, flags, data);
            }
        }
    }

    [PacketHandlerAttr(0x04, 0x71)]
    public class MovementEndHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            FullMovementData movData = reader.ReadStruct<FullMovementData>();

            if (movData.entity1.ID == 0 && movData.entity2.ID != 0)
                movData.entity1 = movData.entity2;

            movData.timestamp = 0;

            // This could be simplified
            PacketWriter writer = new PacketWriter();
            writer.WriteStruct(movData);

            Logger.WriteInternal("[MOV] {0} stopped moving at ({1}, {2}, {3})", context.Character.Name,
                                 Helper.FloatFromHalfPrecision(movData.currentPos.x), Helper.FloatFromHalfPrecision(movData.currentPos.y),
                                 Helper.FloatFromHalfPrecision(movData.currentPos.z));

            foreach (var c in Server.Instance.Clients)
            {
                if (c == context || c.Character == null || c.CurrentZone != context.CurrentZone)
                    continue;

                c.SendPacket(0x04, 0x71, 0x40, writer.ToArray());
            }
        }
    }

    [PacketHandlerAttr(0x4, 0x8)]
    public class MovementActionHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);

            reader.ReadStruct<ObjectHeader>(); // Skip blank entity header.

            var preformer = reader.ReadStruct<ObjectHeader>(); // Preformer
            byte[] preData = reader.ReadBytes(40);
            string command = reader.ReadAscii(0x922D, 0x45);
            byte[] rest = reader.ReadBytes(4);
            uint thingCount = reader.ReadMagic(0x922D, 0x45);
            byte[] things;

            PacketWriter thingWriter = new PacketWriter();
            for (int i = 0; i < thingCount; i++)
                thingWriter.Write(reader.ReadBytes(4));

            things = thingWriter.ToArray();

            byte[] final = reader.ReadBytes(4);

            Logger.WriteInternal("[ACT] {0} is preforming {1}", context.Character.Name, command);

            foreach (var c in Server.Instance.Clients)
            {
                if (c == context || c.Character == null || c.CurrentZone != context.CurrentZone)
                    continue;

                PacketWriter output = new PacketWriter();

                output.WriteStruct(new ObjectHeader((uint)context.User.PlayerID, EntityType.Player));
                output.WriteStruct(preformer);
                output.Write(preData);
                output.WriteAscii(command, 0x4315, 0x7A);
                output.Write(rest);
                output.WriteMagic(thingCount, 0x4315, 0x7A);
                output.Write(things);
                output.Write(final);

                c.SendPacket(0x4, 0x80, 0x44, output.ToArray());
            }
        }
    }

    [PacketHandlerAttr(0x4, 0x3C)]
    public class ActionUpdateHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);

            reader.ReadStruct<ObjectHeader>(); // Read the blank

            ObjectHeader actor = reader.ReadStruct<ObjectHeader>(); // Read the actor

            byte[] rest = reader.ReadBytes(32); // TODO Map this out and do stuff with it!

            foreach (var c in Server.Instance.Clients)
            {
                if (c == context || c.Character == null || c.CurrentZone != context.CurrentZone)
                    continue;

                PacketWriter writer = new PacketWriter();

                writer.WriteStruct(new ObjectHeader((uint)c.User.PlayerID, EntityType.Player));
                writer.WriteStruct(actor);
                writer.Write(rest);

                c.SendPacket(0x4, 0x81, 0x40, writer.ToArray());
            }
        }
    }
}
