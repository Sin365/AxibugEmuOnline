using cpu.m6800;
using cpu.m6809;
using System.IO;

namespace MAME.Core
{
    public unsafe partial class Namcos1
    {
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i, j;
            writer.Write(dipsw);
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(bank_ram20, 0, 0x2000);
            writer.Write(bank_ram30, 0, 0x80);
            writer.Write(namcos1_videoram, 0, 0x8000);
            writer.Write(namcos1_cus116, 0, 0x10);
            writer.Write(namcos1_spriteram, 0, 0x1000);
            writer.Write(namcos1_playfield_control, 0, 0x20);
            writer.Write(copy_sprites);
            writer.Write(s1ram, 0, 0x8000);
            writer.Write(namcos1_triram, 0, 0x800);
            writer.Write(namcos1_paletteram, 0, 0x8000);
            writer.Write(key, 0, 8);
            writer.Write(audiocpurom_offset);
            writer.Write(mcu_patch_data);
            writer.Write(mcurom_offset);
            writer.Write(namcos1_reset);
            writer.Write(wdog);
            writer.Write(dac0_value);
            writer.Write(dac1_value);
            writer.Write(dac0_gain);
            writer.Write(dac1_gain);
            writer.Write(Generic.generic_nvram, 0, 0x800);
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    writer.Write(cus117_offset[i, j]);
                }
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    writer.Write(user1rom_offset[i, j]);
                }
            }
            for (i = 0; i < 3; i++)
            {
                M6809.mm1[i].SaveStateBinary(writer);
            }
            M6800.m1.SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            Namco.SaveStateBinary(writer);
            DAC.SaveStateBinary(writer);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.namcostream.output_sampindex);
            writer.Write(Sound.namcostream.output_base_sampindex);
            writer.Write(Sound.dacstream.output_sampindex);
            writer.Write(Sound.dacstream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i, j;
            dipsw = reader.ReadByte();
            for (i = 0; i < 0x2000; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            bank_ram20_set = reader.ReadBytes(0x2000);
            bank_ram30_set = reader.ReadBytes(0x80);
            namcos1_videoram = reader.ReadBytes(0x8000);
            namcos1_cus116 = reader.ReadBytes(0x10);
            namcos1_spriteram = reader.ReadBytes(0x1000);
            namcos1_playfield_control = reader.ReadBytes(0x20);
            copy_sprites = reader.ReadInt32();
            s1ram = reader.ReadBytes(0x8000);
            namcos1_triram = reader.ReadBytes(0x800);
            namcos1_paletteram = reader.ReadBytes(0x8000);
            key = reader.ReadBytes(8);
            audiocpurom_offset = reader.ReadInt32();
            mcu_patch_data = reader.ReadInt32();
            mcurom_offset = reader.ReadInt32();
            namcos1_reset = reader.ReadInt32();
            wdog = reader.ReadInt32();
            dac0_value = reader.ReadInt32();
            dac1_value = reader.ReadInt32();
            dac0_gain = reader.ReadInt32();
            dac1_gain = reader.ReadInt32();
            Generic.generic_nvram_set = reader.ReadBytes(0x800);
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    cus117_offset[i, j] = reader.ReadInt32();
                }
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    user1rom_offset[i, j] = reader.ReadInt32();
                }
            }
            for (i = 0; i < 3; i++)
            {
                M6809.mm1[i].LoadStateBinary(reader);
            }
            M6800.m1.LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            Namco.LoadStateBinary(reader);
            DAC.LoadStateBinary(reader);
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.namcostream.output_sampindex = reader.ReadInt32();
            Sound.namcostream.output_base_sampindex = reader.ReadInt32();
            Sound.dacstream.output_sampindex = reader.ReadInt32();
            Sound.dacstream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
