using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Lifetime;
namespace VirtualNes.Core.Emu2413
{
    public static class Emu2413API
    {
        static sbyte[][] default_inst = Const.Create_Default_Inst();

        public const int OPLL_TONE_NUM = 2;

        /* Size of Sintable ( 1 -- 18 can be used, but 7 -- 14 recommended.)*/
        public const int PG_BITS = 9;
        public const int PG_WIDTH = (1 << PG_BITS);

        /* Phase increment counter */
        public const int DP_BITS = 18;
        public const int DP_WIDTH = (1 << DP_BITS);
        public const int DP_BASE_BITS = (DP_BITS - PG_BITS);

        /* Dynamic range */
        public const double DB_STEP = 0.375;
        public const int DB_BITS = 7;
        public const int DB_MUTE = (1 << DB_BITS);

        /* Dynamic range of envelope */
        public const double EG_STEP = 0.375;
        public const int EG_BITS = 7;
        //public const int EG_MUTE = (1 << EB_BITS); ?? 原文如此 EB_BITS 根本不存在
        public const int EG_MUTE = (1 << EG_BITS);

        /* Dynamic range of total level */
        public const double TL_STEP = 0.75;
        public const int TL_BITS = 6;
        public const int TL_MUTE = (1 << TL_BITS);

        /* Dynamic range of sustine level */
        public const double SL_STEP = 3.0;
        public const int SL_BITS = 4;
        public const int SL_MUTE = (1 << SL_BITS);

        static int EG2DB(int d)
        {
            return (d * (int)(EG_STEP / DB_STEP));
        }

        static uint TL2EG(int d)
        {
            return (uint)(d * (int)(TL_STEP / EG_STEP));
        }

        static int SL2EG(int d)
        {
            return (d * (int)(SL_STEP / EG_STEP));
        }

        /* Volume of Noise (dB) */
        public const double DB_NOISE = 24;

        static uint DB_POS(double x)
        {
            return (uint)(x / DB_STEP);
        }

        static uint DB_NEG(double x)
        {
            return (uint)(DB_MUTE + DB_MUTE + x / DB_STEP);
        }

        /* Bits for liner value */
        public const int DB2LIN_AMP_BITS = 10;
        public const int SLOT_AMP_BITS = (DB2LIN_AMP_BITS);

        /* Bits for envelope phase incremental counter */
        public const int EG_DP_BITS = 22;
        public const int EG_DP_WIDTH = (1 << EG_DP_BITS);

        /* Bits for Pitch and Amp modulator */
        public const int PM_PG_BITS = 8;
        public const int PM_PG_WIDTH = 1 << PM_PG_BITS;
        public const int PM_DP_BITS = 16;
        public const int PM_DP_WIDTH = (1 << PM_DP_BITS);
        public const int AM_PG_BITS = 8;
        public const int AM_PG_WIDTH = (1 << AM_PG_BITS);
        public const int AM_DP_BITS = 16;
        public const int AM_DP_WIDTH = (1 << AM_DP_BITS);

        /* Mask */
        static int OPLL_MASK_CH(int x)
        {
            return 1 << x;
        }
        public const int OPLL_MASK_HH = 1 << 9;
        public const int OPLL_MASK_CYM = (1 << (10));
        public const int OPLL_MASK_TOM = (1 << (11));
        public const int OPLL_MASK_SD = (1 << (12));
        public const int OPLL_MASK_BD = (1 << (13));
        public const int OPLL_MASK_RYTHM = OPLL_MASK_HH | OPLL_MASK_CYM | OPLL_MASK_TOM | OPLL_MASK_SD | OPLL_MASK_BD;

        /* PM table is calcurated by PM_AMP * pow(2,PM_DEPTH*sin(x)/1200) */
        public const int PM_AMP_BITS = 8;
        public const int PM_AMP = (1 << PM_AMP_BITS);

        /* PM speed(Hz) and depth(cent) */
        public const double PM_SPEED = 6.4d;
        public const double PM_DEPTH = 13.75d;

        /* AM speed(Hz) and depth(dB) */
        public const double AM_SPEED = 3.7;
        public const double AM_DEPTH = 4.8;

        /* Cut the lower b bit(s) off. */
        static int HIGHBITS(int c, int b)
        {
            return c >> b;
        }

        /* Leave the lower b bit(s). */
        static int LOWBITS(int c, int b)
        {
            return c & ((1 << b) - 1);
        }

        /* Expand x which is s bits to d bits. */
        static int EXPAND_BITS(int x, int s, int d)
        {
            return (x << (d - s));
        }

        /* Expand x which is s bits to d bits and fill expanded bits '1' */
        static int EXPAND_BITS_X(int x, int s, int d)
        {
            return ((x << (d - s)) | ((1 << (d - s)) - 1));
        }

        /* Adjust envelope speed which depends on sampling rate. */
        static uint rate_adjust(int x)
        {
            return (uint)((double)x * clk / 72 / rate + 0.5); /* +0.5 to round */
        }

        static OPLL_SLOT MOD(this OPLL opll, int x)
        {
            return opll.ch[x].mod;
        }
        static OPLL_SLOT CAR(this OPLL opll, int x)
        {
            return opll.ch[x].car;
        }

        /* Sampling rate */
        static uint rate;
        /* Input clock */
        static uint clk;

        /* WaveTable for each envelope amp */
        static uint[] fullsintable = new uint[PG_WIDTH];
        static uint[] halfsintable = new uint[PG_WIDTH];
        static uint[] snaretable = new uint[PG_WIDTH];

        static int[] noiseAtable = new int[64]
        {
            -1,1,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,
            -1,1,0,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0,-1,1,0,0
        };

        static int[] noiseBtable = new int[8]
        {
            -1,1,-1,1,0,0,0,0
        };

        static uint[][] waveform = new uint[5][]
        {
            fullsintable, halfsintable,snaretable,null,null
        };

        /* Noise and LFO */
        static uint pm_dphase;
        static uint am_dphase;

        /* dB to Liner table */
        static int[] DB2LIN_TABLE = new int[(DB_MUTE + DB_MUTE) * 2];

        /* Liner to Log curve conversion table (for Attack rate). */
        static uint[] AR_ADJUST_TABLE = new uint[1 << EG_BITS];

        /* Empty voice data */
        static OPLL_PATCH null_patch = new OPLL_PATCH();

        /* Basic voice Data */
        static OPLL_PATCH[][] default_patch = Const.Create_Default_Patch();

        /* Definition of envelope mode */
        enum EnvelopeMode { SETTLE, ATTACK, DECAY, SUSHOLD, SUSTINE, RELEASE, FINISH };

