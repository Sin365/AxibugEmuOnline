using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Tehkan
    {
        public static byte byte0, byte1, byte2;
        public static byte byte0_old, byte1_old, byte2_old;
        public static byte Z0ReadOp(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte Z0ReadOp_pbaction3(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0x7fff)
            {
                result = mainromop[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte Z0ReadMemory(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0xc000 && address <= 0xcfff)
            {
                int offset = address - 0xc000;
                result = Memory.mainram[offset];
            }
            else if (address >= 0xd000 && address <= 0xd3ff)
            {
                int offset = address - 0xd000;
                result = pbaction_videoram2[offset];
            }
            else if (address >= 0xd400 && address <= 0xd7ff)
            {
                int offset = address - 0xd400;
                result = pbaction_colorram2[offset];
            }
            else if (address >= 0xd800 && address <= 0xdbff)
            {
                int offset = address - 0xd800;
                result = Generic.videoram[offset];
            }
            else if (address >= 0xdc00 && address <= 0xdfff)
            {
                int offset = address - 0xdc00;
                result = Generic.colorram[offset];
            }
            else if (address >= 0xe000 && address <= 0xe07f)
            {
                int offset = address - 0xe000;
                result = Generic.spriteram[offset];
            }
            else if (address >= 0xe400 && address <= 0xe5ff)
            {
                int offset = address - 0xe400;
                result = Generic.paletteram[offset];
            }
            else if (address == 0xe600)
            {
                result = byte0;
            }
            else if (address == 0xe601)
            {
                result = byte1;
            }
            else if (address == 0xe602)
            {
                result = byte2;
            }
            else if (address == 0xe604)
            {
                result = dsw1;
            }
            else if (address == 0xe605)
            {
                result = dsw2;
            }
            else if (address == 0xe606)
            {
                result = 0;
            }
            return result;
        }
        public static byte Z0ReadMemory_pbaction3(ushort address)
        {
            byte result = 0;
            if (address == 0xc000)
            {
                result = pbaction3_prot_kludge_r();
            }
            else
            {
                result = Z0ReadMemory(address);
            }
            return result;
        }
        public static void Z0WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7fff)
            {
                Memory.mainrom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                Memory.mainrom[address] = value;
            }
            else if (address >= 0xc000 && address <= 0xcfff)
            {
                int offset = address - 0xc000;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xd000 && address <= 0xd3ff)
            {
                int offset = address - 0xd000;
                pbaction_videoram2_w(offset, value);
            }
            else if (address >= 0xd400 && address <= 0xd7ff)
            {
                int offset = address - 0xd400;
                pbaction_colorram2_w(offset, value);
            }
            else if (address >= 0xd800 && address <= 0xdbff)
            {
                int offset = address - 0xd800;
                pbaction_videoram_w(offset, value);
            }
            else if (address >= 0xdc00 && address <= 0xdfff)
            {
                int offset = address - 0xdc00;
                pbaction_colorram_w(offset, value);
            }
            else if (address >= 0xe000 && address <= 0xe07f)
            {
                int offset = address - 0xe000;
                Generic.spriteram[offset] = value;
            }
            else if (address >= 0xe400 && address <= 0xe5ff)
            {
                int offset = address - 0xe400;
                Generic.paletteram_xxxxBBBBGGGGRRRR_le_w(offset, value);
            }
            else if (address == 0xe600)
            {
                Generic.interrupt_enable_w(value);
            }
            else if (address == 0xe604)
            {
                pbaction_flipscreen_w(value);
            }
            else if (address == 0xe606)
            {
                pbaction_scroll_w(value);
            }
            else if (address == 0xe800)
            {
                pbaction_sh_command_w(value);
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
        public static byte Z1ReadOp(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0x1fff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static byte Z1ReadMemory(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0x1fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x4000 && address <= 0x47ff)
            {
                int offset = address - 0x4000;
                result = Memory.audioram[offset];
            }
            else if (address == 0x8000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            return result;
        }
        public static void Z1WriteMemory(ushort address, byte value)
        {
            if (address >= 0 && address <= 0x1fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x4000 && address <= 0x47ff)
            {
                int offset = address - 0x4000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xffff)
            {

            }
        }
        public static byte Z1ReadHardware(ushort address)
        {
            byte result = 0;
            return result;
        }
        public static void Z1WriteHardware(ushort address, byte value)
        {
            address &= 0xff;
            if (address >= 0x10 && address <= 0x11)
            {
                int offset = address - 0x10;
                AY8910.AA8910[0].ay8910_write_ym(offset, value);
            }
            else if (address >= 0x20 && address <= 0x21)
            {
                int offset = address - 0x20;
                AY8910.AA8910[1].ay8910_write_ym(offset, value);
            }
            else if (address >= 0x30 && address <= 0x31)
            {
                int offset = address - 0x30;
                AY8910.AA8910[2].ay8910_write_ym(offset, value);
            }
        }
        public static int Z1IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[1].cpunum, 0);
        }
    }
}
