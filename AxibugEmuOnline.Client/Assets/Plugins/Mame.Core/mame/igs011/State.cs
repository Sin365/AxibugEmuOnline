using cpu.m68000;
using System.IO;

namespace MAME.Core
{
    public unsafe partial class IGS011
    {
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i, j;
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Generic.generic_nvram, 0, 0x4000);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(priority_ram[i]);
            }
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(paletteram16[i]);
            }
            writer.Write(prot1);
            writer.Write(prot2);
            writer.Write(prot1_swap);
            writer.Write(prot1_addr);
            for (i = 0; i < 2; i++)
            {
                writer.Write(igs003_reg[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(vbowl_trackball[i]);
            }
            writer.Write(priority);
            writer.Write(igs_dips_sel);
            writer.Write(igs_input_sel);
            writer.Write(lhb_irq_enable);
            writer.Write(igs012_prot);
            writer.Write(igs012_prot_swap);
            writer.Write(igs012_prot_mode);
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 0x20000; j++)
                {
                    writer.Write(layer[i][j]);
                }
            }
            writer.Write(lhb2_pen_hi);
            writer.Write(blitter.x);
            writer.Write(blitter.y);
            writer.Write(blitter.w);
            writer.Write(blitter.h);
            writer.Write(blitter.gfx_lo);
            writer.Write(blitter.gfx_hi);
            writer.Write(blitter.depth);
            writer.Write(blitter.pen);
            writer.Write(blitter.flags);
            MC68000.m1.SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            OKI6295.SaveStateBinary(writer);
            YM3812.SaveStateBinary(writer);
            writer.Write(Sound.okistream.output_sampindex);
            writer.Write(Sound.okistream.output_base_sampindex);
            writer.Write(Sound.ym3812stream.output_sampindex);
            writer.Write(Sound.ym3812stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i, j;
            for (i = 0; i < 0x800; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Generic.generic_nvram_set = reader.ReadBytes(0x4000);
            for (i = 0; i < 0x800; i++)
            {
                priority_ram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x1000; i++)
            {
                paletteram16[i] = reader.ReadUInt16();
            }
            prot1 = reader.ReadByte();
            prot2 = reader.ReadByte();
            prot1_swap = reader.ReadByte();
            prot1_addr = reader.ReadUInt32();
            for (i = 0; i < 2; i++)
            {
                igs003_reg[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                vbowl_trackball[i] = reader.ReadUInt16();
            }
            priority = reader.ReadUInt16();
            igs_dips_sel = reader.ReadUInt16();
            igs_input_sel = reader.ReadUInt16();
            lhb_irq_enable = reader.ReadUInt16();
            igs012_prot = reader.ReadByte();
            igs012_prot_swap = reader.ReadByte();
            igs012_prot_mode = reader.ReadBoolean();
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 0x20000; j++)
                {
                    layer[i][j] = reader.ReadByte();
                }
            }
            lhb2_pen_hi = reader.ReadByte();
            blitter.x = reader.ReadUInt16();
            blitter.y = reader.ReadUInt16();
            blitter.w = reader.ReadUInt16();
            blitter.h = reader.ReadUInt16();
            blitter.gfx_lo = reader.ReadUInt16();
            blitter.gfx_hi = reader.ReadUInt16();
            blitter.depth = reader.ReadUInt16();
            blitter.pen = reader.ReadUInt16();
            blitter.flags = reader.ReadUInt16();
            MC68000.m1.LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            OKI6295.LoadStateBinary(reader);
            YM3812.LoadStateBinary(reader);
            Sound.okistream.output_sampindex = reader.ReadInt32();
            Sound.okistream.output_base_sampindex = reader.ReadInt32();
            Sound.ym3812stream.output_sampindex = reader.ReadInt32();
            Sound.ym3812stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
