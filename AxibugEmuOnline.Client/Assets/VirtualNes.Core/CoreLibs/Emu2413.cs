using System;

namespace VirtualNes.Core
{
    public class OPLL_PATCH
    {
        public uint TL, FB, EG, ML, AR, DR, SL, RR, KR, KL, AM, PM, WF;
    }

    public class OPLL_SLOT
    {
        public OPLL_PATCH patch;

        public int type;          /* 0 : modulator 1 : carrier */

        /* OUTPUT */
        public Int32 feedback;
        public Int32[] output = new Int32[5];      /* Output value of slot */

        /* for Phase Generator (PG) */
        public UInt32 sintbl;    /* Wavetable */
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
        public UInt32 plfo_pm;
        public UInt32 plfo_am;
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

        public int masterVolume; /* 0min -- 64 -- 127 max (Liner) */
    }

    public static class Emu2413API
    {
        /* Bits for Pitch and Amp modulator */
        public const int PM_PG_BITS = 8;
        public const int PM_PG_WIDTH = 1 << PM_PG_BITS;
        public const int PM_DP_BITS = 16;
        public const int PM_DP_WIDTH = (1 << PM_DP_BITS);
        public const int AM_PG_BITS = 8;
        public const int AM_PG_WIDTH = (1 << AM_PG_BITS);
        public const int AM_DP_BITS = 16;
        public const int AM_DP_WIDTH = (1 << AM_DP_BITS);

        /* PM table is calcurated by PM_AMP * pow(2,PM_DEPTH*sin(x)/1200) */
        public const int PM_AMP_BITS = 8;
        public const int PM_AMP = (1 << PM_AMP_BITS);

        /* PM speed(Hz) and depth(cent) */
        public const double PM_SPEED = 6.4d;
        public const double PM_DEPTH = 13.75d;

        public const int OPLL_2413_TONE = 0;
        public const int OPLL_VRC7_TONE = 1;

        static int[] pmtable = new int[PM_PG_WIDTH];
        static int[] amtable = new int[AM_PG_WIDTH];

        public static void OPLL_init(UInt32 c, UInt32 r)
        {
            makePmTable();
            makeAmTable();
            makeDB2LinTable();
            makeAdjustTable();
            makeTllTable();
            makeRksTable();
            makeSinTable();
            makeDefaultPatch();
            OPLL_setClock(c, r);
        }

        internal static void OPLL_setClock(uint c, uint r)
        {
            throw new NotImplementedException();
        }

        private static void makeDefaultPatch()
        {
            throw new NotImplementedException();
        }

        private static void makeSinTable()
        {
            throw new NotImplementedException();
        }

        private static void makeRksTable()
        {
            throw new NotImplementedException();
        }

        private static void makeTllTable()
        {
            throw new NotImplementedException();
        }

        private static void makeAdjustTable()
        {
            throw new NotImplementedException();
        }

        private static void makeDB2LinTable()
        {
            throw new NotImplementedException();
        }

        private static void makeAmTable()
        {
            throw new NotImplementedException();
        }

        private static void makePmTable()
        {
            int i;

            for (i = 0; i < PM_PG_WIDTH; i++)
                pmtable[i] = (int)(PM_AMP * Math.Pow(2, PM_DEPTH * Math.Sin(2.0 * Math.PI * i / PM_PG_WIDTH) / 1200));
        }

        internal static OPLL OPLL_new()
        {
            throw new NotImplementedException();
        }

        internal static void OPLL_reset(OPLL vRC7_OPLL)
        {
            throw new NotImplementedException();
        }

        internal static void OPLL_reset_patch(OPLL vRC7_OPLL, int oPLL_VRC7_TONE)
        {
            throw new NotImplementedException();
        }

        internal static void OPLL_delete(OPLL vRC7_OPLL)
        {
            throw new NotImplementedException();
        }

        internal static void OPLL_writeReg(OPLL opll, UInt32 reg, UInt32 data)
        {
            throw new NotImplementedException();
        }

        internal static int OPLL_calc(OPLL opll)
        {
            throw new NotImplementedException();
        }
    }
}
