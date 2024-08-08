using System;

namespace VirtualNes.Core.Emu2413
{
    public static class Const
    {
        internal static sbyte[][] Create_Default_Inst()
        {
            unchecked
            {
                sbyte[][] res = new sbyte[Emu2413API.OPLL_TONE_NUM][]
                {
                    new sbyte[]
                    {
                        (sbyte)0x00,(sbyte) 0x00, (sbyte)0x00, (sbyte)0x00,(sbyte) 0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61,(sbyte) 0x61, (sbyte)0x1e, (sbyte)0x17,(sbyte) 0xf0, (sbyte)0x7f, (sbyte)0x07, (sbyte)0x17, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x13,(sbyte) 0x41, (sbyte)0x0f, (sbyte)0x0d,(sbyte) 0xce, (sbyte)0xd2, (sbyte)0x43, (sbyte)0x13, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x03,(sbyte) 0x01, (sbyte)0x99, (sbyte)0x04,(sbyte) 0xff, (sbyte)0xc3, (sbyte)0x03, (sbyte)0x73, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x21,(sbyte) 0x61, (sbyte)0x1b, (sbyte)0x07,(sbyte) 0xaf, (sbyte)0x63, (sbyte)0x40, (sbyte)0x28, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x22,(sbyte) 0x21, (sbyte)0x1e, (sbyte)0x06,(sbyte) 0xf0, (sbyte)0x76, (sbyte)0x08, (sbyte)0x28, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x31,(sbyte) 0x22, (sbyte)0x16, (sbyte)0x05,(sbyte) 0x90, (sbyte)0x71, (sbyte)0x00, (sbyte)0x18, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x21,(sbyte) 0x61, (sbyte)0x1d, (sbyte)0x07,(sbyte) 0x82, (sbyte)0x81, (sbyte)0x10, (sbyte)0x17, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x23,(sbyte) 0x21, (sbyte)0x2d, (sbyte)0x16,(sbyte) 0xc0, (sbyte)0x70, (sbyte)0x07, (sbyte)0x07, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61,(sbyte) 0x21, (sbyte)0x1b, (sbyte)0x06,(sbyte) 0x64, (sbyte)0x65, (sbyte)0x18, (sbyte)0x18, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61,(sbyte) 0x61, (sbyte)0x0c, (sbyte)0x18,(sbyte) 0x85, (sbyte)0xa0, (sbyte)0x79, (sbyte)0x07, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x23,(sbyte) 0x21, (sbyte)0x87, (sbyte)0x11,(sbyte) 0xf0, (sbyte)0xa4, (sbyte)0x00, (sbyte)0xf7, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x97,(sbyte) 0xe1, (sbyte)0x28, (sbyte)0x07,(sbyte) 0xff, (sbyte)0xf3, (sbyte)0x02, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61,(sbyte) 0x10, (sbyte)0x0c, (sbyte)0x05,(sbyte) 0xf2, (sbyte)0xc4, (sbyte)0x40, (sbyte)0xc8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x01,(sbyte) 0x01, (sbyte)0x56, (sbyte)0x03,(sbyte) 0xb4, (sbyte)0xb2, (sbyte)0x23, (sbyte)0x58, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61,(sbyte) 0x41, (sbyte)0x89, (sbyte)0x03,(sbyte) 0xf1, (sbyte)0xf4, (sbyte)0xf0, (sbyte)0x13, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x04,(sbyte) 0x21, (sbyte)0x28, (sbyte)0x00,(sbyte) 0xdf, (sbyte)0xf8, (sbyte)0xff, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x23,(sbyte) 0x22, (sbyte)0x00, (sbyte)0x00,(sbyte) 0xd8, (sbyte)0xf8, (sbyte)0xf8, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x25,(sbyte) 0x18, (sbyte)0x00, (sbyte)0x00,(sbyte) 0xf8, (sbyte)0xda, (sbyte)0xf8, (sbyte)0x55, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                    },
                    new sbyte[]
                    {
                        (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x33, (sbyte)0x01, (sbyte)0x09, (sbyte)0x0e, (sbyte)0x94, (sbyte)0x90, (sbyte)0x40, (sbyte)0x01, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x13, (sbyte)0x41, (sbyte)0x0f, (sbyte)0x0d, (sbyte)0xce, (sbyte)0xd3, (sbyte)0x43, (sbyte)0x13, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x01, (sbyte)0x12, (sbyte)0x1b, (sbyte)0x06, (sbyte)0xff, (sbyte)0xd2, (sbyte)0x00, (sbyte)0x32, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x61, (sbyte)0x61, (sbyte)0x1b, (sbyte)0x07, (sbyte)0xaf, (sbyte)0x63, (sbyte)0x20, (sbyte)0x28, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x22, (sbyte)0x21, (sbyte)0x1e, (sbyte)0x06, (sbyte)0xf0, (sbyte)0x76, (sbyte)0x08, (sbyte)0x28, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x66, (sbyte)0x21, (sbyte)0x15, (sbyte)0x00, (sbyte)0x93, (sbyte)0x94, (sbyte)0x20, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x21, (sbyte)0x61, (sbyte)0x1c, (sbyte)0x07, (sbyte)0x82, (sbyte)0x81, (sbyte)0x10, (sbyte)0x17, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x23, (sbyte)0x21, (sbyte)0x20, (sbyte)0x1f, (sbyte)0xc0, (sbyte)0x71, (sbyte)0x07, (sbyte)0x47, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x25, (sbyte)0x31, (sbyte)0x26, (sbyte)0x05, (sbyte)0x64, (sbyte)0x41, (sbyte)0x18, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x17, (sbyte)0x21, (sbyte)0x28, (sbyte)0x07, (sbyte)0xff, (sbyte)0x83, (sbyte)0x02, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x97, (sbyte)0x81, (sbyte)0x25, (sbyte)0x07, (sbyte)0xcf, (sbyte)0xc8, (sbyte)0x02, (sbyte)0x14, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x21, (sbyte)0x21, (sbyte)0x54, (sbyte)0x0f, (sbyte)0x80, (sbyte)0x7f, (sbyte)0x07, (sbyte)0x07, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x01, (sbyte)0x01, (sbyte)0x56, (sbyte)0x03, (sbyte)0xd3, (sbyte)0xb2, (sbyte)0x43, (sbyte)0x58, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x31, (sbyte)0x21, (sbyte)0x0c, (sbyte)0x03, (sbyte)0x82, (sbyte)0xc0, (sbyte)0x40, (sbyte)0x07, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x21, (sbyte)0x01, (sbyte)0x0c, (sbyte)0x03, (sbyte)0xd4, (sbyte)0xd3, (sbyte)0x40, (sbyte)0x84, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x04, (sbyte)0x21, (sbyte)0x28, (sbyte)0x00, (sbyte)0xdf, (sbyte)0xf8, (sbyte)0xff, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x23, (sbyte)0x22, (sbyte)0x00, (sbyte)0x00, (sbyte)0xa8, (sbyte)0xf8, (sbyte)0xf8, (sbyte)0xf8, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                        (sbyte)0x25, (sbyte)0x18, (sbyte)0x00, (sbyte)0x00, (sbyte)0xf8, (sbyte)0xa9, (sbyte)0xf8, (sbyte)0x55, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00, (sbyte)0x00,
                    }
                };

                return res;
            }
        }

        internal static OPLL_PATCH[][] Create_Default_Patch()
        {
            OPLL_PATCH[][] res = new OPLL_PATCH[Emu2413API.OPLL_TONE_NUM][]
            {
                new OPLL_PATCH[(16 + 3) * 2],
                new OPLL_PATCH[(16 + 3) * 2],
            };

            for (int x = 0; x < Emu2413API.OPLL_TONE_NUM; x++)
                for (int y = 0; y < (16 + 3) * 2; y++)
                    res[x][y] = new OPLL_PATCH();

            return res;
        }

        internal static uint[,,,] Create_tllTable()
        {
            var res = new uint[16, 8, 1 << Emu2413API.TL_BITS, 4];
            return res;
        }

        internal static Int32[,,] Create_rksTable()
        {
            return new int[2, 8, 2];
        }

        internal static UInt32[,,] Create_dphaseTable()
        {
            return new uint[512, 8, 16];
        }
    }
    public class OPLL_PATCH
    {
        public uint TL, FB, EG, ML, AR, DR, SL, RR, KR, KL, AM, PM, WF;

