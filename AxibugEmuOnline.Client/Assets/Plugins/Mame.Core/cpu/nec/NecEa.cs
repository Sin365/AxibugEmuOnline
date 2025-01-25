namespace cpu.nec
{
    partial class Nec
    {
        static int EA;
        static ushort EO;
        static ushort E16;
        int EA_000()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_001()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_002()
        {
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100);
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_003()
        {
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100);
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_004()
        {
            EO = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_005()
        {
            EO = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_006()
        {
            EO = FETCH();
            EO += (ushort)(FETCH() << 8);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_007()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_100()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_101()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_102()
        {
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_103()
        {
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_104()
        {
            EO = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_105()
        {
            EO = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_106()
        {
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_107()
        {
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + (sbyte)FETCH());
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_200()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100 + (short)E16);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_201()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100 + (short)E16);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_202()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[12] + I.regs.b[13] * 0x100 + (short)E16);
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_203()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + I.regs.b[14] + I.regs.b[15] * 0x100 + (short)E16);
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_204()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (short)E16);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_205()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (short)E16);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
        int EA_206()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[10] + I.regs.b[11] * 0x100 + (short)E16);
            EA = DefaultBase(2, I) + EO;
            return EA;
        }
        int EA_207()
        {
            E16 = FETCH();
            E16 += (ushort)(FETCH() << 8);
            EO = (ushort)(I.regs.b[6] + I.regs.b[7] * 0x100 + (short)E16);
            EA = DefaultBase(3, I) + EO;
            return EA;
        }
    }
}
