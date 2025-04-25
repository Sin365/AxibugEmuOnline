using cpu.m68000;
using cpu.m6809;
using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Capcom
    {
        public static void SaveStateBinary_gng(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(bytedsw1);
            writer.Write(bytedsw2);
            writer.Write(basebankmain);
            writer.Write(gng_fgvideoram, 0, 0x800);
            writer.Write(gng_bgvideoram, 0, 0x800);
            writer.Write(scrollx, 0, 2);
            writer.Write(scrolly, 0, 2);
            writer.Write(Generic.paletteram, 0, 0x100);
            writer.Write(Generic.paletteram_2, 0, 0x100);
            writer.Write(Generic.spriteram, 0, 0x200);
            writer.Write(Generic.buffered_spriteram, 0, 0x200);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1e00);
            M6809.mm1[0].SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            AY8910.AA8910[0].SaveStateBinary(writer);
            AY8910.AA8910[1].SaveStateBinary(writer);
            YM2203.FF2203[0].SaveStateBinary(writer);
            YM2203.FF2203[1].SaveStateBinary(writer);
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            writer.Write(AY8910.AA8910[0].stream.output_sampindex);
            writer.Write(AY8910.AA8910[0].stream.output_base_sampindex);
            writer.Write(AY8910.AA8910[1].stream.output_sampindex);
            writer.Write(AY8910.AA8910[1].stream.output_base_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(YM2203.FF2203[1].stream.output_sampindex);
            writer.Write(YM2203.FF2203[1].stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_gng(System.IO.BinaryReader reader)
        {
            int i;
            bytedsw1 = reader.ReadByte();
            bytedsw2 = reader.ReadByte();
            basebankmain = reader.ReadInt32();
            gng_fgvideoram_set = reader.ReadBytes(0x800);
            gng_bgvideoram_set = reader.ReadBytes(0x800);
            scrollx_set = reader.ReadBytes(2);
            scrolly_set = reader.ReadBytes(2);
            Generic.paletteram_set = reader.ReadBytes(0x100);
            Generic.paletteram_2_set = reader.ReadBytes(0x100);
            Generic.spriteram_set = reader.ReadBytes(0x200);
            Generic.buffered_spriteram_set = reader.ReadBytes(0x200);
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1e00));
            M6809.mm1[0].LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            AY8910.AA8910[0].LoadStateBinary(reader);
            AY8910.AA8910[1].LoadStateBinary(reader);
            YM2203.FF2203[0].LoadStateBinary(reader);
            YM2203.FF2203[1].LoadStateBinary(reader);
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
            AY8910.AA8910[0].stream.output_sampindex = reader.ReadInt32();
            AY8910.AA8910[0].stream.output_base_sampindex = reader.ReadInt32();
            AY8910.AA8910[1].stream.output_sampindex = reader.ReadInt32();
            AY8910.AA8910[1].stream.output_base_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            YM2203.FF2203[1].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[1].stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_sf(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(basebanksnd1);
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(sf_objectram[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(sf_videoram[i]);
            }
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            writer.Write(bg_scrollx);
            writer.Write(fg_scrollx);
            writer.Write(sf_active);
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x6000);
            MC68000.m1.SaveStateBinary(writer);
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
            YM2151.SaveStateBinary(writer);
            MSM5205.mm1[0].SaveStateBinary(writer);
            MSM5205.mm1[1].SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_base_sampindex);
            writer.Write(MSM5205.mm1[1].voice.stream.output_sampindex);
            writer.Write(MSM5205.mm1[1].voice.stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_sf(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadUInt16();
            dsw2 = reader.ReadUInt16();
            basebanksnd1 = reader.ReadInt32();
            for (i = 0; i < 0x1000; i++)
            {
                sf_objectram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                sf_videoram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x400; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            bg_scrollx = reader.ReadInt32();
            fg_scrollx = reader.ReadInt32();
            sf_active = reader.ReadInt32();
            for (i = 0; i < 0x400; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x6000));
            MC68000.m1.LoadStateBinary(reader);
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
            YM2151.LoadStateBinary(reader);
            MSM5205.mm1[0].LoadStateBinary(reader);
            MSM5205.mm1[1].LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            MSM5205.mm1[0].voice.stream.output_sampindex = reader.ReadInt32();
            MSM5205.mm1[0].voice.stream.output_base_sampindex = reader.ReadInt32();
            MSM5205.mm1[1].voice.stream.output_sampindex = reader.ReadInt32();
            MSM5205.mm1[1].voice.stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
