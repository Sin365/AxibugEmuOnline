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
    }

    public static class Emu2413API
    {
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

        private static void OPLL_setClock(uint c, uint r)
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
            throw new NotImplementedException();
        }
    }
}