        public void Copy(OPLL_PATCH other)
        {
            TL = other.TL;
            FB = other.FB;
            EG = other.EG;
            ML = other.ML;
            AR = other.AR;
            DR = other.DR;
            SL = other.SL;
            RR = other.RR;
            KR = other.KR;
            KL = other.KL;
            AM = other.AM;
            PM = other.PM;
            WF = other.WF;
        }
    }

    public class OPLL_SLOT
    {
        public OPLL_PATCH patch;

        public int type;          /* 0 : modulator 1 : carrier */

        /* OUTPUT */
        public Int32 feedback;
        public Int32[] output = new Int32[5];      /* Output value of slot */

        /* for Phase Generator (PG) */
        public UInt32[] sintbl;    /* Wavetable */
        public UInt32 phase;      /* Phase */
        public UInt32 dphase;     /* Phase increment amount */
        public UInt32 pgout;      /* output */

        /* for Envelope Generator (EG) */
        public int fnum;          /* F-Number */
        public int block;         /* Block */
        public int volume;        /* Current volume */
        public int sustine;       /* Sustine 1 = ON, 0 = OFF */
        public UInt32 tll;       /* Total Level + Key scale level*/
        public UInt32 rks;        /* Key scale offset (Rks) */
        public int eg_mode;       /* Current state */
        public UInt32 eg_phase;   /* Phase */
        public UInt32 eg_dphase;  /* Phase increment amount */
        public UInt32 egout;      /* output */


