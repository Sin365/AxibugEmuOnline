
using Sony.Vita.Dialog;
using System;
using static MAME.Core.FM;
using static MAME.Core.YM2151;

namespace MAME.Core
{
    public unsafe class YM2610
    {
        public byte[] REGS;
        public FM.FM_OPN OPN;
        public byte addr_A1;
        public byte[] pcmbuf;
        public int pcm_size;
        public byte adpcmTL;
        public FM.ADPCM_CH[] adpcm;
        public byte[] adpcmreg;
        public byte adpcm_arrivedEndAddress;
        public static YM2610 F2610 = new YM2610();
        public static EmuTimer.emu_timer[] timer;
        public void timer_callback_0()
        {
            F2610.ym2610_timer_over(0);
        }
        public void timer_callback_1()
        {
            F2610.ym2610_timer_over(1);
        }
        public static void timer_handler(int c, int count, int clock)
        {
            if (count == 0)
            {
                EmuTimer.timer_enable(timer[c], false);
            }
            else
            {
                Atime period = Attotime.attotime_mul(new Atime(0, Attotime.ATTOSECONDS_PER_SECOND / clock), (uint)count);//8000000
                if (!EmuTimer.timer_enable(timer[c], true))
                {
                    EmuTimer.timer_adjust_periodic(timer[c], period, Attotime.ATTOTIME_NEVER);
                }
            }
        }
        public static void ym2610_update_request()
        {
            Sound.ym2610stream.stream_update();
        }
        //public void ADPCMA_calc_chan(int c)
        //{
        //    uint step;
        //    byte data;
        //    int i;
        //    adpcm[c].now_step += adpcm[c].step;
        //    if (adpcm[c].now_step >= (1 << 16))
        //    {
        //        step = adpcm[c].now_step >> 16;
        //        adpcm[c].now_step &= (1 << 16) - 1;
        //        for (i = 0; i < step; i++)
        //        {
        //            if ((adpcm[c].now_addr & ((1 << 21) - 1)) == ((adpcm[c].end << 1) & ((1 << 21) - 1)))
        //            {
        //                adpcm[c].flag = 0;
        //                adpcm_arrivedEndAddress |= adpcm[c].flagMask;
        //                return;
        //            }
        //            if ((adpcm[c].now_addr & 1) != 0)
        //            {
        //                data = (byte)(adpcm[c].now_data & 0x0f);
        //            }
        //            else
        //            {
        //                adpcm[c].now_data = FM.ymsndrom[adpcm[c].now_addr >> 1];
        //                data = (byte)((adpcm[c].now_data >> 4) & 0x0f);
        //            }
        //            adpcm[c].now_addr++;
        //            adpcm[c].adpcm_acc += FM.jedi_table[adpcm[c].adpcm_step + data];
        //            if ((adpcm[c].adpcm_acc & ~0x7ff) != 0)
        //            {
        //                adpcm[c].adpcm_acc |= ~0xfff;
        //            }
        //            else
        //            {
        //                adpcm[c].adpcm_acc &= 0xfff;
        //            }
        //            adpcm[c].adpcm_step += FM.step_inc[data & 7];
        //            adpcm[c].adpcm_step = FM.Limit(adpcm[c].adpcm_step, 48 * 16, 0);
        //        }
        //        adpcm[c].adpcm_out = ((adpcm[c].adpcm_acc * adpcm[c].vol_mul) >> adpcm[c].vol_shift) & ~3;
        //    }
        //    FM.out_adpcm[FM.ipan[c]] += adpcm[c].adpcm_out;
        //}

        //手动内联
        public void ADPCMA_calc_chan(int c)
        {
            uint step;
            byte data;
            int i;
            fixed (FM.ADPCM_CH* adpcm_c = &adpcm[c])
            {
                adpcm_c->now_step += adpcm_c->step;
                if (adpcm_c->now_step >= (1 << 16))
                {
                    step = adpcm_c->now_step >> 16;
                    adpcm_c->now_step &= (1 << 16) - 1;
                    for (i = 0; i < step; i++)
                    {
                        if ((adpcm_c->now_addr & ((1 << 21) - 1)) == ((adpcm_c->end << 1) & ((1 << 21) - 1)))
                        {
                            adpcm_c->flag = 0;
                            adpcm_arrivedEndAddress |= adpcm_c->flagMask;
                            return;
                        }
                        if ((adpcm_c->now_addr & 1) != 0)
                        {
                            data = (byte)(adpcm_c->now_data & 0x0f);
                        }
                        else
                        {
                            adpcm_c->now_data = FM.ymsndrom[adpcm_c->now_addr >> 1];
                            data = (byte)((adpcm_c->now_data >> 4) & 0x0f);
                        }
                        adpcm_c->now_addr++;
                        adpcm_c->adpcm_acc += FM.jedi_table[adpcm_c->adpcm_step + data];
                        if ((adpcm_c->adpcm_acc & ~0x7ff) != 0)
                        {
                            adpcm_c->adpcm_acc |= ~0xfff;
                        }
                        else
                        {
                            adpcm_c->adpcm_acc &= 0xfff;
                        }
                        adpcm_c->adpcm_step += FM.step_inc[data & 7];
                        //adpcm_c->adpcm_step = FM.Limit(adpcm_c->adpcm_step, 48 * 16, 0);//(int val, int max, int min)
                        int val = adpcm_c->adpcm_step;
                        const int max = 48 * 16;
                        adpcm_c->adpcm_step = Math.Min(max, adpcm_c->adpcm_step);
                        adpcm_c->adpcm_step = Math.Max(0, adpcm_c->adpcm_step);
                    }
                    adpcm_c->adpcm_out = ((adpcm_c->adpcm_acc * adpcm_c->vol_mul) >> adpcm_c->vol_shift) & ~3;
                }
                FM.out_adpcm[FM.ipan[c]] += adpcm_c->adpcm_out;
            }
        }


