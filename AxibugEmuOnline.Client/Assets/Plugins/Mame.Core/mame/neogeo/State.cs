using cpu.m68000;
using cpu.z80;
using System.IO;

namespace MAME.Core
{
    public unsafe partial class Neogeo
    {
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i, j;
            writer.Write(dsw);
            writer.Write(display_position_interrupt_control);
            writer.Write(display_counter);
            writer.Write(vblank_interrupt_pending);
            writer.Write(display_position_interrupt_pending);
            writer.Write(irq3_pending);
            writer.Write(controller_select);
            writer.Write(main_cpu_bank_address);
            writer.Write(main_cpu_vector_table_source);
            writer.Write(audio_cpu_banks, 0, 4);
            writer.Write(save_ram_unlocked);
            writer.Write(audio_cpu_nmi_enabled);
            writer.Write(audio_cpu_nmi_pending);
            writer.Write(mainram2, 0, 0x10000);
            writer.Write(pvc_cartridge_ram, 0, 0x2000);
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 0x1000; j++)
                {
                    writer.Write(palettes[i, j]);
                }
            }
            for (i = 0; i < 0x10000; i++)
            {
                writer.Write(neogeo_videoram[i]);
            }
            writer.Write(videoram_read_buffer);
            writer.Write(videoram_modulo);
            writer.Write(videoram_offset);
            writer.Write(fixed_layer_source);
            writer.Write(screen_dark);
            writer.Write(palette_bank);
            writer.Write(neogeo_scanline_param);
            writer.Write(auto_animation_speed);
            writer.Write(auto_animation_disabled);
            writer.Write(auto_animation_counter);
            writer.Write(auto_animation_frame_counter);
            writer.Write(Memory.mainram, 0, 0x10000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            writer.Write(Video.screenstate.vblank_start_time.seconds);
            writer.Write(Video.screenstate.vblank_start_time.attoseconds);
            writer.Write(Video.screenstate.frame_number);
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
            writer.Write(AY8910.AA8910[0].stream.output_sampindex);
            writer.Write(AY8910.AA8910[0].stream.output_base_sampindex);
            writer.Write(Sound.ym2610stream.output_sampindex);
            writer.Write(Sound.ym2610stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Pd4900a.SaveStateBinary(writer);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i, j;
            dsw = reader.ReadByte();
            display_position_interrupt_control = reader.ReadByte();
            display_counter = reader.ReadUInt32();
            vblank_interrupt_pending = reader.ReadInt32();
            display_position_interrupt_pending = reader.ReadInt32();
            irq3_pending = reader.ReadInt32();
            controller_select = reader.ReadByte();
            main_cpu_bank_address = reader.ReadInt32();
            main_cpu_vector_table_source = reader.ReadByte();
            audio_cpu_banks = reader.ReadBytes(4);
            save_ram_unlocked = reader.ReadByte();
            audio_cpu_nmi_enabled = reader.ReadBoolean();
            audio_cpu_nmi_pending = reader.ReadBoolean();
            mainram2_set = reader.ReadBytes(0x10000);
            pvc_cartridge_ram = reader.ReadBytes(0x2000);
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 0x1000; j++)
                {
                    palettes[i, j] = reader.ReadUInt16();
                }
            }
            for (i = 0; i < 0x10000; i++)
            {
                neogeo_videoram[i] = reader.ReadUInt16();
            }
            videoram_read_buffer = reader.ReadUInt16();
            videoram_modulo = reader.ReadUInt16();
            videoram_offset = reader.ReadUInt16();
            fixed_layer_source = reader.ReadByte();
            screen_dark = reader.ReadByte();
            palette_bank = reader.ReadByte();
            neogeo_scanline_param = reader.ReadInt32();
            auto_animation_speed = reader.ReadByte();
            auto_animation_disabled = reader.ReadByte();
            auto_animation_counter = reader.ReadInt32();
            auto_animation_frame_counter = reader.ReadInt32();
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.screenstate.vblank_start_time.seconds = reader.ReadInt32();
            Video.screenstate.vblank_start_time.attoseconds = reader.ReadInt64();
            Video.screenstate.frame_number = reader.ReadInt64();
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
            AY8910.AA8910[0].stream.output_sampindex = reader.ReadInt32();
            AY8910.AA8910[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym2610stream.output_sampindex = reader.ReadInt32();
            Sound.ym2610stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Pd4900a.LoadStateBinary(reader);
        }
    }
}
