using cpu.m68000;
using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class PGM
    {
        public unsafe static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, j;
            writer.Write(pgm_tx_videoram, 0, 0x2000);
            writer.Write(pgm_bg_videoram, 0, 0x4000);
            writer.Write(pgm_rowscrollram, 0, 0x800);
            writer.Write(pgm_videoregs, 0, 0x10000);
            writer.Write(CalVal);
            writer.Write(CalMask);
            writer.Write(CalCom);
            writer.Write(CalCnt);
            writer.Write(asic3_reg);
            writer.Write(asic3_x);
            for (i = 0; i < 3; i++)
            {
                writer.Write(asic3_latch[i]);
            }
            writer.Write(asic3_hold);
            writer.Write(asic3_hilo);
            for (i = 0; i < 0x900; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x901; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x20000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x10000);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            ICS2115.SaveStateBinary(writer);
            for (i = 0; i < 3; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 3; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(Sound.ics2115stream.output_sampindex);
            writer.Write(Sound.ics2115stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, j;
            pgm_tx_videoram = reader.ReadBytes(0x2000);
            pgm_bg_videoram = reader.ReadBytes(0x4000);
            pgm_rowscrollram = reader.ReadBytes(0x800);
            pgm_videoregs = reader.ReadBytes(0x10000);
            CalVal = reader.ReadByte();
            CalMask = reader.ReadByte();
            CalCom = reader.ReadByte();
            CalCnt = reader.ReadByte();
            asic3_reg = reader.ReadByte();
            asic3_x = reader.ReadByte();
            for (i = 0; i < 3; i++)
            {
                asic3_latch[i] = reader.ReadByte();
            }
            asic3_hold = reader.ReadUInt16();
            asic3_hilo = reader.ReadUInt16();
            for (i = 0; i < 0x900; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x901; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x20000));
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x10000));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            ICS2115.LoadStateBinary(reader);
            for (i = 0; i < 3; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 3; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            Sound.ics2115stream.output_sampindex = reader.ReadInt32();
            Sound.ics2115stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
