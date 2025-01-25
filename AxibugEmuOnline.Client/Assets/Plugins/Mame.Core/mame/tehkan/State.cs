using cpu.z80;
using System.IO;

namespace MAME.Core
{
    public partial class Tehkan
    {
        public unsafe static void SaveStateBinary_pbaction(BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(scroll);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1000);
            writer.Write(Generic.videoram, 0, 0x400);
            writer.Write(pbaction_videoram2, 0, 0x400);
            writer.Write(Generic.colorram, 0, 0x400);
            writer.Write(pbaction_colorram2, 0, 0x400);
            writer.Write(Generic.spriteram, 0, 0x80);
            writer.Write(Generic.paletteram, 0, 0x200);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Z80A.zz1[1].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            for (i = 0; i < 3; i++)
            {
                AY8910.AA8910[i].SaveStateBinary(writer);
            }
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            for (i = 0; i < 3; i++)
            {
                writer.Write(AY8910.AA8910[i].stream.output_sampindex);
                writer.Write(AY8910.AA8910[i].stream.output_base_sampindex);
            }
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_pbaction(BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            scroll = reader.ReadInt32();
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1000));
            Generic.videoram_set = reader.ReadBytes(0x400);
            pbaction_videoram2 = reader.ReadBytes(0x400);
            Generic.colorram_set = reader.ReadBytes(0x400);
            pbaction_colorram2 = reader.ReadBytes(0x400);
            Generic.spriteram_set = reader.ReadBytes(0x80);
            Generic.paletteram_set = reader.ReadBytes(0x200);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Z80A.zz1[1].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            for (i = 0; i < 3; i++)
            {
                AY8910.AA8910[i].LoadStateBinary(reader);
            }
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
            for (i = 0; i < 3; i++)
            {
                AY8910.AA8910[i].stream.output_sampindex = reader.ReadInt32();
                AY8910.AA8910[i].stream.output_base_sampindex = reader.ReadInt32();
            }
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
