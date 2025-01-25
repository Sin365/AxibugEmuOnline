namespace cpu.nec
{
    partial class Nec
    {
        ushort RegWord(int ModRM)
        {
            return (ushort)(I.regs.b[mod_RM.regw[ModRM] * 2] + I.regs.b[mod_RM.regw[ModRM] * 2 + 1] * 0x100);// I.regs.w[mod_RM.regw[ModRM]];
        }
        byte RegByte(int ModRM)
        {
            return I.regs.b[mod_RM.regb[ModRM]];
        }
        ushort GetRMWord(int ModRM)
        {
            return (ushort)(ModRM >= 0xc0 ? I.regs.b[mod_RM.RMw[ModRM] * 2] + I.regs.b[mod_RM.RMw[ModRM] * 2 + 1] * 0x100 : ReadWord(GetEA[ModRM]()));
        }
        void PutbackRMWord(int ModRM, ushort val)
        {
            if (ModRM >= 0xc0)
            {
                //I.regs.w[mod_RM.RMw[ModRM]] = val;
                I.regs.b[mod_RM.RMw[ModRM] * 2] = (byte)(val % 0x100);
                I.regs.b[mod_RM.RMw[ModRM] * 2 + 1] = (byte)(val / 0x100);
            }
            else
            {
                WriteWord(EA, val);
            }
        }
        ushort GetnextRMWord()
        {
            return ReadWord((EA & 0xf0000) | ((EA + 2) & 0xffff));
        }
        void PutRMWord(int ModRM, ushort val)
        {
            if (ModRM >= 0xc0)
            {
                //I.regs.w[mod_RM.RMw[ModRM]] = val;
                I.regs.b[mod_RM.RMw[ModRM] * 2] = (byte)(val % 0x100);
                I.regs.b[mod_RM.RMw[ModRM] * 2 + 1] = (byte)(val / 0x100);
            }
            else
            {
                WriteWord(GetEA[ModRM](), val);
            }
        }
        void PutImmRMWord(int ModRM)
        {
            ushort val;
            if (ModRM >= 0xc0)
            {
                //I.regs.w[mod_RM.RMw[ModRM]] = FETCHWORD();
                ushort w = FETCHWORD();
                I.regs.b[mod_RM.RMw[ModRM] * 2] = (byte)(w % 0x100);
                I.regs.b[mod_RM.RMw[ModRM] * 2 + 1] = (byte)(w / 0x100);
            }
            else
            {
                EA = GetEA[ModRM]();
                val = FETCHWORD();
                WriteWord(EA, val);
            }
        }
        byte GetRMByte(int ModRM)
        {
            return ((ModRM) >= 0xc0 ? I.regs.b[mod_RM.RMb[ModRM]] : ReadByte(GetEA[ModRM]()));
        }
        void PutRMByte(int ModRM, byte val)
        {
            if (ModRM >= 0xc0)
            {
                I.regs.b[mod_RM.RMb[ModRM]] = val;
            }
            else
            {
                WriteByte(GetEA[ModRM](), val);
            }
        }
        void PutImmRMByte(int ModRM)
        {
            if (ModRM >= 0xc0)
            {
                I.regs.b[mod_RM.RMb[ModRM]] = FETCH();
            }
            else
            {
                EA = GetEA[ModRM]();
                WriteByte(EA, FETCH());
            }
        }
        void PutbackRMByte(int ModRM, byte val)
        {
            if (ModRM >= 0xc0)
            {
                I.regs.b[mod_RM.RMb[ModRM]] = val;
            }
            else
            {
                WriteByte(EA, val);
            }
        }
        void DEF_br8(out int ModRM, out byte src, out byte dst)
        {
            ModRM = FETCH();
            src = RegByte(ModRM);
            dst = GetRMByte(ModRM);
        }
        void DEF_wr16(out int ModRM, out ushort src, out ushort dst)
        {
            ModRM = FETCH();
            src = RegWord(ModRM);
            dst = GetRMWord(ModRM);
        }
        void DEF_r8b(out int ModRM, out byte src, out byte dst)
        {
            ModRM = FETCH();
            dst = RegByte(ModRM);
            src = GetRMByte(ModRM);
        }
        void DEF_r16w(out int ModRM, out ushort src, out ushort dst)
        {
            ModRM = FETCH();
            dst = RegWord(ModRM);
            src = GetRMWord(ModRM);
        }
        void DEF_ald8(out byte src, out byte dst)
        {
            src = FETCH();
            dst = I.regs.b[0];
        }
        void DEF_axd16(out ushort src, out ushort dst)
        {
            src = FETCH();
            dst = (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100);// I.regs.w[0];
            src += (ushort)(FETCH() << 8);
        }
    }
}
