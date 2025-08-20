using MAME.Core;
using System;

namespace cpu.m6805
{
    public partial class M6805 : cpuexec_data
    {
        public static M6805 m1;
        public Register ea, pc, s;
        public int subtype;
        public ushort sp_mask;
        public ushort sp_low;
        public byte a, x, cc;
        public ushort pending_interrupts;
        public delegate int irq_delegate(int i);
        public irq_delegate irq_callback;
        public int[] irq_state;
        public int nmi_state;
        public byte CFLAG = 0x01, ZFLAG = 0x02, NFLAG = 0x04, IFLAG = 0x08, HFLAG = 0x10;
        public int SUBTYPE_M6805 = 0, SUBTYPE_M68705 = 1, SUBTYPE_HD63705 = 2;
        public byte[] flags8i = new byte[256]	 /* increment */
        {
            0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04
        };
        public byte[] flags8d = new byte[256] /* decrement */
        {
            0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,
            0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04
        };
        public byte[] cycles1 = new byte[]
        {
              /* 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F */
          /*0*/ 10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
          /*1*/  7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
          /*2*/  4, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
          /*3*/  6, 0, 0, 6, 6, 0, 6, 6, 6, 6, 6, 6, 0, 6, 6, 0,
          /*4*/  4, 0, 0, 4, 4, 0, 4, 4, 4, 4, 4, 0, 4, 4, 0, 4,
          /*5*/  4, 0, 0, 4, 4, 0, 4, 4, 4, 4, 4, 0, 4, 4, 0, 4,
          /*6*/  7, 0, 0, 7, 7, 0, 7, 7, 7, 7, 7, 0, 7, 7, 0, 7,
          /*7*/  6, 0, 0, 6, 6, 0, 6, 6, 6, 6, 6, 0, 6, 6, 0, 6,
          /*8*/  9, 6, 0,11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
          /*9*/  0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 2,
          /*A*/  2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 0, 8, 2, 0,
          /*B*/  4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 3, 7, 4, 5,
          /*C*/  5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 4, 8, 5, 6,
          /*D*/  6, 6, 6, 6, 6, 6, 6, 7, 6, 6, 6, 6, 5, 9, 6, 7,
          /*E*/  5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 4, 8, 5, 6,
          /*F*/  4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 3, 7, 4, 5
        };
        private ulong totalExecutedCycles;
        private int pendingCycles;
        public override ulong TotalExecutedCycles
        {
            get
            {
                return totalExecutedCycles;
            }
            set
            {
                totalExecutedCycles = value;
            }
        }
        public override int PendingCycles
        {
            get
            {
                return pendingCycles;
            }
            set
            {
                pendingCycles = value;
            }
        }
        public Func<ushort, byte> ReadOp, ReadOpArg;
        public Func<ushort, byte> ReadMemory;
        public Action<ushort, byte> WriteMemory;

