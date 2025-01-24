using MAME.Core;
using System;
using System.IO;

namespace cpu.nec
{
    public partial class Nec : cpuexec_data
    {
        protected ulong totalExecutedCycles;
        public int pendingCycles;
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
        public override void set_irq_line(int irqline, LineState state)
        {
            if (irqline == (int)LineState.INPUT_LINE_NMI)
            {
                if (I.nmi_state == (uint)state)
                {
                    return;
                }
                I.nmi_state = (uint)state;
                if (state != LineState.CLEAR_LINE)
                {
                    I.pending_irq |= 0x02;
                }
            }
            else
            {
                I.irq_state = (uint)state;
                if (state == LineState.CLEAR_LINE)
                {
                    I.pending_irq &= 0xfffffffe;
                }
                else
                {
                    I.pending_irq |= 0x01;
                }
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            if (line >= 0 && line < 35)
            {
                irq _irq = ObjectPoolAuto.Acquire<irq>();
                _irq.Init(cpunum, line, state, vector, EmuTimer.get_current_time());
                Cpuint.lirq.Add(_irq);
                //Cpuint.lirq.Add(new irq(cpunum, line, state, vector, EmuTimer.get_current_time()));
                int event_index = Cpuint.input_event_index[cpunum, line]++;
                if (event_index >= 35)
                {
                    Cpuint.input_event_index[cpunum, line]--;
                    //Cpuint.cpunum_empty_event_queue(machine, NULL, cpunum | (line << 8));
                    event_index = Cpuint.input_event_index[cpunum, line]++;
                }
                if (event_index < 35)
                {
                    //Cpuint.input_event_queue[cpunum][line][event_index] = input_event;
                    //if (event_index == 0)
                    {
                        EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
                    }
                }
            }
        }
        public struct necbasicregs
        {
            //public ushort[] w;//[8];
            public byte[] b;//[16];
        }
        public struct nec_Regs
        {
            public necbasicregs regs;
            public ushort[] sregs;//[4];
            public ushort ip;
            public int SignVal;
            public uint AuxVal, OverVal, ZeroVal, CarryVal, ParityVal;
            public bool TF, IF, DF, MF;
            public uint int_vector;
            public uint pending_irq;
            public uint nmi_state;
            public uint irq_state;
            public bool poll_state;
            public byte no_interrupt;
            //int (*irq_callback)(int irqline);
            //memory_interface	mem;
            //const nec_config *config;
        }
        public struct Mod_RM
        {
            public int[] regw;
            public int[] regb;
            public int[] RMw;
            public int[] RMb;
        }
        public int iNOP;
        public static Nec[] nn1;
        public Mod_RM mod_RM;
        public byte[] v25v35_decryptiontable;
        public nec_Regs I;
        public int chip_type;
        public static int prefix_base;
        public static int seg_prefix;
        public int INT_IRQ = 0x01, NMI_IRQ = 0x02;
        public bool[] parity_table = new bool[0x100];
        public Func<int, byte> ReadOp, ReadOpArg;
        public Func<int, byte> ReadByte;
        public Action<int, byte> WriteByte;
        public Func<int, ushort> ReadWord;
        public Action<int, ushort> WriteWord;
        public Func<int, byte> ReadIOByte;
        public Action<int, byte> WriteIOByte;
        public Func<int, ushort> ReadIOWord;
        public Action<int, ushort> WriteIOWord;
        public delegate void nec_delegate();
        public nec_delegate[] nec_instruction;
        public delegate int getea_delegate();
        public getea_delegate[] GetEA;
        public Nec()
        {
            nec_init();
        }
        private int DefaultBase(int Seg, nec_Regs I)
        {
            return (((seg_prefix != 0) && (Seg == 3 || Seg == 2)) ? prefix_base : (int)(I.sregs[Seg] << 4));
        }
        private byte GetMemB(int Seg, int Off)
        {
            return ReadByte(DefaultBase(Seg, I) + Off);
        }
        private ushort GetMemW(int Seg, int Off)
        {
            return ReadWord(DefaultBase(Seg, I) + Off);
        }
        private void PutMemB(int Seg, int Off, byte x)
        {
            WriteByte(DefaultBase(Seg, I) + Off, x);
        }
        private void PutMemW(int Seg, int Off, ushort x)
        {
            WriteWord(DefaultBase(Seg, I) + Off, x);
        }
        private byte FETCH()
        {
            return ReadOpArg(((I.sregs[1] << 4) + I.ip++) ^ 0);
        }
        public byte fetchop()
        {
            byte ret = ReadOp(((I.sregs[1] << 4) + I.ip++) ^ 0);
            if (I.MF)
            {
                if (v25v35_decryptiontable != null)
                {
                    ret = v25v35_decryptiontable[ret];
                }
            }
            return ret;
        }
        public ushort FETCHWORD()
        {
            ushort var = (ushort)(ReadOpArg(((I.sregs[1] << 4) + I.ip) ^ 0) + (ReadOpArg(((I.sregs[1] << 4) + I.ip + 1) ^ 0) << 8));
            I.ip += 2;
            return var;
        }
        public int GetModRM()
        {
            int ModRM = ReadOpArg(((I.sregs[1] << 4) + I.ip++) ^ 0);
            return ModRM;
        }
        public void PUSH(ushort val)
        {
            //I.regs.w[4] -= 2;
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 - 2);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
            WriteWord((I.sregs[2] << 4) + I.regs.b[8] + I.regs.b[9] * 0x100, val);
        }
        public void POP(ref ushort var)
        {
            var = ReadWord((I.sregs[2] << 4) + I.regs.b[8] + I.regs.b[9] * 0x100);
            //I.regs.w[4] += 2;
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 + 2);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
        }
        public void POPW(int i)
        {
            ushort var = ReadWord((I.sregs[2] << 4) + I.regs.b[8] + I.regs.b[9] * 0x100);
            I.regs.b[i * 2] = (byte)(var % 0x100);
            I.regs.b[i * 2 + 1] = (byte)(var / 0x100);
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 + 2);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
        }
        public byte PEEK(uint addr)
        {
            return (byte)ReadOpArg((int)(addr ^ 0));
        }
        public byte PEEKOP(uint addr)
        {
            return (byte)ReadOp((int)(addr ^ 0));
        }
        public void SetCFB(uint x)
        {
            I.CarryVal = x & 0x100;
        }
        public void SetCFW(uint x)
        {
            I.CarryVal = x & 0x10000;
        }
        public void SetAF(int x, int y, int z)
        {
            I.AuxVal = (uint)(((x) ^ ((y) ^ (z))) & 0x10);
        }
        public void SetSZPF_Byte(int x)
        {
            I.ZeroVal = I.ParityVal = (uint)((sbyte)x);
            I.SignVal = (int)I.ZeroVal;
        }
        public void SetSZPF_Word(int x)
        {
            I.ZeroVal = I.ParityVal = (uint)((short)x);
            I.SignVal = (int)I.ZeroVal;
        }
        public void SetOFW_Add(int x, int y, int z)
        {
            I.OverVal = (uint)(((x) ^ (y)) & ((x) ^ (z)) & 0x8000);
        }
        public void SetOFB_Add(int x, int y, int z)
        {
            I.OverVal = (uint)(((x) ^ (y)) & ((x) ^ (z)) & 0x80);
        }
        public void SetOFW_Sub(int x, int y, int z)
        {
            I.OverVal = (uint)(((z) ^ (y)) & ((z) ^ (x)) & 0x8000);
        }
        public void SetOFB_Sub(int x, int y, int z)
        {
            I.OverVal = (uint)(((z) ^ (y)) & ((z) ^ (x)) & 0x80);
        }
        public void ADDB(ref byte src, ref byte dst)
        {
            uint res = (uint)(dst + src);
            SetCFB((uint)res);
            SetOFB_Add((int)res, src, dst);
            SetAF((int)res, src, dst);
            SetSZPF_Byte((int)res);
            dst = (byte)res;
        }
        public void ADDW(ref ushort src, ref ushort dst)
        {
            uint res = (uint)(dst + src);
            SetCFW(res);
            SetOFW_Add((int)res, src, dst);
            SetAF((int)res, src, dst);
            SetSZPF_Word((int)res);
            dst = (ushort)res;
        }
        public void SUBB(ref byte src, ref byte dst)
        {
            uint res = (uint)(dst - src);
            SetCFB(res);
            SetOFB_Sub((int)res, src, dst);
            SetAF((int)res, src, dst);
            SetSZPF_Byte((int)res);
            dst = (byte)res;
        }
        public void SUBW(ref ushort src, ref ushort dst)
        {
            uint res = (uint)(dst - src);
            SetCFW(res);
            SetOFW_Sub((int)res, src, dst);
            SetAF((int)res, src, dst);
            SetSZPF_Word((int)res);
            dst = (ushort)res;
        }
        public void ORB(ref byte src, ref byte dst)
        {
            dst |= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Byte(dst);
        }
        public void ORW(ref ushort src, ref ushort dst)
        {
            dst |= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Word(dst);
        }
        public void ANDB(ref byte src, ref byte dst)
        {
            dst &= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Byte(dst);
        }
        public void ANDW(ref ushort src, ref ushort dst)
        {
            dst &= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Word(dst);
        }
        public void XORB(ref byte src, ref byte dst)
        {
            dst ^= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Byte(dst);
        }
        public void XORW(ref ushort src, ref ushort dst)
        {
            dst ^= src;
            I.CarryVal = I.OverVal = I.AuxVal = 0;
            SetSZPF_Word(dst);
        }
        public bool CF()
        {
            return (I.CarryVal != 0);
        }
        public bool SF()
        {
            return (I.SignVal < 0);
        }
        public bool ZF()
        {
            return (I.ZeroVal == 0);
        }
        public bool PF()
        {
            return parity_table[(byte)I.ParityVal];
        }
        public bool AF()
        {
            return (I.AuxVal != 0);
        }
        public bool OF()
        {
            return (I.OverVal != 0);
        }
        public bool MD()
        {
            return I.MF;
        }
        public void CLK(int all)
        {
            pendingCycles -= all;
        }
        public void CLKS(int v20, int v30, int v33)
        {
            int ccount = (v20 << 16) | (v30 << 8) | v33;
            pendingCycles -= (ccount >> chip_type) & 0x7f;
        }
        public void CLKW(int v20o, int v30o, int v33o, int v20e, int v30e, int v33e, int addr)
        {
            int ocount = (v20o << 16) | (v30o << 8) | v33o, ecount = (v20e << 16) | (v30e << 8) | v33e;
            pendingCycles -= ((addr & 1) != 0) ? ((ocount >> chip_type) & 0x7f) : ((ecount >> chip_type) & 0x7f);
        }
        public void CLKM(int ModRM, int v20, int v30, int v33, int v20m, int v30m, int v33m)
        {
            int ccount = (v20 << 16) | (v30 << 8) | v33, mcount = (v20m << 16) | (v30m << 8) | v33m;
            pendingCycles -= (ModRM >= 0xc0) ? ((ccount >> chip_type) & 0x7f) : ((mcount >> chip_type) & 0x7f);
        }
        public void CLKR(int ModRM, int v20o, int v30o, int v33o, int v20e, int v30e, int v33e, int vall, int addr)
        {
            int ocount = (v20o << 16) | (v30o << 8) | v33o, ecount = (v20e << 16) | (v30e << 8) | v33e;
            if (ModRM >= 0xc0)
            {
                pendingCycles -= vall;
            }
            else
            {
                pendingCycles -= ((addr & 1) != 0) ? ((ocount >> chip_type) & 0x7f) : ((ecount >> chip_type) & 0x7f);
            }
        }
        public ushort CompressFlags()
        {
            return (ushort)((CF() ? 1 : 0) | ((PF() ? 1 : 0) << 2) | ((AF() ? 1 : 0) << 4) | ((ZF() ? 1 : 0) << 6) | ((SF() ? 1 : 0) << 7) | ((I.TF ? 1 : 0) << 8) | ((I.IF ? 1 : 0) << 9) | ((I.DF ? 1 : 0) << 10) | ((OF() ? 1 : 0) << 11) | ((MD() ? 1 : 0) << 15));
        }
        public void ExpandFlags(ushort f)
        {
            I.CarryVal = (uint)(f & 1);
            I.ParityVal = (uint)((f & 4) == 0 ? 1 : 0);
            I.AuxVal = (uint)(f & 16);
            I.ZeroVal = (uint)((f & 64) == 0 ? 1 : 0);
            I.SignVal = ((f & 128) != 0) ? -1 : 0;
            I.TF = (f & 256) == 256;
            I.IF = (f & 512) == 512;
            I.DF = (f & 1024) == 1024;
            I.OverVal = (uint)(f & 2048);
            I.MF = (f & 0x8000) == 0x8000;
        }
        public void IncWordReg(int Reg)
        {
            int tmp = (int)(I.regs.b[Reg * 2] + I.regs.b[Reg * 2 + 1] * 0x100);
            int tmp1 = tmp + 1;
            I.OverVal = (uint)((tmp == 0x7fff) ? 1 : 0);
            SetAF(tmp1, tmp, 1);
            SetSZPF_Word(tmp1);
            //I.regs.w[Reg] = (ushort)tmp1;
            I.regs.b[Reg * 2] = (byte)((ushort)tmp1 % 0x100);
            I.regs.b[Reg * 2 + 1] = (byte)((ushort)tmp1 / 0x100);
        }
        public void DecWordReg(int Reg)
        {
            int tmp = (int)(I.regs.b[Reg * 2] + I.regs.b[Reg * 2 + 1] * 0x100);
            int tmp1 = tmp - 1;
            I.OverVal = (uint)((tmp == 0x8000) ? 1 : 0);
            SetAF(tmp1, tmp, 1);
            SetSZPF_Word(tmp1);
            //I.regs.w[Reg] = (ushort)tmp1;
            I.regs.b[Reg * 2] = (byte)((ushort)tmp1 % 0x100);
            I.regs.b[Reg * 2 + 1] = (byte)((ushort)tmp1 / 0x100);
        }
        public void JMP(bool flag)
        {
            int tmp = (int)((sbyte)FETCH());
            if (flag)
            {
                byte[] table = new byte[] { 3, 10, 10 };
                I.ip = (ushort)(I.ip + tmp);
                pendingCycles -= table[chip_type / 8];
                //PC = (I.sregs[1] << 4) + I.ip;
                return;
            }
        }
        public void ADJ4(int param1, int param2)
        {
            if (AF() || ((I.regs.b[0] & 0xf) > 9))
            {
                ushort tmp;
                tmp = (ushort)(I.regs.b[0] + param1);
                I.regs.b[0] = (byte)tmp;
                I.AuxVal = 1;
                I.CarryVal |= (uint)(tmp & 0x100);
            }
            if (CF() || (I.regs.b[0] > 0x9f))
            {
                I.regs.b[0] += (byte)param2;
                I.CarryVal = 1;
            }
            SetSZPF_Byte(I.regs.b[0]);
        }
        public void ADJB(int param1, int param2)
        {
            if (AF() || ((I.regs.b[0] & 0xf) > 9))
            {
                I.regs.b[0] += (byte)param1;
                I.regs.b[1] += (byte)param2;
                I.AuxVal = 1;
                I.CarryVal = 1;
            }
            else
            {
                I.AuxVal = 0;
                I.CarryVal = 0;
            }
            I.regs.b[0] &= 0x0F;
        }
        public void BITOP_BYTE(ref int ModRM, ref int tmp)
        {
            ModRM = FETCH();
            if (ModRM >= 0xc0)
            {
                tmp = I.regs.b[mod_RM.RMb[ModRM]];
            }
            else
            {
                EA = GetEA[ModRM]();
                tmp = ReadByte(EA);
            }
        }
        public void BITOP_WORD(ref int ModRM, ref int tmp)
        {
            ModRM = FETCH();
            if (ModRM >= 0xc0)
            {
                tmp = I.regs.b[mod_RM.RMw[ModRM] * 2] + I.regs.b[mod_RM.RMw[ModRM] * 2 + 1] * 0x100;
            }
            else
            {
                EA = GetEA[ModRM]();
                tmp = ReadWord(EA);
            }
        }
        public void BIT_NOT(ref int tmp, ref int tmp2)
        {
            if ((tmp & (1 << tmp2)) != 0)
            {
                tmp &= (~(1 << tmp2));
            }
            else
            {
                tmp |= (1 << tmp2);
            }
        }
        public void XchgAWReg(int Reg)
        {
            ushort tmp;
            tmp = (ushort)(I.regs.b[Reg * 2] + I.regs.b[Reg * 2 + 1] * 0x100);
            //I.regs.w[Reg] = I.regs.w[0];
            //I.regs.w[0] = tmp;
            I.regs.b[Reg * 2] = I.regs.b[0];
            I.regs.b[Reg * 2 + 1] = I.regs.b[1];
            I.regs.b[0] = (byte)(tmp % 0x100);
            I.regs.b[1] = (byte)(tmp / 0x100);
        }
        public void ROL_BYTE(ref int dst)
        {
            I.CarryVal = (uint)(dst & 0x80);
            dst = (dst << 1) + (CF() ? 1 : 0);
        }
        public void ROL_WORD(ref int dst)
        {
            I.CarryVal = (uint)(dst & 0x8000);
            dst = (dst << 1) + (CF() ? 1 : 0);
        }
        public void ROR_BYTE(ref int dst)
        {
            I.CarryVal = (uint)(dst & 0x1);
            dst = (dst >> 1) + ((CF() ? 1 : 0) << 7);
        }
        public void ROR_WORD(ref int dst)
        {
            I.CarryVal = (uint)(dst & 0x1);
            dst = (dst >> 1) + ((CF() ? 1 : 0) << 15);
        }
        public void ROLC_BYTE(ref int dst)
        {
            dst = (dst << 1) + (CF() ? 1 : 0);
            SetCFB((uint)dst);
        }
        public void ROLC_WORD(ref int dst)
        {
            dst = (dst << 1) + (CF() ? 1 : 0);
            SetCFW((uint)dst);
        }
        public void RORC_BYTE(ref int dst)
        {
            dst = ((CF() ? 1 : 0) << 8) + dst;
            I.CarryVal = (uint)(dst & 0x01);
            dst >>= 1;
        }
        public void RORC_WORD(ref int dst)
        {
            dst = ((CF() ? 1 : 0) << 16) + dst;
            I.CarryVal = (uint)(dst & 0x01);
            dst >>= 1;
        }
        public void SHL_BYTE(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst <<= c;
            SetCFB((uint)dst);
            SetSZPF_Byte(dst);
            PutbackRMByte(ModRM, (byte)dst);
        }
        public void SHL_WORD(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst <<= c;
            SetCFW((uint)dst);
            SetSZPF_Word(dst);
            PutbackRMWord(ModRM, (ushort)dst);
        }
        public void SHR_BYTE(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst >>= c - 1;
            I.CarryVal = (uint)(dst & 0x1);
            dst >>= 1;
            SetSZPF_Byte(dst);
            PutbackRMByte(ModRM, (byte)dst);
        }
        public void SHR_WORD(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst >>= c - 1;
            I.CarryVal = (uint)(dst & 0x1);
            dst >>= 1;
            SetSZPF_Word(dst);
            PutbackRMWord(ModRM, (ushort)dst);
        }
        public void SHRA_BYTE(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst = ((sbyte)dst) >> (c - 1);
            I.CarryVal = (uint)(dst & 0x1);
            dst = ((sbyte)((byte)dst)) >> 1;
            SetSZPF_Byte(dst);
            PutbackRMByte(ModRM, (byte)dst);
        }
        public void SHRA_WORD(int c, ref int dst, int ModRM)
        {
            pendingCycles -= c;
            dst = ((short)dst) >> (c - 1);
            I.CarryVal = (uint)(dst & 0x1);
            dst = ((short)((ushort)dst)) >> 1;
            SetSZPF_Word(dst);
            PutbackRMWord(ModRM, (ushort)dst);
        }
        public void DIVUB(int tmp, out bool b1)
        {
            int uresult, uresult2;
            b1 = false;
            uresult = I.regs.b[0] + I.regs.b[1] * 0x100;
            uresult2 = uresult % tmp;
            if ((uresult /= tmp) > 0xff)
            {
                nec_interrupt(0, false);
                b1 = true;
            }
            else
            {
                I.regs.b[0] = (byte)uresult;
                I.regs.b[1] = (byte)uresult2;
            }
        }
        public void DIVB(int tmp, out bool b1)
        {
            int result, result2;
            b1 = false;
            result = (short)(I.regs.b[0] + I.regs.b[1] * 0x100);
            result2 = result % (short)((sbyte)tmp);
            if ((result /= (short)((sbyte)tmp)) > 0xff)
            {
                nec_interrupt(0, false);
                b1 = true;
            }
            else
            {
                I.regs.b[0] = (byte)result;
                I.regs.b[1] = (byte)result2;
            }
        }
        public void DIVUW(int tmp, out bool b1)
        {
            uint uresult, uresult2;
            b1 = false;
            uresult = ((uint)(I.regs.b[4] + I.regs.b[5] * 0x100) << 16) | (uint)(I.regs.b[0] + I.regs.b[1] * 0x100);
            uresult2 = (uint)(uresult % tmp);
            if ((uresult /= (uint)tmp) > 0xffff)
            {
                nec_interrupt(0, false);
                b1 = true;
            }
            else
            {
                //I.regs.w[0] = (ushort)uresult;
                //I.regs.w[2] = (ushort)uresult2;
                I.regs.b[0] = (byte)(uresult % 0x100);
                I.regs.b[1] = (byte)(uresult / 0x100);
                I.regs.b[4] = (byte)(uresult2 % 0x100);
                I.regs.b[5] = (byte)(uresult2 / 0x100);
            }
        }
        public void DIVW(int tmp, out bool b1)
        {
            int result, result2;
            b1 = false;
            result = (int)(((uint)(I.regs.b[4] + I.regs.b[5] * 0x100) << 16) + (I.regs.b[0] + I.regs.b[1] * 0x100));
            result2 = result % (int)((short)tmp);
            if ((result /= (int)((short)tmp)) > 0xffff)
            {
                nec_interrupt(0, false);
                b1 = true;
            }
            else
            {
                //I.regs.w[0] = (ushort)result;
                //I.regs.w[2] = (ushort)result2;
                I.regs.b[0] = (byte)((ushort)result % 0x100);
                I.regs.b[1] = (byte)((ushort)result / 0x100);
                I.regs.b[4] = (byte)((ushort)result2 % 0x100);
                I.regs.b[5] = (byte)((ushort)result2 / 0x100);
            }
        }
        public void ADD4S(ref int tmp, ref int tmp2)
        {
            int i, v1, v2, result;
            int count = (I.regs.b[2] + 1) / 2;
            ushort di = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            ushort si = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            byte[] table = new byte[] { 18, 19, 19 };
            I.ZeroVal = I.CarryVal = 0;
            for (i = 0; i < count; i++)
            {
                pendingCycles -= table[chip_type / 8];
                tmp = GetMemB(3, si);
                tmp2 = GetMemB(0, di);
                v1 = (tmp >> 4) * 10 + (tmp & 0xf);
                v2 = (tmp2 >> 4) * 10 + (tmp2 & 0xf);
                result = (int)(v1 + v2 + I.CarryVal);
                I.CarryVal = (uint)(result > 99 ? 1 : 0);
                result = result % 100;
                v1 = ((result / 10) << 4) | (result % 10);
                PutMemB(0, di, (byte)v1);
                if (v1 != 0)
                {
                    I.ZeroVal = 1;
                }
                si++;
                di++;
            }
        }
        public void SUB4S(ref int tmp, ref int tmp2)
        {
            int count = (I.regs.b[2] + 1) / 2;
            int i, v1, v2, result;
            ushort di = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            ushort si = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            byte[] table = new byte[3] { 18, 19, 19 };
            I.ZeroVal = I.CarryVal = 0;
            for (i = 0; i < count; i++)
            {
                pendingCycles -= table[chip_type / 8];
                tmp = GetMemB(0, di);
                tmp2 = GetMemB(3, si);
                v1 = (tmp >> 4) * 10 + (tmp & 0xf);
                v2 = (tmp2 >> 4) * 10 + (tmp2 & 0xf);
                if (v1 < (v2 + I.CarryVal))
                {
                    v1 += 100;
                    result = (int)(v1 - (v2 + I.CarryVal));
                    I.CarryVal = 1;
                }
                else
                {
                    result = (int)(v1 - (v2 + I.CarryVal));
                    I.CarryVal = 0;
                }
                v1 = ((result / 10) << 4) | (result % 10);
                PutMemB(0, di, (byte)v1);
                if (v1 != 0)
                {
                    I.ZeroVal = 1;
                }
                si++;
                di++;
            }
        }
        private void CMP4S(ref int tmp, ref int tmp2)
        {
            int count = (I.regs.b[2] + 1) / 2;
            int i, v1, v2, result;
            ushort di = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            ushort si = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            byte[] table = new byte[3] { 14, 19, 19 };
            I.ZeroVal = I.CarryVal = 0;
            for (i = 0; i < count; i++)
            {
                pendingCycles -= table[chip_type / 8];
                tmp = GetMemB(0, di);
                tmp2 = GetMemB(3, si);
                v1 = (tmp >> 4) * 10 + (tmp & 0xf);
                v2 = (tmp2 >> 4) * 10 + (tmp2 & 0xf);
                if (v1 < (v2 + I.CarryVal))
                {
                    v1 += 100;
                    result = (int)(v1 - (v2 + I.CarryVal));
                    I.CarryVal = 1;
                }
                else
                {
                    result = (int)(v1 - (v2 + I.CarryVal));
                    I.CarryVal = 0;
                }
                v1 = ((result / 10) << 4) | (result % 10);
                if (v1 != 0)
                {
                    I.ZeroVal = 1;
                }
                si++;
                di++;
            }
        }
        public void nec_init()
        {
            mod_RM = new Mod_RM();
            mod_RM.regw = new int[256];
            mod_RM.regb = new int[256];
            mod_RM.RMw = new int[256];
            mod_RM.RMb = new int[256];
            nec_instruction = new nec_delegate[]{
                i_add_br8,
                i_add_wr16,
                i_add_r8b,
                i_add_r16w,
                i_add_ald8,
                i_add_axd16,
                i_push_es,
                i_pop_es,
                i_or_br8,
                i_or_wr16,
                i_or_r8b,
                i_or_r16w,
                i_or_ald8,
                i_or_axd16,
                i_push_cs,
                i_pre_nec,
                i_adc_br8,
                i_adc_wr16,
                i_adc_r8b,
                i_adc_r16w,
                i_adc_ald8,
                i_adc_axd16,
                i_push_ss,
                i_pop_ss,
                i_sbb_br8,
                i_sbb_wr16,
                i_sbb_r8b,
                i_sbb_r16w,
                i_sbb_ald8,
                i_sbb_axd16,
                i_push_ds,
                i_pop_ds,
                i_and_br8,
                i_and_wr16,
                i_and_r8b,
                i_and_r16w,
                i_and_ald8,
                i_and_axd16,
                i_es,
                i_daa,
                i_sub_br8,
                i_sub_wr16,
                i_sub_r8b,
                i_sub_r16w,
                i_sub_ald8,
                i_sub_axd16,
                i_cs,
                i_das,
                i_xor_br8,
                i_xor_wr16,
                i_xor_r8b,
                i_xor_r16w,
                i_xor_ald8,
                i_xor_axd16,
                i_ss,
                i_aaa,
                i_cmp_br8,
                i_cmp_wr16,
                i_cmp_r8b,
                i_cmp_r16w,
                i_cmp_ald8,
                i_cmp_axd16,
                i_ds,
                i_aas,
                i_inc_ax,
                i_inc_cx,
                i_inc_dx,
                i_inc_bx,
                i_inc_sp,
                i_inc_bp,
                i_inc_si,
                i_inc_di,
                i_dec_ax,
                i_dec_cx,
                i_dec_dx,
                i_dec_bx,
                i_dec_sp,
                i_dec_bp,
                i_dec_si,
                i_dec_di,
                i_push_ax,
                i_push_cx,
                i_push_dx,
                i_push_bx,
                i_push_sp,
                i_push_bp,
                i_push_si,
                i_push_di,
                i_pop_ax,
                i_pop_cx,
                i_pop_dx,
                i_pop_bx,
                i_pop_sp,
                i_pop_bp,
                i_pop_si,
                i_pop_di,
                i_pusha,
                i_popa,
                i_chkind,
                i_brkn,
                i_repnc,
                i_repc,
                i_invalid,
                i_invalid,
                i_push_d16,
                i_imul_d16,
                i_push_d8,
                i_imul_d8,
                i_insb,
                i_insw,
                i_outsb,
                i_outsw,
                i_jo,
                i_jno,
                i_jc,
                i_jnc,
                i_jz,
                i_jnz,
                i_jce,
                i_jnce,
                i_js,
                i_jns,
                i_jp,
                i_jnp,
                i_jl,
                i_jnl,
                i_jle,
                i_jnle,
                i_80pre,
                i_81pre,
                i_82pre,
                i_83pre,
                i_test_br8,
                i_test_wr16,
                i_xchg_br8,
                i_xchg_wr16,
                i_mov_br8,
                i_mov_wr16,
                i_mov_r8b,
                i_mov_r16w,
                i_mov_wsreg,
                i_lea,
                i_mov_sregw,
                i_popw,
                i_nop,
                i_xchg_axcx,
                i_xchg_axdx,
                i_xchg_axbx,
                i_xchg_axsp,
                i_xchg_axbp,
                i_xchg_axsi,
                i_xchg_axdi,
                i_cbw,
                i_cwd,
                i_call_far,
                i_wait,
                i_pushf,
                i_popf,
                i_sahf,
                i_lahf,
                i_mov_aldisp,
                i_mov_axdisp,
                i_mov_dispal,
                i_mov_dispax,
                i_movsb,
                i_movsw,
                i_cmpsb,
                i_cmpsw,
                i_test_ald8,
                i_test_axd16,
                i_stosb,
                i_stosw,
                i_lodsb,
                i_lodsw,
                i_scasb,
                i_scasw,
                i_mov_ald8,
                i_mov_cld8,
                i_mov_dld8,
                i_mov_bld8,
                i_mov_ahd8,
                i_mov_chd8,
                i_mov_dhd8,
                i_mov_bhd8,
                i_mov_axd16,
                i_mov_cxd16,
                i_mov_dxd16,
                i_mov_bxd16,
                i_mov_spd16,
                i_mov_bpd16,
                i_mov_sid16,
                i_mov_did16,
                i_rotshft_bd8,
                i_rotshft_wd8,
                i_ret_d16,
                i_ret,
                i_les_dw,
                i_lds_dw,
                i_mov_bd8,
                i_mov_wd16,
                i_enter,
                i_leave,
                i_retf_d16,
                i_retf,
                i_int3,
                i_int,
                i_into,
                i_iret,
                i_rotshft_b,
                i_rotshft_w,
                i_rotshft_bcl,
                i_rotshft_wcl,
                i_aam,
                i_aad,
                i_setalc,
                i_trans,
                i_fpo,
                i_fpo,
                i_fpo,
                i_fpo,
                i_fpo,
                i_fpo,
                i_fpo,
                i_fpo,
                i_loopne,
                i_loope,
                i_loop,
                i_jcxz,
                i_inal,
                i_inax,
                i_outal,
                i_outax,
                i_call_d16,
                i_jmp_d16,
                i_jmp_far,
                i_jmp_d8,
                i_inaldx,
                i_inaxdx,
                i_outdxal,
                i_outdxax,
                i_lock,
                i_invalid,
                i_repne,
                i_repe,
                i_hlt,
                i_cmc,
                i_f6pre,
                i_f7pre,
                i_clc,
                i_stc,
                i_di,
                i_ei,
                i_cld,
                i_std,
                i_fepre,
                i_ffpre
            };
            GetEA = new getea_delegate[192]{
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,
                EA_000, EA_001, EA_002, EA_003, EA_004, EA_005, EA_006, EA_007,

                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,
                EA_100, EA_101, EA_102, EA_103, EA_104, EA_105, EA_106, EA_107,

                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207,
                EA_200, EA_201, EA_202, EA_203, EA_204, EA_205, EA_206, EA_207
            };
        }
        public override void Reset()
        {
            nec_reset();
        }
        public void nec_reset()
        {
            //const nec_config *config;
            uint i, j, c;
            //BREGS[] reg_name = new BREGS[8] { BREGS.AL, BREGS.CL, BREGS.DL, BREGS.BL, BREGS.AH, BREGS.CH, BREGS.DH, BREGS.BH };
            int[] reg_name = new int[8] { 0, 2, 4, 6, 1, 3, 5, 7 };
            //int (*save_irqcallback)(int);
            //memory_interface save_mem;
            //save_irqcallback = I.irq_callback;
            //save_mem = I.mem;
            //config = I.config;
            I.sregs = new ushort[4];
            I.regs.b = new byte[16];
            for (i = 0; i < 4; i++)
            {
                I.sregs[i] = 0;
            }
            I.ip = 0;
            I.SignVal = 0;
            I.AuxVal = 0;
            I.OverVal = 0;
            I.ZeroVal = 0;
            I.CarryVal = 0;
            I.ParityVal = 0;
            I.TF = false;
            I.IF = false;
            I.DF = false;
            I.MF = false;
            I.int_vector = 0;
            I.pending_irq = 0;
            I.irq_state = 0;
            I.poll_state = false;
            I.no_interrupt = 0;
            //I.irq_callback = save_irqcallback;
            //I.mem = save_mem;
            //I.config = config;
            I.sregs[1] = 0xffff;
            //PC = (I.sregs[1] << 4) + I.ip;
            for (i = 0; i < 256; i++)
            {
                for (j = i, c = 0; j > 0; j >>= 1)
                {
                    if ((j & 1) != 0)
                    {
                        c++;
                    }
                }
                parity_table[i] = ((c & 1) == 0);
            }
            I.ZeroVal = I.ParityVal = 1;
            I.MF = true;
            for (i = 0; i < 256; i++)
            {
                mod_RM.regb[i] = reg_name[(i & 0x38) >> 3];
                mod_RM.regw[i] = (int)((i & 0x38) >> 3);
            }
            for (i = 0xc0; i < 0x100; i++)
            {
                mod_RM.RMw[i] = (int)(i & 7);
                mod_RM.RMb[i] = reg_name[i & 7];
            }
            I.poll_state = true;
        }
        public void nec_interrupt(int int_num, bool md_flag)
        {
            uint dest_seg, dest_off;
            i_pushf();
            I.TF = I.IF = false;
            if (md_flag)
            {
                I.MF = false;
            }
            if (int_num == -1)
            {
                int_num = Cpuint.cpu_irq_callback(cpunum, 0);
                I.irq_state = 0;
                I.pending_irq &= 0xfffffffe;
            }
            dest_off = ReadWord(int_num * 4);
            dest_seg = ReadWord(int_num * 4 + 2);
            PUSH(I.sregs[1]);
            PUSH(I.ip);
            I.ip = (ushort)dest_off;
            I.sregs[1] = (ushort)dest_seg;
            //CHANGE_PC;
        }
        public void nec_trap()
        {
            nec_instruction[fetchop()]();
            nec_interrupt(1, false);
        }
        public void external_int()
        {
            if ((I.pending_irq & 0x02) != 0)
            {
                nec_interrupt(2, false);
                I.pending_irq &= unchecked((uint)(~2));
            }
            else if (I.pending_irq != 0)
            {
                nec_interrupt(-1, false);
            }
        }
        public void SaveStateBinary(BinaryWriter writer)
        {
            int i;
            writer.Write(I.regs.b, 0, 16);
            for (i = 0; i < 4; i++)
            {
                writer.Write(I.sregs[i]);
            }
            writer.Write(I.ip);
            writer.Write(I.TF);
            writer.Write(I.IF);
            writer.Write(I.DF);
            writer.Write(I.MF);
            writer.Write(I.SignVal);
            writer.Write(I.int_vector);
            writer.Write(I.pending_irq);
            writer.Write(I.nmi_state);
            writer.Write(I.irq_state);
            writer.Write(I.poll_state);
            writer.Write(I.AuxVal);
            writer.Write(I.OverVal);
            writer.Write(I.ZeroVal);
            writer.Write(I.CarryVal);
            writer.Write(I.ParityVal);
            writer.Write(I.no_interrupt);
            writer.Write(prefix_base);
            writer.Write(seg_prefix);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(BinaryReader reader)
        {
            int i;
            I.regs.b = reader.ReadBytes(16);
            for (i = 0; i < 4; i++)
            {
                I.sregs[i] = reader.ReadUInt16();
            }
            I.ip = reader.ReadUInt16();
            I.TF = reader.ReadBoolean();
            I.IF = reader.ReadBoolean();
            I.DF = reader.ReadBoolean();
            I.MF = reader.ReadBoolean();
            I.SignVal = reader.ReadInt32();
            I.int_vector = reader.ReadUInt32();
            I.pending_irq = reader.ReadUInt32();
            I.nmi_state = reader.ReadUInt32();
            I.irq_state = reader.ReadUInt32();
            I.poll_state = reader.ReadBoolean();
            I.AuxVal = reader.ReadUInt32();
            I.OverVal = reader.ReadUInt32();
            I.ZeroVal = reader.ReadUInt32();
            I.CarryVal = reader.ReadUInt32();
            I.ParityVal = reader.ReadUInt32();
            I.no_interrupt = reader.ReadByte();
            prefix_base = reader.ReadInt32();
            seg_prefix = reader.ReadInt32();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
    }
    public class V30 : Nec
    {
        public V30()
        {
            nec_init();
            chip_type = 8;
        }
        public override int ExecuteCycles(int cycles)
        {
            return v30_execute(cycles);
        }
        public int v30_execute(int cycles)
        {
            pendingCycles = cycles;
            while (pendingCycles > 0)
            {
                int prevCycles = pendingCycles;
                if (I.pending_irq != 0 && I.no_interrupt == 0)
                {
                    if ((I.pending_irq & NMI_IRQ) != 0)
                    {
                        external_int();
                    }
                    else if (I.IF)
                    {
                        external_int();
                    }
                }
                if (I.no_interrupt != 0)
                {
                    I.no_interrupt--;
                }
                iNOP = fetchop();
                nec_instruction[iNOP]();
                int delta = prevCycles - pendingCycles;
                totalExecutedCycles += (ulong)delta;
            }
            return cycles - pendingCycles;
        }
    }
    public class V33 : Nec
    {
        public V33()
        {
            nec_init();
            chip_type = 0;
        }
        public override int ExecuteCycles(int cycles)
        {
            return v33_execute(cycles);
        }
        public int v33_execute(int cycles)
        {
            pendingCycles = cycles;
            while (pendingCycles > 0)
            {
                int prevCycles = pendingCycles;
                if (I.pending_irq != 0 && I.no_interrupt == 0)
                {
                    if ((I.pending_irq & NMI_IRQ) != 0)
                    {
                        external_int();
                    }
                    else if (I.IF)
                    {
                        external_int();
                    }
                }
                if (I.no_interrupt != 0)
                {
                    I.no_interrupt--;
                }
                iNOP = fetchop();
                nec_instruction[iNOP]();
                int delta = prevCycles - pendingCycles;
                totalExecutedCycles += (ulong)delta;
            }
            return cycles - pendingCycles;
        }
    }
}
