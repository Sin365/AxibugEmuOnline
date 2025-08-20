using cpu.m68000;
using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class PGM
    {
        public static byte[] mainbiosrom, videobios, audiobios;
        public static byte[] pgm_bg_videoram, pgm_tx_videoram, pgm_rowscrollram, pgm_videoregs, sprmaskrom, sprcolrom, tilesrom, /*tiles1rom,*/ /*tiles2rom, */pgm_sprite_a_region;
        public static byte CalVal, CalMask, CalCom = 0, CalCnt = 0;
        public static uint[] arm7_shareram;
        public static uint arm7_latch;
        public static int pgm_sprite_a_region_allocate;

        #region //指针化 tiles1rom
        static byte[] tiles1rom_src;
        static GCHandle tiles1rom_handle;
        public static byte* tiles1rom;
        public static int tiles1romLength;
        public static bool tiles1rom_IsNull => tiles1rom == null;
        public static byte[] tiles1rom_set
        {
            set
            {
                tiles1rom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                tiles1rom_src = value;
                tiles1romLength = value.Length;
                tiles1rom_src.GetObjectPtr(ref tiles1rom_handle, ref tiles1rom);
            }
        }
        #endregion

        #region //指针化 tiles2rom
        static byte[] tiles2rom_src;
        static GCHandle tiles2rom_handle;
        public static byte* tiles2rom;
        public static int tiles2romLength;
        public static bool tiles2rom_IsNull => tiles2rom == null;
        public static byte[] tiles2rom_set
        {
            set
            {
                tiles2rom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                tiles2rom_src = value;
                tiles2romLength = value.Length;
                tiles2rom_src.GetObjectPtr(ref tiles2rom_handle, ref tiles2rom);
            }
        }
        #endregion

        public static void PGMInit()
        {
            Machine.bRom = true;
            mainbiosrom = MameMainMotion.resource.pgmmainbios;
            videobios = MameMainMotion.resource.pgmvideobios;
            audiobios = MameMainMotion.resource.pgmaudiobios;
            ICS2115.icsrom = audiobios;
            byte[] bb1, bb2;
            int i3, n1, n2, n3;
            bb1 = Machine.GetRom("ics.rom");
            bb2 = Machine.GetRom("tiles.rom");
            if (bb1 == null)
            {
                bb1 = new byte[0];
            }
            n1 = bb1.Length;
            n2 = bb2.Length;
            ICS2115.icsrom = new byte[0x400000 + n1];
            Array.Copy(audiobios, ICS2115.icsrom, 0x200000);
            Array.Copy(bb1, 0, ICS2115.icsrom, 0x400000, n1);
            tilesrom = new byte[0x400000 + n2];
            Array.Copy(videobios, tilesrom, 0x200000);
            Array.Copy(bb2, 0, tilesrom, 0x400000, n2);
            n3 = tilesrom.Length;
            tiles1rom_set = new byte[n3 * 2];
            for (i3 = 0; i3 < n3; i3++)
            {
                tiles1rom[i3 * 2] = (byte)(tilesrom[i3] & 0x0f);
                tiles1rom[i3 * 2 + 1] = (byte)(tilesrom[i3] >> 4);
            }
            Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
            sprmaskrom = Machine.GetRom("sprmask.rom");
            sprcolrom = Machine.GetRom("sprcol.rom");
            expand_32x32x5bpp();
            expand_colourdata();
            Memory.Set_mainram(new byte[0x20000]);
            pgm_bg_videoram = new byte[0x4000];
            pgm_tx_videoram = new byte[0x2000];
            pgm_rowscrollram = new byte[0x800];
            Generic.paletteram16_set = new ushort[0x900];
            pgm_videoregs = new byte[0x10000];
            Memory.Set_audioram(new byte[0x10000]);
            if (Memory.mainrom_IsNull || sprmaskrom == null || pgm_sprite_a_region == null)
            {
                Machine.bRom = false;
            }
        }
        private static void expand_32x32x5bpp()
        {
            int n2 = tilesrom.Length / 5 * 8;
            tiles2rom_set = new byte[n2];
            int cnt;
            byte pix;
            for (cnt = 0; cnt < tilesrom.Length / 5; cnt++)
            {
                pix = (byte)((tilesrom[0 + 5 * cnt] >> 0) & 0x1f); tiles2rom[0 + 8 * cnt] = pix;
                pix = (byte)(((tilesrom[0 + 5 * cnt] >> 5) & 0x07) | ((tilesrom[1 + 5 * cnt] << 3) & 0x18)); tiles2rom[1 + 8 * cnt] = pix;
                pix = (byte)((tilesrom[1 + 5 * cnt] >> 2) & 0x1f); tiles2rom[2 + 8 * cnt] = pix;
                pix = (byte)(((tilesrom[1 + 5 * cnt] >> 7) & 0x01) | ((tilesrom[2 + 5 * cnt] << 1) & 0x1e)); tiles2rom[3 + 8 * cnt] = pix;
                pix = (byte)(((tilesrom[2 + 5 * cnt] >> 4) & 0x0f) | ((tilesrom[3 + 5 * cnt] << 4) & 0x10)); tiles2rom[4 + 8 * cnt] = pix;
                pix = (byte)((tilesrom[3 + 5 * cnt] >> 1) & 0x1f); tiles2rom[5 + 8 * cnt] = pix;
                pix = (byte)(((tilesrom[3 + 5 * cnt] >> 6) & 0x03) | ((tilesrom[4 + 5 * cnt] << 2) & 0x1c)); tiles2rom[6 + 8 * cnt] = pix;
                pix = (byte)((tilesrom[4 + 5 * cnt] >> 3) & 0x1f); tiles2rom[7 + 8 * cnt] = pix;
            }
        }
        private static void expand_colourdata()
        {
            int srcsize = sprcolrom.Length;
            int cnt;
            int needed = srcsize / 2 * 3;
            int pgm_sprite_a_region_allocate = 1;
            int colpack;
            while (pgm_sprite_a_region_allocate < needed)
            {
                pgm_sprite_a_region_allocate = pgm_sprite_a_region_allocate << 1;
            }
            pgm_sprite_a_region = new byte[pgm_sprite_a_region_allocate];
            for (cnt = 0; cnt < srcsize / 2; cnt++)
            {
                colpack = sprcolrom[cnt * 2] | (sprcolrom[cnt * 2 + 1] << 8);
                pgm_sprite_a_region[cnt * 3 + 0] = (byte)((colpack >> 0) & 0x1f);
                pgm_sprite_a_region[cnt * 3 + 1] = (byte)((colpack >> 5) & 0x1f);
                pgm_sprite_a_region[cnt * 3 + 2] = (byte)((colpack >> 10) & 0x1f);
            }
        }
        public static void machine_reset_pgm()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_HALT, LineState.ASSERT_LINE);
            device_reset();
        }
        public unsafe static byte z80_ram_r(int offset)
        {
            return Memory.audioram[offset];
        }
        public unsafe static void z80_ram_w(int offset, byte data)
        {
            int pc = MC68000.m1.PC;
            Memory.audioram[offset] = data;
            if (pc != 0xf12 && pc != 0xde2 && pc != 0x100c50 && pc != 0x100b20)
            {
                //error
            }
        }
        public static void z80_reset_w(ushort data)
        {
            if (data == 0x5050)
            {
                ICS2115.ics2115_reset();
                Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_HALT, LineState.CLEAR_LINE);
                Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.PULSE_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_HALT, LineState.ASSERT_LINE);
            }
        }
        public static void z80_ctrl_w()
        {

        }
        public static void m68k_l1_w(byte data)
        {
            //if(ACCESSING_BITS_0_7)
            Sound.soundlatch_w(data);
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static void m68k_l1_w(ushort data)
        {
            Sound.soundlatch_w(data);
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static void z80_l3_w(byte data)
        {
            Sound.soundlatch3_w(data);
        }
        public static void sound_irq(int level)
        {
            Cpuint.cpunum_set_input_line(1, 0, (LineState)level);
        }
        public static byte bcd(byte data)
        {
            return (byte)(((data / 10) << 4) | (data % 10));
        }
        public static byte pgm_calendar_r()
        {
            byte calr;
            calr = (byte)(((CalVal & CalMask) != 0) ? 1 : 0);
            CalMask <<= 1;
            return calr;
        }
        public static void pgm_calendar_w(ushort data)
        {
            //DateTime time = DateTime.Now;
            DateTime time = DateTime.Parse("1970-01-01 08:00:00");
            CalCom <<= 1;
            CalCom |= (byte)(data & 1);
            ++CalCnt;
            if (CalCnt == 4)
            {
                CalMask = 1;
                CalVal = 1;
                CalCnt = 0;
                switch (CalCom & 0xf)
                {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                    case 9:
                    case 0xb:
                    case 0xd:
                        CalVal++;
                        break;
                    case 0:
                        CalVal = bcd((byte)time.DayOfWeek); //??
                        break;
                    case 2:  //Hours
                        CalVal = bcd((byte)time.Hour);
                        break;
                    case 4:  //Seconds
                        CalVal = bcd((byte)time.Second);
                        break;
                    case 6:  //Month
                        CalVal = bcd((byte)(time.Month)); //?? not bcd in MVS
                        break;
                    case 8:
                        CalVal = 0; //Controls blinking speed, maybe milliseconds
                        break;
                    case 0xa: //Day
                        CalVal = bcd((byte)time.Day);
                        break;
                    case 0xc: //Minute
                        CalVal = bcd((byte)time.Minute);
                        break;
                    case 0xe:  //Year
                        CalVal = bcd((byte)(time.Year % 100));
                        break;
                    case 0xf:  //Load Date
                        //mame_get_base_datetime(machine, &systime);
                        break;
                }
            }
        }

    }
}
