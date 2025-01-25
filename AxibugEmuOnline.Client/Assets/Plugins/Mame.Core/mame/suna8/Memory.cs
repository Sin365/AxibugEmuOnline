using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class SunA8
    {
        public static byte byte1, byte2;
        public static byte byte1_old, byte2_old;
        public static byte Z0ReadOp_starfigh(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = mainromop[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte Z0ReadMemory_starfigh(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else if (address == 0xc000)
            {
                result = byte1;
            }
            else if (address == 0xc001)
            {
                result = byte2;
            }
            else if (address == 0xc002)
            {
                result = dsw1;
            }
            else if (address == 0xc003)
            {
                result = dsw2;
            }
            else if (address == 0xc080)
            {
                result = starfigh_cheats_r();
            }
            else if (address >= 0xc600 && address <= 0xc7ff)
            {
                int offset = address - 0xc600;
                result = suna8_banked_paletteram_r(offset);
            }
            else if (address >= 0xc800 && address <= 0xdfff)
            {
                int offset = address - 0xc800;
                result = Memory.mainram[offset];
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                int offset = address - 0xe000;
                result = suna8_banked_spriteram_r(offset);
            }
            return result;
        }
        public static void Z0WriteMemory_starfigh(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                Memory.mainrom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                Memory.mainrom[basebankmain + offset] = value;
            }
            else if (address == 0xc200)
            {
                starfigh_spritebank_w();
            }
            else if (address >= 0xc280 && address <= 0xc2ff)
            {
                starfigh_rombank_latch_w(value);
            }
            else if (address == 0xc300)
            {
                hardhea2_flipscreen_w(value);
            }
            else if (address >= 0xc380 && address <= 0xc3ff)
            {
                starfigh_spritebank_latch_w(value);
            }
            else if (address == 0xc400)
            {
                starfigh_leds_w(value);
            }
            else if (address == 0xc500)
            {
                starfigh_sound_latch_w(value);
            }
            else if (address >= 0xc600 && address <= 0xc7ff)
            {
                int offset = address - 0xc600;
                Generic.paletteram_RRRRGGGGBBBBxxxx_be_w(offset, value);
            }
            else if (address >= 0xc800 && address <= 0xdfff)
            {
                int offset = address - 0xc800;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                int offset = address - 0xe000;
                suna8_banked_spriteram_w(offset, value);
            }
        }
        public static byte Z0ReadHardware(ushort address)
        {
            return 0;
        }
        public static void Z0WriteHardware(ushort address, byte value)
        {

        }
        public static int Z0IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[0].cpunum, 0);
        }
        public static byte Z1ReadOp_hardhead(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static byte Z1ReadMemory_hardhead(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                int offset = address - 0xc000;
                result = Memory.audioram[offset];
            }
            else if (address == 0xc800)
            {
                result = FMOpl.ym3812_read(0);
            }
            else if (address == 0xd800)
            {
                result = (byte)Sound.soundlatch_r();
            }
            return result;
        }
        public static void Z1Write_Memory_hardhead(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0xa000 && address <= 0xa001)
            {
                int offset = address - 0xa000;
                FMOpl.ym3812_write(offset, value);
            }
            else if (address >= 0xa002 && address <= 0xa003)
            {
                int offset = address - 0xa002;
                AY8910.AA8910[0].ay8910_write_ym(offset, value);
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                int offset = address - 0xc000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xd000)
            {
                Sound.soundlatch2_w(value);
            }
        }
        public static byte Z1ReadHardware(ushort address)
        {
            return 0;
        }
        public static void Z1WriteHardware(ushort address, byte value)
        {

        }
        public static int Z1IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[1].cpunum, 0);
        }
    }
}
