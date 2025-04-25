using cpu.nec;
using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw);
            writer.Write(setvector_param);
            writer.Write(irqvector);
            writer.Write(sample_addr);
            writer.Write(protection_ram, 0, 0x1000);
            writer.Write(m72_irq_base);
            writer.Write(m72_scanline_param);
            for (i = 0; i < 0x600; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x600; i++)
            {
                writer.Write(Generic.paletteram16_2[i]);
            }
            for (i = 0; i < 0x200; i++)
            {
                writer.Write(Generic.spriteram16[i]);
            }
            writer.Write(m72_videoram1, 0, 0x4000);
            writer.Write(m72_videoram2, 0, 0x4000);
            writer.Write(m72_raster_irq_position);
            writer.Write(video_off);
            writer.Write(scrollx1);
            writer.Write(scrolly1);
            writer.Write(scrollx2);
            writer.Write(scrolly2);
            for (i = 0; i < 0x200; i++)
            {
                writer.Write(m72_spriteram[i]);
            }
            //majtitle_rowscrollram spriteram_size majtitle_rowscroll
            for (i = 0; i < 0x201; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x4000);
            Nec.nn1[0].SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x10000);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            Cpuint.SaveStateBinary_v(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            DAC.SaveStateBinary(writer);
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.dacstream.output_sampindex);
            writer.Write(Sound.dacstream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            dsw = reader.ReadUInt16();
            setvector_param = reader.ReadInt32();
            irqvector = reader.ReadByte();
            sample_addr = reader.ReadInt32();
            protection_ram = reader.ReadBytes(0x1000);
            m72_irq_base = reader.ReadByte();
            m72_scanline_param = reader.ReadInt32();
            for (i = 0; i < 0x600; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x600; i++)
            {
                Generic.paletteram16_2[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x200; i++)
            {
                Generic.spriteram16[i] = reader.ReadUInt16();
            }
            m72_videoram1 = reader.ReadBytes(0x4000);
            m72_videoram2 = reader.ReadBytes(0x4000);
            m72_raster_irq_position = reader.ReadInt32();
            video_off = reader.ReadInt32();
            scrollx1 = reader.ReadInt32();
            scrolly1 = reader.ReadInt32();
            scrollx2 = reader.ReadInt32();
            scrolly2 = reader.ReadInt32();
            for (i = 0; i < 0x200; i++)
            {
                m72_spriteram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x201; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x4000));
            Nec.nn1[0].LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x10000));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            Cpuint.LoadStateBinary_v(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            DAC.LoadStateBinary(reader);
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.dacstream.output_sampindex = reader.ReadInt32();
            Sound.dacstream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