        static void ReInit_timer()
        {
            if (timer != null)
            {
                if (timer[0] != null) EmuTimer.emu_timer.SetNull(ref timer[0]);
                if (timer[1] != null) EmuTimer.emu_timer.SetNull(ref timer[1]);
            }
            else
            {
                timer = new EmuTimer.emu_timer[2];
            }
        }

        public static void ym2610_start(int clock)
        {
            F2610 = new YM2610();
            F2610.OPN = new FM.FM_OPN();
            AY8910.ay8910_interface generic_ay8910 = new AY8910.ay8910_interface();
            generic_ay8910.flags = 3;
            generic_ay8910.res_load = new int[3] { 1000, 1000, 1000 };
            generic_ay8910.portAread = null;
            generic_ay8910.portBread = null;
            generic_ay8910.portAwrite = null;
            generic_ay8910.portBwrite = null;
            int rate = clock / 72;
            AY8910.ay8910_start_ym(17, 0, clock, generic_ay8910);
            ReInit_timer();
            //timer = new EmuTimer.emu_timer[2];
            //timer[0] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM2610_F2610_timer_callback_0, false);
            EmuTimer.timer_alloc_common(ref timer[0], EmuTimer.TIME_ACT.YM2610_F2610_timer_callback_0, false);
            //timer[1] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM2610_F2610_timer_callback_1, false);
            EmuTimer.timer_alloc_common(ref timer[1], EmuTimer.TIME_ACT.YM2610_F2610_timer_callback_1, false);
            ym2610_init(clock, rate);
        }
        public static void ym2610_init(int clock, int rate)
        {
            FM.FM_init();
            F2610.REGS = new byte[512];
            F2610.adpcmreg = new byte[0x30];
            F2610.adpcm = new FM.ADPCM_CH[6];
            F2610.OPN = new FM.FM_OPN();
            F2610.OPN.type = FM.TYPE_YM2610;
            F2610.OPN.ST.clock = clock;
            F2610.OPN.ST.rate = rate;
            F2610.OPN.ST.timer_handler = YM2610.timer_handler;
            switch (Machine.sBoard)
            {
                case "Taito B":
                    switch (Machine.sName)
                    {
                        case "pbobble":
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            F2610.OPN.ST.IRQ_Handler = Taitob.irqhandler;
                            F2610.OPN.ST.SSG.set_clock = AY8910.AA8910[0].ay8910_set_clock_ym;
                            F2610.OPN.ST.SSG.write = AY8910.AA8910[0].ay8910_write_ym;
                            F2610.OPN.ST.SSG.read = AY8910.AA8910[0].ay8910_read_ym;
                            F2610.OPN.ST.SSG.reset = AY8910.AA8910[0].ay8910_reset_ym;
                            break;
                    }
                    break;
                case "Neo Geo":
                    F2610.OPN.ST.IRQ_Handler = Neogeo.audio_cpu_irq;
                    F2610.OPN.ST.SSG.set_clock = AY8910.AA8910[0].ay8910_set_clock_ym;
                    F2610.OPN.ST.SSG.write = AY8910.AA8910[0].ay8910_write_ym;
                    F2610.OPN.ST.SSG.read = AY8910.AA8910[0].ay8910_read_ym;
                    F2610.OPN.ST.SSG.reset = AY8910.AA8910[0].ay8910_reset_ym;
                    break;
            }
            F2610.pcmbuf = FM.ymsndrom;
            if (F2610.pcmbuf == null)
            {
                F2610.pcm_size = 0;
            }
            else
            {
                F2610.pcm_size = F2610.pcmbuf.Length;
            }
            YMDeltat.DELTAT.reg = new byte[16];
            if (YMDeltat.ymsnddeltatrom == null)
            {
                YMDeltat.ymsnddeltatrom = FM.ymsndrom;
            }
            if (YMDeltat.ymsnddeltatrom == null)
            {
                YMDeltat.DELTAT.memory_size = 0;
            }
            else
            {
                YMDeltat.DELTAT.memory_size = YMDeltat.ymsnddeltatrom.Length;
            }
            YMDeltat.DELTAT.status_set_handler = F2610.YM2610_deltat_status_set;
            YMDeltat.DELTAT.status_reset_handler = F2610.YM2610_deltat_status_reset;
            YMDeltat.DELTAT.status_change_EOS_bit = 0x80;
            F2610.ym2610_reset_chip();
            FM.Init_ADPCMATable();
        }
        public void ym2610_reset_chip()
        {
            int i;
            OPN.OPNSetPres(6 * 24, 6 * 24, 4 * 2);
            OPN.ST.SSG.reset();
            OPN.FM_IRQMASK_SET(0x03);
            OPN.FM_BUSY_CLEAR();
            OPN.OPNWriteMode(0x27, 0x30);
            OPN.eg_timer = 0;
            OPN.eg_cnt = 0;
            OPN.FM_STATUS_RESET(0xff);
            OPN.reset_channels(6);
            for (i = 0xb6; i >= 0xb4; i--)
            {
                OPN.OPNWriteReg(i, 0xc0);
                OPN.OPNWriteReg(i | 0x100, 0xc0);
            }
            for (i = 0xb2; i >= 0x30; i--)
            {
                OPN.OPNWriteReg(i, 0);
                OPN.OPNWriteReg(i | 0x100, 0);
            }
            for (i = 0x26; i >= 0x20; i--)
            {
                OPN.OPNWriteReg(i, 0);
            }
            for (i = 0; i < 6; i++)
            {
                adpcm[i].step = (uint)((float)(1 << 16) * ((float)F2610.OPN.ST.freqbase) / 3.0);
                adpcm[i].now_addr = 0;
                adpcm[i].now_step = 0;
                adpcm[i].start = 0;
                adpcm[i].end = 0;
                adpcm[i].vol_mul = 0;
                FM.ipan[i] = 3;
                adpcm[i].flagMask = (byte)(1 << i);
                adpcm[i].flag = 0;
                adpcm[i].adpcm_acc = 0;
                adpcm[i].adpcm_step = 0;
                adpcm[i].adpcm_out = 0;
            }
            adpcmTL = 0x3f;
            adpcm_arrivedEndAddress = 0;
            YMDeltat.DELTAT.freqbase = F2610.OPN.ST.freqbase;
            YMDeltat.DELTAT.output_pointer = 0;
            YMDeltat.DELTAT.portshift = 8;
            YMDeltat.DELTAT.output_range = 1 << 23;
            YMDeltat.YM_DELTAT_ADPCM_Reset(3, 1);
        }
        public void ym2610_write(int a, byte v)
        {
            int addr;
            int ch;
            v &= 0xff;
            switch (a & 3)
            {
                case 0:
                    OPN.ST.address = v;
                    addr_A1 = 0;
                    if (v < 16)
                    {
                        OPN.ST.SSG.write(0, v);
                    }
                    break;
                case 1:
                    if (addr_A1 != 0)
                        break;
                    addr = OPN.ST.address;
                    REGS[addr] = v;
                    switch (addr & 0xf0)
                    {
                        case 0x00:
                            OPN.ST.SSG.write(a, v);
                            break;
                        case 0x10:
                            ym2610_update_request();
                            switch (addr)
                            {
                                case 0x10:
                                case 0x11:
                                case 0x12:
                                case 0x13:
                                case 0x14:
                                case 0x15:
                                case 0x19:
                                case 0x1a:
                                case 0x1b:
                                    YMDeltat.YM_DELTAT_ADPCM_Write(addr - 0x10, v);
                                    break;
                                case 0x1c:
                                    {
                                        byte statusmask = (byte)~v;
                                        for (ch = 0; ch < 6; ch++)
                                        {
                                            adpcm[ch].flagMask = (byte)(statusmask & (1 << ch));
                                        }
                                        YMDeltat.DELTAT.status_change_EOS_bit = (byte)(statusmask & 0x80);
                                        adpcm_arrivedEndAddress &= statusmask;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 0x20:
                            ym2610_update_request();
                            OPN.OPNWriteMode(addr, v);
                            break;
                        default:
                            ym2610_update_request();
                            OPN.OPNWriteReg(addr, v);
                            break;
                    }
                    break;
                case 2:
                    OPN.ST.address = v;
                    addr_A1 = 1;
                    break;
                case 3:
                    if (addr_A1 != 1)
                        break;
                    ym2610_update_request();
                    addr = OPN.ST.address;
                    REGS[addr | 0x100] = v;
                    if (addr < 0x30)
                    {
                        FM_ADPCMAWrite(addr, v);
                    }
                    else
                    {
                        OPN.OPNWriteReg(addr | 0x100, v);
                    }
                    break;
            }
        }
        public byte ym2610_read(int a)
        {
            int addr = OPN.ST.address;
            byte ret = 0;
            switch (a & 3)
            {
                case 0:
                    ret = (byte)(OPN.FM_STATUS_FLAG() & 0x83);
                    break;
                case 1:
                    if (addr < 16)
                    {
                        ret = OPN.ST.SSG.read();
                    }
                    if (addr == 0xff)
                    {
                        ret = 0x01;
                    }
                    break;
                case 2:
                    ret = adpcm_arrivedEndAddress;
                    break;
                case 3:
                    ret = 0;
                    break;
            }
            return ret;
        }
        public int ym2610_timer_over(int c)
        {
            if (c != 0)
            {
                OPN.TimerBOver();
            }
            else
            {
                ym2610_update_request();
                OPN.TimerAOver();
                if ((OPN.ST.mode & 0x80) != 0)
                {
                    OPN.CSMKeyControll();
                }
            }
            return OPN.ST.irq;
        }
        private void FM_ADPCMAWrite(int r, byte v)
        {
            byte c = (byte)(r & 0x07);
            adpcmreg[r] = (byte)(v & 0xff);
            switch (r)
            {
                case 0x00:
                    if ((v & 0x80) == 0)
                    {
                        for (c = 0; c < 6; c++)
                        {
                            if (((v >> c) & 1) != 0)
                            {
                                adpcm[c].step = (uint)((float)(1 << 16) * ((float)F2610.OPN.ST.freqbase) / 3.0);
                                adpcm[c].now_addr = adpcm[c].start << 1;
                                adpcm[c].now_step = 0;
                                adpcm[c].adpcm_acc = 0;
                                adpcm[c].adpcm_step = 0;
                                adpcm[c].adpcm_out = 0;
                                adpcm[c].flag = 1;
                                if (pcmbuf == null)
                                {
                                    adpcm[c].flag = 0;
                                }
                                else
                                {
                                    if (adpcm[c].end >= pcm_size)
                                    {

                                    }
                                    if (adpcm[c].start >= pcm_size)
                                    {
                                        adpcm[c].flag = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (c = 0; c < 6; c++)
                        {
                            if (((v >> c) & 1) != 0)
                            {
                                adpcm[c].flag = 0;
                            }
                        }
                    }
                    break;
                case 0x01:
                    adpcmTL = (byte)((v & 0x3f) ^ 0x3f);
                    for (c = 0; c < 6; c++)
                    {
                        int volume = F2610.adpcmTL + adpcm[c].IL;
                        if (volume >= 63)
                        {
                            adpcm[c].vol_mul = 0;
                            adpcm[c].vol_shift = 0;
                        }
                        else
                        {
                            adpcm[c].vol_mul = (sbyte)(15 - (volume & 7));
                            adpcm[c].vol_shift = (byte)(1 + (volume >> 3));
                        }
                        adpcm[c].adpcm_out = ((adpcm[c].adpcm_acc * adpcm[c].vol_mul) >> adpcm[c].vol_shift) & ~3;
                    }
                    break;
                default:
                    c = (byte)(r & 0x07);
                    if (c >= 0x06)
                        return;
                    switch (r & 0x38)
                    {
                        case 0x08:
                            {
                                int volume;
                                adpcm[c].IL = (byte)((v & 0x1f) ^ 0x1f);
                                volume = adpcmTL + adpcm[c].IL;
                                if (volume >= 63)	/* This is correct, 63 = quiet */
                                {
                                    adpcm[c].vol_mul = 0;
                                    adpcm[c].vol_shift = 0;
                                }
                                else
                                {
                                    adpcm[c].vol_mul = (sbyte)(15 - (volume & 7));
                                    adpcm[c].vol_shift = (byte)(1 + (volume >> 3));
                                }
                                FM.ipan[c] = (v >> 6) & 0x03;
                                adpcm[c].adpcm_out = ((adpcm[c].adpcm_acc * adpcm[c].vol_mul) >> adpcm[c].vol_shift) & ~3;
                            }
                            break;
                        case 0x10:
                        case 0x18:
                            adpcm[c].start = (uint)((F2610.adpcmreg[0x18 + c] * 0x0100 | adpcmreg[0x10 + c]) << 8);
                            break;
                        case 0x20:
                        case 0x28:
                            adpcm[c].end = (uint)((F2610.adpcmreg[0x28 + c] * 0x0100 | adpcmreg[0x20 + c]) << 8);
                            adpcm[c].end += (1 << 8) - 1;
                            break;
                    }
                    break;
            }
        }
        public void ym2610_update_one(int offset, int length)
        {
            int i, j;
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 1);
            if ((OPN.ST.mode & 0xc0) != 0)
            {
                if (OPN.CH[2].SLOT[0].Incr == -1)
                {
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 0, (int)OPN.SL3.fc[1], F2610.OPN.SL3.kcode[1]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 2, (int)OPN.SL3.fc[2], F2610.OPN.SL3.kcode[2]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 1, (int)OPN.SL3.fc[0], F2610.OPN.SL3.kcode[0]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 3, (int)OPN.CH[2].fc, F2610.OPN.CH[2].kcode);
                }
            }
            else
            {
                OPN.refresh_fc_eg_chan(F2610.OPN.type, 2);
            }
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 4);
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 5);
            for (i = 0; i < length; i++)
            {
                OPN.advance_lfo();
                FM.out_adpcm[2] = FM.out_adpcm[1] = FM.out_adpcm[3] = 0;
                FM.out_delta[2] = FM.out_delta[1] = FM.out_delta[3] = 0;
                FM.out_fm[1] = 0;
                FM.out_fm[2] = 0;
                FM.out_fm[4] = 0;
                FM.out_fm[5] = 0;
                OPN.eg_timer += OPN.eg_timer_add;
                while (OPN.eg_timer >= OPN.eg_timer_overflow)
                {
                    OPN.eg_timer -= OPN.eg_timer_overflow;
                    OPN.eg_cnt++;
                    OPN.advance_eg_channel(1);
                    OPN.advance_eg_channel(2);
                    OPN.advance_eg_channel(4);
                    OPN.advance_eg_channel(5);
                }
                //OPN.chan_calc(1, 1);
                //OPN.chan_calc(2, 2);
                //OPN.chan_calc(4, 4);
                //OPN.chan_calc(5, 5);
                OPN.chan_calc(1,false);
                OPN.chan_calc(2,true);
                OPN.chan_calc(4,false);
                OPN.chan_calc(5,false);
                if ((YMDeltat.DELTAT.portstate & 0x80) != 0)
                {
                    YMDeltat.YM_DELTAT_ADPCM_CALC();
                }
                for (j = 0; j < 6; j++)
                {
                    if (adpcm[j].flag != 0)
                    {
                        ADPCMA_calc_chan(j);
                    }
                }
                int lt, rt;
                lt = FM.out_adpcm[2] + FM.out_adpcm[3];
                rt = FM.out_adpcm[1] + FM.out_adpcm[3];
                lt += (FM.out_delta[2] + FM.out_delta[3]) >> 9;
                rt += (FM.out_delta[1] + FM.out_delta[3]) >> 9;
                lt += (int)((FM.out_fm[1] >> 1) & OPN.pan[2]);
                rt += (int)((FM.out_fm[1] >> 1) & OPN.pan[3]);
                lt += (int)((FM.out_fm[2] >> 1) & OPN.pan[4]);
                rt += (int)((FM.out_fm[2] >> 1) & OPN.pan[5]);
                lt += (int)((FM.out_fm[4] >> 1) & OPN.pan[8]);
                rt += (int)((FM.out_fm[4] >> 1) & OPN.pan[9]);
                lt += (int)((FM.out_fm[5] >> 1) & OPN.pan[10]);
                rt += (int)((FM.out_fm[5] >> 1) & OPN.pan[11]);
                //lt = FM.Limit(lt, 32767, -32768);
                //rt = FM.Limit(rt, 32767, -32768);
                lt = Math.Min(lt, 32767);
                lt = Math.Max(lt, -32768);
                rt = Math.Min(rt, 32767);
                rt = Math.Max(rt, -32768);
                Sound.ym2610stream.streamoutput_Ptrs[0][offset + i] = lt;
                Sound.ym2610stream.streamoutput_Ptrs[1][offset + i] = rt;
            }
        }
        public void ym2610b_update_one(int offset, int length)
        {
            int i, j;
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 0);
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 1);
            if ((OPN.ST.mode & 0xc0) != 0)
            {
                if (OPN.CH[2].SLOT[0].Incr == -1)
                {
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 0, (int)OPN.SL3.fc[1], OPN.SL3.kcode[1]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 2, (int)OPN.SL3.fc[2], OPN.SL3.kcode[2]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 1, (int)OPN.SL3.fc[0], OPN.SL3.kcode[0]);
                    OPN.refresh_fc_eg_slot(F2610.OPN.type, 2, 3, (int)OPN.CH[2].fc, OPN.CH[2].kcode);
                }
            }
            else
            {
                OPN.refresh_fc_eg_chan(F2610.OPN.type, 2);
            }
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 3);
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 4);
            OPN.refresh_fc_eg_chan(F2610.OPN.type, 5);

            for (i = 0; i < length; i++)
            {
                OPN.advance_lfo();
                FM.out_adpcm[2] = FM.out_adpcm[1] = FM.out_adpcm[3] = 0;
                FM.out_delta[2] = FM.out_delta[1] = FM.out_delta[3] = 0;
                FM.out_fm[0] = 0;
                FM.out_fm[1] = 0;
                FM.out_fm[2] = 0;
                FM.out_fm[3] = 0;
                FM.out_fm[4] = 0;
                FM.out_fm[5] = 0;
                OPN.eg_timer += OPN.eg_timer_add;
                while (OPN.eg_timer >= OPN.eg_timer_overflow)
                {
                    OPN.eg_timer -= OPN.eg_timer_overflow;
                    OPN.eg_cnt++;
                    OPN.advance_eg_channel(0);
                    OPN.advance_eg_channel(1);
                    OPN.advance_eg_channel(2);
                    OPN.advance_eg_channel(3);
                    OPN.advance_eg_channel(4);
                    OPN.advance_eg_channel(5);
                }
                //OPN.chan_calc(0, 0);
                //OPN.chan_calc(1, 1);
                //OPN.chan_calc(2, 2);
                //OPN.chan_calc(3, 3);
                //OPN.chan_calc(4, 4);
                //OPN.chan_calc(5, 5);
                OPN.chan_calc(0, false);
                OPN.chan_calc(1, false);
                OPN.chan_calc(2, true);
                OPN.chan_calc(3, false);
                OPN.chan_calc(4, false);
                OPN.chan_calc(5, false);
                if ((YMDeltat.DELTAT.portstate & 0x80) != 0)
                {
                    YMDeltat.YM_DELTAT_ADPCM_CALC();
                }
                for (j = 0; j < 6; j++)
                {
                    if (adpcm[j].flag != 0)
                    {
                        ADPCMA_calc_chan(j);
                    }
                }
                int lt, rt;
                lt = FM.out_adpcm[2] + FM.out_adpcm[3];
                rt = FM.out_adpcm[1] + FM.out_adpcm[3];
                lt += (FM.out_delta[2] + FM.out_delta[3]) >> 9;
                rt += (FM.out_delta[1] + FM.out_delta[3]) >> 9;
                lt += (int)((FM.out_fm[0] >> 1) & OPN.pan[0]);
                rt += (int)((FM.out_fm[0] >> 1) & OPN.pan[1]);
                lt += (int)((FM.out_fm[1] >> 1) & OPN.pan[2]);
                rt += (int)((FM.out_fm[1] >> 1) & OPN.pan[3]);
                lt += (int)((FM.out_fm[2] >> 1) & OPN.pan[4]);
                rt += (int)((FM.out_fm[2] >> 1) & OPN.pan[5]);
                lt += (int)((FM.out_fm[3] >> 1) & OPN.pan[6]);
                rt += (int)((FM.out_fm[3] >> 1) & OPN.pan[7]);
                lt += (int)((FM.out_fm[4] >> 1) & OPN.pan[8]);
                rt += (int)((FM.out_fm[4] >> 1) & OPN.pan[9]);
                lt += (int)((FM.out_fm[5] >> 1) & OPN.pan[10]);
                rt += (int)((FM.out_fm[5] >> 1) & OPN.pan[11]);
                //lt = FM.Limit(lt, 32767, -32768);
                //rt = FM.Limit(rt, 32767, -32768);
                lt = Math.Min(lt, 32767);
                lt = Math.Max(lt, -32768);
                rt = Math.Min(rt, 32767);
                rt = Math.Max(rt, -32768);
                Sound.ym2610stream.streamoutput_Ptrs[0][offset + i] = lt;
                Sound.ym2610stream.streamoutput_Ptrs[1][offset + i] = rt;
            }
        }
        public void ym2610_postload()
        {
            byte r;
            for (r = 0; r < 16; r++)
            {
                OPN.ST.SSG.write(0, r);
                OPN.ST.SSG.write(1, REGS[r]);
            }
            for (r = 0x30; r < 0x9e; r++)
            {
                if ((r & 3) != 3)
                {
                    OPN.OPNWriteReg(r, F2610.REGS[r]);
                    OPN.OPNWriteReg(r | 0x100, F2610.REGS[r | 0x100]);
                }
            }
            for (r = 0xb0; r < 0xb6; r++)
            {
                if ((r & 3) != 3)
                {
                    OPN.OPNWriteReg(r, F2610.REGS[r]);
                    OPN.OPNWriteReg(r | 0x100, F2610.REGS[r | 0x100]);
                }
            }
            FM_ADPCMAWrite(1, F2610.REGS[0x101]);
            for (r = 0; r < 6; r++)
            {
                FM_ADPCMAWrite(r + 0x08, REGS[r + 0x108]);
                FM_ADPCMAWrite(r + 0x10, REGS[r + 0x110]);
                FM_ADPCMAWrite(r + 0x18, REGS[r + 0x118]);
                FM_ADPCMAWrite(r + 0x20, REGS[r + 0x120]);
                FM_ADPCMAWrite(r + 0x28, REGS[r + 0x128]);
            }
            YMDeltat.YM_DELTAT_postload(REGS, 0x010);
        }
        private void YM2610_deltat_status_set(byte changebits)
        {
            adpcm_arrivedEndAddress |= changebits;
        }
        private void YM2610_deltat_status_reset(byte changebits)
        {
            adpcm_arrivedEndAddress &= (byte)(~changebits);
        }
        public void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, j;
            writer.Write(REGS, 0, 512);
            writer.Write(addr_A1);
            writer.Write(adpcmTL);
            writer.Write(adpcmreg, 0, 0x30);
            writer.Write(adpcm_arrivedEndAddress);
            writer.Write(OPN.ST.freqbase);
            writer.Write(OPN.ST.timer_prescaler);
            writer.Write(OPN.ST.busy_expiry_time.seconds);
            writer.Write(OPN.ST.busy_expiry_time.attoseconds);
            writer.Write(OPN.ST.address);
            writer.Write(OPN.ST.irq);
            writer.Write(OPN.ST.irqmask);
            writer.Write(OPN.ST.status);
            writer.Write(OPN.ST.mode);
            writer.Write(OPN.ST.prescaler_sel);
            writer.Write(OPN.ST.fn_h);
            writer.Write(OPN.ST.TA);
            writer.Write(OPN.ST.TAC);
            writer.Write(OPN.ST.TB);
            writer.Write(OPN.ST.TBC);
            for (i = 0; i < 12; i++)
            {
                writer.Write(OPN.pan[i]);
            }
            writer.Write(OPN.eg_cnt);
            writer.Write(OPN.eg_timer);
            writer.Write(OPN.eg_timer_add);
            writer.Write(OPN.eg_timer_overflow);
            writer.Write(OPN.lfo_cnt);
            writer.Write(OPN.lfo_inc);
            for (i = 0; i < 8; i++)
            {
                writer.Write(OPN.lfo_freq[i]);
            }
            for (i = 0; i < 6; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    writer.Write(OPN.CH[i].SLOT[j].KSR);
                    writer.Write(OPN.CH[i].SLOT[j].ar);
                    writer.Write(OPN.CH[i].SLOT[j].d1r);
                    writer.Write(OPN.CH[i].SLOT[j].d2r);
                    writer.Write(OPN.CH[i].SLOT[j].rr);
                    writer.Write(OPN.CH[i].SLOT[j].ksr);
                    writer.Write(OPN.CH[i].SLOT[j].mul);
                    writer.Write(OPN.CH[i].SLOT[j].phase);
                    writer.Write(OPN.CH[i].SLOT[j].Incr);
                    writer.Write(OPN.CH[i].SLOT[j].state);
                    writer.Write(OPN.CH[i].SLOT[j].tl);
                    writer.Write(OPN.CH[i].SLOT[j].volume);
                    writer.Write(OPN.CH[i].SLOT[j].sl);
                    writer.Write(OPN.CH[i].SLOT[j].vol_out);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sh_ar);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sel_ar);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sh_d1r);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sel_d1r);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sh_d2r);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sel_d2r);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sh_rr);
                    writer.Write(OPN.CH[i].SLOT[j].eg_sel_rr);
                    writer.Write(OPN.CH[i].SLOT[j].ssg);
                    writer.Write(OPN.CH[i].SLOT[j].ssgn);
                    writer.Write(OPN.CH[i].SLOT[j].key);
                    writer.Write(OPN.CH[i].SLOT[j].AMmask);
                }
            }
            for (i = 0; i < 6; i++)
            {
                writer.Write(adpcm[i].flag);
                writer.Write(adpcm[i].flagMask);
                writer.Write(adpcm[i].now_data);
                writer.Write(adpcm[i].now_addr);
                writer.Write(adpcm[i].now_step);
                writer.Write(adpcm[i].step);
                writer.Write(adpcm[i].start);
                writer.Write(adpcm[i].end);
                writer.Write(adpcm[i].IL);
                writer.Write(adpcm[i].adpcm_acc);
                writer.Write(adpcm[i].adpcm_step);
                writer.Write(adpcm[i].adpcm_out);
                writer.Write(adpcm[i].vol_mul);
                writer.Write(adpcm[i].vol_shift);
            }
            for (i = 0; i < 6; i++)
            {
                writer.Write(OPN.CH[i].ALGO);
                writer.Write(OPN.CH[i].FB);
                writer.Write(OPN.CH[i].op1_out0);
                writer.Write(OPN.CH[i].op1_out1);
                writer.Write(OPN.CH[i].mem_value);
                writer.Write(OPN.CH[i].pms);
                writer.Write(OPN.CH[i].ams);
                writer.Write(OPN.CH[i].fc);
                writer.Write(OPN.CH[i].kcode);
                writer.Write(OPN.CH[i].block_fnum);
            }
            for (i = 0; i < 3; i++)
            {
                writer.Write(OPN.SL3.fc[i]);
            }
            writer.Write(OPN.SL3.fn_h);
            writer.Write(OPN.SL3.kcode, 0, 3);
            for (i = 0; i < 3; i++)
            {
                writer.Write(OPN.SL3.block_fnum[i]);
            }
            writer.Write(YMDeltat.DELTAT.portstate);
            writer.Write(YMDeltat.DELTAT.now_addr);
            writer.Write(YMDeltat.DELTAT.now_step);
            writer.Write(YMDeltat.DELTAT.acc);
            writer.Write(YMDeltat.DELTAT.prev_acc);
            writer.Write(YMDeltat.DELTAT.adpcmd);
            writer.Write(YMDeltat.DELTAT.adpcml);
        }
        public void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, j;
            REGS = reader.ReadBytes(512);
            addr_A1 = reader.ReadByte();
            adpcmTL = reader.ReadByte();
            adpcmreg = reader.ReadBytes(0x30);
            adpcm_arrivedEndAddress = reader.ReadByte();
            OPN.ST.freqbase = reader.ReadDouble();
            OPN.ST.timer_prescaler = reader.ReadInt32();
            OPN.ST.busy_expiry_time.seconds = reader.ReadInt32();
            OPN.ST.busy_expiry_time.attoseconds = reader.ReadInt64();
            OPN.ST.address = reader.ReadByte();
            OPN.ST.irq = reader.ReadByte();
            OPN.ST.irqmask = reader.ReadByte();
            OPN.ST.status = reader.ReadByte();
            OPN.ST.mode = reader.ReadByte();
            OPN.ST.prescaler_sel = reader.ReadByte();
            OPN.ST.fn_h = reader.ReadByte();
            OPN.ST.TA = reader.ReadInt32();
            OPN.ST.TAC = reader.ReadInt32();
            OPN.ST.TB = reader.ReadByte();
            OPN.ST.TBC = reader.ReadInt32();
            for (i = 0; i < 12; i++)
            {
                OPN.pan[i] = reader.ReadUInt32();
            }
            OPN.eg_cnt = reader.ReadUInt32();
            OPN.eg_timer = reader.ReadUInt32();
            OPN.eg_timer_add = reader.ReadUInt32();
            OPN.eg_timer_overflow = reader.ReadUInt32();
            OPN.lfo_cnt = reader.ReadInt32();
            OPN.lfo_inc = reader.ReadInt32();
            for (i = 0; i < 8; i++)
            {
                OPN.lfo_freq[i] = reader.ReadInt32();
            }
            for (i = 0; i < 6; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    OPN.CH[i].SLOT[j].KSR = reader.ReadByte();
                    OPN.CH[i].SLOT[j].ar = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].d1r = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].d2r = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].rr = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].ksr = reader.ReadByte();
                    OPN.CH[i].SLOT[j].mul = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].phase = reader.ReadUInt32();
                    OPN.CH[i].SLOT[j].Incr = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].state = reader.ReadByte();
                    OPN.CH[i].SLOT[j].tl = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].volume = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].sl = reader.ReadInt32();
                    OPN.CH[i].SLOT[j].vol_out = reader.ReadUInt32();
                    OPN.CH[i].SLOT[j].eg_sh_ar = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sel_ar = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sh_d1r = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sel_d1r = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sh_d2r = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sel_d2r = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sh_rr = reader.ReadByte();
                    OPN.CH[i].SLOT[j].eg_sel_rr = reader.ReadByte();
                    OPN.CH[i].SLOT[j].ssg = reader.ReadByte();
                    OPN.CH[i].SLOT[j].ssgn = reader.ReadByte();
                    OPN.CH[i].SLOT[j].key = reader.ReadUInt32();
                    OPN.CH[i].SLOT[j].AMmask = reader.ReadUInt32();
                }
            }
            for (i = 0; i < 6; i++)
            {
                adpcm[i].flag = reader.ReadByte();
                adpcm[i].flagMask = reader.ReadByte();
                adpcm[i].now_data = reader.ReadByte();
                adpcm[i].now_addr = reader.ReadUInt32();
                adpcm[i].now_step = reader.ReadUInt32();
                adpcm[i].step = reader.ReadUInt32();
                adpcm[i].start = reader.ReadUInt32();
                adpcm[i].end = reader.ReadUInt32();
                adpcm[i].IL = reader.ReadByte();
                adpcm[i].adpcm_acc = reader.ReadInt32();
                adpcm[i].adpcm_step = reader.ReadInt32();
                adpcm[i].adpcm_out = reader.ReadInt32();
                adpcm[i].vol_mul = reader.ReadSByte();
                adpcm[i].vol_shift = reader.ReadByte();
            }
            for (i = 0; i < 6; i++)
            {
                OPN.CH[i].ALGO = reader.ReadByte();
                OPN.CH[i].FB = reader.ReadByte();
                OPN.CH[i].op1_out0 = reader.ReadInt32();
                OPN.CH[i].op1_out1 = reader.ReadInt32();
                OPN.CH[i].mem_value = reader.ReadInt32();
                OPN.CH[i].pms = reader.ReadInt32();
                OPN.CH[i].ams = reader.ReadByte();
                OPN.CH[i].fc = reader.ReadUInt32();
                OPN.CH[i].kcode = reader.ReadByte();
                OPN.CH[i].block_fnum = reader.ReadUInt32();
            }
            for (i = 0; i < 3; i++)
            {
                OPN.SL3.fc[i] = reader.ReadUInt32();
            }
            OPN.SL3.fn_h = reader.ReadByte();
            OPN.SL3.kcode = reader.ReadBytes(3);
            for (i = 0; i < 3; i++)
            {
                OPN.SL3.block_fnum[i] = reader.ReadUInt32();
            }
            YMDeltat.DELTAT.portstate = reader.ReadByte();
            YMDeltat.DELTAT.now_addr = reader.ReadInt32();
            YMDeltat.DELTAT.now_step = reader.ReadInt32();
            YMDeltat.DELTAT.acc = reader.ReadInt32();
            YMDeltat.DELTAT.prev_acc = reader.ReadInt32();
            YMDeltat.DELTAT.adpcmd = reader.ReadInt32();
            YMDeltat.DELTAT.adpcml = reader.ReadInt32();
        }
    }
}