        /* refer to opll-> */
        public int plfo_pm => m_host.lfo_pm;
        public int plfo_am => m_host.lfo_am;

        private OPLL m_host;
        public void SetHost(OPLL host)
        {
            m_host = host;
        }
    }

    public class OPLL_CH
    {
        public int patch_number;
        public int key_status;
        public OPLL_SLOT mod;
        public OPLL_SLOT car;
    }

    public class OPLL
    {
        public UInt32 adr;
        public Int32[] output = new Int32[2];

        /* Register */
        public byte[] reg = new byte[0x40];
        public int[] slot_on_flag = new int[18];

        /* Rythm Mode : 0 = OFF, 1 = ON */
        public int rythm_mode;

        /* Pitch Modulator */
        public UInt32 pm_phase;
        public Int32 lfo_pm;

        /* Amp Modulator */
        public Int32 am_phase;
        public Int32 lfo_am;

        /* Noise Generator */
        public UInt32 noise_seed;
        public UInt32 whitenoise;
        public UInt32 noiseA;
        public UInt32 noiseB;
        public UInt32 noiseA_phase;
        public UInt32 noiseB_phase;
        public UInt32 noiseA_idx;
        public UInt32 noiseB_idx;
        public UInt32 noiseA_dphase;
        public UInt32 noiseB_dphase;

        /* Channel & Slot */
        public OPLL_CH[] ch = new OPLL_CH[9];
        public OPLL_SLOT[] slot = new OPLL_SLOT[18];

        /* Voice Data */
        public OPLL_PATCH[] patch = new OPLL_PATCH[19 * 2];
        public int[] patch_update = new int[2]; /* flag for check patch update */

        public UInt32 mask;

        public int masterVolume; /* 0min -- 64 -- 127 max (Liner) */
    }
}