        public M6805()
        {
            irq_state = new int[9];
            m6805_init(Cpuint.cpu_3_irq_callback);
        }
        public override void Reset()
        {
            m6805_reset();
        }
        private void SP_INC()
        {
            if (++s.LowWord > sp_mask)
            {
                s.LowWord = sp_low;
            }
        }
        private void SP_DEC()
        {
            if (--s.LowWord < sp_low)
            {
                s.LowWord = sp_mask;
            }
        }
        private ushort SP_ADJUST(ushort a)
        {
            return (ushort)(((a) & sp_mask) | sp_low);
        }
        private void IMMBYTE(ref byte b)
        {
            b = ReadOpArg(pc.LowWord++);
        }
        private void IMMWORD(ref Register w)
        {
            w.d = 0;
            w.HighByte = ReadOpArg(pc.LowWord);
            w.LowByte = ReadOpArg((ushort)(pc.LowWord + 1));
            pc.LowWord += 2;
        }
        private void PUSHBYTE(ref byte b)
        {
            wr_s_handler_b(ref b);
        }
        private void PUSHWORD(ref Register w)
        {
            wr_s_handler_w(ref w);
        }
        private void PULLBYTE(ref byte b)
        {
            rd_s_handler_b(ref b);
        }
        private void PULLWORD(ref Register w)
        {
            rd_s_handler_w(ref w);
        }
        private void CLR_NZ()
        {
            cc &= (byte)~(NFLAG | ZFLAG);
        }
        private void CLR_HNZC()
        {
            cc &= (byte)~(HFLAG | NFLAG | ZFLAG | CFLAG);
        }
        private void CLR_Z()
        {
            cc &= (byte)~(ZFLAG);
        }
        private void CLR_NZC()
        {
            cc &= (byte)~(NFLAG | ZFLAG | CFLAG);
        }
        private void CLR_ZC()
        {
            cc &= (byte)~(ZFLAG | CFLAG);
        }
        private void SET_Z(byte b)
        {
            if (b == 0)
            {
                SEZ();
            }
        }
        private void SET_Z8(byte b)
        {
            SET_Z((byte)b);
        }
        private void SET_N8(byte b)
        {
            cc |= (byte)((b & 0x80) >> 5);
        }
        private void SET_H(byte a, byte b, byte r)
        {
            cc |= (byte)((a ^ b ^ r) & 0x10);
        }
        private void SET_C8(ushort b)
        {
            cc |= (byte)((b & 0x100) >> 8);
        }
        private void SET_FLAGS8I(byte b)
        {
            cc |= flags8i[b & 0xff];
        }
        private void SET_FLAGS8D(byte b)
        {
            cc |= flags8d[b & 0xff];
        }
        private void SET_NZ8(byte b)
        {
            SET_N8(b);
            SET_Z(b);
        }
        private void SET_FLAGS8(byte a, byte b, ushort r)
        {
            SET_N8((byte)r);
            SET_Z8((byte)r);
            SET_C8(r);
        }
        private short SIGNED(byte b)
        {
            return (short)((b & 0x80) != 0 ? b | 0xff00 : b);
        }
        private void DIRECT()
        {
            ea.d = 0;
            IMMBYTE(ref ea.LowByte);
        }
        private void IMM8()
        {
            ea.LowWord = pc.LowWord++;
        }
        private void EXTENDED()
        {
            IMMWORD(ref ea);
        }
        private void INDEXED()
        {
            ea.LowWord = x;
        }
        private void INDEXED1()
        {
            ea.d = 0;
            IMMBYTE(ref ea.LowByte);
            ea.LowWord += x;
        }
        private void INDEXED2()
        {
            IMMWORD(ref ea);
            ea.LowWord += x;
        }
        private void SEC()
        {
            cc |= CFLAG;
        }
        private void CLC()
        {
            cc &= (byte)~CFLAG;
        }
        private void SEZ()
        {
            cc |= ZFLAG;
        }
        private void CLZ()
        {
            cc &= (byte)~ZFLAG;
        }
        private void SEN()
        {
            cc |= NFLAG;
        }
        private void CLN()
        {
            cc &= (byte)~NFLAG;
        }
        private void SEH()
        {
            cc |= HFLAG;
        }
        private void CLH()
        {
            cc &= (byte)~HFLAG;
        }
        private void SEI()
        {
            cc |= IFLAG;
        }
        private void CLI()
        {
            cc &= (byte)~IFLAG;
        }
        private void DIRBYTE(ref byte b)
        {
            DIRECT();
            b = ReadMemory((ushort)ea.d);
        }
        private void EXTBYTE(ref byte b)
        {
            EXTENDED();
            b = ReadMemory((ushort)ea.d);
        }
        private void IDXBYTE(ref byte b)
        {
            INDEXED();
            b = ReadMemory((ushort)ea.d);
        }
        private void IDX1BYTE(ref byte b)
        {
            INDEXED1();
            b = ReadMemory((ushort)ea.d);
        }
        private void IDX2BYTE(ref byte b)
        {
            INDEXED2();
            b = ReadMemory((ushort)ea.d);
        }
        private void BRANCH(bool f)
        {
            byte t = 0;
            IMMBYTE(ref t);
            if (f)
            {
                pc.LowWord += (ushort)SIGNED(t);
                //change_pc(PC);
                if (t == 0xfe)
                {
                    if (pendingCycles > 0)
                    {
                        pendingCycles = 0;
                    }
                }
            }
        }
        private void CLEAR_PAIR(ref Register p)
        {
            p.d = 0;
        }
        private void rd_s_handler_b(ref byte b)
        {
            SP_INC();
            b = ReadMemory(s.LowWord);
        }
        private void rd_s_handler_w(ref Register p)
        {
            CLEAR_PAIR(ref p);
            SP_INC();
            p.HighByte = ReadMemory(s.LowWord);
            SP_INC();
            p.LowByte = ReadMemory(s.LowWord);
        }
        private void wr_s_handler_b(ref byte b)
        {
            WriteMemory(s.LowWord, b);
            SP_DEC();
        }
        private void wr_s_handler_w(ref Register p)
        {
            WriteMemory(s.LowWord, p.LowByte);
            SP_DEC();
            WriteMemory(s.LowWord, p.HighByte);
            SP_DEC();
        }
        protected void RM16(uint Addr, ref Register p)
        {
            CLEAR_PAIR(ref p);
            p.HighByte = ReadMemory((ushort)Addr);
            ++Addr;
            p.LowByte = ReadMemory((ushort)Addr);
        }
        private void m68705_Interrupt()
        {
            if ((pending_interrupts & ((1 << 0) | 0x03)) != 0)
            {
                if ((cc & IFLAG) == 0)
                {
                    PUSHWORD(ref pc);
                    PUSHBYTE(ref x);
                    PUSHBYTE(ref a);
                    PUSHBYTE(ref cc);
                    SEI();
                    if (irq_callback != null)
                    {
                        irq_callback(0);
                    }
                    if ((pending_interrupts & (1 << 0)) != 0)
                    {
                        pending_interrupts &= unchecked((ushort)(~(1 << 0)));
                        RM16(0xfffa, ref pc);
                        //change_pc(PC);
                    }
                    else if ((pending_interrupts & (1 << 0x01)) != 0)
                    {
                        pending_interrupts &= unchecked((ushort)(~(1 << 0x01)));
                        RM16(0xfff8, ref pc);
                        //change_pc(PC);
                    }
                }
                pendingCycles -= 11;
            }
        }
        private void Interrupt()
        {
            if ((pending_interrupts & (1 << 0x08)) != 0)
            {
                PUSHWORD(ref pc);
                PUSHBYTE(ref x);
                PUSHBYTE(ref a);
                PUSHBYTE(ref cc);
                SEI();
                if (irq_callback != null)
                {
                    irq_callback(0);
                }
                RM16(0x1ffc, ref pc);
                //change_pc(PC);
                pending_interrupts &= unchecked((ushort)(~(1 << 0x08)));
                pendingCycles -= 11;
            }
            else if ((pending_interrupts & ((1 << 0) | 0x1ff)) != 0)
            {
                if ((cc & IFLAG) == 0)
                {
                    {
                        PUSHWORD(ref pc);
                        PUSHBYTE(ref x);
                        PUSHBYTE(ref a);
                        PUSHBYTE(ref cc);
                        SEI();
                        if (irq_callback != null)
                        {
                            irq_callback(0);
                        }
                        if (subtype == SUBTYPE_HD63705)
                        {
                            if ((pending_interrupts & (1 << 0x00)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x00));
                                RM16(0x1ff8, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x01)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x01));
                                RM16(0x1fec, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x07)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x07));
                                RM16(0x1fea, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x02)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x02));
                                RM16(0x1ff6, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x03)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x03));
                                RM16(0x1ff4, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x04)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x04));
                                RM16(0x1ff2, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x05)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x05));
                                RM16(0x1ff0, ref pc);
                                //change_pc(PC);
                            }
                            else if ((pending_interrupts & (1 << 0x06)) != 0)
                            {
                                pending_interrupts &= unchecked((ushort)~(1 << 0x06));
                                RM16(0x1fee, ref pc);
                                //change_pc(PC);
                            }
                        }
                        else
                        {
                            RM16(0xffff - 5, ref pc);
                            //change_pc(PC);
                        }

                    }	// CC & IFLAG
                    pending_interrupts &= unchecked((ushort)~(1 << 0));
                }
                pendingCycles -= 11;
            }
        }
        private void m6805_init(irq_delegate irqcallback)
        {
            irq_callback = irqcallback;
        }
        protected void m6805_reset()
        {
            /*int (*save_irqcallback)(int) = m6805.irq_callback;
            memset(&m6805, 0, sizeof(m6805));
            m6805.irq_callback = save_irqcallback;*/
            int i;
            ea.d = 0;
            pc.d = 0;
            s.d = 0;
            a = 0;
            x = 0;
            cc = 0;
            pending_interrupts = 0;
            for (i = 0; i < 9; i++)
            {
                irq_state[i] = 0;
            }
            nmi_state = 0;
            subtype = SUBTYPE_M6805;
            sp_mask = 0x07f;
            sp_low = 0x060;
            s.LowWord = sp_mask;
            SEI();
            RM16(0xfffe, ref pc);
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            if (irq_state[0] == (int)state)
            {
                return;
            }
            irq_state[0] = (int)state;
            if (state != (int)LineState.CLEAR_LINE)
            {
                pending_interrupts |= 1 << 0;
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
        }
        public override int ExecuteCycles(int cycles)
        {
            return m6805_execute(cycles);
        }
        public int m6805_execute(int cycles)
        {
            byte ireg;
            pendingCycles = cycles;
            do
            {
                int prevCycles = pendingCycles;
                if (pending_interrupts != 0)
                {
                    if (subtype == SUBTYPE_M68705)
                    {
                        m68705_Interrupt();
                    }
                    else
                    {
                        Interrupt();
                    }
                }
                ireg = ReadOp(pc.LowWord++);
                switch (ireg)
                {
                    case 0x00: brset(0x01); break;
                    case 0x01: brclr(0x01); break;
                    case 0x02: brset(0x02); break;
                    case 0x03: brclr(0x02); break;
                    case 0x04: brset(0x04); break;
                    case 0x05: brclr(0x04); break;
                    case 0x06: brset(0x08); break;
                    case 0x07: brclr(0x08); break;
                    case 0x08: brset(0x10); break;
                    case 0x09: brclr(0x10); break;
                    case 0x0A: brset(0x20); break;
                    case 0x0B: brclr(0x20); break;
                    case 0x0C: brset(0x40); break;
                    case 0x0D: brclr(0x40); break;
                    case 0x0E: brset(0x80); break;
                    case 0x0F: brclr(0x80); break;
                    case 0x10: bset(0x01); break;
                    case 0x11: bclr(0x01); break;
                    case 0x12: bset(0x02); break;
                    case 0x13: bclr(0x02); break;
                    case 0x14: bset(0x04); break;
                    case 0x15: bclr(0x04); break;
                    case 0x16: bset(0x08); break;
                    case 0x17: bclr(0x08); break;
                    case 0x18: bset(0x10); break;
                    case 0x19: bclr(0x10); break;
                    case 0x1a: bset(0x20); break;
                    case 0x1b: bclr(0x20); break;
                    case 0x1c: bset(0x40); break;
                    case 0x1d: bclr(0x40); break;
                    case 0x1e: bset(0x80); break;
                    case 0x1f: bclr(0x80); break;
                    case 0x20: bra(); break;
                    case 0x21: brn(); break;
                    case 0x22: bhi(); break;
                    case 0x23: bls(); break;
                    case 0x24: bcc(); break;
                    case 0x25: bcs(); break;
                    case 0x26: bne(); break;
                    case 0x27: beq(); break;
                    case 0x28: bhcc(); break;
                    case 0x29: bhcs(); break;
                    case 0x2a: bpl(); break;
                    case 0x2b: bmi(); break;
                    case 0x2c: bmc(); break;
                    case 0x2d: bms(); break;
                    case 0x2e: bil(); break;
                    case 0x2f: bih(); break;
                    case 0x30: neg_di(); break;
                    case 0x31: illegal(); break;
                    case 0x32: illegal(); break;
                    case 0x33: com_di(); break;
                    case 0x34: lsr_di(); break;
                    case 0x35: illegal(); break;
                    case 0x36: ror_di(); break;
                    case 0x37: asr_di(); break;
                    case 0x38: lsl_di(); break;
                    case 0x39: rol_di(); break;
                    case 0x3a: dec_di(); break;
                    case 0x3b: illegal(); break;
                    case 0x3c: inc_di(); break;
                    case 0x3d: tst_di(); break;
                    case 0x3e: illegal(); break;
                    case 0x3f: clr_di(); break;
                    case 0x40: nega(); break;
                    case 0x41: illegal(); break;
                    case 0x42: illegal(); break;
                    case 0x43: coma(); break;
                    case 0x44: lsra(); break;
                    case 0x45: illegal(); break;
                    case 0x46: rora(); break;
                    case 0x47: asra(); break;
                    case 0x48: lsla(); break;
                    case 0x49: rola(); break;
                    case 0x4a: deca(); break;
                    case 0x4b: illegal(); break;
                    case 0x4c: inca(); break;
                    case 0x4d: tsta(); break;
                    case 0x4e: illegal(); break;
                    case 0x4f: clra(); break;
                    case 0x50: negx(); break;
                    case 0x51: illegal(); break;
                    case 0x52: illegal(); break;
                    case 0x53: comx(); break;
                    case 0x54: lsrx(); break;
                    case 0x55: illegal(); break;
                    case 0x56: rorx(); break;
                    case 0x57: asrx(); break;
                    case 0x58: aslx(); break;
                    case 0x59: rolx(); break;
                    case 0x5a: decx(); break;
                    case 0x5b: illegal(); break;
                    case 0x5c: incx(); break;
                    case 0x5d: tstx(); break;
                    case 0x5e: illegal(); break;
                    case 0x5f: clrx(); break;
                    case 0x60: neg_ix1(); break;
                    case 0x61: illegal(); break;
                    case 0x62: illegal(); break;
                    case 0x63: com_ix1(); break;
                    case 0x64: lsr_ix1(); break;
                    case 0x65: illegal(); break;
                    case 0x66: ror_ix1(); break;
                    case 0x67: asr_ix1(); break;
                    case 0x68: lsl_ix1(); break;
                    case 0x69: rol_ix1(); break;
                    case 0x6a: dec_ix1(); break;
                    case 0x6b: illegal(); break;
                    case 0x6c: inc_ix1(); break;
                    case 0x6d: tst_ix1(); break;
                    case 0x6e: illegal(); break;
                    case 0x6f: clr_ix1(); break;
                    case 0x70: neg_ix(); break;
                    case 0x71: illegal(); break;
                    case 0x72: illegal(); break;
                    case 0x73: com_ix(); break;
                    case 0x74: lsr_ix(); break;
                    case 0x75: illegal(); break;
                    case 0x76: ror_ix(); break;
                    case 0x77: asr_ix(); break;
                    case 0x78: lsl_ix(); break;
                    case 0x79: rol_ix(); break;
                    case 0x7a: dec_ix(); break;
                    case 0x7b: illegal(); break;
                    case 0x7c: inc_ix(); break;
                    case 0x7d: tst_ix(); break;
                    case 0x7e: illegal(); break;
                    case 0x7f: clr_ix(); break;
                    case 0x80: rti(); break;
                    case 0x81: rts(); break;
                    case 0x82: illegal(); break;
                    case 0x83: swi(); break;
                    case 0x84: illegal(); break;
                    case 0x85: illegal(); break;
                    case 0x86: illegal(); break;
                    case 0x87: illegal(); break;
                    case 0x88: illegal(); break;
                    case 0x89: illegal(); break;
                    case 0x8a: illegal(); break;
                    case 0x8b: illegal(); break;
                    case 0x8c: illegal(); break;
                    case 0x8d: illegal(); break;
                    case 0x8e: illegal(); break;
                    case 0x8f: illegal(); break;
                    case 0x90: illegal(); break;
                    case 0x91: illegal(); break;
                    case 0x92: illegal(); break;
                    case 0x93: illegal(); break;
                    case 0x94: illegal(); break;
                    case 0x95: illegal(); break;
                    case 0x96: illegal(); break;
                    case 0x97: tax(); break;
                    case 0x98: CLC(); break;
                    case 0x99: SEC(); break;
                    case 0x9a: CLI(); break;
                    case 0x9b: SEI(); break;
                    case 0x9c: rsp(); break;
                    case 0x9d: nop(); break;
                    case 0x9e: illegal(); break;
                    case 0x9f: txa(); break;
                    case 0xa0: suba_im(); break;
                    case 0xa1: cmpa_im(); break;
                    case 0xa2: sbca_im(); break;
                    case 0xa3: cpx_im(); break;
                    case 0xa4: anda_im(); break;
                    case 0xa5: bita_im(); break;
                    case 0xa6: lda_im(); break;
                    case 0xa7: illegal(); break;
                    case 0xa8: eora_im(); break;
                    case 0xa9: adca_im(); break;
                    case 0xaa: ora_im(); break;
                    case 0xab: adda_im(); break;
                    case 0xac: illegal(); break;
                    case 0xad: bsr(); break;
                    case 0xae: ldx_im(); break;
                    case 0xaf: illegal(); break;
                    case 0xb0: suba_di(); break;
                    case 0xb1: cmpa_di(); break;
                    case 0xb2: sbca_di(); break;
                    case 0xb3: cpx_di(); break;
                    case 0xb4: anda_di(); break;
                    case 0xb5: bita_di(); break;
                    case 0xb6: lda_di(); break;
                    case 0xb7: sta_di(); break;
                    case 0xb8: eora_di(); break;
                    case 0xb9: adca_di(); break;
                    case 0xba: ora_di(); break;
                    case 0xbb: adda_di(); break;
                    case 0xbc: jmp_di(); break;
                    case 0xbd: jsr_di(); break;
                    case 0xbe: ldx_di(); break;
                    case 0xbf: stx_di(); break;
                    case 0xc0: suba_ex(); break;
                    case 0xc1: cmpa_ex(); break;
                    case 0xc2: sbca_ex(); break;
                    case 0xc3: cpx_ex(); break;
                    case 0xc4: anda_ex(); break;
                    case 0xc5: bita_ex(); break;
                    case 0xc6: lda_ex(); break;
                    case 0xc7: sta_ex(); break;
                    case 0xc8: eora_ex(); break;
                    case 0xc9: adca_ex(); break;
                    case 0xca: ora_ex(); break;
                    case 0xcb: adda_ex(); break;
                    case 0xcc: jmp_ex(); break;
                    case 0xcd: jsr_ex(); break;
                    case 0xce: ldx_ex(); break;
                    case 0xcf: stx_ex(); break;
                    case 0xd0: suba_ix2(); break;
                    case 0xd1: cmpa_ix2(); break;
                    case 0xd2: sbca_ix2(); break;
                    case 0xd3: cpx_ix2(); break;
                    case 0xd4: anda_ix2(); break;
                    case 0xd5: bita_ix2(); break;
                    case 0xd6: lda_ix2(); break;
                    case 0xd7: sta_ix2(); break;
                    case 0xd8: eora_ix2(); break;
                    case 0xd9: adca_ix2(); break;
                    case 0xda: ora_ix2(); break;
                    case 0xdb: adda_ix2(); break;
                    case 0xdc: jmp_ix2(); break;
                    case 0xdd: jsr_ix2(); break;
                    case 0xde: ldx_ix2(); break;
                    case 0xdf: stx_ix2(); break;
                    case 0xe0: suba_ix1(); break;
                    case 0xe1: cmpa_ix1(); break;
                    case 0xe2: sbca_ix1(); break;
                    case 0xe3: cpx_ix1(); break;
                    case 0xe4: anda_ix1(); break;
                    case 0xe5: bita_ix1(); break;
                    case 0xe6: lda_ix1(); break;
                    case 0xe7: sta_ix1(); break;
                    case 0xe8: eora_ix1(); break;
                    case 0xe9: adca_ix1(); break;
                    case 0xea: ora_ix1(); break;
                    case 0xeb: adda_ix1(); break;
                    case 0xec: jmp_ix1(); break;
                    case 0xed: jsr_ix1(); break;
                    case 0xee: ldx_ix1(); break;
                    case 0xef: stx_ix1(); break;
                    case 0xf0: suba_ix(); break;
                    case 0xf1: cmpa_ix(); break;
                    case 0xf2: sbca_ix(); break;
                    case 0xf3: cpx_ix(); break;
                    case 0xf4: anda_ix(); break;
                    case 0xf5: bita_ix(); break;
                    case 0xf6: lda_ix(); break;
                    case 0xf7: sta_ix(); break;
                    case 0xf8: eora_ix(); break;
                    case 0xf9: adca_ix(); break;
                    case 0xfa: ora_ix(); break;
                    case 0xfb: adda_ix(); break;
                    case 0xfc: jmp_ix(); break;
                    case 0xfd: jsr_ix(); break;
                    case 0xfe: ldx_ix(); break;
                    case 0xff: stx_ix(); break;
                }
                pendingCycles -= cycles1[ireg];
                int delta = prevCycles - pendingCycles;
                totalExecutedCycles += (ulong)delta;
            }
            while (pendingCycles > 0);
            return cycles - pendingCycles;
        }
        public void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(ea.LowWord);
            writer.Write(pc.LowWord);
            writer.Write(s.LowWord);
            writer.Write(subtype);
            writer.Write(a);
            writer.Write(x);
            writer.Write(cc);
            writer.Write(pending_interrupts);
            for (i = 0; i < 9; i++)
            {
                writer.Write(irq_state[i]);
            }
            writer.Write(nmi_state);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            ea.LowWord = reader.ReadUInt16();
            pc.LowWord = reader.ReadUInt16();
            s.LowWord = reader.ReadUInt16();
            subtype = reader.ReadInt32();
            a = reader.ReadByte();
            x = reader.ReadByte();
            cc = reader.ReadByte();
            pending_interrupts = reader.ReadUInt16();
            for (i = 0; i < 9; i++)
            {
                irq_state[i] = reader.ReadInt32();
            }
            nmi_state = reader.ReadInt32();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
    }
    public class M68705 : M6805
    {
        public M68705()
        {
            m68705_init(Cpuint.cpu_3_irq_callback);
        }
        private void m68705_init(irq_delegate irqcallback)
        {
            irq_callback = irqcallback;
        }
        public override void Reset()
        {
            m68705_reset();
        }
        private void m68705_reset()
        {
            m6805_reset();
            subtype = SUBTYPE_M68705;
            RM16(0xfffe, ref pc);
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            m68705_set_irq_line(irqline, state);
        }
        private void m68705_set_irq_line(int irqline, LineState state)
        {
            if (irq_state[irqline] == (int)state)
            {
                return;
            }
            irq_state[irqline] = (int)state;
            if (state != (int)LineState.CLEAR_LINE)
            {
                pending_interrupts |= (ushort)(1 << irqline);
            }
        }
    }
}