        /* Phase incr table for Attack */
        static uint[][] dphaseARTable = new uint[16][]
        {
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],
        };
        /* Phase incr table for Decay and Release */
        static uint[][] dphaseDRTable = new uint[16][]
        {
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],new uint[16],new uint[16],new uint[16],new uint[16],
            new uint[16],
        };

        /* KSL + TL Table */
        static uint[,,,] tllTable = Const.Create_tllTable();
        static int[,,] rksTable = Const.Create_rksTable();

        /* Phase incr table for PG */
        static uint[,,] dphaseTable = Const.Create_dphaseTable();

        public const int OPLL_2413_TONE = 0;
        public const int OPLL_VRC7_TONE = 1;

        static int[] pmtable = new int[PM_PG_WIDTH];
        static int[] amtable = new int[AM_PG_WIDTH];

        static int Min(int i, int j)
        {
            if (i < j) return i; else return j;
        }

        /* Table for AR to LogCurve. */
        static void makeAdjustTable()
        {
            int i;

            AR_ADJUST_TABLE[0] = (1 << EG_BITS);
            for (i = 1; i < 128; i++)
                AR_ADJUST_TABLE[i] = (uint)((double)(1 << EG_BITS) - 1 - (1 << EG_BITS) * Math.Log(i) / Math.Log(128));
        }

        /* Table for dB(0 -- (1<<DB_BITS)) to Liner(0 -- DB2LIN_AMP_WIDTH) */
        static void makeDB2LinTable()
        {
            int i;

            for (i = 0; i < DB_MUTE + DB_MUTE; i++)
            {
                DB2LIN_TABLE[i] = (int)((double)((1 << DB2LIN_AMP_BITS) - 1) * Math.Pow(10, -(double)i * DB_STEP / 20));
                if (i >= DB_MUTE) DB2LIN_TABLE[i] = 0;
                DB2LIN_TABLE[i + DB_MUTE + DB_MUTE] = -DB2LIN_TABLE[i];
            }
        }

        /* Liner(+0.0 - +1.0) to dB((1<<DB_BITS) - 1 -- 0) */
        static int lin2db(double d)
        {
            if (d == 0) return (DB_MUTE - 1);
            else return Min(-(int)(20.0 * Math.Log10(d) / DB_STEP), DB_MUTE - 1); /* 0 -- 128 */
        }

        /* Sin Table */
        static void makeSinTable()
        {
            int i;

            for (i = 0; i < PG_WIDTH / 4; i++)
            {
                fullsintable[i] = (uint)lin2db(Math.Sin(2.0 * Math.PI * i / PG_WIDTH));
                snaretable[i] = (uint)((6.0) / DB_STEP);
            }

            for (i = 0; i < PG_WIDTH / 4; i++)
            {
                fullsintable[PG_WIDTH / 2 - 1 - i] = fullsintable[i];
                snaretable[PG_WIDTH / 2 - 1 - i] = snaretable[i];
            }

            for (i = 0; i < PG_WIDTH / 2; i++)
            {
                fullsintable[PG_WIDTH / 2 + i] = DB_MUTE + DB_MUTE + fullsintable[i];
                snaretable[PG_WIDTH / 2 + i] = DB_MUTE + DB_MUTE + snaretable[i];
            }

            for (i = 0; i < PG_WIDTH / 2; i++) halfsintable[i] = fullsintable[i];
            for (i = PG_WIDTH / 2; i < PG_WIDTH; i++) halfsintable[i] = fullsintable[0];

            for (i = 0; i < 64; i++)
            {
                if (noiseAtable[i] > 0) noiseAtable[i] = (int)DB_POS(0);
                else if (noiseAtable[i] < 0) noiseAtable[i] = (int)DB_NEG(0);
                else noiseAtable[i] = DB_MUTE - 1;
            }

            for (i = 0; i < 8; i++)
            {
                if (noiseBtable[i] > 0) noiseBtable[i] = (int)DB_POS(0);
                else if (noiseBtable[i] < 0) noiseBtable[i] = (int)DB_NEG(0);
                else noiseBtable[i] = DB_MUTE - 1;
            }

        }

        /* Table for Amp Modulator */
        static void makeAmTable()
        {
            int i;

            for (i = 0; i < AM_PG_WIDTH; i++)
                amtable[i] = (int)((double)AM_DEPTH / 2 / DB_STEP * (1.0 + Math.Sin(2.0 * Math.PI * i / PM_PG_WIDTH)));
        }

        static uint[] mltable_makeDphaseTable = new uint[16] { 1, 1 * 2, 2 * 2, 3 * 2, 4 * 2, 5 * 2, 6 * 2, 7 * 2, 8 * 2, 9 * 2, 10 * 2, 10 * 2, 12 * 2, 12 * 2, 15 * 2, 15 * 2 };
        /* Phase increment counter table */
        static void makeDphaseTable()
        {
            uint fnum, block, ML;

            for (fnum = 0; fnum < 512; fnum++)
                for (block = 0; block < 8; block++)
                    for (ML = 0; ML < 16; ML++)
                        dphaseTable[fnum, block, ML] = rate_adjust((int)((fnum * mltable_makeDphaseTable[ML]) << (int)block) >> (20 - DP_BITS));
        }

        static uint dB2(double x)
        {
            return (uint)(x * 2);
        }

        static uint[] kltable = new uint[16]
        {
            dB2( 0.000),dB2( 9.000),dB2(12.000),dB2(13.875),dB2(15.000),dB2(16.125),dB2(16.875),dB2(17.625),
            dB2(18.000),dB2(18.750),dB2(19.125),dB2(19.500),dB2(19.875),dB2(20.250),dB2(20.625),dB2(21.000)
        };

        static void makeTllTable()
        {
            int tmp;
            int fnum, block, TL, KL;

            for (fnum = 0; fnum < 16; fnum++)
                for (block = 0; block < 8; block++)
                    for (TL = 0; TL < 64; TL++)
                        for (KL = 0; KL < 4; KL++)
                        {
                            if (KL == 0)
                            {
                                tllTable[fnum, block, TL, KL] = TL2EG(TL);
                            }
                            else
                            {
                                tmp = (int)(kltable[fnum] - dB2(3.000) * (7 - block));
                                if (tmp <= 0)
                                    tllTable[fnum, block, TL, KL] = TL2EG(TL);
                                else
                                    tllTable[fnum, block, TL, KL] = (uint)((tmp >> (3 - KL)) / EG_STEP) + TL2EG(TL);
                            }
                        }
        }

        /* Rate Table for Attack */
        static void makeDphaseARTable()
        {
            int AR, Rks, RM, RL;

            for (AR = 0; AR < 16; AR++)
                for (Rks = 0; Rks < 16; Rks++)
                {
                    RM = AR + (Rks >> 2);
                    if (RM > 15) RM = 15;
                    RL = Rks & 3;
                    switch (AR)
                    {
                        case 0:
                            dphaseARTable[AR][Rks] = 0;
                            break;
                        case 15:
                            dphaseARTable[AR][Rks] = EG_DP_WIDTH;
                            break;
                        default:
                            dphaseARTable[AR][Rks] = rate_adjust((3 * (RL + 4) << (RM + 1)));
                            break;
                    }
                }
        }

        /* Rate Table for Decay */
        static void makeDphaseDRTable()
        {
            int DR, Rks, RM, RL;

            for (DR = 0; DR < 16; DR++)
                for (Rks = 0; Rks < 16; Rks++)
                {
                    RM = DR + (Rks >> 2);
                    RL = Rks & 3;
                    if (RM > 15) RM = 15;
                    switch (DR)
                    {
                        case 0:
                            dphaseDRTable[DR][Rks] = 0;
                            break;
                        default:
                            dphaseDRTable[DR][Rks] = rate_adjust((RL + 4) << (RM - 1));
                            break;
                    }
                }
        }

        static void makeRksTable()
        {

            int fnum8, block, KR;

            for (fnum8 = 0; fnum8 < 2; fnum8++)
                for (block = 0; block < 8; block++)
                    for (KR = 0; KR < 2; KR++)
                    {
                        if (KR != 0)
                            rksTable[fnum8, block, KR] = (block << 1) + fnum8;
                        else
                            rksTable[fnum8, block, KR] = block >> 1;
                    }
        }

        private static void makePmTable()
        {
            int i;

            for (i = 0; i < PM_PG_WIDTH; i++)
                pmtable[i] = (int)(PM_AMP * Math.Pow(2, PM_DEPTH * Math.Sin(2.0 * Math.PI * i / PM_PG_WIDTH) / 1200));
        }

        static void dump2patch(ArrayRef<sbyte> dump, ArrayRef<OPLL_PATCH> patch)
        {
            patch[0].AM = (uint)((dump[0] >> 7) & 1);
            patch[1].AM = (uint)((dump[1] >> 7) & 1);
            patch[0].PM = (uint)((dump[0] >> 6) & 1);
            patch[1].PM = (uint)((dump[1] >> 6) & 1);
            patch[0].EG = (uint)((dump[0] >> 5) & 1);
            patch[1].EG = (uint)((dump[1] >> 5) & 1);
            patch[0].KR = (uint)((dump[0] >> 4) & 1);
            patch[1].KR = (uint)((dump[1] >> 4) & 1);
            patch[0].ML = (uint)((dump[0]) & 15);
            patch[1].ML = (uint)((dump[1]) & 15);
            patch[0].KL = (uint)((dump[2] >> 6) & 3);
            patch[1].KL = (uint)((dump[3] >> 6) & 3);
            patch[0].TL = (uint)((dump[2]) & 63);
            patch[0].FB = (uint)((dump[3]) & 7);
            patch[0].WF = (uint)((dump[3] >> 3) & 1);
            patch[1].WF = (uint)((dump[3] >> 4) & 1);
            patch[0].AR = (uint)((dump[4] >> 4) & 15);
            patch[1].AR = (uint)((dump[5] >> 4) & 15);
            patch[0].DR = (uint)((dump[4]) & 15);
            patch[1].DR = (uint)((dump[5]) & 15);
            patch[0].SL = (uint)((dump[6] >> 4) & 15);
            patch[1].SL = (uint)((dump[7] >> 4) & 15);
            patch[0].RR = (uint)((dump[6]) & 15);
            patch[1].RR = (uint)((dump[7]) & 15);
        }

        static ArrayRef<sbyte> instSpan = new ArrayRef<sbyte>();
        static ArrayRef<OPLL_PATCH> patchSpan = new ArrayRef<OPLL_PATCH>();
        static void makeDefaultPatch()
        {
            int i, j;

            for (i = 0; i < OPLL_TONE_NUM; i++)
                for (j = 0; j < 19; j++)
                {
                    instSpan.SetArray(default_inst[i], j * 16);
                    patchSpan.SetArray(default_patch[i], j * 2);
                    dump2patch(instSpan, patchSpan);
                }
        }

        static uint calc_eg_dphase(OPLL_SLOT slot)
        {

            switch ((EnvelopeMode)slot.eg_mode)
            {
                case EnvelopeMode.ATTACK:
                    return dphaseARTable[slot.patch.AR][slot.rks];

                case EnvelopeMode.DECAY:
                    return dphaseDRTable[slot.patch.DR][slot.rks];

                case EnvelopeMode.SUSHOLD:
                    return 0;

                case EnvelopeMode.SUSTINE:
                    return dphaseDRTable[slot.patch.RR][slot.rks];

                case EnvelopeMode.RELEASE:
                    if (slot.sustine != 0)
                        return dphaseDRTable[5][slot.rks];
                    else if (slot.patch.EG != 0)
                        return dphaseDRTable[slot.patch.RR][slot.rks];
                    else
                        return dphaseDRTable[7][slot.rks];

                case EnvelopeMode.FINISH:
                    return 0;

                default:
                    return 0;
            }
        }

        public const int SLOT_BD1 = 12;
        public const int SLOT_BD2 = 13;
        public const int SLOT_HH = 14;
        public const int SLOT_SD = 15;
        public const int SLOT_TOM = 16;
        public const int SLOT_CYM = 17;

        static void UPDATE_PG(OPLL_SLOT S)
        {
            S.dphase = dphaseTable[S.fnum, S.block, S.patch.ML];
        }

        static void UPDATE_TLL(OPLL_SLOT S)
        {
            if (S.type == 0)
            {
                S.tll = tllTable[S.fnum >> 5, S.block, S.patch.TL, S.patch.KL];
            }
            else
            {
                S.tll = tllTable[S.fnum >> 5, S.block, S.volume, S.patch.KL];
            }
        }

        static void UPDATE_RKS(OPLL_SLOT S)
        {
            S.rks = (uint)rksTable[(S.fnum) >> 8, S.block, S.patch.KR];
        }

        static void UPDATE_WF(OPLL_SLOT S)
        {
            S.sintbl = waveform[S.patch.WF];
        }

        static void UPDATE_EG(OPLL_SLOT S)
        {
            S.eg_dphase = calc_eg_dphase(S);
        }

        static void UPDATE_ALL(OPLL_SLOT S)
        {
            UPDATE_PG(S);
            UPDATE_TLL(S);
            UPDATE_RKS(S);
            UPDATE_WF(S);
            UPDATE_EG(S); /* G should be last */
        }

        /* Force Refresh (When external program changes some parameters). */
        static void OPLL_forceRefresh(OPLL opll)
        {
            int i;

            if (opll == null) return;

            for (i = 0; i < 18; i++)
            {
                UPDATE_PG(opll.slot[i]);
                UPDATE_RKS(opll.slot[i]);
                UPDATE_TLL(opll.slot[i]);
                UPDATE_WF(opll.slot[i]);
                UPDATE_EG(opll.slot[i]);
            }
        }

        /* Slot key on  */
        static void slotOn(OPLL_SLOT slot)
        {
            slot.eg_mode = (int)EnvelopeMode.ATTACK;
            slot.phase = 0;
            slot.eg_phase = 0;
        }

        /* Slot key off */
        static void slotOff(OPLL_SLOT slot)
        {
            if (slot.eg_mode == (int)EnvelopeMode.ATTACK)
                slot.eg_phase = (uint)EXPAND_BITS((int)AR_ADJUST_TABLE[HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS)], EG_BITS, EG_DP_BITS);
            slot.eg_mode = (int)EnvelopeMode.RELEASE;
        }

        /* Channel key on */
        static void keyOn(OPLL opll, int i)
        {
            if (opll.slot_on_flag[i * 2] == 0) slotOn(opll.MOD(i));
            if (opll.slot_on_flag[i * 2 + 1] == 0) slotOn(opll.CAR(i));
            opll.ch[i].key_status = 1;
        }

        static void keyOff(OPLL opll, int i)
        {
            if (opll.slot_on_flag[i * 2 + 1] != 0) slotOff(opll.CAR(i));
            opll.ch[i].key_status = 0;
        }

        static void keyOn_BD(OPLL opll) { keyOn(opll, 6); }
        static void keyOn_SD(OPLL opll) { if (opll.slot_on_flag[SLOT_SD] == 0) slotOn(opll.CAR(7)); }
        static void keyOn_TOM(OPLL opll) { if (opll.slot_on_flag[SLOT_TOM] == 0) slotOn(opll.MOD(8)); }
        static void keyOn_HH(OPLL opll) { if (opll.slot_on_flag[SLOT_HH] == 0) slotOn(opll.MOD(7)); }
        static void keyOn_CYM(OPLL opll) { if (opll.slot_on_flag[SLOT_CYM] == 0) slotOn(opll.CAR(8)); }

        /* Drum key off */

        static void keyOff_BD(OPLL opll) { keyOff(opll, 6); }
        static void keyOff_SD(OPLL opll) { if (opll.slot_on_flag[SLOT_SD] != 0) slotOff(opll.CAR(7)); }
        static void keyOff_TOM(OPLL opll) { if (opll.slot_on_flag[SLOT_TOM] != 0) slotOff(opll.MOD(8)); }
        static void keyOff_HH(OPLL opll) { if (opll.slot_on_flag[SLOT_HH] != 0) slotOff(opll.MOD(7)); }
        static void keyOff_CYM(OPLL opll) { if (opll.slot_on_flag[SLOT_CYM] != 0) slotOff(opll.CAR(8)); }

        /* Change a voice */
        static void setPatch(OPLL opll, int i, int num)
        {
            opll.ch[i].patch_number = num;
            opll.MOD(i).patch = opll.patch[num * 2 + 0];
            opll.CAR(i).patch = opll.patch[num * 2 + 1];
        }

        /* Change a rythm voice */
        static void setSlotPatch(OPLL_SLOT slot, OPLL_PATCH patch)
        {
            slot.patch = patch;
        }

        /* Set sustine parameter */
        static void setSustine(OPLL opll, int c, int sustine)
        {
            opll.CAR(c).sustine = sustine;
            if (opll.MOD(c).type != 0) opll.MOD(c).sustine = sustine;
        }

        /* Volume : 6bit ( Volume register << 2 ) */
        static void setVolume(OPLL opll, int c, int volume)
        {
            opll.CAR(c).volume = volume;
        }

        static void setSlotVolume(OPLL_SLOT slot, int volume)
        {
            slot.volume = volume;
        }

        /* Set F-Number ( fnum : 9bit ) */
        static void setFnumber(OPLL opll, int c, int fnum)
        {
            opll.CAR(c).fnum = fnum;
            opll.MOD(c).fnum = fnum;
        }

        /* Set Block data (block : 3bit ) */
        static void setBlock(OPLL opll, int c, int block)
        {
            opll.CAR(c).block = block;
            opll.MOD(c).block = block;
        }

        /* Change Rythm Mode */
        static void setRythmMode(OPLL opll, int mode)
        {
            opll.rythm_mode = mode;

            if (mode != 0)
            {
                opll.ch[6].patch_number = 16;
                opll.ch[7].patch_number = 17;
                opll.ch[8].patch_number = 18;
                setSlotPatch(opll.slot[SLOT_BD1], opll.patch[16 * 2 + 0]);
                setSlotPatch(opll.slot[SLOT_BD2], opll.patch[16 * 2 + 1]);
                setSlotPatch(opll.slot[SLOT_HH], opll.patch[17 * 2 + 0]);
                setSlotPatch(opll.slot[SLOT_SD], opll.patch[17 * 2 + 1]);
                opll.slot[SLOT_HH].type = 1;
                setSlotPatch(opll.slot[SLOT_TOM], opll.patch[18 * 2 + 0]);
                setSlotPatch(opll.slot[SLOT_CYM], opll.patch[18 * 2 + 1]);
                opll.slot[SLOT_TOM].type = 1;
            }
            else
            {
                setPatch(opll, 6, opll.reg[0x36] >> 4);
                setPatch(opll, 7, opll.reg[0x37] >> 4);
                opll.slot[SLOT_HH].type = 0;
                setPatch(opll, 8, opll.reg[0x38] >> 4);
                opll.slot[SLOT_TOM].type = 0;
            }

            if (opll.slot_on_flag[SLOT_BD1] == 0)
                opll.slot[SLOT_BD1].eg_mode = (int)EnvelopeMode.FINISH;
            if (opll.slot_on_flag[SLOT_BD2] == 0)
                opll.slot[SLOT_BD2].eg_mode = (int)EnvelopeMode.FINISH;
            if (opll.slot_on_flag[SLOT_HH] == 0)
                opll.slot[SLOT_HH].eg_mode = (int)EnvelopeMode.FINISH;
            if (opll.slot_on_flag[SLOT_SD] == 0)
                opll.slot[SLOT_SD].eg_mode = (int)EnvelopeMode.FINISH;
            if (opll.slot_on_flag[SLOT_TOM] == 0)
                opll.slot[SLOT_TOM].eg_mode = (int)EnvelopeMode.FINISH;
            if (opll.slot_on_flag[SLOT_CYM] == 0)
                opll.slot[SLOT_CYM].eg_mode = (int)EnvelopeMode.FINISH;
        }

        static void OPLL_copyPatch(OPLL opll, int num, OPLL_PATCH patch)
        {
            opll.patch[num].Copy(patch);
        }

        static void OPLL_SLOT_reset(OPLL_SLOT slot)
        {
            slot.sintbl = waveform[0];
            slot.phase = 0;
            slot.dphase = 0;
            slot.output[0] = 0;
            slot.output[1] = 0;
            slot.feedback = 0;
            slot.eg_mode = (int)EnvelopeMode.SETTLE;
            slot.eg_phase = EG_DP_WIDTH;
            slot.eg_dphase = 0;
            slot.rks = 0;
            slot.tll = 0;
            slot.sustine = 0;
            slot.fnum = 0;
            slot.block = 0;
            slot.volume = 0;
            slot.pgout = 0;
            slot.egout = 0;
            slot.patch = null_patch;
        }

        static OPLL_SLOT OPLL_SLOT_new()
        {
            OPLL_SLOT slot;
            slot = new OPLL_SLOT();

            return slot;
        }

        static void OPLL_SLOT_delete(OPLL_SLOT slot)
        {
            //free(slot);  // c# just do nothing
        }

        static void OPLL_CH_reset(OPLL_CH ch)
        {
            if (ch.mod != null) OPLL_SLOT_reset(ch.mod);
            if (ch.car != null) OPLL_SLOT_reset(ch.car);
            ch.key_status = 0;
        }

        static OPLL_CH OPLL_CH_new()
        {
            OPLL_CH ch;
            OPLL_SLOT mod, car;

            mod = OPLL_SLOT_new();
            if (mod == null) return null;

            car = OPLL_SLOT_new();
            if (car == null)
            {
                OPLL_SLOT_delete(mod);
                return null;
            }

            ch = new OPLL_CH();
            if (ch == null)
            {
                OPLL_SLOT_delete(mod);
                OPLL_SLOT_delete(car);
                return null;
            }

            mod.type = 0;
            car.type = 1;
            ch.mod = mod;
            ch.car = car;

            return ch;
        }

        static void OPLL_CH_delete(OPLL_CH ch)
        {
            OPLL_SLOT_delete(ch.mod);
            OPLL_SLOT_delete(ch.car);
            //free(ch); C# just do nothing
        }

        public static OPLL OPLL_new()
        {
            OPLL opll;
            OPLL_CH[] ch = new OPLL_CH[9];
            OPLL_PATCH[] patch = new OPLL_PATCH[19 * 2];
            int i, j;

            for (i = 0; i < 19 * 2; i++)
            {
                patch[i] = new OPLL_PATCH();
            }

            for (i = 0; i < 9; i++)
            {
                ch[i] = OPLL_CH_new();
            }

            opll = new OPLL();

            for (i = 0; i < 19 * 2; i++)
                opll.patch[i] = patch[i];


            for (i = 0; i < 9; i++)
            {
                opll.ch[i] = ch[i];
                opll.slot[i * 2 + 0] = opll.ch[i].mod;
                opll.slot[i * 2 + 1] = opll.ch[i].car;
            }

            for (i = 0; i < 18; i++)
            {
                opll.slot[i].SetHost(opll);
            }

            opll.mask = 0;

            OPLL_reset(opll);
            OPLL_reset_patch(opll, 0);

            opll.masterVolume = 32;

            return opll;
        }

        public static void OPLL_delete(OPLL opll)
        {
            int i;

            for (i = 0; i < 9; i++)
                OPLL_CH_delete(opll.ch[i]);

            //for (i = 0; i < 19 * 2; i++)
            //    free(opll->patch[i]);

            //free(opll);
        }

        /* Reset patch datas by system default. */
        public static void OPLL_reset_patch(OPLL opll, int type)
        {
            int i;

            for (i = 0; i < 19 * 2; i++)
                OPLL_copyPatch(opll, i, default_patch[type % OPLL_TONE_NUM][i]);
        }

        /* Reset whole of OPLL except patch datas. */
        public static void OPLL_reset(OPLL opll)
        {
            int i;

            if (opll == null) return;

            opll.adr = 0;

            opll.output[0] = 0;
            opll.output[1] = 0;

            opll.pm_phase = 0;
            opll.am_phase = 0;

            opll.noise_seed = 0xffff;
            opll.noiseA = 0;
            opll.noiseB = 0;
            opll.noiseA_phase = 0;
            opll.noiseB_phase = 0;
            opll.noiseA_dphase = 0;
            opll.noiseB_dphase = 0;
            opll.noiseA_idx = 0;
            opll.noiseB_idx = 0;

            for (i = 0; i < 9; i++)
            {
                OPLL_CH_reset(opll.ch[i]);
                setPatch(opll, i, 0);
            }

            for (i = 0; i < 0x40; i++) OPLL_writeReg(opll, (uint)i, 0);
        }

        public static void OPLL_setClock(uint c, uint r)
        {
            clk = c;
            rate = r;
            makeDphaseTable();
            makeDphaseARTable();
            makeDphaseDRTable();
            pm_dphase = rate_adjust((int)(PM_SPEED * PM_DP_WIDTH / (clk / 72)));
            am_dphase = rate_adjust((int)(AM_SPEED * AM_DP_WIDTH / (clk / 72)));
        }

        public static void OPLL_init(uint c, uint r)
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
        static void OPLL_close()
        {
        }

        static int wave2_2pi(int e)
        {
            return (e) >> (SLOT_AMP_BITS - PG_BITS);
        }

        static int wave2_4pi(int e)
        {
            return e;
        }

        static int wave2_8pi(int e)
        {
            return (e) << (2 + PG_BITS - SLOT_AMP_BITS);
        }

        /* 16bit rand */
        static uint mrand(uint seed)
        {
            return ((seed >> 15) ^ ((seed >> 12) & 1)) | ((seed << 1) & 0xffff);
        }

        static uint DEC(uint db)
        {
            if (db < DB_MUTE + DB_MUTE)
            {
                return (uint)Min((int)(db + DB_POS(0.375 * 2)), DB_MUTE - 1);
            }
            else
            {
                return (uint)Min((int)(db + DB_POS(0.375 * 2)), DB_MUTE + DB_MUTE + DB_MUTE - 1);
            }
        }

        /* Update Noise unit */
        static void update_noise(OPLL opll)
        {
            opll.noise_seed = mrand(opll.noise_seed);
            opll.whitenoise = opll.noise_seed & 1;

            opll.noiseA_phase = (opll.noiseA_phase + opll.noiseA_dphase);
            opll.noiseB_phase = (opll.noiseB_phase + opll.noiseB_dphase);

            if (opll.noiseA_phase < (1 << 11))
            {
                if (opll.noiseA_phase > 16) opll.noiseA = DB_MUTE - 1;
            }
            else
            {
                opll.noiseA_phase &= (1 << 11) - 1;
                opll.noiseA_idx = (opll.noiseA_idx + 1) & 63;
                opll.noiseA = (uint)noiseAtable[opll.noiseA_idx];
            }

            if (opll.noiseB_phase < (1 << 12))
            {
                if (opll.noiseB_phase > 16) opll.noiseB = DB_MUTE - 1;
            }
            else
            {
                opll.noiseB_phase &= (1 << 12) - 1;
                opll.noiseB_idx = (opll.noiseB_idx + 1) & 7;
                opll.noiseB = (uint)noiseBtable[opll.noiseB_idx];
            }
        }

        /* Update AM, PM unit */
        static void update_ampm(OPLL opll)
        {
            opll.pm_phase = (opll.pm_phase + pm_dphase) & (PM_DP_WIDTH - 1);
            opll.am_phase = (int)(opll.am_phase + am_dphase) & (AM_DP_WIDTH - 1);
            opll.lfo_am = amtable[HIGHBITS(opll.am_phase, AM_DP_BITS - AM_PG_BITS)];
            opll.lfo_pm = pmtable[HIGHBITS((int)(opll.pm_phase), PM_DP_BITS - PM_PG_BITS)];
        }

        /* PG */
        static uint calc_phase(OPLL_SLOT slot)
        {
            if (slot.patch.PM != 0)
                slot.phase = (uint)(slot.phase + (slot.dphase * (slot.plfo_pm)) >> PM_AMP_BITS);
            else
                slot.phase += slot.dphase;

            slot.phase &= (DP_WIDTH - 1);

            return (uint)HIGHBITS((int)slot.phase, DP_BASE_BITS);
        }

        static uint S2E(int x)
        {
            return (uint)(SL2EG((int)(x / SL_STEP)) << (EG_DP_BITS - EG_BITS));
        }

        static uint[] SL = new uint[16]
        {
            S2E( 0), S2E( 3), S2E( 6), S2E( 9), S2E(12), S2E(15), S2E(18), S2E(21),
            S2E(24), S2E(27), S2E(30), S2E(33), S2E(36), S2E(39), S2E(42), S2E(48)
        };

        /* EG */
        static uint calc_envelope(OPLL_SLOT slot)
        {
            uint egout;

            switch ((EnvelopeMode)slot.eg_mode)
            {

                case EnvelopeMode.ATTACK:
                    slot.eg_phase += slot.eg_dphase;
                    if ((EG_DP_WIDTH & slot.eg_phase) != 0)
                    {
                        egout = 0;
                        slot.eg_phase = 0;
                        slot.eg_mode = (int)EnvelopeMode.DECAY;
                        UPDATE_EG(slot);
                    }
                    else
                    {
                        egout = AR_ADJUST_TABLE[HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS)];
                    }
                    break;

                case EnvelopeMode.DECAY:
                    slot.eg_phase += slot.eg_dphase;
                    egout = (uint)HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS);
                    if (slot.eg_phase >= SL[slot.patch.SL])
                    {
                        if (slot.patch.EG != 0)
                        {
                            slot.eg_phase = SL[slot.patch.SL];
                            slot.eg_mode = (int)EnvelopeMode.SUSHOLD;
                            UPDATE_EG(slot);
                        }
                        else
                        {
                            slot.eg_phase = SL[slot.patch.SL];
                            slot.eg_mode = (int)EnvelopeMode.SUSTINE;
                            UPDATE_EG(slot);
                        }
                        egout = (uint)HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS);
                    }
                    break;

                case EnvelopeMode.SUSHOLD:
                    egout = (uint)HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS);
                    if (slot.patch.EG == 0)
                    {
                        slot.eg_mode = (int)EnvelopeMode.SUSTINE;
                        UPDATE_EG(slot);
                    }
                    break;

                case EnvelopeMode.SUSTINE:
                case EnvelopeMode.RELEASE:
                    slot.eg_phase += slot.eg_dphase;
                    egout = (uint)HIGHBITS((int)slot.eg_phase, EG_DP_BITS - EG_BITS);
                    if (egout >= (1 << EG_BITS))
                    {
                        slot.eg_mode = (int)EnvelopeMode.FINISH;
                        egout = (1 << EG_BITS) - 1;
                    }
                    break;

                case EnvelopeMode.FINISH:
                    egout = (1 << EG_BITS) - 1;
                    break;

                default:
                    egout = (1 << EG_BITS) - 1;
                    break;
            }

            if (slot.patch.AM != 0) egout = (uint)(EG2DB((int)(egout + slot.tll)) + (slot.plfo_am));
            else egout = (uint)EG2DB((int)(egout + slot.tll));

            if (egout >= DB_MUTE) egout = DB_MUTE - 1;
            return egout;
        }

        static int calc_slot_car(OPLL_SLOT slot, int fm)
        {
            slot.egout = calc_envelope(slot);
            slot.pgout = calc_phase(slot);
            if (slot.egout >= (DB_MUTE - 1)) return 0;

            return DB2LIN_TABLE[slot.sintbl[(slot.pgout + wave2_8pi(fm)) & (PG_WIDTH - 1)] + slot.egout];
        }

        static int calc_slot_mod(OPLL_SLOT slot)
        {
            int fm;

            slot.output[1] = slot.output[0];
            slot.egout = calc_envelope(slot);
            slot.pgout = calc_phase(slot);

            if (slot.egout >= (DB_MUTE - 1))
            {
                slot.output[0] = 0;
            }
            else if (slot.patch.FB != 0)
            {
                fm = (wave2_4pi(slot.feedback) >> (int)(7 - slot.patch.FB));
                slot.output[0] = DB2LIN_TABLE[slot.sintbl[(slot.pgout + fm) & (PG_WIDTH - 1)] + slot.egout];
            }
            else
            {
                slot.output[0] = DB2LIN_TABLE[slot.sintbl[slot.pgout] + slot.egout];
            }

            slot.feedback = (slot.output[1] + slot.output[0]) >> 1;

            return slot.feedback;
        }

        static int calc_slot_tom(OPLL_SLOT slot)
        {
            slot.egout = calc_envelope(slot);
            slot.pgout = calc_phase(slot);
            if (slot.egout >= (DB_MUTE - 1)) return 0;

            return DB2LIN_TABLE[slot.sintbl[slot.pgout] + slot.egout];
        }

        /* calc SNARE slot */
        static int calc_slot_snare(OPLL_SLOT slot, uint whitenoise)
        {
            slot.egout = calc_envelope(slot);
            slot.pgout = calc_phase(slot);
            if (slot.egout >= (DB_MUTE - 1)) return 0;

            if (whitenoise != 0)
                return DB2LIN_TABLE[snaretable[slot.pgout] + slot.egout] + DB2LIN_TABLE[slot.egout + 6];
            else
                return DB2LIN_TABLE[snaretable[slot.pgout] + slot.egout];
        }

        static int calc_slot_cym(OPLL_SLOT slot, int a, int b, int c)
        {
            slot.egout = calc_envelope(slot);
            if (slot.egout >= (DB_MUTE - 1)) return 0;

            return DB2LIN_TABLE[slot.egout + a]
              + ((DB2LIN_TABLE[slot.egout + b] + DB2LIN_TABLE[slot.egout + c]) >> 2);
        }

        static int calc_slot_hat(OPLL_SLOT slot, int a, int b, int c, uint whitenoise)
        {
            slot.egout = calc_envelope(slot);
            if (slot.egout >= (DB_MUTE - 1)) return 0;

            if (whitenoise != 0)
            {
                return DB2LIN_TABLE[slot.egout + a]
                  + ((DB2LIN_TABLE[slot.egout + b] + DB2LIN_TABLE[slot.egout + c]) >> 2);
            }
            else
            {
                return 0;
            }
        }

        public static short OPLL_calc(OPLL opll)
        {
            int inst = 0, perc = 0, @out = 0;
            int rythmC = 0, rythmH = 0;
            int i;

            update_ampm(opll);
            update_noise(opll);

            for (i = 0; i < 6; i++)
                if ((opll.mask & OPLL_MASK_CH(i)) == 0 && (opll.CAR(i).eg_mode != (int)EnvelopeMode.FINISH))
                    inst += calc_slot_car(opll.CAR(i), calc_slot_mod(opll.MOD(i)));

            if (opll.rythm_mode == 0)
            {
                for (i = 6; i < 9; i++)
                    if ((opll.mask & OPLL_MASK_CH(i)) == 0 && (opll.CAR(i).eg_mode != (int)EnvelopeMode.FINISH))
                        inst += calc_slot_car(opll.CAR(i), calc_slot_mod(opll.MOD(i)));
            }
            else
            {
                opll.MOD(7).pgout = calc_phase(opll.MOD(7));
                opll.CAR(8).pgout = calc_phase(opll.CAR(8));
                if (opll.MOD(7).phase < 256) rythmH = (int)DB_NEG(12.0); else rythmH = DB_MUTE - 1;
                if (opll.CAR(8).phase < 256) rythmC = (int)DB_NEG(12.0); else rythmC = DB_MUTE - 1;

                if ((opll.mask & OPLL_MASK_BD) == 0 && (opll.CAR(6).eg_mode != (int)EnvelopeMode.FINISH))
                    perc += calc_slot_car(opll.CAR(6), calc_slot_mod(opll.MOD(6)));

                if ((opll.mask & OPLL_MASK_HH) == 0 && (opll.MOD(7).eg_mode != (int)EnvelopeMode.FINISH))
                    perc += calc_slot_hat(opll.MOD(7), (int)opll.noiseA, (int)opll.noiseB, rythmH, opll.whitenoise);

                if ((opll.mask & OPLL_MASK_SD) == 0 && (opll.CAR(7).eg_mode != (int)EnvelopeMode.FINISH))
                    perc += calc_slot_snare(opll.CAR(7), opll.whitenoise);

                if ((opll.mask & OPLL_MASK_TOM) == 0 && (opll.MOD(8).eg_mode != (int)EnvelopeMode.FINISH))
                    perc += calc_slot_tom(opll.MOD(8));

                if ((opll.mask & OPLL_MASK_CYM) == 0 && (opll.CAR(8).eg_mode != (int)EnvelopeMode.FINISH))
                    perc += calc_slot_cym(opll.CAR(8), (int)opll.noiseA, (int)opll.noiseB, rythmC);
            }

            inst = (inst >> (SLOT_AMP_BITS - 8));
            perc = (perc >> (SLOT_AMP_BITS - 9));

            @out = ((inst + perc) * opll.masterVolume) >> 2;

            if (@out > 32767) return 32767;
            if (@out < -32768) return -32768;

            return (short)@out;
        }

        static uint OPLL_setMask(OPLL opll, uint mask)
        {
            uint ret;

            if (opll != null)
            {
                ret = opll.mask;
                opll.mask = mask;
                return ret;
            }
            else return 0;
        }

        static uint OPLL_toggleMask(OPLL opll, uint mask)
        {
            uint ret;

            if (opll != null)
            {
                ret = opll.mask;
                opll.mask ^= mask;
                return ret;
            }
            else return 0;
        }

        public static void OPLL_writeReg(OPLL opll, uint reg, uint data)
        {

            int i, v, ch;

            data = data & 0xff;
            reg = reg & 0x3f;

            switch (reg)
            {
                case 0x00:
                    opll.patch[0].AM = (data >> 7) & 1;
                    opll.patch[0].PM = (data >> 6) & 1;
                    opll.patch[0].EG = (data >> 5) & 1;
                    opll.patch[0].KR = (data >> 4) & 1;
                    opll.patch[0].ML = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_PG(opll.MOD(i));
                            UPDATE_RKS(opll.MOD(i));
                            UPDATE_EG(opll.MOD(i));
                        }
                    }
                    break;

                case 0x01:
                    opll.patch[1].AM = (data >> 7) & 1;
                    opll.patch[1].PM = (data >> 6) & 1;
                    opll.patch[1].EG = (data >> 5) & 1;
                    opll.patch[1].KR = (data >> 4) & 1;
                    opll.patch[1].ML = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_PG(opll.CAR(i));
                            UPDATE_RKS(opll.CAR(i));
                            UPDATE_EG(opll.CAR(i));
                        }
                    }
                    break;

                case 0x02:
                    opll.patch[0].KL = (data >> 6) & 3;
                    opll.patch[0].TL = (data) & 63;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_TLL(opll.MOD(i));
                        }
                    }
                    break;

                case 0x03:
                    opll.patch[1].KL = (data >> 6) & 3;
                    opll.patch[1].WF = (data >> 4) & 1;
                    opll.patch[0].WF = (data >> 3) & 1;
                    opll.patch[0].FB = (data) & 7;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_WF(opll.MOD(i));
                            UPDATE_WF(opll.CAR(i));
                        }
                    }
                    break;

                case 0x04:
                    opll.patch[0].AR = (data >> 4) & 15;
                    opll.patch[0].DR = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_EG(opll.MOD(i));
                        }
                    }
                    break;

                case 0x05:
                    opll.patch[1].AR = (data >> 4) & 15;
                    opll.patch[1].DR = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_EG(opll.CAR(i));
                        }
                    }
                    break;

                case 0x06:
                    opll.patch[0].SL = (data >> 4) & 15;
                    opll.patch[0].RR = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_EG(opll.MOD(i));
                        }
                    }
                    break;

                case 0x07:
                    opll.patch[1].SL = (data >> 4) & 15;
                    opll.patch[1].RR = (data) & 15;
                    for (i = 0; i < 9; i++)
                    {
                        if (opll.ch[i].patch_number == 0)
                        {
                            UPDATE_EG(opll.CAR(i));
                        }
                    }
                    break;

                case 0x0e:

                    if (opll.rythm_mode != 0)
                    {
                        opll.slot_on_flag[SLOT_BD1] = (opll.reg[0x0e] & 0x10) | (opll.reg[0x26] & 0x10);
                        opll.slot_on_flag[SLOT_BD2] = (opll.reg[0x0e] & 0x10) | (opll.reg[0x26] & 0x10);
                        opll.slot_on_flag[SLOT_SD] = (opll.reg[0x0e] & 0x08) | (opll.reg[0x27] & 0x10);
                        opll.slot_on_flag[SLOT_HH] = (opll.reg[0x0e] & 0x01) | (opll.reg[0x27] & 0x10);
                        opll.slot_on_flag[SLOT_TOM] = (opll.reg[0x0e] & 0x04) | (opll.reg[0x28] & 0x10);
                        opll.slot_on_flag[SLOT_CYM] = (opll.reg[0x0e] & 0x02) | (opll.reg[0x28] & 0x10);
                    }
                    else
                    {
                        opll.slot_on_flag[SLOT_BD1] = (opll.reg[0x26] & 0x10);
                        opll.slot_on_flag[SLOT_BD2] = (opll.reg[0x26] & 0x10);
                        opll.slot_on_flag[SLOT_SD] = (opll.reg[0x27] & 0x10);
                        opll.slot_on_flag[SLOT_HH] = (opll.reg[0x27] & 0x10);
                        opll.slot_on_flag[SLOT_TOM] = (opll.reg[0x28] & 0x10);
                        opll.slot_on_flag[SLOT_CYM] = (opll.reg[0x28] & 0x10);
                    }

                    if ((((data >> 5) & 1) ^ (opll.rythm_mode)) != 0)
                    {
                        setRythmMode(opll, (int)(data & 32) >> 5);
                    }

                    if (opll.rythm_mode != 0)
                    {
                        if ((data & 0x10) != 0) keyOn_BD(opll); else keyOff_BD(opll);
                        if ((data & 0x8) != 0) keyOn_SD(opll); else keyOff_SD(opll);
                        if ((data & 0x4) != 0) keyOn_TOM(opll); else keyOff_TOM(opll);
                        if ((data & 0x2) != 0) keyOn_CYM(opll); else keyOff_CYM(opll);
                        if ((data & 0x1) != 0) keyOn_HH(opll); else keyOff_HH(opll);
                    }

                    UPDATE_ALL(opll.MOD(6));
                    UPDATE_ALL(opll.CAR(6));
                    UPDATE_ALL(opll.MOD(7));
                    UPDATE_ALL(opll.CAR(7));
                    UPDATE_ALL(opll.MOD(8));
                    UPDATE_ALL(opll.CAR(8));
                    break;

                case 0x0f:
                    break;

                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                    ch = (int)(reg - 0x10);
                    setFnumber(opll, ch, (int)(data + ((opll.reg[0x20 + ch] & 1) << 8)));
                    UPDATE_ALL(opll.MOD(ch));
                    UPDATE_ALL(opll.CAR(ch));
                    switch (reg)
                    {
                        case 0x17:
                            opll.noiseA_dphase = (uint)((data + ((opll.reg[0x27] & 1) << 8)) << ((opll.reg[0x27] >> 1) & 7));
                            break;
                        case 0x18:
                            opll.noiseB_dphase = (uint)((data + ((opll.reg[0x28] & 1) << 8)) << ((opll.reg[0x28] >> 1) & 7));
                            break;
                        default:
                            break;
                    }
                    break;

                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:

                    ch = (int)(reg - 0x20);
                    setFnumber(opll, ch, (int)(((data & 1) << 8) + opll.reg[0x10 + ch]));
                    setBlock(opll, ch, (int)((data >> 1) & 7));
                    opll.slot_on_flag[ch * 2] = opll.slot_on_flag[ch * 2 + 1] = (opll.reg[reg]) & 0x10;

                    if (opll.rythm_mode != 0)
                    {
                        switch (reg)
                        {
                            case 0x26:
                                opll.slot_on_flag[SLOT_BD1] |= (opll.reg[0x0e]) & 0x10;
                                opll.slot_on_flag[SLOT_BD2] |= (opll.reg[0x0e]) & 0x10;
                                break;

                            case 0x27:
                                opll.noiseA_dphase = (uint)((int)((data & 1) << 8 + opll.reg[0x17]) << (int)((data >> 1) & 7));
                                opll.slot_on_flag[SLOT_SD] |= (opll.reg[0x0e]) & 0x08;
                                opll.slot_on_flag[SLOT_HH] |= (opll.reg[0x0e]) & 0x01;
                                break;

                            case 0x28:
                                opll.noiseB_dphase = (uint)((int)((data & 1) << 8) + opll.reg[0x18]) << (int)((data >> 1) & 7);
                                opll.slot_on_flag[SLOT_TOM] |= (opll.reg[0x0e]) & 0x04;
                                opll.slot_on_flag[SLOT_CYM] |= (opll.reg[0x0e]) & 0x02;
                                break;

                            default:
                                break;
                        }
                    }

                    if (((opll.reg[reg] ^ data) & 0x20) != 0) setSustine(opll, ch, (int)((data >> 5) & 1));
                    if ((data & 0x10) != 0) keyOn(opll, ch); else keyOff(opll, ch);
                    UPDATE_ALL(opll.MOD(ch));
                    UPDATE_ALL(opll.CAR(ch));
                    break;

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x38:
                    i = (int)((data >> 4) & 15);
                    v = (int)(data & 15);
                    if ((opll.rythm_mode) != 0 && (reg >= 0x36))
                    {
                        switch (reg)
                        {
                            case 0x37:
                                setSlotVolume(opll.MOD(7), i << 2);
                                break;
                            case 0x38:
                                setSlotVolume(opll.MOD(8), i << 2);
                                break;
                        }
                    }
                    else
                    {
                        setPatch(opll, (int)(reg - 0x30), i);
                    }

                    setVolume(opll, (int)(reg - 0x30), v << 2);
                    UPDATE_ALL(opll.MOD((int)(reg - 0x30)));
                    UPDATE_ALL(opll.CAR((int)(reg - 0x30)));
                    break;

                default:
                    break;

            }

            opll.reg[reg] = (byte)data;
        }

        static void OPLL_writeIO(OPLL opll, uint adr, uint val)
        {
            adr &= 0xff;
            if (adr == 0x7C) opll.adr = val;
            else if (adr == 0x7D) OPLL_writeReg(opll, opll.adr, val);
        }
    }
}
