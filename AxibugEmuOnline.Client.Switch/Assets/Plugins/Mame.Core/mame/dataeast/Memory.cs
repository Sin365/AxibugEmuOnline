namespace MAME.Core
{
    public unsafe partial class Dataeast
    {
        public static byte byte1, byte2;
        public static byte byte1_old, byte2_old;
        public static byte D0ReadOp(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.mainram[address];
            }
            else if (address >= 0x4000 && address <= 0x5fff)
            {
                int offset = address - 0x4000;
                result = Memory.mainrom[basebankmain1 + offset];
            }
            else if (address >= 0x6000 && address <= 0x7fff)
            {
                int offset = address - 0x6000;
                result = Memory.mainrom[basebankmain2 + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte D0ReadOpArg(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.mainram[address];
            }
            else if (address >= 0x4000 && address <= 0x5fff)
            {
                int offset = address - 0x4000;
                result = Memory.mainrom[basebankmain1 + offset];
            }
            else if (address >= 0x6000 && address <= 0x7fff)
            {
                int offset = address - 0x6000;
                result = Memory.mainrom[basebankmain2 + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte D0ReadMemory(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.mainram[address];
            }
            else if (address == 0x1800)
            {
                result = byte1;
            }
            else if (address == 0x1a00)
            {
                result = byte2;
            }
            else if (address == 0x1c00)
            {
                result = dsw;
            }
            else if (address >= 0x4000 && address <= 0x5fff)
            {
                int offset = address - 0x4000;
                result = Memory.mainrom[basebankmain1 + offset];
            }
            else if (address >= 0x6000 && address <= 0x7fff)
            {
                int offset = address - 0x6000;
                result = Memory.mainrom[basebankmain2 + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static void D0WriteMemory(ushort address, byte data)
        {
            if (address <= 0x7ff)
            {
                Memory.mainram[address] = data;
            }
            else if (address >= 0x800 && address <= 0xfff)
            {
                int offset = address - 0x800;
                pcktgal_videoram_w(offset, data);
            }
            else if (address >= 0x1000 && address <= 0x11ff)
            {
                int offset = address - 0x1000;
                Generic.spriteram[offset] = data;
            }
            else if (address == 0x1801)
            {
                pcktgal_flipscreen_w(data);
            }
            else if (address == 0x1a00)
            {
                pcktgal_sound_w(data);
            }
            else if (address == 0x1c00)
            {
                pcktgal_bank_w(data);
            }
            else if (address >= 0x4000 && address <= 0xffff)
            {
                Memory.mainrom[address] = data;
            }
        }
        public static byte D1ReadOp(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.audioram[address];
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = audioromop[basebanksnd + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                int offset = address - 0x8000;
                result = audioromop[offset];
            }
            return result;
        }
        public static byte D1ReadOp_2(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.audioram[address];
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static byte D1ReadOpArg(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.audioram[address];
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static byte D1ReadMemory(ushort address)
        {
            byte result = 0;
            if (address <= 0x7ff)
            {
                result = Memory.audioram[address];
            }
            else if (address == 0x3000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0x3400)
            {
                result = pcktgal_adpcm_reset_r();
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0x8000 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static void D1WriteMemory(ushort address, byte data)
        {
            if (address <= 0x7ff)
            {
                Memory.audioram[address] = data;
            }
            else if (address == 0x0800)
            {
                YM2203.ym2203_control_port_0_w(data);
            }
            else if (address == 0x0801)
            {
                YM2203.ym2203_write_port_0_w(data);
            }
            else if (address == 0x1000)
            {
                YM3812.ym3812_control_port_0_w(data);
            }
            else if (address == 0x1001)
            {
                YM3812.ym3812_write_port_0_w(data);
            }
            else if (address == 0x1800)
            {
                pcktgal_adpcm_data_w(data);
            }
            else if (address == 0x2000)
            {
                pcktgal_sound_bank_w(data);
            }
            else if (address >= 0x4000 && address <= 0xffff)
            {
                Memory.audiorom[address] = data;
            }
        }
    }
}
