namespace MAME.Core
{
    public unsafe partial class Taitob
    {
        public static sbyte MReadOpByte_silentd(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x300000 && address <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x400000 && address <= 0x403fff)
            {
                result = (sbyte)Memory.mainram[address - 0x400000];
            }
            return result;
        }
        public static sbyte MReadByte_silentd(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address <= 0x100001)
            {
                result = 0;
            }
            else if (address >= 0x100002 && address <= 0x100003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(Taitosnd.taitosound_comm16_msb_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Taitosnd.taitosound_comm16_msb_r();
                }
            }
            else if (address >= 0x200000 && address <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)TC0220IOC_halfword_r(offset);
                }
            }
            else if (address >= 0x210000 && address <= 0x210001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte3;
                }
            }
            else if (address >= 0x220000 && address <= 0x220001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte4;
                }
            }
            else if (address >= 0x230000 && address <= 0x230001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte5;
                }
            }
            else if (address >= 0x300000 && address <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x400000 && address <= 0x403fff)
            {
                result = (sbyte)Memory.mainram[address - 0x400000];
            }
            else if (address >= 0x500000 && address <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(TC0180VCU_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)TC0180VCU_word_r(offset);
                }
            }
            else if (address >= 0x510000 && address <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(taitob_spriteram[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)taitob_spriteram[offset];
                }
            }
            else if (address >= 0x511980 && address <= 0x5137ff)
            {
                result = (sbyte)mainram2[address - 0x511980];
            }
            else if (address >= 0x513800 && address <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(taitob_scroll[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)taitob_scroll[offset];
                }
            }
            else if (address >= 0x518000 && address <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(taitob_v_control_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)taitob_v_control_r(offset);
                }
            }
            else if (address >= 0x540000 && address <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(TC0180VCU_framebuffer_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)TC0180VCU_framebuffer_word_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_silentd(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x300000 && address + 1 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x400000 && address + 1 <= 0x403fff)
            {
                result = (short)(Memory.mainram[address - 0x400000] * 0x100 + Memory.mainram[address - 0x400000 + 1]);
            }
            return result;
        }
        public static short MReadWord_silentd(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address + 1 <= 0x100001)
            {
                result = 0;
            }
            else if (address >= 0x100002 && address + 1 <= 0x100003)
            {
                result = (short)Taitosnd.taitosound_comm16_msb_r();
            }
            else if (address >= 0x200000 && address + 1 <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)TC0220IOC_halfword_r(offset);
            }
            else if (address >= 0x210000 && address + 1 <= 0x210001)
            {
                result = (short)sbyte3;
            }
            else if (address >= 0x220000 && address + 1 <= 0x220001)
            {
                result = (short)sbyte4;
            }
            else if (address >= 0x230000 && address + 1 <= 0x230001)
            {
                result = (short)sbyte5;
            }
            else if (address >= 0x300000 && address + 1 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x400000 && address + 1 <= 0x403fff)
            {
                result = (short)(Memory.mainram[address - 0x400000] * 0x100 + Memory.mainram[address - 0x400000 + 1]);
            }
            else if (address >= 0x500000 && address + 1 <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                result = (short)TC0180VCU_word_r(offset);
            }
            else if (address >= 0x510000 && address + 1 <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                result = (short)taitob_spriteram[offset];
            }
            else if (address >= 0x511980 && address + 1 <= 0x5137ff)
            {
                result = (short)(mainram2[address - 0x511980] * 0x100 + mainram2[address - 0x511980 + 1]);
            }
            else if (address >= 0x513800 && address + 1 <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                result = (short)taitob_scroll[offset];
            }
            else if (address >= 0x518000 && address + 1 <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                result = (short)taitob_v_control_r(offset);
            }
            else if (address >= 0x540000 && address + 1 <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                result = (short)TC0180VCU_framebuffer_word_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_silentd(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x300000 && address + 3 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x400000 && address + 3 <= 0x403fff)
            {
                result = (int)(Memory.mainram[address - 0x400000] * 0x1000000 + Memory.mainram[address - 0x400000 + 1] * 0x10000 + Memory.mainram[address - 0x400000 + 2] * 0x100 + Memory.mainram[address - 0x400000 + 3]);
            }
            return result;
        }
        public static int MReadLong_silentd(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x200000 && address + 3 <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(TC0220IOC_halfword_r(offset) * 0x10000 + TC0220IOC_halfword_r(offset + 1));
            }
            else if (address >= 0x300000 && address + 1 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x400000 && address + 1 <= 0x403fff)
            {
                result = (int)(Memory.mainram[address - 0x400000] * 0x1000000 + Memory.mainram[address - 0x400000 + 1] * 0x10000 + Memory.mainram[address - 0x400000 + 2] * 0x100 + Memory.mainram[address - 0x400000 + 3]);
            }
            else if (address >= 0x500000 && address + 1 <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                result = (int)(TC0180VCU_word_r(offset) * 0x10000 + TC0180VCU_word_r(offset + 1));
            }
            else if (address >= 0x510000 && address + 1 <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                result = (int)(taitob_spriteram[offset] * 0x10000 + taitob_spriteram[offset + 1]);
            }
            else if (address >= 0x511980 && address + 1 <= 0x5137ff)
            {
                result = (int)(mainram2[address - 0x511980] * 0x1000000 + mainram2[address - 0x511980 + 1] * 0x10000 + mainram2[address - 0x511980 + 2] * 0x100 + mainram2[address - 0x511980 + 3]);
            }
            else if (address >= 0x513800 && address + 1 <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                result = (int)(taitob_scroll[offset] * 0x10000 + taitob_scroll[offset + 1]);
            }
            else if (address >= 0x518000 && address + 1 <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                result = (int)(taitob_v_control_r(offset) * 0x10000 + taitob_v_control_r(offset + 1));
            }
            else if (address >= 0x540000 && address + 1 <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                result = (int)(TC0180VCU_framebuffer_word_r(offset) * 0x10000 + TC0180VCU_framebuffer_word_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_silentd(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address <= 0x07ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)value;
                }
            }
            else if (address >= 0x100000 && address <= 0x100001)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_port16_msb_w1((byte)value);
                }
            }
            else if (address >= 0x100002 && address <= 0x100003)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_comm16_msb_w1((byte)value);
                }
            }
            else if (address >= 0x200000 && address <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                TC0220IOC_halfword_w1(offset, (byte)value);
            }
            else if (address >= 0x240000 && address <= 0x240001)
            {

            }
            else if (address >= 0x300000 && address <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_RRRRGGGGBBBBRGBx_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_RRRRGGGGBBBBRGBx_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x400000 && address <= 0x403fff)
            {
                int offset = address - 0x400000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x500000 && address <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {
                    TC0180VCU_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    TC0180VCU_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x510000 && address <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                if (address % 2 == 0)
                {
                    taitob_spriteram[offset] = (ushort)((value << 8) | (taitob_spriteram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    taitob_spriteram[offset] = (ushort)((taitob_spriteram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x511980 && address <= 0x5137ff)
            {
                int offset = address - 0x511980;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0x513800 && address <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                if (address % 2 == 0)
                {
                    taitob_scroll[offset] = (ushort)((value << 8) | (taitob_scroll[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    taitob_scroll[offset] = (ushort)((taitob_scroll[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x518000 && address <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                if (address % 2 == 0)
                {
                    taitob_v_control_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    taitob_v_control_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x540000 && address <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                if (address % 2 == 0)
                {
                    TC0180VCU_framebuffer_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    TC0180VCU_framebuffer_word_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_silentd(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 1 <= 0x07ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 8);
                    Memory.mainrom[address + 1] = (byte)value;
                }
            }
            else if (address >= 0x100000 && address + 1 <= 0x100001)
            {
                Taitosnd.taitosound_port16_msb_w((byte)value);
            }
            else if (address >= 0x100002 && address + 1 <= 0x100003)
            {
                Taitosnd.taitosound_comm16_msb_w((ushort)value);
            }
            else if (address >= 0x200000 && address + 1 <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                TC0220IOC_halfword_w(offset, (ushort)value);
            }
            else if (address >= 0x240000 && address + 1 <= 0x240001)
            {

            }
            else if (address >= 0x300000 && address + 1 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                Generic.paletteram16_RRRRGGGGBBBBRGBx_word_w(offset, (ushort)value);
            }
            else if (address >= 0x400000 && address + 1 <= 0x403fff)
            {
                int offset = address - 0x400000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x500000 && address + 1 <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                TC0180VCU_word_w(offset, (ushort)value);
            }
            else if (address >= 0x510000 && address + 1 <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                taitob_spriteram[offset] = (ushort)value;
            }
            else if (address >= 0x511980 && address + 1 <= 0x5137ff)
            {
                int offset = address - 0x511980;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0x513800 && address + 1 <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                taitob_scroll[offset] = (ushort)value;
            }
            else if (address >= 0x518000 && address + 1 <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                taitob_v_control_w(offset, (ushort)value);
            }
            else if (address >= 0x540000 && address + 1 <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                TC0180VCU_framebuffer_word_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_silentd(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 24);
                    Memory.mainrom[address + 1] = (byte)(value >> 16);
                    Memory.mainrom[address + 2] = (byte)(value >> 8);
                    Memory.mainrom[address + 3] = (byte)value;
                }
            }
            else if (address >= 0x200000 && address + 3 <= 0x20000f)
            {
                int offset = (address - 0x200000) / 2;
                TC0220IOC_halfword_w(offset, (ushort)(value >> 16));
                TC0220IOC_halfword_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x300000 && address + 3 <= 0x301fff)
            {
                int offset = (address - 0x300000) / 2;
                Generic.paletteram16_RRRRGGGGBBBBRGBx_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_RRRRGGGGBBBBRGBx_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x403fff)
            {
                int offset = address - 0x400000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x500000 && address + 3 <= 0x50ffff)
            {
                int offset = (address - 0x500000) / 2;
                TC0180VCU_word_w(offset, (ushort)(value >> 16));
                TC0180VCU_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x510000 && address + 3 <= 0x51197f)
            {
                int offset = (address - 0x510000) / 2;
                taitob_spriteram[offset] = (ushort)(value >> 16);
                taitob_spriteram[offset + 1] = (ushort)value;
            }
            else if (address >= 0x511980 && address + 3 <= 0x5137ff)
            {
                int offset = address - 0x511980;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0x513800 && address + 3 <= 0x513fff)
            {
                int offset = (address - 0x513800) / 2;
                taitob_scroll[offset] = (ushort)(value >> 16);
                taitob_scroll[offset + 1] = (ushort)value;
            }
            else if (address >= 0x518000 && address + 3 <= 0x51801f)
            {
                int offset = (address - 0x518000) / 2;
                taitob_v_control_w(offset, (ushort)(value >> 16));
                taitob_v_control_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x540000 && address + 3 <= 0x57ffff)
            {
                int offset = (address - 0x540000) / 2;
                TC0180VCU_framebuffer_word_w(offset, (ushort)(value >> 16));
                TC0180VCU_framebuffer_word_w(offset + 1, (ushort)value);
            }
        }
    }
}
