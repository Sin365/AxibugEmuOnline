using cpu.m68000;
using cpu.z80;
using System.IO;

namespace MAME.Core
{
    public unsafe partial class CPS
    {
        public static void SaveStateBinaryC(BinaryWriter writer)
        {
            int i;
            writer.Write(dswa);
            writer.Write(dswb);
            writer.Write(dswc);
            writer.Write(basebanksnd);
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_a_regs[i]);
            }
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_b_regs[i]);
            }
            for (i = 0; i < 0xc00; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x10000);
            writer.Write(gfxram, 0, 0x30000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            writer.Write(Video.screenstate.frame_number);
            writer.Write(Sound.last_update_second);
            for (i = 0; i < 2; i++)
            {
                writer.Write(Cpuexec.cpu[i].suspend);
                writer.Write(Cpuexec.cpu[i].nextsuspend);
                writer.Write(Cpuexec.cpu[i].eatcycles);
                writer.Write(Cpuexec.cpu[i].nexteatcycles);
                writer.Write(Cpuexec.cpu[i].localtime.seconds);
                writer.Write(Cpuexec.cpu[i].localtime.attoseconds);
            }
            EmuTimer.SaveStateBinary(writer);
            YM2151.SaveStateBinary(writer);
            OKI6295.SaveStateBinary(writer);
            for (i = 0; i < 2; i++)
            {
                writer.Write(Sound.latched_value[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(Sound.utempdata[i]);
            }
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(Sound.okistream.output_sampindex);
            writer.Write(Sound.okistream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            switch (RomInfo.Rom.Name)
            {
                case "forgottn":
                case "forgottna":
                case "forgottnu":
                case "forgottnue":
                case "forgottnuc":
                case "forgottnua":
                case "forgottnuaa":
                case "lostwrld":
                case "lostwrldo":
                    writer.Write(Inptport.portdata.last_delta_nsec);
                    break;
            }
        }
        public static void SaveStateBinaryQ(BinaryWriter writer)
        {
            int i;
            writer.Write(dswa);
            writer.Write(dswb);
            writer.Write(dswc);
            writer.Write(basebanksnd);
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_a_regs[i]);
            }
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_b_regs[i]);
            }
            for (i = 0; i < 0xc00; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x10000);
            writer.Write(gfxram, 0, 0x30000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            writer.Write(Video.screenstate.frame_number);
            writer.Write(Sound.last_update_second);
            for (i = 0; i < 2; i++)
            {
                writer.Write(Cpuexec.cpu[i].suspend);
                writer.Write(Cpuexec.cpu[i].nextsuspend);
                writer.Write(Cpuexec.cpu[i].eatcycles);
                writer.Write(Cpuexec.cpu[i].nexteatcycles);
                writer.Write(Cpuexec.cpu[i].localtime.seconds);
                writer.Write(Cpuexec.cpu[i].localtime.attoseconds);
            }
            EmuTimer.SaveStateBinary(writer);
            //writer.Write(qsound_sharedram1);
            //writer.Write(qsound_sharedram2);
            writer.Write(qsound_sharedram1, 0, qsound_sharedram1Length);
            writer.Write(qsound_sharedram2, 0, qsound_sharedram2Length);
            QSound.SaveStateBinary(writer);
            writer.Write(Sound.qsoundstream.output_sampindex);
            writer.Write(Sound.qsoundstream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void SaveStateBinaryC2(BinaryWriter writer)
        {
            int i;
            writer.Write(basebanksnd);
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_a_regs[i]);
            }
            for (i = 0; i < 0x20; i++)
            {
                writer.Write(cps_b_regs[i]);
            }
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(cps2_objram1[i]);
            }
            for (i = 0; i < 0x1000; i++)
            {
                writer.Write(cps2_objram2[i]);
            }
            for (i = 0; i < 6; i++)
            {
                writer.Write(cps2_output[i]);
            }
            writer.Write(cps2networkpresent);
            writer.Write(cps2_objram_bank);
            writer.Write(scancount);
            writer.Write(cps1_scanline1);
            writer.Write(cps1_scanline2);
            writer.Write(cps1_scancalls);
            for (i = 0; i < 0xc00; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x10000);
            writer.Write(gfxram, 0, 0x30000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x800);
            Z80A.zz1[0].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            writer.Write(Video.screenstate.frame_number);
            writer.Write(Sound.last_update_second);
            for (i = 0; i < 2; i++)
            {
                writer.Write(Cpuexec.cpu[i].suspend);
                writer.Write(Cpuexec.cpu[i].nextsuspend);
                writer.Write(Cpuexec.cpu[i].eatcycles);
                writer.Write(Cpuexec.cpu[i].nexteatcycles);
                writer.Write(Cpuexec.cpu[i].localtime.seconds);
                writer.Write(Cpuexec.cpu[i].localtime.attoseconds);
            }
            EmuTimer.SaveStateBinary(writer);
            //writer.Write(qsound_sharedram1);
            //writer.Write(qsound_sharedram2);
            writer.Write(qsound_sharedram1, 0, qsound_sharedram1Length);
            writer.Write(qsound_sharedram2, 0, qsound_sharedram2Length);
            QSound.SaveStateBinary(writer);
            writer.Write(Sound.qsoundstream.output_sampindex);
            writer.Write(Sound.qsoundstream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
            Eeprom.SaveStateBinary(writer);
        }
        public static void LoadStateBinaryC(BinaryReader reader)
        {
            int i;
            dswa = reader.ReadByte();
            dswb = reader.ReadByte();
            dswc = reader.ReadByte();
            basebanksnd = reader.ReadInt32();
            for (i = 0; i < 0x20; i++)
            {
                cps_a_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x20; i++)
            {
                cps_b_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0xc00; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            gfxram_set = reader.ReadBytes(0x30000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.screenstate.frame_number = reader.ReadInt64();
            Sound.last_update_second = reader.ReadInt32();
            for (i = 0; i < 2; i++)
            {
                Cpuexec.cpu[i].suspend = reader.ReadByte();
                Cpuexec.cpu[i].nextsuspend = reader.ReadByte();
                Cpuexec.cpu[i].eatcycles = reader.ReadByte();
                Cpuexec.cpu[i].nexteatcycles = reader.ReadByte();
                Cpuexec.cpu[i].localtime.seconds = reader.ReadInt32();
                Cpuexec.cpu[i].localtime.attoseconds = reader.ReadInt64();
            }
            EmuTimer.LoadStateBinary(reader);
            YM2151.LoadStateBinary(reader);
            OKI6295.LoadStateBinary(reader);
            for (i = 0; i < 2; i++)
            {
                Sound.latched_value[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                Sound.utempdata[i] = reader.ReadUInt16();
            }
            Sound.ym2151stream.output_sampindex = reader.ReadInt32();
            Sound.ym2151stream.output_base_sampindex = reader.ReadInt32();
            Sound.okistream.output_sampindex = reader.ReadInt32();
            Sound.okistream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            switch (RomInfo.Rom.Name)
            {
                case "forgottn":
                case "forgottna":
                case "forgottnu":
                case "forgottnue":
                case "forgottnuc":
                case "forgottnua":
                case "forgottnuaa":
                case "lostwrld":
                case "lostwrldo":
                    Inptport.portdata.last_delta_nsec = reader.ReadInt64();
                    break;
            }
        }
        public static void LoadStateBinaryQ(BinaryReader reader)
        {
            int i;
            dswa = reader.ReadByte();
            dswb = reader.ReadByte();
            dswc = reader.ReadByte();
            basebanksnd = reader.ReadInt32();
            for (i = 0; i < 0x20; i++)
            {
                cps_a_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x20; i++)
            {
                cps_b_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0xc00; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            gfxram_set = reader.ReadBytes(0x30000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.screenstate.frame_number = reader.ReadInt64();
            Sound.last_update_second = reader.ReadInt32();
            for (i = 0; i < 2; i++)
            {
                Cpuexec.cpu[i].suspend = reader.ReadByte();
                Cpuexec.cpu[i].nextsuspend = reader.ReadByte();
                Cpuexec.cpu[i].eatcycles = reader.ReadByte();
                Cpuexec.cpu[i].nexteatcycles = reader.ReadByte();
                Cpuexec.cpu[i].localtime.seconds = reader.ReadInt32();
                Cpuexec.cpu[i].localtime.attoseconds = reader.ReadInt64();
            }
            EmuTimer.LoadStateBinary(reader);
            qsound_sharedram1_set = reader.ReadBytes(0x1000);
            qsound_sharedram2_set = reader.ReadBytes(0x1000);
            QSound.LoadStateBinary(reader);
            Sound.qsoundstream.output_sampindex = reader.ReadInt32();
            Sound.qsoundstream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
        public static void LoadStateBinaryC2(BinaryReader reader)
        {
            int i;
            basebanksnd = reader.ReadInt32();
            for (i = 0; i < 0x20; i++)
            {
                cps_a_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x20; i++)
            {
                cps_b_regs[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x1000; i++)
            {
                cps2_objram1[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x1000; i++)
            {
                cps2_objram2[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 6; i++)
            {
                cps2_output[i] = reader.ReadUInt16();
            }
            cps2networkpresent = reader.ReadInt32();
            cps2_objram_bank = reader.ReadInt32();
            scancount = reader.ReadInt32();
            cps1_scanline1 = reader.ReadInt32();
            cps1_scanline2 = reader.ReadInt32();
            cps1_scancalls = reader.ReadInt32();
            for (i = 0; i < 0xc00; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x10000));
            gfxram_set = reader.ReadBytes(0x30000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            Z80A.zz1[0].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.screenstate.frame_number = reader.ReadInt64();
            Sound.last_update_second = reader.ReadInt32();
            for (i = 0; i < 2; i++)
            {
                Cpuexec.cpu[i].suspend = reader.ReadByte();
                Cpuexec.cpu[i].nextsuspend = reader.ReadByte();
                Cpuexec.cpu[i].eatcycles = reader.ReadByte();
                Cpuexec.cpu[i].nexteatcycles = reader.ReadByte();
                Cpuexec.cpu[i].localtime.seconds = reader.ReadInt32();
                Cpuexec.cpu[i].localtime.attoseconds = reader.ReadInt64();
            }
            EmuTimer.LoadStateBinary(reader);
            qsound_sharedram1_set = reader.ReadBytes(0x1000);
            qsound_sharedram2_set = reader.ReadBytes(0x1000);
            QSound.LoadStateBinary(reader);
            Sound.qsoundstream.output_sampindex = reader.ReadInt32();
            Sound.qsoundstream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
            Eeprom.LoadStateBinary(reader);
        }
    }
}
