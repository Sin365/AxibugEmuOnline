using cpu.m6800;
using cpu.m68000;
using cpu.m6805;
using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Taito
    {
        public static void SaveStateBinary_tokio(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw0);
            writer.Write(dsw1);
            writer.Write(basebankmain);
            writer.Write(videoram, 0, 0x1d00);
            writer.Write(bublbobl_objectram, 0, 0x300);
            writer.Write(Generic.paletteram, 0, 0x200);
            writer.Write(bublbobl_video_enable);
            writer.Write(tokio_prot_count);
            writer.Write(sound_nmi_enable);
            writer.Write(pending_nmi);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1800);
            writer.Write(Memory.audioram, 0, 0x1000);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].SaveStateBinary(writer);
            }
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
            YM2203.FF2203[0].SaveStateBinary(writer);
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
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_tokio(System.IO.BinaryReader reader)
        {
            int i;
            dsw0 = reader.ReadByte();
            dsw1 = reader.ReadByte();
            basebankmain = reader.ReadInt32();
            videoram = reader.ReadBytes(0x1d00);
            bublbobl_objectram = reader.ReadBytes(0x300);
            Generic.paletteram_set = reader.ReadBytes(0x200);
            bublbobl_video_enable = reader.ReadInt32();
            tokio_prot_count = reader.ReadInt32();
            sound_nmi_enable = reader.ReadInt32();
            pending_nmi = reader.ReadInt32();
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1800));
            Memory.Set_audioram(reader.ReadBytes(0x1000));
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].LoadStateBinary(reader);
            }
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
            YM2203.FF2203[0].LoadStateBinary(reader);
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
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_bublbobl(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw0);
            writer.Write(dsw1);
            writer.Write(basebankmain);
            writer.Write(videoram, 0, 0x1d00);
            writer.Write(bublbobl_objectram, 0, 0x300);
            writer.Write(Generic.paletteram, 0, 0x200);
            writer.Write(bublbobl_mcu_sharedram, 0, 0x400);
            writer.Write(bublbobl_video_enable);
            writer.Write(ddr1);
            writer.Write(ddr2);
            writer.Write(ddr3);
            writer.Write(ddr4);
            writer.Write(port1_in);
            writer.Write(port2_in);
            writer.Write(port3_in);
            writer.Write(port4_in);
            writer.Write(port1_out);
            writer.Write(port2_out);
            writer.Write(port3_out);
            writer.Write(port4_out);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1800);
            writer.Write(Memory.audioram, 0, 0x1000);
            writer.Write(mcuram, 0, 0xc0);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].SaveStateBinary(writer);
            }
            M6800.m1.SaveStateBinary(writer);
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
            YM2203.FF2203[0].SaveStateBinary(writer);
            YM3812.SaveStateBinary_YM3526(writer);
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
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(Sound.ym3526stream.output_sampindex);
            writer.Write(Sound.ym3526stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_bublbobl(System.IO.BinaryReader reader)
        {
            int i;
            dsw0 = reader.ReadByte();
            dsw1 = reader.ReadByte();
            basebankmain = reader.ReadInt32();
            videoram = reader.ReadBytes(0x1d00);
            bublbobl_objectram = reader.ReadBytes(0x300);
            Generic.paletteram_set = reader.ReadBytes(0x200);
            bublbobl_mcu_sharedram = reader.ReadBytes(0x400);
            bublbobl_video_enable = reader.ReadInt32();
            ddr1 = reader.ReadByte();
            ddr2 = reader.ReadByte();
            ddr3 = reader.ReadByte();
            ddr4 = reader.ReadByte();
            port1_in = reader.ReadByte();
            port2_in = reader.ReadByte();
            port3_in = reader.ReadByte();
            port4_in = reader.ReadByte();
            port1_out = reader.ReadByte();
            port2_out = reader.ReadByte();
            port3_out = reader.ReadByte();
            port4_out = reader.ReadByte();
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1800));
            Memory.Set_audioram(reader.ReadBytes(0x1000));
            mcuram = reader.ReadBytes(0xc0);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].LoadStateBinary(reader);
            }
            M6800.m1.LoadStateBinary(reader);
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
            YM2203.FF2203[0].LoadStateBinary(reader);
            YM3812.LoadStateBinary_YM3526(reader);
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
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_boblbobl(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw0);
            writer.Write(dsw1);
            writer.Write(basebankmain);
            writer.Write(videoram, 0, 0x1d00);
            writer.Write(bublbobl_objectram, 0, 0x300);
            writer.Write(Generic.paletteram, 0, 0x200);
            writer.Write(bublbobl_video_enable);
            writer.Write(ic43_a);
            writer.Write(ic43_b);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1800);
            writer.Write(mainram2, 0, 0x100);
            writer.Write(mainram3, 0, 0x100);
            writer.Write(Memory.audioram, 0, 0x1000);
            writer.Write(mcuram, 0, 0xc0);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].SaveStateBinary(writer);
            }
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
            YM2203.FF2203[0].SaveStateBinary(writer);
            YM3812.SaveStateBinary_YM3526(writer);
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
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(Sound.ym3526stream.output_sampindex);
            writer.Write(Sound.ym3526stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_boblbobl(System.IO.BinaryReader reader)
        {
            int i;
            dsw0 = reader.ReadByte();
            dsw1 = reader.ReadByte();
            basebankmain = reader.ReadInt32();
            videoram = reader.ReadBytes(0x1d00);
            bublbobl_objectram = reader.ReadBytes(0x300);
            Generic.paletteram_set = reader.ReadBytes(0x200);
            bublbobl_video_enable = reader.ReadInt32();
            ic43_a = reader.ReadInt32();
            ic43_b = reader.ReadInt32();
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1800));
            mainram2_set = reader.ReadBytes(0x100);
            mainram3 = reader.ReadBytes(0x100);
            Memory.Set_audioram(reader.ReadBytes(0x1000));
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].LoadStateBinary(reader);
            }
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
            YM2203.FF2203[0].LoadStateBinary(reader);
            YM3812.LoadStateBinary_YM3526(reader);
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
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_bub68705(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw0);
            writer.Write(dsw1);
            writer.Write(basebankmain);
            writer.Write(videoram, 0, 0x1d00);
            writer.Write(bublbobl_objectram, 0, 0x300);
            writer.Write(Generic.paletteram, 0, 0x200);
            writer.Write(bublbobl_mcu_sharedram, 0, 0x400);
            writer.Write(bublbobl_video_enable);
            writer.Write(portA_in);
            writer.Write(portA_out);
            writer.Write(ddrA);
            writer.Write(portB_in);
            writer.Write(portB_out);
            writer.Write(ddrB);
            for (i = 0; i < 0x100; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x1800);
            writer.Write(Memory.audioram, 0, 0x1000);
            writer.Write(mcuram, 0, 0xc0);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].SaveStateBinary(writer);
            }
            M6805.m1.SaveStateBinary(writer);
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
            YM2203.FF2203[0].SaveStateBinary(writer);
            YM3812.SaveStateBinary_YM3526(writer);
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
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(Sound.ym3526stream.output_sampindex);
            writer.Write(Sound.ym3526stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_bub68705(System.IO.BinaryReader reader)
        {
            int i;
            dsw0 = reader.ReadByte();
            dsw1 = reader.ReadByte();
            basebankmain = reader.ReadInt32();
            videoram = reader.ReadBytes(0x1d00);
            bublbobl_objectram = reader.ReadBytes(0x300);
            Generic.paletteram_set = reader.ReadBytes(0x200);
            bublbobl_mcu_sharedram = reader.ReadBytes(0x400);
            bublbobl_video_enable = reader.ReadInt32();
            portA_in = reader.ReadByte();
            portA_out = reader.ReadByte();
            ddrA = reader.ReadByte();
            portB_in = reader.ReadByte();
            portB_out = reader.ReadByte();
            ddrB = reader.ReadByte();
            for (i = 0; i < 0x100; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x1800));
            Memory.Set_audioram(reader.ReadBytes(0x1000));
            mcuram = reader.ReadBytes(0xc0);
            for (i = 0; i < 3; i++)
            {
                Z80A.zz1[i].LoadStateBinary(reader);
            }
            M6805.m1.LoadStateBinary(reader);
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
            YM2203.FF2203[0].LoadStateBinary(reader);
            YM3812.LoadStateBinary_YM3526(reader);
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
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_sampindex = reader.ReadInt32();
            Sound.ym3526stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
        public static void SaveStateBinary_opwolf(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dswa);
            writer.Write(dswb);
            writer.Write(Inptport.analog_p1x.accum);
            writer.Write(Inptport.analog_p1x.previous);
            writer.Write(Inptport.analog_p1x.lastdigital);
            writer.Write(Inptport.analog_p1y.accum);
            writer.Write(Inptport.analog_p1y.previous);
            writer.Write(Inptport.analog_p1y.lastdigital);
            writer.Write(Inptport.portdata.last_frame_time.seconds);
            writer.Write(Inptport.portdata.last_frame_time.attoseconds);
            writer.Write(Inptport.portdata.last_delta_nsec);
            writer.Write(basebanksnd);
            writer.Write(PC080SN_chips);
            for (i = 0; i < 8; i++)
            {
                writer.Write(PC080SN_ctrl[0][i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(PC080SN_bg_ram_offset[0][i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(PC080SN_bgscroll_ram_offset[0][i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(PC080SN_bgscrollx[0][i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(PC080SN_bgscrolly[0][i]);
            }
            for (i = 0; i < 0x8000; i++)
            {
                writer.Write(PC080SN_ram[0][i]);
            }
            writer.Write(PC080SN_xoffs);
            writer.Write(PC080SN_yoffs);
            writer.Write(PC080SN_yinvert);
            writer.Write(PC080SN_dblwidth);
            writer.Write(PC090OJ_ctrl);
            writer.Write(PC090OJ_buffer);
            writer.Write(PC090OJ_gfxnum);
            writer.Write(PC090OJ_sprite_ctrl);
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(PC090OJ_ram[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(PC090OJ_ram_buffered[i]);
            }
            writer.Write(PC090OJ_xoffs);
            writer.Write(PC090OJ_yoffs);
            writer.Write(opwolf_region);
            writer.Write(cchip_ram, 0, 0x2000);
            writer.Write(adpcm_b, 0, 8);
            writer.Write(adpcm_c, 0, 8);
            writer.Write(opwolf_gun_xoffs);
            writer.Write(opwolf_gun_yoffs);
            for (i = 0; i < 2; i++)
            {
                writer.Write(adpcm_pos[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(adpcm_end[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(adpcm_data[i]);
            }
            writer.Write(m_sprite_ctrl);
            writer.Write(m_sprites_flipscreen);
            writer.Write(current_bank);
            writer.Write(current_cmd);
            writer.Write(cchip_last_7a);
            writer.Write(cchip_last_04);
            writer.Write(cchip_last_05);
            writer.Write(cchip_coins_for_credit, 0, 2);
            writer.Write(cchip_credits_for_coin, 0, 2);
            writer.Write(cchip_coins, 0, 2);
            writer.Write(c588);
            writer.Write(c589);
            writer.Write(c58a);
            writer.Write(m_triggeredLevel1b);
            writer.Write(m_triggeredLevel2);
            writer.Write(m_triggeredLevel2b);
            writer.Write(m_triggeredLevel2c);
            writer.Write(m_triggeredLevel3b);
            writer.Write(m_triggeredLevel13b);
            writer.Write(m_triggeredLevel4);
            writer.Write(m_triggeredLevel5);
            writer.Write(m_triggeredLevel7);
            writer.Write(m_triggeredLevel8);
            writer.Write(m_triggeredLevel9);
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(Generic.paletteram16[i]);
            }
            for (i = 0; i < 0x2000; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x8000);
            writer.Write(mainram2, 0, 0x10000);
            MC68000.m1.SaveStateBinary(writer);
            writer.Write(Memory.audioram, 0, 0x1000);
            for (i = 0; i < Z80A.nZ80; i++)
            {
                Z80A.zz1[i].SaveStateBinary(writer);
            }
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
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            writer.Write(Sound.ym2151stream.output_sampindex);
            writer.Write(Sound.ym2151stream.output_base_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_base_sampindex);
            writer.Write(MSM5205.mm1[1].voice.stream.output_sampindex);
            writer.Write(MSM5205.mm1[1].voice.stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_opwolf(System.IO.BinaryReader reader)
        {
            int i;
            dswa = reader.ReadByte();
            dswb = reader.ReadByte();
            Inptport.analog_p1x.accum = reader.ReadInt32();
            Inptport.analog_p1x.previous = reader.ReadInt32();
            Inptport.analog_p1x.lastdigital = reader.ReadByte();
            Inptport.analog_p1y.accum = reader.ReadInt32();
            Inptport.analog_p1y.previous = reader.ReadInt32();
            Inptport.analog_p1y.lastdigital = reader.ReadByte();
            Inptport.portdata.last_frame_time.seconds = reader.ReadInt32();
            Inptport.portdata.last_frame_time.attoseconds = reader.ReadInt64();
            Inptport.portdata.last_delta_nsec = reader.ReadInt64();
            basebanksnd = reader.ReadInt32();
            PC080SN_chips = reader.ReadInt32();
            for (i = 0; i < 8; i++)
            {
                PC080SN_ctrl[0][i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                PC080SN_bg_ram_offset[0][i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                PC080SN_bgscroll_ram_offset[0][i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                PC080SN_bgscrollx[0][i] = reader.ReadInt32();
            }
            for (i = 0; i < 2; i++)
            {
                PC080SN_bgscrolly[0][i] = reader.ReadInt32();
            }
            for (i = 0; i < 0x8000; i++)
            {
                PC080SN_ram[0][i] = reader.ReadUInt16();
            }
            PC080SN_xoffs = reader.ReadInt32();
            PC080SN_yoffs = reader.ReadInt32();
            PC080SN_yinvert = reader.ReadInt32();
            PC080SN_dblwidth = reader.ReadInt32();
            PC090OJ_ctrl = reader.ReadUInt16();
            PC090OJ_buffer = reader.ReadUInt16();
            PC090OJ_gfxnum = reader.ReadUInt16();
            PC090OJ_sprite_ctrl = reader.ReadUInt16();
            for (i = 0; i < 0x2000; i++)
            {
                PC090OJ_ram[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                PC090OJ_ram_buffered[i] = reader.ReadUInt16();
            }
            PC090OJ_xoffs = reader.ReadInt32();
            PC090OJ_yoffs = reader.ReadInt32();
            opwolf_region = reader.ReadInt32();
            cchip_ram = reader.ReadBytes(0x2000);
            adpcm_b = reader.ReadBytes(8);
            adpcm_c = reader.ReadBytes(8);
            opwolf_gun_xoffs = reader.ReadInt32();
            opwolf_gun_yoffs = reader.ReadInt32();
            for (i = 0; i < 2; i++)
            {
                adpcm_pos[i] = reader.ReadInt32();
            }
            for (i = 0; i < 2; i++)
            {
                adpcm_end[i] = reader.ReadInt32();
            }
            for (i = 0; i < 2; i++)
            {
                adpcm_data[i] = reader.ReadInt32();
            }
            m_sprite_ctrl = reader.ReadUInt16();
            m_sprites_flipscreen = reader.ReadUInt16();
            current_bank = reader.ReadByte();
            current_cmd = reader.ReadByte();
            cchip_last_7a = reader.ReadByte();
            cchip_last_04 = reader.ReadByte();
            cchip_last_05 = reader.ReadByte();
            cchip_coins_for_credit = reader.ReadBytes(2);
            cchip_credits_for_coin = reader.ReadBytes(2);
            cchip_coins = reader.ReadBytes(2);
            c588 = reader.ReadByte();
            c589 = reader.ReadByte();
            c58a = reader.ReadByte();
            m_triggeredLevel1b = reader.ReadByte();
            m_triggeredLevel2 = reader.ReadByte();
            m_triggeredLevel2b = reader.ReadByte();
            m_triggeredLevel2c = reader.ReadByte();
            m_triggeredLevel3b = reader.ReadByte();
            m_triggeredLevel13b = reader.ReadByte();
            m_triggeredLevel4 = reader.ReadByte();
            m_triggeredLevel5 = reader.ReadByte();
            m_triggeredLevel7 = reader.ReadByte();
            m_triggeredLevel8 = reader.ReadByte();
            m_triggeredLevel9 = reader.ReadByte();
            for (i = 0; i < 0x800; i++)
            {
                Generic.paletteram16[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x2000; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x8000));
            mainram2_set = reader.ReadBytes(0x10000);
            MC68000.m1.LoadStateBinary(reader);
            Memory.Set_audioram(reader.ReadBytes(0x1000));
            for (i = 0; i < Z80A.nZ80; i++)
            {
                Z80A.zz1[i].LoadStateBinary(reader);
            }
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
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
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
