using cpu.m68000;
using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        public static void SaveStateBinary_cuebrick(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(cuebrick_snd_irqlatch);
            writer.Write(cuebrick_nvram_bank);
            for (i = 0; i < 0x8000; i++)
            {
                writer.Write(cuebrick_nvram[i]);
            }
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K051960(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            writer.Write(mainram2, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_cuebrick(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            cuebrick_snd_irqlatch = reader.ReadInt32();
            cuebrick_nvram_bank = reader.ReadInt32();
            for (i = 0; i < 0x8000; i++)
            {
                cuebrick_nvram[i] = reader.ReadUInt16();
            }
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K051960(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            mainram2_set = reader.ReadBytes(0x4000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_mia(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K051960(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            writer.Write(mainram2, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K007232.SaveStateBinary(writer);
            for (i = 0; i < 1; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 1; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k007232stream.output_sampindex);
            writer.Write(Sound.k007232stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_mia(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K051960(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            mainram2_set = reader.ReadBytes(0x4000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K007232.LoadStateBinary(reader);
            for (i = 0; i < 1; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 1; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k007232stream.output_sampindex = reader.ReadInt32();
            Sound.k007232stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_tmnt(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(tmnt_soundlatch);
            for (i = 0; i < 0x40000; i++)
            {
                writer.Write(sampledata[i]);
            }
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K051960(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K007232.SaveStateBinary(writer);
            Upd7759.SaveStateBinary(writer);
            Sample.SaveStateBinary(writer);
            for (i = 0; i < 1; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 1; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k007232stream.output_sampindex);
            writer.Write(Sound.k007232stream.output_base_sampindex);
            writer.Write(Sound.upd7759stream.output_sampindex);
            writer.Write(Sound.upd7759stream.output_base_sampindex);
            writer.Write(Sound.samplestream.output_sampindex);
            writer.Write(Sound.samplestream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_tmnt(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            tmnt_soundlatch = reader.ReadInt32();
            for (i = 0; i < 0x40000; i++)
            {
                sampledata[i] = reader.ReadInt16();
            }
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K051960(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K007232.LoadStateBinary(reader);
            Upd7759.LoadStateBinary(reader);
            Sample.LoadStateBinary(reader);
            for (i = 0; i < 1; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 1; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k007232stream.output_sampindex = reader.ReadInt32();
            Sound.k007232stream.output_base_sampindex = reader.ReadInt32();
            Sound.upd7759stream.output_sampindex = reader.ReadInt32();
            Sound.upd7759stream.output_base_sampindex = reader.ReadInt32();
            Sound.samplestream.output_sampindex = reader.ReadInt32();
            Sound.samplestream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_punkshot(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K051960(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_punkshot(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K051960(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_lgtnfght(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_lgtnfght(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_blswhstl(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(bytee);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinary_blswhstl(System.IO.BinaryReader reader)
        {
            int i;
            bytee = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
        public static void SaveStateBinary_glfgreat(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw1);
            writer.Write(dsw2);
            writer.Write(dsw3);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            SaveStateBinary_K053936(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_glfgreat(System.IO.BinaryReader reader)
        {
            int i;
            dsw1 = reader.ReadByte();
            dsw2 = reader.ReadByte();
            dsw3 = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            LoadStateBinary_K053936(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_tmnt2(System.IO.BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 0x10; i++)
            {
                writer.Write(tmnt2_1c0800[i]);
            }
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            writer.Write(mainram2, 0, 0x80);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinary_tmnt2(System.IO.BinaryReader reader)
        {
            int i;
            for (i = 0; i < 0x10; i++)
            {
                tmnt2_1c0800[i] = reader.ReadUInt16();
            }
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            mainram2_set = reader.ReadBytes(0x80);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
        public static void SaveStateBinary_ssriders(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            writer.Write(mainram2, 0, 0x80);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinary_ssriders(System.IO.BinaryReader reader)
        {
            int i;
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            mainram2_set = reader.ReadBytes(0x80);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
        public static void SaveStateBinary_thndrx2(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(bytee);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K051960(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            K053260.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.k053260stream.output_sampindex);
            writer.Write(Sound.k053260stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinary_thndrx2(System.IO.BinaryReader reader)
        {
            int i;
            bytee = reader.ReadByte();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K051960(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            K053260.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_sampindex = reader.ReadInt32();
            Sound.k053260stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
        public static void SaveStateBinary_prmrsocr(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(basebanksnd);
            writer.Write(init_eeprom_count);
            writer.Write(toggle);
            writer.Write(dim_c);
            writer.Write(dim_v);
            writer.Write(lastdim);
            writer.Write(lasten);
            writer.Write(sprite_colorbase);
            writer.Write(bg_colorbase);
            for (i = 0; i < 3; i++)
            {
                writer.Write(layer_colorbase[i]);
            }
            SaveStateBinary_K053251(writer);
            SaveStateBinary_K052109(writer);
            SaveStateBinary_K053245(writer);
            SaveStateBinary_K053936(writer);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x2000);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            K054539.SaveStateBinary(writer);
            for (i = 0; i < 3; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 3; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(Sound.k054539stream.output_sampindex);
            writer.Write(Sound.k054539stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinary_prmrsocr(System.IO.BinaryReader reader)
        {
            int i;
            basebanksnd = reader.ReadInt32();
            init_eeprom_count = reader.ReadInt32();
            toggle = reader.ReadInt32();
            dim_c = reader.ReadInt32();
            dim_v = reader.ReadInt32();
            lastdim = reader.ReadInt32();
            lasten = reader.ReadInt32();
            sprite_colorbase = reader.ReadInt32();
            bg_colorbase = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                layer_colorbase[i] = reader.ReadInt32();
            }
            LoadStateBinary_K053251(reader);
            LoadStateBinary_K052109(reader);
            LoadStateBinary_K053245(reader);
            LoadStateBinary_K053936(reader);
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x2000));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            K054539.LoadStateBinary(reader);
            for (i = 0; i < 3; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 3; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            Sound.k054539stream.output_sampindex = reader.ReadInt32();
            Sound.k054539stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
    }
}
