using System;

namespace MAME.Core
{
    public class YM2413
    {
        public struct OPLL_SLOT
        {
            public uint ar;
            public uint dr;
            public uint rr;
            public byte KSR;
            public byte ksl;
            public byte ksr;
            public byte mul;

            public uint phase;
            public uint freq;
            public byte fb_shift;
            public int[] op1_out;

            public byte eg_type;
            public byte state;
            public uint TL;
            public int TLL;
            public int volume;
            public uint sl;

            public byte eg_sh_dp;
            public byte eg_sel_dp;
            public byte eg_sh_ar;
            public byte eg_sel_ar;
            public byte eg_sh_dr;
            public byte eg_sel_dr;
            public byte eg_sh_rr;
            public byte eg_sel_rr;
            public byte eg_sh_rs;
            public byte eg_sel_rs;

            public uint key;

            public uint AMmask;
            public byte vib;

            public uint wavetable;
        }
        public struct OPLL_CH
        {
            public OPLL_SLOT[] SLOT;
            public uint block_fnum;
            public uint fc;
            public uint ksl_base;
            public byte kcode;
            public byte sus;
        }
        public struct YM2413Struct
        {
            public OPLL_CH[] P_CH;
            public byte[] instvol_r;

            public uint eg_cnt;
            public uint eg_timer;
            public uint eg_timer_add;
            public uint eg_timer_overflow;

            public byte rhythm;

            public uint lfo_am_cnt;
            public uint lfo_am_inc;
            public uint lfo_pm_cnt;
            public uint lfo_pm_inc;

            public uint noise_rng;
            public uint noise_p;
            public uint noise_f;

            public byte[][] inst_tab;

            public OPLL_UPDATEHANDLER UpdateHandler;
            //void * UpdateParam;

            public uint[] fn_tab;

            public byte address;
            public byte status;

