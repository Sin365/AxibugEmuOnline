namespace cpu.m6502
{
    public partial class M6502
    {
        private void ANC(int tmp)
        {
            p &= (byte)~F_C;
            a = (byte)(a & tmp);
            if ((a & 0x80) != 0)
            {
                p |= F_C;
            }
            SET_NZ(a);
        }
        private void ASR(ref int tmp)
        {
            tmp &= a;
            LSR(ref tmp);
        }
        private void AST(int tmp)
        {
            sp.LowByte &= (byte)tmp;
            a = x = sp.LowByte;
            SET_NZ(a);
        }
        private void ARR(ref int tmp)
        {
            if ((p & F_D) != 0)
            {
                int lo, hi, t;
                tmp &= a;
                t = tmp;
                hi = tmp & 0xf0;
                lo = tmp & 0x0f;
                if ((p & F_C) != 0)
                {
                    tmp = (tmp >> 1) | 0x80;
                    p |= F_N;
                }
                else
                {
                    tmp >>= 1;
                    p &= (byte)~F_N;
                }
                if (tmp != 0)
                {
                    p &= (byte)~F_Z;
                }
                else
                {
                    p |= F_Z;
                }
                if (((t ^ tmp) & 0x40) != 0)
                {
                    p |= F_V;
                }
                else
                {
                    p &= (byte)~F_V;
                }
                if (lo + (lo & 0x01) > 0x05)
                {
                    tmp = (tmp & 0xf0) | ((tmp + 6) & 0xf);
                }
                if (hi + (hi & 0x10) > 0x50)
                {
                    p |= F_C;
                    tmp = (tmp + 0x60) & 0xff;
                }
                else
                {
                    p &= (byte)~F_C;
                }
            }
            else
            {
                tmp &= a;
                ROR(ref tmp);
                p &= (byte)~(F_V | F_C);
                if ((tmp & 0x40) != 0)
                {
                    p |= F_C;
                }
                if ((tmp & 0x60) == 0x20 || (tmp & 0x60) == 0x40)
                {
                    p |= F_V;
                }
            }
        }
        private void ASX(int tmp)
        {
            p &= (byte)~F_C;
            x &= a;
            if (x >= tmp)
            {
                p |= F_C;
            }
            x = (byte)(x - tmp);
            SET_NZ(x);
        }
        private void AXA(int tmp)
        {
            a = (byte)((a | 0xee) & x & tmp);
            SET_NZ(a);
        }
        private void DCP(ref int tmp)
        {
            tmp = (byte)(tmp - 1);
            p &= (byte)~F_C;
            if (a >= tmp)
            {
                p |= F_C;
            }
            SET_NZ((byte)(a - tmp));
        }
        private void ISB(ref int tmp)
        {
            tmp = (byte)(tmp + 1);
            SBC(tmp);
        }
        private void LAX(int tmp)
        {
            a = x = (byte)tmp;
            SET_NZ(a);
        }
        private void OAL(int tmp)
        {
            a = x = (byte)((a | 0xee) & tmp);
            SET_NZ(a);
        }
        private void RLA(ref int tmp)
        {
            tmp = (tmp << 1) | (p & F_C);
            p = (byte)((p & ~F_C) | ((tmp >> 8) & F_C));
            tmp = (byte)tmp;
            a &= (byte)tmp;
            SET_NZ(a);
        }
        private void RRA(ref int tmp)
        {
            tmp |= (p & F_C) << 8;
            p = (byte)((p & ~F_C) | (tmp & F_C));
            tmp = (byte)(tmp >> 1);
            ADC(tmp);
        }
        private void SAX(ref int tmp)
        {
            tmp = a & x;
        }
        private void SLO(ref int tmp)
        {
            p = (byte)((p & ~F_C) | ((tmp >> 7) & F_C));
            tmp = (byte)(tmp << 1);
            a |= (byte)tmp;
            SET_NZ(a);
        }
        private void SRE(ref int tmp)
        {
            p = (byte)((p & ~F_C) | (tmp & F_C));
            tmp = (byte)tmp >> 1;
            a ^= (byte)tmp;
            SET_NZ(a);
        }
        private void SAH(ref int tmp)
        {
            tmp = a & x & (ea.HighByte + 1);
        }
        private void SSH(ref int tmp)
        {
            sp.LowByte = (byte)(a & x);
            tmp = sp.LowByte & (ea.HighByte + 1);
        }
        private void SXH(ref int tmp)
        {
            tmp = x & (ea.HighByte + 1);
        }
        private void SYH(ref int tmp)
        {
            tmp = y & (ea.HighByte + 1);
        }
        private void KIL()
        {
            pc.LowWord--;
        }

    }
}
