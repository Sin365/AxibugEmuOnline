using System.IO;

namespace MAME.Core
{
    public unsafe class ICS2115
    {
        public static byte V_ON = 1, V_DONE = 2;
        public static voice_struct[] voice2;
        public static timer_struct[] timer;
        public static byte[] icsrom;
        public static short[] ulaw = new short[256];
        public static byte osc_select;
        public static byte reg_select;
        public static byte irq_enabled, irq_pending;
        public static bool irq_on;
        public struct timer_struct
        {
            public byte scale, preset;
            public EmuTimer.emu_timer timer;
            public long period;
        }
        public static void ics2115_w(int offset, byte data)
        {
            switch (offset)
            {
                case 1:
                    reg_select = data;
                    break;
                case 2:
                    reg_write(data, false);
                    break;
                case 3:
                    reg_write(data, true);
                    break;
                default:
                    break;
            }
        }
        public static void timer_cb_0()
        {
            irq_pending |= 1 << 0;
            recalc_irq();
        }
        public static void timer_cb_1()
        {
            irq_pending |= 1 << 1;
            recalc_irq();
        }

        public struct voice_struct
        {
            public ushort fc, addrh, addrl, strth, endh, volacc;
            public byte strtl, endl, saddr, pan, conf, ctl;
            public byte vstart, vend, vctl;
            public byte state;
        }
        public static void recalc_irq()
        {
            int i;
            bool irq = false;
            if ((irq_enabled & irq_pending) != 0)
                irq = true;
            for (i = 0; irq == false && i < 32; i++)
                if ((voice2[i].state & V_DONE) != 0)
                    irq = true;
            if (irq != irq_on)
            {
                irq_on = irq;
                PGM.sound_irq(irq ? (int)LineState.ASSERT_LINE : (int)LineState.CLEAR_LINE);
            }
        }
        public static void ics2115_update(int offset, int length)
        {
            int osc, i;
            bool irq_invalid = false;
            for (i = 0; i < length; i++)
            {
                Sound.ics2115stream.streamoutput_Ptrs[0][offset + i] = 0;
                Sound.ics2115stream.streamoutput_Ptrs[1][offset + i] = 0;
            }
            for (osc = 0; osc < 32; osc++)
            {
                if ((voice2[osc].state & V_ON) != 0)
                {
                    uint adr = (uint)((voice2[osc].addrh << 16) | voice2[osc].addrl);
                    uint end = (uint)((voice2[osc].endh << 16) | (voice2[osc].endl << 8));
                    uint loop = (uint)((voice2[osc].strth << 16) | (voice2[osc].strtl << 8));
                    uint badr = (uint)((voice2[osc].saddr << 20) & 0xffffff);
                    uint delta = (uint)(voice2[osc].fc << 2);
                    byte conf = voice2[osc].conf;
                    int vol = voice2[osc].volacc;
                    vol = (((vol & 0xff0) | 0x1000) << (vol >> 12)) >> 12;
                    for (i = 0; i < length; i++)
                    {
                        int v;
                        if ((badr | adr >> 12) >= icsrom.Length)
                        {
                            v = 0;
                        }
                        else
                        {
                            v = icsrom[badr | (adr >> 12)];
                        }
                        if ((conf & 1) != 0)
                            v = ulaw[v];
                        else
                            v = ((sbyte)v) << 6;
                        v = (v * vol) >> (16 + 5);
                        Sound.ics2115stream.streamoutput_Ptrs[0][offset + i] += v;
                        Sound.ics2115stream.streamoutput_Ptrs[1][offset + i] += v;
                        adr += delta;
                        if (adr >= end)
                        {
                            adr -= (end - loop);
                            voice2[osc].state &= (byte)(~V_ON);
                            voice2[osc].state |= V_DONE;
                            irq_invalid = true;
                            break;
                        }
                    }
                    voice2[osc].addrh = (ushort)(adr >> 16);
                    voice2[osc].addrl = (ushort)(adr);
                }
            }
            if (irq_invalid)
            {
                recalc_irq();
            }
        }
        public static void keyon()
        {
            voice2[osc_select].state |= V_ON;
        }
        public static void recalc_timer(int i)
        {
            long period = (long)(1000000000 * timer[i].scale * timer[i].preset / 33868800);
            if (period != 0)
                period = (long)(1000000000 / 62.8206);
            if (timer[i].period != period)
            {
                timer[i].period = period;
                if (period != 0)
                    EmuTimer.timer_adjust_periodic(timer[i].timer, Attotime.ATTOTIME_IN_NSEC(period), Attotime.ATTOTIME_IN_NSEC(period));
                else
                    EmuTimer.timer_adjust_periodic(timer[i].timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
            }
        }
        static void reg_write(byte data, bool msb)
        {
            switch (reg_select)
            {
                case 0x00: // [osc] Oscillator Configuration
                    if (msb)
                    {
                        voice2[osc_select].conf = data;
                    }
                    break;
                case 0x01: // [osc] Wavesample frequency
                    // freq = fc*33075/1024 in 32 voices mode, fc*44100/1024 in 24 voices mode
                    if (msb)
                        voice2[osc_select].fc = (ushort)((voice2[osc_select].fc & 0xff) | (data << 8));
                    else
                        voice2[osc_select].fc = (ushort)((voice2[osc_select].fc & 0xff00) | data);
                    break;
                case 0x02: // [osc] Wavesample loop start address 19-4
                    if (msb)
                        voice2[osc_select].strth = (ushort)((voice2[osc_select].strth & 0xff) | (data << 8));
                    else
                        voice2[osc_select].strth = (ushort)((voice2[osc_select].strth & 0xff00) | data);
                    break;
                case 0x03: // [osc] Wavesample loop start address 3-0.3-0
                    if (msb)
                    {
                        voice2[osc_select].strtl = data;
                    }
                    break;
                case 0x04: // [osc] Wavesample loop end address 19-4
                    if (msb)
                        voice2[osc_select].endh = (ushort)((voice2[osc_select].endh & 0xff) | (data << 8));
                    else
                        voice2[osc_select].endh = (ushort)((voice2[osc_select].endh & 0xff00) | data);
                    break;
                case 0x05: // [osc] Wavesample loop end address 3-0.3-0
                    if (msb)
                    {
                        voice2[osc_select].endl = data;
                    }
                    break;
                case 0x07: // [osc] Volume Start
                    if (msb)
                    {
                        voice2[osc_select].vstart = data;
                    }
                    break;
                case 0x08: // [osc] Volume End
                    if (msb)
                    {
                        voice2[osc_select].vend = data;
                    }
                    break;
                case 0x09: // [osc] Volume accumulator
                    if (msb)
                        voice2[osc_select].volacc = (ushort)((voice2[osc_select].volacc & 0xff) | (data << 8));
                    else
                        voice2[osc_select].volacc = (ushort)((voice2[osc_select].volacc & 0xff00) | data);
                    break;
                case 0x0a: // [osc] Wavesample address 19-4
                    if (msb)
                        voice2[osc_select].addrh = (ushort)((voice2[osc_select].addrh & 0xff) | (data << 8));
                    else
                        voice2[osc_select].addrh = (ushort)((voice2[osc_select].addrh & 0xff00) | data);
                    break;
                case 0x0b: // [osc] Wavesample address 3-0.8-0
                    if (msb)
                        voice2[osc_select].addrl = (ushort)((voice2[osc_select].addrl & 0xff) | (data << 8));
                    else
                        voice2[osc_select].addrl = (ushort)((voice2[osc_select].addrl & 0xff00) | data);
                    break;
                case 0x0c: // [osc] Pan
                    if (msb)
                    {
                        voice2[osc_select].pan = data;
                    }
                    break;
                case 0x0d: // [osc] Volume Enveloppe Control
                    if (msb)
                    {
                        voice2[osc_select].vctl = data;
                    }
                    break;
                case 0x10: // [osc] Oscillator Control
                    if (msb)
                    {
                        voice2[osc_select].ctl = data;
                        if (data == 0)
                            keyon();
                    }
                    break;
                case 0x11: // [osc] Wavesample static address 27-20
                    if (msb)
                    {
                        voice2[osc_select].saddr = data;
                    }
                    break;

                case 0x40: // Timer 1 Preset
                    if (!msb)
                    {
                        timer[0].preset = data;
                        recalc_timer(0);
                    }
                    break;
                case 0x41: // Timer 2 Preset
                    if (!msb)
                    {
                        timer[1].preset = data;
                        recalc_timer(1);
                    }
                    break;
                case 0x42: // Timer 1 Prescaler
                    if (!msb)
                    {
                        timer[0].scale = data;
                        recalc_timer(0);
                    }
                    break;
                case 0x43: // Timer 2 Prescaler
                    if (!msb)
                    {
                        timer[1].scale = data;
                        recalc_timer(1);
                    }
                    break;
                case 0x4a: // IRQ Enable
                    if (!msb)
                    {
                        irq_enabled = data;
                        recalc_irq();
                    }
                    break;

                case 0x4f: // Oscillator Address being Programmed
                    if (!msb)
                    {
                        osc_select = (byte)(data & 31);
                    }
                    break;
                default:
                    break;
            }
        }
        public static ushort reg_read()
        {
            switch (reg_select)
            {
                case 0x0d:
                    return 0x100;
                case 0x0f:
                    {
                        int osc;
                        byte res = 0xff;
                        for (osc = 0; osc < 32; osc++)
                            if ((voice2[osc].state & V_DONE) != 0)
                            {
                                voice2[osc].state &= (byte)(~V_DONE);
                                recalc_irq();
                                res = (byte)(0x40 | osc);
                                break;
                            }
                        return (ushort)(res << 8);
                    }
                case 0x40:
                    irq_pending &= unchecked((byte)(~(1 << 0)));
                    recalc_irq();
                    return timer[0].preset;
                case 0x41:
                    irq_pending &= unchecked((byte)(~(1 << 1)));
                    recalc_irq();
                    return timer[1].preset;
                case 0x43:
                    return (ushort)(irq_pending & 3);
                case 0x4a:
                    return irq_pending;
                case 0x4b:
                    return 0x80;
                case 0x4c:
                    return 0x01;
                default:
                    return 0;
            }
        }
        public static void ics2115_start()
        {
            int i;
            voice2 = new voice_struct[32];
            timer = new timer_struct[2];
            timer[0].timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.ICS2115_timer_cb_0, false);
            timer[1].timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.ICS2115_timer_cb_1, false);
            ulaw = new short[256];
            for (i = 0; i < 256; i++)
            {
                byte c = (byte)(~i);
                int v;
                v = ((c & 15) << 1) + 33;
                v <<= ((c & 0x70) >> 4);
                if ((c & 0x80) != 0)
                    v = 33 - v;
                else
                    v = v - 33;
                ulaw[i] = (short)v;
            }
        }
        public static byte ics2115_r(int offset)
        {
            byte ret = 0;
            switch (offset)
            {
                case 0:
                    if (irq_on)
                    {
                        int i;
                        ret |= 0x80;
                        if ((irq_enabled & irq_pending & 3) != 0)
                            ret |= 1;
                        for (i = 0; i < 32; i++)
                            if ((voice2[i].state & V_DONE) != 0)
                            {
                                ret |= 2;
                                break;
                            }
                    }
                    break;
                case 1:
                    ret = reg_select;
                    break;
                case 2:
                    ret = (byte)(reg_read());
                    break;
                case 3:
                    ret = (byte)(reg_read() >> 8);
                    break;
                default:
                    break;
            }
            return ret;
        }
        public static void ics2115_reset()
        {
            int i;
            irq_enabled = 0;
            irq_pending = 0;
            for (i = 0; i < 32; i++)
            {
                voice2[i].fc = 0;
                voice2[i].addrh = 0;
                voice2[i].addrl = 0;
                voice2[i].strth = 0;
                voice2[i].endh = 0;
                voice2[i].volacc = 0;
                voice2[i].strtl = 0;
                voice2[i].endl = 0;
                voice2[i].saddr = 0;
                voice2[i].pan = 0;
                voice2[i].conf = 0;
                voice2[i].ctl = 0;
                voice2[i].vstart = 0;
                voice2[i].vend = 0;
                voice2[i].vctl = 0;
                voice2[i].state = 0;
            }
            for (i = 0; i < 2; i++)
            {
                EmuTimer.timer_adjust_periodic(timer[i].timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
                timer[i].period = 0;
            }
            recalc_irq();
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 32; i++)
            {
                writer.Write(voice2[i].fc);
                writer.Write(voice2[i].addrh);
                writer.Write(voice2[i].addrl);
                writer.Write(voice2[i].strth);
                writer.Write(voice2[i].endh);
                writer.Write(voice2[i].volacc);
                writer.Write(voice2[i].strtl);
                writer.Write(voice2[i].endl);
                writer.Write(voice2[i].saddr);
                writer.Write(voice2[i].pan);
                writer.Write(voice2[i].conf);
                writer.Write(voice2[i].ctl);
                writer.Write(voice2[i].vstart);
                writer.Write(voice2[i].vend);
                writer.Write(voice2[i].vctl);
                writer.Write(voice2[i].state);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(timer[i].scale);
                writer.Write(timer[i].preset);
                writer.Write(timer[i].period);
            }
            writer.Write(osc_select);
            writer.Write(reg_select);
            writer.Write(irq_enabled);
            writer.Write(irq_pending);
            writer.Write(irq_on);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i;
            for (i = 0; i < 32; i++)
            {
                voice2[i].fc = reader.ReadUInt16();
                voice2[i].addrh = reader.ReadUInt16();
                voice2[i].addrl = reader.ReadUInt16();
                voice2[i].strth = reader.ReadUInt16();
                voice2[i].endh = reader.ReadUInt16();
                voice2[i].volacc = reader.ReadUInt16();
                voice2[i].strtl = reader.ReadByte();
                voice2[i].endl = reader.ReadByte();
                voice2[i].saddr = reader.ReadByte();
                voice2[i].pan = reader.ReadByte();
                voice2[i].conf = reader.ReadByte();
                voice2[i].ctl = reader.ReadByte();
                voice2[i].vstart = reader.ReadByte();
                voice2[i].vend = reader.ReadByte();
                voice2[i].vctl = reader.ReadByte();
                voice2[i].state = reader.ReadByte();
            }
            for (i = 0; i < 2; i++)
            {
                timer[i].scale = reader.ReadByte();
                timer[i].preset = reader.ReadByte();
                timer[i].period = reader.ReadInt64();
            }
            osc_select = reader.ReadByte();
            reg_select = reader.ReadByte();
            irq_enabled = reader.ReadByte();
            irq_pending = reader.ReadByte();
            irq_on = reader.ReadBoolean();
        }
    }
}
