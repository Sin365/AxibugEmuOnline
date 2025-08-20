using MAME.Core;

namespace cpu.m6502
{
    public partial class M6502
    {
        public byte F_C = 0x01, F_Z = 0x02, F_I = 0x04, F_D = 0x08, F_B = 0x10, F_T = 0x20, F_V = 0x40, F_N = 0x80;
        private void SET_NZ(byte n)
        {
            if (n == 0)
            {
                p = (byte)((p & ~F_N) | F_Z);
            }
            else
            {
                p = (byte)((p & ~(F_N | F_Z)) | (n & F_N));
            }
        }
        private void SET_Z(byte n)
        {
            if ((n) == 0)
            {
                p |= F_Z;
            }
            else
            {
                p &= (byte)(~F_Z);
            }
        }
        private byte RDOPARG()
        {
            byte b1;
            b1 = ReadOpArg(pc.LowWord++);
            pendingCycles -= 1;
            return b1;
        }
        private byte RDMEM(ushort addr)
        {
            byte b1;
            b1 = ReadMemory(addr);
            pendingCycles -= 1;
            return b1;
        }
        private void WRMEM(ushort addr, byte data)
        {
            WriteMemory(addr, data);
            pendingCycles -= 1;
        }
        private void BRA(bool cond)
        {
            sbyte tmp2 = (sbyte)RDOPARG();
            if (cond)
            {
                RDMEM(pc.LowWord);
                ea.LowWord = (ushort)(pc.LowWord + (sbyte)tmp2);
                if (ea.HighByte != pc.HighByte)
                {
                    RDMEM((ushort)((pc.HighByte << 8) | ea.LowByte));
                }
                pc.d = ea.d;
            }
        }
        private void EA_ZPG()
        {
            zp.LowByte = RDOPARG();
            ea.d = zp.d;
        }
        private void EA_ZPX()
        {
            zp.LowByte = RDOPARG();
            RDMEM((ushort)zp.d);
            zp.LowByte = (byte)(x + zp.LowByte);
            ea.d = zp.d;
        }
        private void EA_ZPY()
        {
            zp.LowByte = RDOPARG();
            RDMEM((ushort)zp.d);
            zp.LowByte = (byte)(y + zp.LowByte);
            ea.d = zp.d;
        }
        private void EA_ABS()
        {
            ea.LowByte = RDOPARG();
            ea.HighByte = RDOPARG();
        }
        private void EA_ABX_P()
        {
            EA_ABS();
            if (ea.LowByte + x > 0xff)
            {
                RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + x) & 0xff)));
            }
            ea.LowWord += x;
        }
        private void EA_ABX_NP()
        {
            EA_ABS();
            RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + x) & 0xff)));
            ea.LowWord += x;
        }
        private void EA_ABY_P()
        {
            EA_ABS();
            if (ea.LowByte + y > 0xff)
            {
                RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + y) & 0xff)));
            }
            ea.LowWord += y;
        }
        private void EA_ABY_NP()
        {
            EA_ABS();
            RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + y) & 0xff)));
            ea.LowWord += y;
        }
        private void EA_IDX()
        {
            zp.LowByte = RDOPARG();
            RDMEM((ushort)zp.d);
            zp.LowByte = (byte)(zp.LowByte + x);
            ea.LowByte = RDMEM((ushort)zp.d);
            zp.LowByte++;
            ea.HighByte = RDMEM((ushort)zp.d);
        }
        private void EA_IDY_P()
        {
            zp.LowByte = RDOPARG();
            ea.LowByte = RDMEM((ushort)zp.d);
            zp.LowByte++;
            ea.HighByte = RDMEM((ushort)zp.d);
            if (ea.LowByte + y > 0xff)
            {
                RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + y) & 0xff)));
            }
            ea.LowWord += y;
        }
        private void EA_IDY_NP()
        {
            zp.LowByte = RDOPARG();
            ea.LowByte = RDMEM((ushort)zp.d);
            zp.LowByte++;
            ea.HighByte = RDMEM((ushort)zp.d);
            RDMEM((ushort)((ea.HighByte << 8) | ((ea.LowByte + y) & 0xff)));
            ea.LowWord += y;
        }
        private void EA_ZPI()
        {
            zp.LowByte = RDOPARG();
            ea.LowByte = RDMEM((ushort)zp.d);
            zp.LowByte++;
            ea.HighByte = RDMEM((ushort)zp.d);
        }
        private void EA_IND()
        {
            byte tmp;
            EA_ABS();
            tmp = RDMEM((ushort)ea.d);
            ea.LowByte++;
            ea.HighByte = RDMEM((ushort)ea.d);
            ea.LowByte = tmp;
        }
        private void PUSH(byte Rg)
        {
            WRMEM((ushort)sp.d, Rg);
            sp.LowByte--;
        }
        private void PULL(ref byte Rg)
        {
            sp.LowByte++;
            Rg = RDMEM((ushort)sp.d);
        }
        private void ADC(int tmp)
        {
            if ((p & F_D) != 0)
            {
                int c = (p & F_C);
                int lo = (a & 0x0f) + (tmp & 0x0f) + c;
                int hi = (a & 0xf0) + (tmp & 0xf0);
                p &= (byte)(~(F_V | F_C | F_N | F_Z));
                if (((lo + hi) & 0xff) == 0)
                {
                    p |= F_Z;
                }
                if (lo > 0x09)
                {
                    hi += 0x10;
                    lo += 0x06;
                }
                if ((hi & 0x80) != 0)
                {
                    p |= F_N;
                }
                if ((~(a ^ tmp) & (a ^ hi) & F_N) != 0)
                {
                    p |= F_V;
                }
                if (hi > 0x90)
                {
                    hi += 0x60;
                }
                if ((hi & 0xff00) != 0)
                {
                    p |= F_C;
                }
                a = (byte)((lo & 0x0f) + (hi & 0xf0));
            }
            else
            {
                int c = (p & F_C);
                int sum = a + tmp + c;
                p &= (byte)(~(F_V | F_C));
                if ((~(a ^ tmp) & (a ^ sum) & F_N) != 0)
                {
                    p |= F_V;
                }
                if ((sum & 0xff00) != 0)
                {
                    p |= F_C;
                }
                a = (byte)sum;
                SET_NZ(a);
            }
        }
        private void AND(int tmp)
        {
            a = (byte)(a & tmp);
            SET_NZ(a);
        }
        private void ASL(ref int tmp)
        {
            p = (byte)((p & ~F_C) | ((tmp >> 7) & F_C));
            tmp = (byte)(tmp << 1);
            SET_NZ((byte)tmp);
        }
        private void BCC()
        {
            BRA((p & F_C) == 0);
        }
        private void BCS()
        {
            BRA((p & F_C) != 0);
        }
        private void BEQ()
        {
            BRA((p & F_Z) != 0);
        }
        private void BIT(int tmp)
        {
            p &= (byte)(~(F_N | F_V | F_Z));
            p |= (byte)(tmp & (F_N | F_V));
            if ((tmp & a) == 0)
            {
                p |= F_Z;
            }
        }
        private void BMI()
        {
            BRA((p & F_N) != 0);
        }
        private void BNE()
        {
            BRA((p & F_Z) == 0);
        }
        private void BPL()
        {
            BRA((p & F_N) == 0);
        }
        private void BRK()
        {
            RDOPARG();
            PUSH(pc.HighByte);
            PUSH(pc.LowByte);
            PUSH((byte)(p | F_B));
            p = ((byte)(p | F_I));
            pc.LowByte = RDMEM(M6502_IRQ_VEC);
            pc.HighByte = RDMEM((ushort)(M6502_IRQ_VEC + 1));
        }
        private void BVC()
        {
            BRA((p & F_V) == 0);
        }
        private void BVS()
        {
            BRA((p & F_V) != 0);
        }
        private void CLC()
        {
            p &= (byte)~F_C;
        }
        private void CLD()
        {
            p &= (byte)~F_D;
        }
        private void CLI()
        {
            if ((irq_state != (byte)LineState.CLEAR_LINE) && ((p & F_I) != 0))
            {
                after_cli = 1;
            }
            p &= (byte)~F_I;
        }
        private void CLV()
        {
            p &= (byte)~F_V;
        }
        private void CMP(int tmp)
        {
            p &= (byte)~F_C;
            if (a >= tmp)
            {
                p |= F_C;
            }
            SET_NZ((byte)(a - tmp));
        }
        private void CPX(int tmp)
        {
            p &= (byte)~F_C;
            if (x >= tmp)
            {
                p |= F_C;
            }
            SET_NZ((byte)(x - tmp));
        }
        private void CPY(int tmp)
        {
            p &= (byte)~F_C;
            if (y >= tmp)
            {
                p |= F_C;
            }
            SET_NZ((byte)(y - tmp));
        }
        private void DEC(ref int tmp)
        {
            tmp = (byte)(tmp - 1);
            SET_NZ((byte)tmp);
        }
        private void DEX()
        {
            x = (byte)(x - 1);
            SET_NZ(x);
        }
        private void DEY()
        {
            y = (byte)(y - 1);
            SET_NZ(y);
        }
        private void EOR(int tmp)
        {
            a = (byte)(a ^ tmp);
            SET_NZ(a);
        }
        private void INC(ref int tmp)
        {
            tmp = (byte)(tmp + 1);
            SET_NZ((byte)tmp);
        }
        private void INX()
        {
            x = (byte)(x + 1);
            SET_NZ(x);
        }
        private void INY()
        {
            y = (byte)(y + 1);
            SET_NZ(y);
        }
        private void JMP()
        {
            if (ea.d == ppc.d && pending_irq == 0 && after_cli == 0)
            {
                if (pendingCycles > 0)
                {
                    pendingCycles = 0;
                }
            }
            pc.d = ea.d;
        }
        private void JSR()
        {
            ea.LowByte = RDOPARG();
            RDMEM((ushort)sp.d);
            PUSH(pc.HighByte);
            PUSH(pc.LowByte);
            ea.HighByte = RDOPARG();
            pc.d = ea.d;
        }
        private void LDA(int tmp)
        {
            a = (byte)tmp;
            SET_NZ(a);
        }
        private void LDX(int tmp)
        {
            x = (byte)tmp;
            SET_NZ(x);
        }
        private void LDY(int tmp)
        {
            y = (byte)tmp;
            SET_NZ(y);
        }
        private void LSR(ref int tmp)
        {
            p = (byte)((p & ~F_C) | (tmp & F_C));
            tmp = (byte)tmp >> 1;
            SET_NZ((byte)tmp);
        }
        private void ORA(int tmp)
        {
            a = (byte)(a | tmp);
            SET_NZ(a);
        }
        private void PHA()
        {
            PUSH(a);
        }
        private void PHP()
        {
            PUSH(p);
        }
        private void PLA()
        {
            RDMEM((ushort)sp.d);
            PULL(ref a);
            SET_NZ(a);
        }
        private void PLP()
        {
            RDMEM((ushort)sp.d);
            if ((p & F_I) != 0)
            {
                PULL(ref p);
                if ((irq_state != (byte)LineState.CLEAR_LINE) && ((p & F_I) == 0))
                {
                    after_cli = 1;
                }
            }
            else
            {
                PULL(ref p);
            }
            p |= (byte)(F_T | F_B);
        }
        private void ROL(ref int tmp)
        {
            tmp = (tmp << 1) | (p & F_C);
            p = (byte)((p & ~F_C) | ((tmp >> 8) & F_C));
            tmp = (byte)tmp;
            SET_NZ((byte)tmp);
        }
        private void ROR(ref int tmp)
        {
            tmp |= (p & F_C) << 8;
            p = (byte)((p & ~F_C) | (tmp & F_C));
            tmp = (byte)(tmp >> 1);
            SET_NZ((byte)tmp);
        }
        private void RTI()
        {
            RDOPARG();
            RDMEM((ushort)sp.d);
            PULL(ref p);
            PULL(ref pc.LowByte);
            PULL(ref pc.HighByte);
            p |= (byte)(F_T | F_B);
            if ((irq_state != (byte)LineState.CLEAR_LINE) && ((p & F_I) == 0))
            {
                after_cli = 1;
            }
        }
        private void RTS()
        {
            RDOPARG();
            RDMEM((ushort)sp.d);
            PULL(ref pc.LowByte);
            PULL(ref pc.HighByte);
            RDMEM(pc.LowWord);
            pc.LowWord++;
        }
        private void SBC(int tmp)
        {
            if ((p & F_D) != 0)
            {
                int c = (p & F_C) ^ F_C;
                int sum = a - tmp - c;
                int lo = (a & 0x0f) - (tmp & 0x0f) - c;
                int hi = (a & 0xf0) - (tmp & 0xf0);
                if ((lo & 0x10) != 0)
                {
                    lo -= 6;
                    hi--;
                }
                p &= (byte)~(F_V | F_C | F_Z | F_N);
                if (((a ^ tmp) & (a ^ sum) & F_N) != 0)
                {
                    p |= F_V;
                }
                if ((hi & 0x0100) != 0)
                {
                    hi -= 0x60;
                }
                if ((sum & 0xff00) == 0)
                {
                    p |= F_C;
                }
                if (((a - tmp - c) & 0xff) == 0)
                {
                    p |= F_Z;
                }
                if (((a - tmp - c) & 0x80) != 0)
                {
                    p |= F_N;
                }
                a = (byte)((lo & 0x0f) | (hi & 0xf0));
            }
            else
            {
                int c = (p & F_C) ^ F_C;
                int sum = a - tmp - c;
                p &= (byte)~(F_V | F_C);
                if (((a ^ tmp) & (a ^ sum) & F_N) != 0)
                {
                    p |= F_V;
                }
                if ((sum & 0xff00) == 0)
                {
                    p |= F_C;
                }
                a = (byte)sum;
                SET_NZ(a);
            }
        }
        private void SEC()
        {
            p |= F_C;
        }
        private void SED()
        {
            p |= F_D;
        }
        private void SEI()
        {
            p |= F_I;
        }
        private void STA(ref int tmp)
        {
            tmp = a;
        }
        private void STX(ref int tmp)
        {
            tmp = x;
        }
        private void STY(ref int tmp)
        {
            tmp = y;
        }
        private void TAX()
        {
            x = a;
            SET_NZ(x);
        }
        private void TAY()
        {
            y = a;
            SET_NZ(y);
        }
        private void TSX()
        {
            x = sp.LowByte;
            SET_NZ(x);
        }
        private void TXA()
        {
            a = x;
            SET_NZ(a);
        }
        private void TXS()
        {
            sp.LowByte = x;
        }
        private void TYA()
        {
            a = y;
            SET_NZ(a);
        }
    }
}
