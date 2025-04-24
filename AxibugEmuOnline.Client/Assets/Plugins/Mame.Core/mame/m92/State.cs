using cpu.nec;

namespace MAME.Core
{
    public unsafe partial class M92
    {
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, j;
            writer.Write(dsw);
            writer.Write(irqvector);
            writer.Write(sound_status);
            writer.Write(bankaddress);
            writer.Write(m92_irq_vectorbase);
            writer.Write(m92_raster_irq_position);
            writer.Write(m92_scanline_param);
            writer.Write(setvector_param);
            writer.Write(m92_sprite_buffer_busy);
            for (i = 0; i < 4; i++)
            {
                writer.Write(pf_master_control[i]);
            }
            writer.Write(m92_sprite_list);
            for (i = 0; i < 0x8000; i++)
            {
                writer.Write(m92_vram_data[i]);
            }
            for (i = 0; i < 8; i++)
            {
                writer.Write(m92_spritecontrol[i]);
            }
            writer.Write(m92_game_kludge);
            writer.Write(m92_palette_bank);
            for (i = 0; i < 3; i++)
            {
                writer.Write(pf_layer[i].vram_base);
            }
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    writer.Write(pf_layer[i].control[j]);
                }
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(Generic.buffered_spriteram16[i]);
            }
            for (i = 0; i < 0x801; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x10000);
            Nec.nn1[0].SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x4000);
            Nec.nn1[1].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            Cpuint.SaveStateBinary_v(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            Iremga20.SaveStateBinary(writer);
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.iremga20stream.output_sampindex);
            writer.Write(Sound.iremga20stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, j;
            dsw = reader.ReadUInt16();
            irqvector = reader.ReadByte();
            sound_status = reader.ReadUInt16();
            bankaddress = reader.ReadInt32();
            m92_irq_vectorbase = reader.ReadByte();
            m92_raster_irq_position = reader.ReadInt32();
            m92_scanline_param = reader.ReadInt32();
            setvector_param = reader.ReadInt32();
            m92_sprite_buffer_busy = reader.ReadByte();
            for (i = 0; i < 4; i++)
            {
                pf_master_control[i] = reader.ReadUInt16();
            }
            m92_sprite_list = reader.ReadInt32();
            for (i = 0; i < 0x8000; i++)
            {
                m92_vram_data[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 8; i++)
            {
                m92_spritecontrol[i] = reader.ReadUInt16();
            }
            m92_game_kludge = reader.ReadInt32();
            m92_palette_bank = reader.ReadInt32();
            for (i = 0; i < 3; i++)
            {
                pf_layer[i].vram_base = reader.ReadUInt16();
            }
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    pf_layer[i].control[j] = reader.ReadUInt16();
                }
            }
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x400; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x400; i++)
            {
                Generic.buffered_spriteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x801; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            Nec.nn1[0].LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x4000));
            Nec.nn1[1].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            Cpuint.LoadStateBinary_v(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            Iremga20.LoadStateBinary(reader);
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.iremga20stream.output_sampindex = reader.ReadInt32();
            Sound.iremga20stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
