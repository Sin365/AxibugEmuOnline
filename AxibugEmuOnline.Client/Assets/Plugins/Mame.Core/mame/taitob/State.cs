using cpu.m68000;
using cpu.z80;
using System;
using System.IO;

namespace MAME.Core
{
    public unsafe partial class Taitob
    {
        public static void SaveStateBinary(BinaryWriter writer)
        {
            //pixel_scroll
            int i;
            writer.Write(dswa);
            writer.Write(dswb);
            writer.Write(basebanksnd);
            writer.Write(eep_latch);
            writer.Write(coin_word);
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(taitob_scroll[i]);
            }
            for (i = 0; i < 0x8000; i++)
            {
                writer.Write(TC0180VCU_ram[i]);
            }
            for (i = 0; i < 0x10; i++)
            {
                writer.Write(TC0180VCU_ctrl[i]);
            }
            writer.Write(TC0220IOC_regs, 0, 8);
            writer.Write(TC0220IOC_port);
            writer.Write(TC0640FIO_regs, 0, 8);
            for (i = 0; i < 0xcc0; i++)
            {
                writer.Write(taitob_spriteram[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(bg_rambank[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(fg_rambank[i]);
            }
            writer.Write(tx_rambank);
            writer.Write(video_control);
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x10000);
            writer.Write(mainram2, 0, 0x1e80);
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
            AY8910.AA8910[0].SaveStateBinary(writer);
            YM2610.F2610.SaveStateBinary(writer);
            for (i = 0; i < 2; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(AY8910.AA8910[0].stream.sample_rate);
            writer.Write(AY8910.AA8910[0].stream.new_sample_rate);
            writer.Write(AY8910.AA8910[0].stream.gain);
            writer.Write(AY8910.AA8910[0].stream.output_sampindex);
            writer.Write(AY8910.AA8910[0].stream.output_base_sampindex);
            writer.Write(Sound.ym2610stream.output_sampindex);
            writer.Write(Sound.ym2610stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            for (i = 0; i < 4; i++)
            {
                writer.Write(c[0].gain[i]);
            }
            writer.Write(c[0].channel_latch);
            for (i = 0; i < 8; i++)
            {
                writer.Write(c[0].latch[i]);
            }
            writer.Write(c[0].reset_comp);
            Eeprom.SaveStateBinary(writer);
            Taitosnd.SaveStateBinary(writer);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i;
            dswa = reader.ReadByte();
            dswb = reader.ReadByte();
            basebanksnd = reader.ReadInt32();
            eep_latch = reader.ReadUInt16();
            coin_word = reader.ReadUInt16();
            for (i = 0; i < 0x1000; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x400; i++)
            {
                taitob_scroll[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x8000; i++)
            {
                TC0180VCU_ram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x10; i++)
            {
                TC0180VCU_ctrl[i] = reader.ReadUInt16();
            }
            TC0220IOC_regs = reader.ReadBytes(8);
            TC0220IOC_port = reader.ReadByte();
            TC0640FIO_regs = reader.ReadBytes(8);
            for (i = 0; i < 0xcc0; i++)
            {
                taitob_spriteram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                bg_rambank[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                fg_rambank[i] = reader.ReadUInt16();
            }
            tx_rambank = reader.ReadUInt16();
            video_control = reader.ReadByte();
            for (i = 0; i < 0x1000; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            mainram2_set = reader.ReadBytes(0x1e80);
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
            AY8910.AA8910[0].LoadStateBinary(reader);
            YM2610.F2610.LoadStateBinary(reader);
            for (i = 0; i < 2; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            AY8910.AA8910[0].stream.sample_rate = reader.ReadInt32();
            AY8910.AA8910[0].stream.new_sample_rate = reader.ReadInt32();
            AY8910.AA8910[0].stream.gain = reader.ReadInt32();
            AY8910.AA8910[0].stream.output_sampindex = reader.ReadInt32();
            AY8910.AA8910[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym2610stream.output_sampindex = reader.ReadInt32();
            Sound.ym2610stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            for (i = 0; i < 4; i++)
            {
                c[0].gain[i] = reader.ReadInt32();
            }
            c[0].channel_latch = reader.ReadInt32();
            c[0].latch = reader.ReadBytes(8);
            c[0].reset_comp = reader.ReadByte();
            Eeprom.LoadStateBinary(reader);
            Taitosnd.LoadStateBinary(reader);
        }
    }
}