            public int index;
            public int clock;
            public int rate;
            public double freqbase;
        }
        private static uint[] ksl_tab = new uint[8 * 16]
        {
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,

            0,0,0,0,
            0,0,0,0,
            0,4,6,8,
            10,12,14,16,

            0,0,0,0,
            0,6,10,14,
            16,20,22,24,
            26,28,30,32,

            0,0,0,10,
            16,22,26,30,
            32,36,38,40,
            42,44,46,48,

            0,0,16,26,
            32,38,42,46,
            48,52,54,56,
            58,60,62,64,

            0,16,32,42,
            48,54,58,62,
            64,68,70,72,
            74,76,78,80,

            0,32,48,58,
            64,70,74,78,
            80,84,86,88,
            90,92,94,96,

            0,48,64,74,
            80,86,90,94,
            96,100,102,104,
            106,108,110,112
        };
        private static uint[] sl_tab = new uint[16]
        {
            0*8, 1*8, 2*8,3 *8,4 *8,5 *8,6 *8, 7*8,
            8*8, 9*8,10*8,11*8,12*8,13*8,14*8,15*8
        };
        private static byte[] eg_inc = new byte[15 * 8]
        {
            0,1, 0,1, 0,1, 0,1,
            0,1, 0,1, 1,1, 0,1,
            0,1, 1,1, 0,1, 1,1,
            0,1, 1,1, 1,1, 1,1,

            1,1, 1,1, 1,1, 1,1,
            1,1, 1,2, 1,1, 1,2,
            1,2, 1,2, 1,2, 1,2,
            1,2, 2,2, 1,2, 2,2,

            2,2, 2,2, 2,2, 2,2,
            2,2, 2,4, 2,2, 2,4,
            2,4, 2,4, 2,4, 2,4,
            2,4, 4,4, 2,4, 4,4,

            4,4, 4,4, 4,4, 4,4,
            8,8, 8,8, 8,8, 8,8,
            0,0, 0,0, 0,0, 0,0,
        };
        private static byte[] eg_rate_select = new byte[96]
        {
            14*8,14*8,14*8,14*8,14*8,14*8,14*8,14*8,
            14*8,14*8,14*8,14*8,14*8,14*8,14*8,14*8,

            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,
            0*8,1*8,2*8,3*8,

            4*8,5*8,6*8,7*8,

            8*8,9*8,10*8,11*8,

            12*8,12*8,12*8,12*8,

            12*8,12*8,12*8,12*8,12*8,12*8,12*8,12*8,
            12*8,12*8,12*8,12*8,12*8,12*8,12*8,12*8,
        };
        private static byte[] eg_rate_shift = new byte[96]
        {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,

            13,13,13,13,
            12,12,12,12,
            11,11,11,11,
            10,10,10,10,
             9, 9, 9, 9,
             8, 8, 8, 8,
             7, 7, 7, 7,
             6, 6, 6, 6,
             5, 5, 5, 5,
             4, 4, 4, 4,
             3, 3, 3, 3,
             2, 2, 2, 2,
             1, 1, 1, 1,

             0, 0, 0, 0,

             0, 0, 0, 0,

             0, 0, 0, 0,

             0, 0, 0, 0, 0, 0, 0, 0,
             0, 0, 0, 0, 0, 0, 0, 0,
        };
        private static byte[] mul_tab = new byte[16]
        {
            1,   1*2, 2*2, 3*2, 4*2, 5*2, 6*2, 7*2,
            8*2, 9*2,10*2,10*2,12*2,12*2,15*2,15*2
        };
        public delegate void OPLL_UPDATEHANDLER(int min_interval_us);
        private static int[] tl_tab;
        private static uint[] sin_tab;
        private static byte[] lfo_am_table = new byte[210]
        {
            0,0,0,0,0,0,0,
            1,1,1,1,
            2,2,2,2,
            3,3,3,3,
            4,4,4,4,
            5,5,5,5,
            6,6,6,6,
            7,7,7,7,
            8,8,8,8,
            9,9,9,9,
            10,10,10,10,
            11,11,11,11,
            12,12,12,12,
            13,13,13,13,
            14,14,14,14,
            15,15,15,15,
            16,16,16,16,
            17,17,17,17,
            18,18,18,18,
            19,19,19,19,
            20,20,20,20,
            21,21,21,21,
            22,22,22,22,
            23,23,23,23,
            24,24,24,24,
            25,25,25,25,
            26,26,26,
            25,25,25,25,
            24,24,24,24,
            23,23,23,23,
            22,22,22,22,
            21,21,21,21,
            20,20,20,20,
            19,19,19,19,
            18,18,18,18,
            17,17,17,17,
            16,16,16,16,
            15,15,15,15,
            14,14,14,14,
            13,13,13,13,
            12,12,12,12,
            11,11,11,11,
            10,10,10,10,
            9,9,9,9,
            8,8,8,8,
            7,7,7,7,
            6,6,6,6,
            5,5,5,5,
            4,4,4,4,
            3,3,3,3,
            2,2,2,2,
            1,1,1,1
        };
        private static sbyte[] lfo_pm_table = new sbyte[64]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            1, 0, 0, 0,-1, 0, 0, 0,
            2, 1, 0,-1,-2,-1, 0, 1,
            3, 1, 0,-1,-3,-1, 0, 1,
            4, 2, 0,-2,-4,-2, 0, 2,
            5, 2, 0,-2,-5,-2, 0, 2,
            6, 3, 0,-3,-6,-3, 0, 3,
            7, 3, 0,-3,-7,-3, 0, 3,
        };
        private static byte[][] table = new byte[19][]
        {
            new byte[]{0x49, 0x4c, 0x4c, 0x12, 0x00, 0x00, 0x00, 0x00 },

            new byte[]{0x61, 0x61, 0x1e, 0x17, 0xf0, 0x78, 0x00, 0x17 },
            new byte[]{0x13, 0x41, 0x1e, 0x0d, 0xd7, 0xf7, 0x13, 0x13 },
            new byte[]{0x13, 0x01, 0x99, 0x04, 0xf2, 0xf4, 0x11, 0x23 },
            new byte[]{0x21, 0x61, 0x1b, 0x07, 0xaf, 0x64, 0x40, 0x27 },

            new byte[]{0x22, 0x21, 0x1e, 0x06, 0xf0, 0x75, 0x08, 0x18 },

            new byte[]{0x31, 0x22, 0x16, 0x05, 0x90, 0x71, 0x00, 0x13 },

            new byte[]{0x21, 0x61, 0x1d, 0x07, 0x82, 0x80, 0x10, 0x17 },
            new byte[]{0x23, 0x21, 0x2d, 0x16, 0xc0, 0x70, 0x07, 0x07 },
            new byte[]{0x61, 0x61, 0x1b, 0x06, 0x64, 0x65, 0x10, 0x17 },

            new byte[]{0x61, 0x61, 0x0c, 0x18, 0x85, 0xf0, 0x70, 0x07 },

            new byte[]{0x23, 0x01, 0x07, 0x11, 0xf0, 0xa4, 0x00, 0x22 },
            new byte[]{0x97, 0xc1, 0x24, 0x07, 0xff, 0xf8, 0x22, 0x12 },

            new byte[]{0x61, 0x10, 0x0c, 0x05, 0xf2, 0xf4, 0x40, 0x44 },

            new byte[]{0x01, 0x01, 0x55, 0x03, 0xf3, 0x92, 0xf3, 0xf3 },
            new byte[]{0x61, 0x41, 0x89, 0x03, 0xf1, 0xf4, 0xf0, 0x13 },

            new byte[]{0x01, 0x01, 0x16, 0x00, 0xfd, 0xf8, 0x2f, 0x6d },
            new byte[]{0x01, 0x01, 0x00, 0x00, 0xd8, 0xd8, 0xf9, 0xf8 },
            new byte[]{0x05, 0x01, 0x00, 0x00, 0xf8, 0xba, 0x49, 0x55 }
        };
        public static YM2413Struct OPLL;
        private static int num_lock = 0;
        private static int[] output;
        private static int outchan;
        private static uint LFO_AM;
        private static int LFO_PM;
        private static int limit(int val, int max, int min)
        {
            if (val > max)
            {
                return max;
            }
            else if (val < min)
            {
                return min;
            }
            else
            {
                return val;
            }
        }
        private static void advance_lfo()
        {
            OPLL.lfo_am_cnt += OPLL.lfo_am_inc;
            if (OPLL.lfo_am_cnt >= ((uint)210 << 24))
                OPLL.lfo_am_cnt -= ((uint)210 << 24);
            LFO_AM = (uint)(lfo_am_table[OPLL.lfo_am_cnt >> 24] >> 1);
            OPLL.lfo_pm_cnt += OPLL.lfo_pm_inc;
            LFO_PM = (int)((OPLL.lfo_pm_cnt >> 24) & 7);
        }
        private static void advance()
        {
            uint i;
            OPLL.eg_timer += OPLL.eg_timer_add;
            while (OPLL.eg_timer >= OPLL.eg_timer_overflow)
            {
                OPLL.eg_timer -= OPLL.eg_timer_overflow;
                OPLL.eg_cnt++;
                for (i = 0; i < 9 * 2; i++)
                {
                    switch (OPLL.P_CH[i / 2].SLOT[i & 1].state)
                    {
                        case 5:
                            if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_dp) - 1)) == 0)
                            {
                                OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_dp + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_dp) & 7)];
                                if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= 0xff)
                                {
                                    OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0xff;
                                    OPLL.P_CH[i / 2].SLOT[i & 1].state = 4;
                                    OPLL.P_CH[i / 2].SLOT[i & 1].phase = 0;
                                }
                            }
                            break;
                        case 4:
                            if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_ar) - 1)) == 0)
                            {
                                OPLL.P_CH[i / 2].SLOT[i & 1].volume += (~OPLL.P_CH[i / 2].SLOT[i & 1].volume * (eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_ar + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_ar) & 7)])) >> 2;
                                if (OPLL.P_CH[i / 2].SLOT[i & 1].volume <= 0)
                                {
                                    OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0;
                                    OPLL.P_CH[i / 2].SLOT[i & 1].state = 3;
                                }
                            }
                            break;
                        case 3:
                            if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_dr) - 1)) == 0)
                            {
                                OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_dr + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_dr) & 7)];
                                if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= OPLL.P_CH[i / 2].SLOT[i & 1].sl)
                                    OPLL.P_CH[i / 2].SLOT[i & 1].state = 2;
                            }
                            break;
                        case 2:
                            if (OPLL.P_CH[i / 2].SLOT[i & 1].eg_type != 0)
                            {

                            }
                            else
                            {
                                if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rr) - 1)) == 0)
                                {
                                    OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_rr + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rr) & 7)];
                                    if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= 0xff)
                                        OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0xff;
                                }
                            }
                            break;
                        case 1:
                            if ((i & 1) != 0 || (((OPLL.rhythm & 0x20) != 0) && (i >= 12)))
                            {
                                if (OPLL.P_CH[i / 2].SLOT[i & 1].eg_type != 0)
                                {
                                    if (OPLL.P_CH[i / 2].sus != 0)
                                    {
                                        if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rs) - 1)) == 0)
                                        {
                                            OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_rs + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rs) & 7)];
                                            if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= 0xff)
                                            {
                                                OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0xff;
                                                OPLL.P_CH[i / 2].SLOT[i & 1].state = 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rr) - 1)) == 0)
                                        {
                                            OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_rr + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rr) & 7)];
                                            if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= 0xff)
                                            {
                                                OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0xff;
                                                OPLL.P_CH[i / 2].SLOT[i & 1].state = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if ((OPLL.eg_cnt & ((1 << OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rs) - 1)) == 0)
                                    {
                                        OPLL.P_CH[i / 2].SLOT[i & 1].volume += eg_inc[OPLL.P_CH[i / 2].SLOT[i & 1].eg_sel_rs + ((OPLL.eg_cnt >> OPLL.P_CH[i / 2].SLOT[i & 1].eg_sh_rs) & 7)];
                                        if (OPLL.P_CH[i / 2].SLOT[i & 1].volume >= 0xff)
                                        {
                                            OPLL.P_CH[i / 2].SLOT[i & 1].volume = 0xff;
                                            OPLL.P_CH[i / 2].SLOT[i & 1].state = 0;
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            for (i = 0; i < 9 * 2; i++)
            {
                if (OPLL.P_CH[i / 2].SLOT[i & 1].vib != 0)
                {
                    byte block;
                    uint fnum_lfo = 8 * ((OPLL.P_CH[i / 2].block_fnum & 0x01c0) >> 6);
                    uint block_fnum = OPLL.P_CH[i / 2].block_fnum * 2;
                    int lfo_fn_table_index_offset = lfo_pm_table[LFO_PM + fnum_lfo];
                    if (lfo_fn_table_index_offset != 0)
                    {
                        block_fnum += (uint)lfo_fn_table_index_offset;
                        block = (byte)((block_fnum & 0x1c00) >> 10);
                        OPLL.P_CH[i / 2].SLOT[i & 1].phase += (OPLL.fn_tab[block_fnum & 0x03ff] >> (7 - block)) * OPLL.P_CH[i / 2].SLOT[i & 1].mul;
                    }
                    else
                    {
                        OPLL.P_CH[i / 2].SLOT[i & 1].phase += OPLL.P_CH[i / 2].SLOT[i & 1].freq;
                    }
                }
                else
                {
                    OPLL.P_CH[i / 2].SLOT[i & 1].phase += OPLL.P_CH[i / 2].SLOT[i & 1].freq;
                }
            }
            OPLL.noise_p += OPLL.noise_f;
            i = OPLL.noise_p >> 16;
            OPLL.noise_p &= 0xffff;
            while (i != 0)
            {
                if ((OPLL.noise_rng & 1) != 0)
                    OPLL.noise_rng ^= 0x800302;
                OPLL.noise_rng >>= 1;
                i--;
            }
        }
        private static int op_calc(uint phase, uint env, int pm, uint wave_tab)
        {
            uint p;
            p = (env << 5) + sin_tab[wave_tab + ((((int)((phase & ~0xffff) + (pm << 17))) >> 16) & 0x3ff)];
            if (p >= 0x1600)
                return 0;
            return tl_tab[p];
        }
        private static int op_calc1(uint phase, uint env, int pm, uint wave_tab)
        {
            uint p;
            int i;
            i = (int)((phase & ~0xffff) + pm);
            p = (env << 5) + sin_tab[wave_tab + ((i >> 16) & 0x3ff)];
            if (p >= 0x1600)
                return 0;
            return tl_tab[p];
        }
        private static uint volume_calc(int chan, int slot)
        {
            uint i1;
            i1 = (uint)(OPLL.P_CH[chan].SLOT[slot].TLL + (uint)OPLL.P_CH[chan].SLOT[slot].volume + (LFO_AM & OPLL.P_CH[chan].SLOT[slot].AMmask));
            return i1;
        }
        private static void chan_calc(int chan)
        {
            uint env;
            int out1;
            int phase_modulation;
            env = volume_calc(chan, 0);
            out1 = OPLL.P_CH[chan].SLOT[0].op1_out[0] + OPLL.P_CH[chan].SLOT[0].op1_out[1];
            OPLL.P_CH[chan].SLOT[0].op1_out[0] = OPLL.P_CH[chan].SLOT[0].op1_out[1];
            phase_modulation = OPLL.P_CH[chan].SLOT[0].op1_out[0];
            OPLL.P_CH[chan].SLOT[0].op1_out[1] = 0;
            if (env < 0xb0)
            {
                if (OPLL.P_CH[chan].SLOT[0].fb_shift == 0)
                    out1 = 0;
                OPLL.P_CH[chan].SLOT[0].op1_out[1] = op_calc1(OPLL.P_CH[chan].SLOT[0].phase, env, (out1 << OPLL.P_CH[chan].SLOT[0].fb_shift), OPLL.P_CH[chan].SLOT[0].wavetable);
            }
            outchan = 0;
            env = volume_calc(chan, 1);
            if (env < 0xb0)
            {
                int outp = op_calc(OPLL.P_CH[chan].SLOT[1].phase, env, phase_modulation, OPLL.P_CH[chan].SLOT[1].wavetable);
                output[0] += outp;
                outchan = outp;
            }
        }
        private static void rhythm_calc(uint noise)
        {
            int out1;
            uint env;
            int phase_modulation;
            env = volume_calc(6, 0);
            out1 = OPLL.P_CH[6].SLOT[0].op1_out[0] + OPLL.P_CH[6].SLOT[0].op1_out[1];
            OPLL.P_CH[6].SLOT[0].op1_out[0] = OPLL.P_CH[6].SLOT[0].op1_out[1];
            phase_modulation = OPLL.P_CH[6].SLOT[0].op1_out[0];
            OPLL.P_CH[6].SLOT[0].op1_out[1] = 0;
            if (env < 0xb0)
            {
                if (OPLL.P_CH[6].SLOT[0].fb_shift == 0)
                    out1 = 0;
                OPLL.P_CH[6].SLOT[0].op1_out[1] = op_calc1(OPLL.P_CH[6].SLOT[0].phase, env, (out1 << OPLL.P_CH[6].SLOT[0].fb_shift), OPLL.P_CH[6].SLOT[0].wavetable);
            }
            env = volume_calc(6, 1);
            if (env < 0xb0)
                output[1] += op_calc(OPLL.P_CH[6].SLOT[1].phase, env, phase_modulation, OPLL.P_CH[6].SLOT[1].wavetable) * 2;
            env = volume_calc(7, 0);
            if (env < 0xb0)
            {
                byte bit7 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 7) & 1);
                byte bit3 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 3) & 1);
                byte bit2 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 2) & 1);
                byte res1 = (byte)((bit2 ^ bit7) | bit3);
                uint phase = (uint)(res1 != 0 ? (0x200 | (0xd0 >> 2)) : 0xd0);
                byte bit5e = (byte)(((OPLL.P_CH[8].SLOT[1].phase >> 16) >> 5) & 1);
                byte bit3e = (byte)(((OPLL.P_CH[8].SLOT[1].phase >> 16) >> 3) & 1);
                byte res2 = (byte)(bit3e | bit5e);
                if (res2 != 0)
                    phase = (0x200 | (0xd0 >> 2));
                if ((phase & 0x200) != 0)
                {
                    if (noise != 0)
                        phase = 0x200 | 0xd0;
                }
                else
                {
                    if (noise != 0)
                        phase = 0xd0 >> 2;
                }
                output[1] += op_calc(phase << 16, env, 0, OPLL.P_CH[7].SLOT[0].wavetable) * 2;
            }
            env = volume_calc(7, 1);
            if (env < 0xb0)
            {
                byte bit8 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 8) & 1);
                uint phase = (uint)(bit8 != 0 ? 0x200 : 0x100);
                if (noise != 0)
                    phase ^= 0x100;
                output[1] += op_calc(phase << 16, env, 0, OPLL.P_CH[7].SLOT[1].wavetable) * 2;
            }
            env = volume_calc(8, 0);
            if (env < 0xb0)
                output[1] += op_calc(OPLL.P_CH[8].SLOT[0].phase, env, 0, OPLL.P_CH[8].SLOT[0].wavetable) * 2;
            env = volume_calc(8, 1);
            if (env < 0xb0)
            {
                byte bit7 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 7) & 1);
                byte bit3 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 3) & 1);
                byte bit2 = (byte)(((OPLL.P_CH[7].SLOT[0].phase >> 16) >> 2) & 1);
                byte res1 = (byte)((bit2 ^ bit7) | bit3);
                uint phase = (uint)(res1 != 0 ? 0x300 : 0x100);
                byte bit5e = (byte)(((OPLL.P_CH[8].SLOT[1].phase >> 16) >> 5) & 1);
                byte bit3e = (byte)(((OPLL.P_CH[8].SLOT[1].phase >> 16) >> 3) & 1);
                byte res2 = (byte)(bit3e | bit5e);
                if (res2 != 0)
                    phase = 0x300;
                output[1] += op_calc(phase << 16, env, 0, OPLL.P_CH[8].SLOT[1].wavetable) * 2;
            }
        }
        private static int init_tables()
        {
            int i, x;
            int n;
            double o, m;
            for (x = 0; x < 0x100; x++)
            {
                m = (1 << 16) / Math.Pow(2, (x + 1) * ((128.0 / 0x400) / 4.0) / 8.0);
                m = Math.Floor(m);
                n = (int)m;
                n >>= 4;
                if ((n & 1) != 0)
                    n = (n >> 1) + 1;
                else
                    n = n >> 1;
                tl_tab[x * 2 + 0] = n;
                tl_tab[x * 2 + 1] = -tl_tab[x * 2 + 0];

                for (i = 1; i < 11; i++)
                {
                    tl_tab[x * 2 + 0 + i * 2 * 0x100] = tl_tab[x * 2 + 0] >> i;
                    tl_tab[x * 2 + 1 + i * 2 * 0x100] = -tl_tab[x * 2 + 0 + i * 2 * 0x100];
                }
            }
            for (i = 0; i < 0x400; i++)
            {
                m = Math.Sin(((i * 2) + 1) * Math.PI / 0x400);
                if (m > 0.0)
                    o = 8 * Math.Log(1.0 / m) / Math.Log(2);
                else
                    o = 8 * Math.Log(-1.0 / m) / Math.Log(2);
                o = o / ((128.0 / 0x400) / 4);
                n = (int)(2.0 * o);
                if ((n & 1) != 0)
                    n = (n >> 1) + 1;
                else
                    n = n >> 1;
                sin_tab[i] = (uint)(n * 2 + (m >= 0.0 ? 0 : 1));
                if ((i & 0x200) != 0)
                    sin_tab[1 * 0x400 + i] = 0x1600;
                else
                    sin_tab[1 * 0x400 + i] = sin_tab[i];
            }
            return 1;
        }
        private static void OPLL_initalize()
        {
            int i;
            //OPLL_init_save();
            OPLL.freqbase = (OPLL.rate != 0) ? ((double)OPLL.clock / 72.0) / OPLL.rate : 0;
            for (i = 0; i < 1024; i++)
            {
                OPLL.fn_tab[i] = (uint)((double)i * 64 * OPLL.freqbase * (1 << (16 - 10)));
            }
            OPLL.lfo_am_inc = (uint)(0x40000 * OPLL.freqbase);
            OPLL.lfo_pm_inc = (uint)(0x4000 * OPLL.freqbase);
            OPLL.noise_f = (uint)(0x10000 * OPLL.freqbase);
            OPLL.eg_timer_add = (uint)(0x10000 * OPLL.freqbase);
            OPLL.eg_timer_overflow = (uint)0x10000;
        }
        private static void KEY_ON(int chan, int slot, uint key_set)
        {
            if (OPLL.P_CH[chan].SLOT[slot].key == 0)
            {
                OPLL.P_CH[chan].SLOT[slot].state = 5;
            }
            OPLL.P_CH[chan].SLOT[slot].key |= key_set;
        }
        private static void KEY_OFF(int chan, int slot, uint key_clr)
        {
            if (OPLL.P_CH[chan].SLOT[slot].key != 0)
            {
                OPLL.P_CH[chan].SLOT[slot].key &= key_clr;
                if (OPLL.P_CH[chan].SLOT[slot].key == 0)
                {
                    if (OPLL.P_CH[chan].SLOT[slot].state > 1)
                        OPLL.P_CH[chan].SLOT[slot].state = 1;
                }
            }
        }
        private static void CALC_FCSLOT(int chan, int slot)
        {
            int ksr;
            uint SLOT_rs;
            uint SLOT_dp;
            OPLL.P_CH[chan].SLOT[slot].freq = OPLL.P_CH[chan].fc * OPLL.P_CH[chan].SLOT[slot].mul;
            ksr = OPLL.P_CH[chan].kcode >> OPLL.P_CH[chan].SLOT[slot].KSR;
            if (OPLL.P_CH[chan].SLOT[slot].ksr != ksr)
            {
                OPLL.P_CH[chan].SLOT[slot].ksr = (byte)ksr;
                if ((OPLL.P_CH[chan].SLOT[slot].ar + OPLL.P_CH[chan].SLOT[slot].ksr) < 16 + 62)
                {
                    OPLL.P_CH[chan].SLOT[slot].eg_sh_ar = eg_rate_shift[OPLL.P_CH[chan].SLOT[slot].ar + OPLL.P_CH[chan].SLOT[slot].ksr];
                    OPLL.P_CH[chan].SLOT[slot].eg_sel_ar = eg_rate_select[OPLL.P_CH[chan].SLOT[slot].ar + OPLL.P_CH[chan].SLOT[slot].ksr];
                }
                else
                {
                    OPLL.P_CH[chan].SLOT[slot].eg_sh_ar = 0;
                    OPLL.P_CH[chan].SLOT[slot].eg_sel_ar = 13 * 8;
                }
                OPLL.P_CH[chan].SLOT[slot].eg_sh_dr = eg_rate_shift[OPLL.P_CH[chan].SLOT[slot].dr + OPLL.P_CH[chan].SLOT[slot].ksr];
                OPLL.P_CH[chan].SLOT[slot].eg_sel_dr = eg_rate_select[OPLL.P_CH[chan].SLOT[slot].dr + OPLL.P_CH[chan].SLOT[slot].ksr];
                OPLL.P_CH[chan].SLOT[slot].eg_sh_rr = eg_rate_shift[OPLL.P_CH[chan].SLOT[slot].rr + OPLL.P_CH[chan].SLOT[slot].ksr];
                OPLL.P_CH[chan].SLOT[slot].eg_sel_rr = eg_rate_select[OPLL.P_CH[chan].SLOT[slot].rr + OPLL.P_CH[chan].SLOT[slot].ksr];
            }
            if (OPLL.P_CH[chan].sus != 0)
                SLOT_rs = 16 + (5 << 2);
            else
                SLOT_rs = 16 + (7 << 2);
            OPLL.P_CH[chan].SLOT[slot].eg_sh_rs = eg_rate_shift[SLOT_rs + OPLL.P_CH[chan].SLOT[slot].ksr];
            OPLL.P_CH[chan].SLOT[slot].eg_sel_rs = eg_rate_select[SLOT_rs + OPLL.P_CH[chan].SLOT[slot].ksr];
            SLOT_dp = 16 + (13 << 2);
            OPLL.P_CH[chan].SLOT[slot].eg_sh_dp = eg_rate_shift[SLOT_dp + OPLL.P_CH[chan].SLOT[slot].ksr];
            OPLL.P_CH[chan].SLOT[slot].eg_sel_dp = eg_rate_select[SLOT_dp + OPLL.P_CH[chan].SLOT[slot].ksr];
        }
        private static void set_mul(int slot, int v)
        {
            OPLL.P_CH[slot / 2].SLOT[slot & 1].mul = mul_tab[v & 0x0f];
            OPLL.P_CH[slot / 2].SLOT[slot & 1].KSR = (byte)((v & 0x10) != 0 ? 0 : 2);
            OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_type = (byte)(v & 0x20);
            OPLL.P_CH[slot / 2].SLOT[slot & 1].vib = (byte)(v & 0x40);
            OPLL.P_CH[slot / 2].SLOT[slot & 1].AMmask = (uint)((v & 0x80) != 0 ? ~0 : 0);
            CALC_FCSLOT(slot / 2, slot & 1);
        }
        private static void set_ksl_tl(int chan, int v)
        {
            int ksl;
            ksl = v >> 6;
            OPLL.P_CH[chan].SLOT[0].ksl = (byte)(ksl != 0 ? 3 - ksl : 31);
            OPLL.P_CH[chan].SLOT[0].TL = (uint)((v & 0x3f) << 1);
            OPLL.P_CH[chan].SLOT[0].TLL = (int)(OPLL.P_CH[chan].SLOT[0].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[0].ksl));
        }
        private static void set_ksl_wave_fb(int chan, int v)
        {
            int ksl;
            OPLL.P_CH[chan].SLOT[0].wavetable = (uint)(((v & 0x08) >> 3) * 0x400);
            OPLL.P_CH[chan].SLOT[0].fb_shift = (byte)((v & 7) != 0 ? (v & 7) + 8 : 0);
            ksl = v >> 6;
            OPLL.P_CH[chan].SLOT[1].ksl = (byte)(ksl != 0 ? 3 - ksl : 31);
            OPLL.P_CH[chan].SLOT[1].TLL = (int)(OPLL.P_CH[chan].SLOT[1].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[1].ksl));
            OPLL.P_CH[chan].SLOT[1].wavetable = (uint)(((v & 0x10) >> 4) * 0x400);
        }
        private static void set_ar_dr(int slot, int v)
        {
            OPLL.P_CH[slot / 2].SLOT[slot & 1].ar = (uint)((v >> 4) != 0 ? 16 + ((v >> 4) << 2) : 0);
            if ((OPLL.P_CH[slot / 2].SLOT[slot & 1].ar + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr) < 16 + 62)
            {
                OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sh_ar = eg_rate_shift[OPLL.P_CH[slot / 2].SLOT[slot & 1].ar + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
                OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sel_ar = eg_rate_select[OPLL.P_CH[slot / 2].SLOT[slot & 1].ar + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
            }
            else
            {
                OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sh_ar = 0;
                OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sel_ar = 13 * 8;
            }
            OPLL.P_CH[slot / 2].SLOT[slot & 1].dr = (uint)((v & 0x0f) != 0 ? 16 + ((v & 0x0f) << 2) : 0);
            OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sh_dr = eg_rate_shift[OPLL.P_CH[slot / 2].SLOT[slot & 1].dr + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
            OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sel_dr = eg_rate_select[OPLL.P_CH[slot / 2].SLOT[slot & 1].dr + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
        }
        private static void set_sl_rr(int slot, int v)
        {
            OPLL.P_CH[slot / 2].SLOT[slot & 1].sl = sl_tab[v >> 4];
            OPLL.P_CH[slot / 2].SLOT[slot & 1].rr = (uint)((v & 0x0f) != 0 ? 16 + ((v & 0x0f) << 2) : 0);
            OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sh_rr = eg_rate_shift[OPLL.P_CH[slot / 2].SLOT[slot & 1].rr + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
            OPLL.P_CH[slot / 2].SLOT[slot & 1].eg_sel_rr = eg_rate_select[OPLL.P_CH[slot / 2].SLOT[slot & 1].rr + OPLL.P_CH[slot / 2].SLOT[slot & 1].ksr];
        }
        private static void load_instrument(int chan, int slot, int inst)
        {
            set_mul(slot, (int)OPLL.inst_tab[inst][0]);
            set_mul(slot + 1, (int)OPLL.inst_tab[inst][1]);
            set_ksl_tl(chan, (int)OPLL.inst_tab[inst][2]);
            set_ksl_wave_fb(chan, (int)OPLL.inst_tab[inst][3]);
            set_ar_dr(slot, (int)OPLL.inst_tab[inst][4]);
            set_ar_dr(slot + 1, (int)OPLL.inst_tab[inst][5]);
            set_sl_rr(slot, (int)OPLL.inst_tab[inst][6]);
            set_sl_rr(slot + 1, (int)OPLL.inst_tab[inst][7]);
        }
        private static void update_instrument_zero(int r)
        {
            int chan;
            int chan_max;
            chan_max = 9;
            if ((OPLL.rhythm & 0x20) != 0)
                chan_max = 6;
            switch (r)
            {
                case 0:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_mul(chan * 2, OPLL.inst_tab[0][0]);
                        }
                    }
                    break;
                case 1:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_mul(chan * 2 + 1, OPLL.inst_tab[0][1]);
                        }
                    }
                    break;
                case 2:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_ksl_tl(chan, OPLL.inst_tab[0][2]);
                        }
                    }
                    break;
                case 3:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_ksl_wave_fb(chan, OPLL.inst_tab[0][3]);
                        }
                    }
                    break;
                case 4:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_ar_dr(chan * 2, OPLL.inst_tab[0][4]);
                        }
                    }
                    break;
                case 5:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_ar_dr(chan * 2 + 1, OPLL.inst_tab[0][5]);
                        }
                    }
                    break;
                case 6:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_sl_rr(chan * 2, OPLL.inst_tab[0][6]);
                        }
                    }
                    break;
                case 7:
                    for (chan = 0; chan < chan_max; chan++)
                    {
                        if ((OPLL.instvol_r[chan] & 0xf0) == 0)
                        {
                            set_sl_rr(chan * 2 + 1, OPLL.inst_tab[0][7]);
                        }
                    }
                    break;
            }
        }
        static void OPLLWriteReg(int r, int v)
        {
            int inst;
            int chan;
            int slot;
            r &= 0xff;
            v &= 0xff;
            switch (r & 0xf0)
            {
                case 0x00:
                    {
                        switch (r & 0x0f)
                        {
                            case 0x00:
                            case 0x01:
                            case 0x02:
                            case 0x03:
                            case 0x04:
                            case 0x05:
                            case 0x06:
                            case 0x07:
                                OPLL.inst_tab[0][r & 0x07] = (byte)v;
                                update_instrument_zero(r & 7);
                                break;
                            case 0x0e:
                                {
                                    if ((v & 0x20) != 0)
                                    {
                                        if ((OPLL.rhythm & 0x20) == 0)
                                        {
                                            chan = 6;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, 16);
                                            chan = 7;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, 17);
                                            OPLL.P_CH[chan].SLOT[0].TL = (uint)(((OPLL.instvol_r[chan] >> 4) << 2) << 1);
                                            OPLL.P_CH[chan].SLOT[0].TLL = (int)(OPLL.P_CH[chan].SLOT[0].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[0].ksl));
                                            chan = 8;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, 18);
                                            OPLL.P_CH[chan].SLOT[0].TL = (uint)(((OPLL.instvol_r[chan] >> 4) << 2) << 1);
                                            OPLL.P_CH[chan].SLOT[0].TLL = (int)(OPLL.P_CH[chan].SLOT[0].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[0].ksl));
                                        }
                                        if ((v & 0x10) != 0)
                                        {
                                            KEY_ON(6, 0, 2);
                                            KEY_ON(6, 1, 2);
                                        }
                                        else
                                        {
                                            KEY_OFF(6, 0, unchecked((uint)(~2)));
                                            KEY_OFF(6, 1, unchecked((uint)(~2)));
                                        }
                                        if ((v & 0x01) != 0)
                                            KEY_ON(7, 0, 2);
                                        else
                                            KEY_OFF(7, 0, unchecked((uint)(~2)));
                                        if ((v & 0x08) != 0)
                                            KEY_ON(7, 1, 2);
                                        else
                                            KEY_OFF(7, 1, unchecked((uint)(~2)));
                                        if ((v & 0x04) != 0)
                                            KEY_ON(8, 0, 2);
                                        else
                                            KEY_OFF(8, 0, unchecked((uint)(~2)));
                                        if ((v & 0x02) != 0)
                                            KEY_ON(8, 1, 2);
                                        else
                                            KEY_OFF(8, 1, unchecked((uint)(~2)));
                                    }
                                    else
                                    {
                                        if ((OPLL.rhythm & 0x20) == 1)
                                        {
                                            chan = 6;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, OPLL.instvol_r[chan] >> 4);
                                            chan = 7;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, OPLL.instvol_r[chan] >> 4);
                                            chan = 8;
                                            slot = chan * 2;
                                            load_instrument(chan, slot, OPLL.instvol_r[chan] >> 4);
                                        }
                                        KEY_OFF(6, 0, unchecked((uint)(~2)));
                                        KEY_OFF(6, 1, unchecked((uint)(~2)));
                                        KEY_OFF(7, 0, unchecked((uint)(~2)));
                                        KEY_OFF(7, 1, unchecked((uint)(~2)));
                                        KEY_OFF(8, 0, unchecked((uint)(~2)));
                                        KEY_OFF(8, 1, unchecked((uint)(~2)));
                                    }
                                    OPLL.rhythm = (byte)(v & 0x3f);
                                }
                                break;
                        }
                    }
                    break;
                case 0x10:
                case 0x20:
                    {
                        int block_fnum;
                        chan = r & 0x0f;
                        if (chan >= 9)
                            chan -= 9;
                        if ((r & 0x10) != 0)
                        {
                            block_fnum = (int)((OPLL.P_CH[chan].block_fnum & 0x0f00) | (uint)v);
                        }
                        else
                        {
                            block_fnum = (int)((uint)((v & 0x0f) << 8) | (OPLL.P_CH[chan].block_fnum & 0xff));
                            if ((v & 0x10) != 0)
                            {
                                KEY_ON(chan, 0, 1);
                                KEY_ON(chan, 1, 1);
                            }
                            else
                            {
                                KEY_OFF(chan, 0, unchecked((uint)(~1)));
                                KEY_OFF(chan, 1, unchecked((uint)(~1)));
                            }
                            OPLL.P_CH[chan].sus = (byte)(v & 0x20);
                        }
                        if (OPLL.P_CH[chan].block_fnum != block_fnum)
                        {
                            byte block;
                            OPLL.P_CH[chan].block_fnum = (uint)block_fnum;
                            OPLL.P_CH[chan].kcode = (byte)((block_fnum & 0x0f00) >> 8);
                            OPLL.P_CH[chan].ksl_base = ksl_tab[block_fnum >> 5];
                            block_fnum = block_fnum * 2;
                            block = (byte)((block_fnum & 0x1c00) >> 10);
                            OPLL.P_CH[chan].fc = OPLL.fn_tab[block_fnum & 0x03ff] >> (7 - block);
                            OPLL.P_CH[chan].SLOT[0].TLL = (int)(OPLL.P_CH[chan].SLOT[0].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[0].ksl));
                            OPLL.P_CH[chan].SLOT[1].TLL = (int)(OPLL.P_CH[chan].SLOT[1].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[1].ksl));
                            CALC_FCSLOT(chan, 0);
                            CALC_FCSLOT(chan, 1);
                        }
                    }
                    break;
                case 0x30:
                    {
                        byte old_instvol;
                        chan = r & 0x0f;
                        if (chan >= 9)
                            chan -= 9;
                        old_instvol = OPLL.instvol_r[chan];
                        OPLL.instvol_r[chan] = (byte)v;
                        OPLL.P_CH[chan].SLOT[1].TL = (uint)((v & 0x0f) << 3);
                        OPLL.P_CH[chan].SLOT[1].TLL = (int)(OPLL.P_CH[chan].SLOT[1].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[1].ksl));
                        if ((chan >= 6) && ((OPLL.rhythm & 0x20) != 0))
                        {
                            if (chan >= 7)
                            {
                                OPLL.P_CH[chan].SLOT[0].TL = (uint)((OPLL.instvol_r[chan] >> 4) << 3);
                                OPLL.P_CH[chan].SLOT[0].TLL = (int)(OPLL.P_CH[chan].SLOT[0].TL + (OPLL.P_CH[chan].ksl_base >> OPLL.P_CH[chan].SLOT[0].ksl));
                            }
                        }
                        else
                        {
                            if ((old_instvol & 0xf0) == (v & 0xf0))
                                return;
                            slot = chan * 2;
                            load_instrument(chan, slot, OPLL.instvol_r[chan] >> 4);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private static int OPLL_LockTable()
        {
            num_lock++;
            if (num_lock > 1)
                return 0;
            if (init_tables() == 0)
            {
                num_lock--;
                return -1;
            }
            return 0;
        }
        private static void OPLL_UnLockTable()
        {
            if (num_lock != 0)
                num_lock--;
        }
        private static void OPLLResetChip()
        {
            int c, s;
            int i;
            OPLL.eg_timer = 0;
            OPLL.eg_cnt = 0;
            OPLL.noise_rng = 1;
            for (i = 0; i < 19; i++)
            {
                for (c = 0; c < 8; c++)
                {
                    OPLL.inst_tab[i][c] = table[i][c];
                }
            }
            OPLLWriteReg(0x0f, 0);
            for (i = 0x3f; i >= 0x10; i--)
                OPLLWriteReg(i, 0x00);
            for (c = 0; c < 9; c++)
            {
                for (s = 0; s < 2; s++)
                {
                    OPLL.P_CH[c].SLOT[s].wavetable = 0;
                    OPLL.P_CH[c].SLOT[s].state = 0;
                    OPLL.P_CH[c].SLOT[s].volume = 0xff;
                }
            }
        }
        private static YM2413Struct OPLLCreate(int clock, int rate, int index)
        {
            OPLL_LockTable();
            OPLL = new YM2413Struct();
            OPLL.index = index;
            OPLL.clock = clock;
            OPLL.rate = rate;
            OPLL_initalize();
            OPLLResetChip();
            return OPLL;
        }
        private static void OPLLDestroy()
        {
            OPLL_UnLockTable();
        }
        /*private static void OPLLSetUpdateHandler(OPLL_UPDATEHANDLER UpdateHandler, void* param)
        {
            OPLL.UpdateHandler = UpdateHandler;
            OPLL.UpdateParam = param;
        }*/
        private static void OPLLWrite(int a, int v)
        {
            if ((a & 1) == 0)
            {
                OPLL.address = (byte)(v & 0xff);
            }
            else
            {
                if (OPLL.UpdateHandler != null)
                {
                    //OPLL.UpdateHandler(OPLL.UpdateParam, 0);
                }
                OPLLWriteReg(OPLL.address, v);
            }
        }
        private static byte OPLLRead(int a)
        {
            if ((a & 1) == 0)
            {
                return OPLL.status;
            }
            return 0xff;
        }
        public static void ym2413_start(int clock)
        {
            int rate = clock / 72;
            ym2413_init(clock, rate, 0);
        }
        public static void ym2413_init(int clock, int rate, int index)
        {
            OPLLCreate(clock, rate, index);
        }
        private static void ym2413_shutdown()
        {
            OPLLDestroy();
        }
        public static void ym2413_reset_chip()
        {
            OPLLResetChip();
        }
        public static void ym2413_write(int a, int v)
        {
            OPLLWrite(a, v);
        }
        private static byte ym2413_read(int a)
        {
            return (byte)(OPLLRead(a) & 0x03);
        }
        /*private static void ym2413_set_update_handler(OPLL_UPDATEHANDLER UpdateHandler, void* param)
        {
            OPLLSetUpdateHandler(UpdateHandler, param);
        }*/
        public unsafe static void ym2413_update_one(int offset, int length)
        {
            byte rhythm = (byte)(OPLL.rhythm & 0x20);
            int i;
            for (i = 0; i < length; i++)
            {
                int mo, ro;
                output[0] = 0;
                output[1] = 0;
                advance_lfo();
                chan_calc(0);
                chan_calc(1);
                chan_calc(2);
                chan_calc(3);
                chan_calc(4);
                chan_calc(5);
                if (rhythm == 0)
                {
                    chan_calc(6);
                    chan_calc(7);
                    chan_calc(8);
                }
                else
                {
                    rhythm_calc((OPLL.noise_rng >> 0) & 1);
                }
                mo = output[0];
                ro = output[1];
                mo = limit(mo, 32767, -32768);
                ro = limit(ro, 32767, -32768);
                Sound.ym2413stream.streamoutput_Ptrs[0][offset + i] = mo;
                Sound.ym2413stream.streamoutput_Ptrs[1][offset + i] = ro;
                advance();
            }
        }
    }
}
