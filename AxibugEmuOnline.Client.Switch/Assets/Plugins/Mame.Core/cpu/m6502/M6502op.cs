namespace cpu.m6502
{
    public partial class M6502
    {
        protected void m6502_00() { BRK(); }
        protected void m6502_20() { JSR(); }
        protected void m6502_40() { RTI(); }
        protected void m6502_60() { RTS(); }
        protected void m6502_80() { int tmp; tmp = RDOPARG(); }
        protected void m6502_a0() { int tmp; tmp = RDOPARG(); LDY(tmp); }
        protected void m6502_c0() { int tmp; tmp = RDOPARG(); CPY(tmp); }
        protected void m6502_e0() { int tmp; tmp = RDOPARG(); CPX(tmp); }

        protected void m6502_10() { BPL(); }
        protected void m6502_30() { BMI(); }
        protected void m6502_50() { BVC(); }
        protected void m6502_70() { BVS(); }
        protected void m6502_90() { BCC(); }
        protected void m6502_b0() { BCS(); }
        protected void m6502_d0() { BNE(); }
        protected void m6502_f0() { BEQ(); }

        protected void m6502_01() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; ORA(tmp); }
        protected void m6502_21() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; AND(tmp); }
        protected void m6502_41() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; EOR(tmp); }
        protected void m6502_61() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; ADC(tmp); }
        protected void m6502_81() { int tmp = 0; STA(ref tmp); EA_IDX(); wrmem_id((ushort)ea.d, (byte)tmp); pendingCycles -= 1; }
        protected void m6502_a1() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; LDA(tmp); }
        protected void m6502_c1() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; CMP(tmp); }
        protected void m6502_e1() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; SBC(tmp); }

        protected void m6502_11() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; ORA(tmp); }
        protected void m6502_31() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; AND(tmp); }
        protected void m6502_51() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; EOR(tmp); }
        protected void m6502_71() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; ADC(tmp); }
        protected void m6502_91() { int tmp = 0; STA(ref tmp); EA_IDY_NP(); wrmem_id((ushort)ea.d, (byte)tmp); pendingCycles -= 1; }
        protected void m6502_b1() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; LDA(tmp); }
        protected void m6502_d1() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; CMP(tmp); }
        protected void m6502_f1() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; SBC(tmp); }

        protected void m6502_02() { KIL(); }
        protected void m6502_22() { KIL(); }
        protected void m6502_42() { KIL(); }
        protected void m6502_62() { KIL(); }
        protected void m6502_82() { int tmp; tmp = RDOPARG(); }
        protected void m6502_a2() { int tmp; tmp = RDOPARG(); LDX(tmp); }
        protected void m6502_c2() { int tmp; tmp = RDOPARG(); }
        protected void m6502_e2() { int tmp; tmp = RDOPARG(); }

        protected void m6502_12() { KIL(); }
        protected void m6502_32() { KIL(); }
        protected void m6502_52() { KIL(); }
        protected void m6502_72() { KIL(); }
        protected void m6502_92() { KIL(); }
        protected void m6502_b2() { KIL(); }
        protected void m6502_d2() { KIL(); }
        protected void m6502_f2() { KIL(); }

        protected void m6502_03() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_23() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_43() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_63() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_83() { int tmp = 0; SAX(ref tmp); EA_IDX(); wrmem_id((ushort)ea.d, (byte)tmp); pendingCycles -= 1; }
        protected void m6502_a3() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; LAX(tmp); }
        protected void m6502_c3() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_e3() { int tmp; EA_IDX(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_13() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_33() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_53() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_73() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_93() { int tmp = 0; EA_IDY_NP(); SAH(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b3() { int tmp; EA_IDY_P(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; LAX(tmp); }
        protected void m6502_d3() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_f3() { int tmp; EA_IDY_NP(); tmp = rdmem_id((ushort)ea.d); pendingCycles -= 1; WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_04() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_24() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); BIT(tmp); }
        protected void m6502_44() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_64() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_84() { int tmp = 0; STY(ref tmp); EA_ZPG(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_a4() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); LDY(tmp); }
        protected void m6502_c4() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); CPY(tmp); }
        protected void m6502_e4() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); CPX(tmp); }

        protected void m6502_14() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_34() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_54() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_74() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_94() { int tmp = 0; STY(ref tmp); EA_ZPX(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b4() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); LDY(tmp); }
        protected void m6502_d4() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_f4() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); }

        protected void m6502_05() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); ORA(tmp); }
        protected void m6502_25() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); AND(tmp); }
        protected void m6502_45() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); EOR(tmp); }
        protected void m6502_65() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); ADC(tmp); }
        protected void m6502_85() { int tmp = 0; STA(ref tmp); EA_ZPG(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_a5() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); LDA(tmp); }
        protected void m6502_c5() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); CMP(tmp); }
        protected void m6502_e5() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); SBC(tmp); }

        protected void m6502_15() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); ORA(tmp); }
        protected void m6502_35() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); AND(tmp); }
        protected void m6502_55() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); EOR(tmp); }
        protected void m6502_75() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); ADC(tmp); }
        protected void m6502_95() { int tmp = 0; STA(ref tmp); EA_ZPX(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b5() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); LDA(tmp); }
        protected void m6502_d5() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); CMP(tmp); }
        protected void m6502_f5() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); SBC(tmp); }

        protected void m6502_06() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ASL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_26() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_46() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); LSR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_66() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_86() { int tmp = 0; STX(ref tmp); EA_ZPG(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_a6() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); LDX(tmp); }
        protected void m6502_c6() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DEC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_e6() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); INC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_16() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ASL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_36() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_56() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); LSR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_76() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_96() { int tmp = 0; STX(ref tmp); EA_ZPY(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b6() { int tmp; EA_ZPY(); tmp = RDMEM((ushort)ea.d); LDX(tmp); }
        protected void m6502_d6() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DEC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_f6() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); INC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_07() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_27() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_47() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_67() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_87() { int tmp = 0; SAX(ref tmp); EA_ZPG(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_a7() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); LAX(tmp); }
        protected void m6502_c7() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_e7() { int tmp; EA_ZPG(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_17() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_37() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_57() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_77() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_97() { int tmp = 0; SAX(ref tmp); EA_ZPY(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b7() { int tmp; EA_ZPY(); tmp = RDMEM((ushort)ea.d); LAX(tmp); }
        protected void m6502_d7() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_f7() { int tmp; EA_ZPX(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_08() { RDMEM(pc.LowWord); PHP(); }
        protected void m6502_28() { RDMEM(pc.LowWord); PLP(); }
        protected void m6502_48() { RDMEM(pc.LowWord); PHA(); }
        protected void m6502_68() { RDMEM(pc.LowWord); PLA(); }
        protected void m6502_88() { RDMEM(pc.LowWord); DEY(); }
        protected void m6502_a8() { RDMEM(pc.LowWord); TAY(); }
        protected void m6502_c8() { RDMEM(pc.LowWord); INY(); }
        protected void m6502_e8() { RDMEM(pc.LowWord); INX(); }

        protected void m6502_18() { RDMEM(pc.LowWord); CLC(); }
        protected void m6502_38() { RDMEM(pc.LowWord); SEC(); }
        protected void m6502_58() { RDMEM(pc.LowWord); CLI(); }
        protected void m6502_78() { RDMEM(pc.LowWord); SEI(); }
        protected void m6502_98() { RDMEM(pc.LowWord); TYA(); }
        protected void m6502_b8() { RDMEM(pc.LowWord); CLV(); }
        protected void m6502_d8() { RDMEM(pc.LowWord); CLD(); }
        protected void m6502_f8() { RDMEM(pc.LowWord); SED(); }

        protected void m6502_09() { int tmp; tmp = RDOPARG(); ORA(tmp); }
        protected void m6502_29() { int tmp; tmp = RDOPARG(); AND(tmp); }
        protected void m6502_49() { int tmp; tmp = RDOPARG(); EOR(tmp); }
        protected void m6502_69() { int tmp; tmp = RDOPARG(); ADC(tmp); }
        protected void m6502_89() { int tmp; tmp = RDOPARG(); }
        protected void m6502_a9() { int tmp; tmp = RDOPARG(); LDA(tmp); }
        protected void m6502_c9() { int tmp; tmp = RDOPARG(); CMP(tmp); }
        protected void m6502_e9() { int tmp; tmp = RDOPARG(); SBC(tmp); }

        protected void m6502_19() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); ORA(tmp); }
        protected void m6502_39() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); AND(tmp); }
        protected void m6502_59() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); EOR(tmp); }
        protected void m6502_79() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); ADC(tmp); }
        protected void m6502_99() { int tmp = 0; STA(ref tmp); EA_ABY_NP(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_b9() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); LDA(tmp); }
        protected void m6502_d9() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); CMP(tmp); }
        protected void m6502_f9() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); SBC(tmp); }

        protected void m6502_0a() { int tmp; RDMEM(pc.LowWord); tmp = a; ASL(ref tmp); a = (byte)tmp; }
        protected void m6502_2a() { int tmp; RDMEM(pc.LowWord); tmp = a; ROL(ref tmp); a = (byte)tmp; }
        protected void m6502_4a() { int tmp; RDMEM(pc.LowWord); tmp = a; LSR(ref tmp); a = (byte)tmp; }
        protected void m6502_6a() { int tmp; RDMEM(pc.LowWord); tmp = a; ROR(ref tmp); a = (byte)tmp; }
        protected void m6502_8a() { RDMEM(pc.LowWord); TXA(); }
        protected void m6502_aa() { RDMEM(pc.LowWord); TAX(); }
        protected void m6502_ca() { RDMEM(pc.LowWord); DEX(); }
        protected void m6502_ea() { RDMEM(pc.LowWord); }

        protected void m6502_1a() { RDMEM(pc.LowWord); }
        protected void m6502_3a() { RDMEM(pc.LowWord); }
        protected void m6502_5a() { RDMEM(pc.LowWord); }
        protected void m6502_7a() { RDMEM(pc.LowWord); }
        protected void m6502_9a() { RDMEM(pc.LowWord); TXS(); }
        protected void m6502_ba() { RDMEM(pc.LowWord); TSX(); }
        protected void m6502_da() { RDMEM(pc.LowWord); }
        protected void m6502_fa() { RDMEM(pc.LowWord); }

        protected void m6502_0b() { int tmp; tmp = RDOPARG(); ANC(tmp); }
        protected void m6502_2b() { int tmp; tmp = RDOPARG(); ANC(tmp); }
        protected void m6502_4b() { int tmp; tmp = RDOPARG(); ASR(ref tmp); a = (byte)tmp; }
        protected void m6502_6b() { int tmp; tmp = RDOPARG(); ARR(ref tmp); a = (byte)tmp; }
        protected void m6502_8b() { int tmp; tmp = RDOPARG(); AXA(tmp); }
        protected void m6502_ab() { int tmp; tmp = RDOPARG(); OAL(tmp); }
        protected void m6502_cb() { int tmp; tmp = RDOPARG(); ASX(tmp); }
        protected void m6502_eb() { int tmp; tmp = RDOPARG(); SBC(tmp); }

        protected void m6502_1b() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_3b() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_5b() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_7b() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_9b() { int tmp = 0; EA_ABY_NP(); SSH(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_bb() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); AST(tmp); }
        protected void m6502_db() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_fb() { int tmp; EA_ABY_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_0c() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_2c() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); BIT(tmp); }
        protected void m6502_4c() { EA_ABS(); JMP(); }
        protected void m6502_6c() { int tmp; EA_IND(); JMP(); }
        protected void m6502_8c() { int tmp = 0; STY(ref tmp); EA_ABS(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ac() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); LDY(tmp); }
        protected void m6502_cc() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); CPY(tmp); }
        protected void m6502_ec() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); CPX(tmp); }

        protected void m6502_1c() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_3c() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_5c() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_7c() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_9c() { int tmp = 0; EA_ABX_NP(); SYH(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_bc() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); LDY(tmp); }
        protected void m6502_dc() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }
        protected void m6502_fc() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); }

        protected void m6502_0d() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); ORA(tmp); }
        protected void m6502_2d() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); AND(tmp); }
        protected void m6502_4d() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); EOR(tmp); }
        protected void m6502_6d() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); ADC(tmp); }
        protected void m6502_8d() { int tmp = 0; STA(ref tmp); EA_ABS(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ad() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); LDA(tmp); }
        protected void m6502_cd() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); CMP(tmp); }
        protected void m6502_ed() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); SBC(tmp); }

        protected void m6502_1d() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); ORA(tmp); }
        protected void m6502_3d() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); AND(tmp); }
        protected void m6502_5d() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); EOR(tmp); }
        protected void m6502_7d() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); ADC(tmp); }
        protected void m6502_9d() { int tmp = 0; STA(ref tmp); EA_ABX_NP(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_bd() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); LDA(tmp); }
        protected void m6502_dd() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); CMP(tmp); }
        protected void m6502_fd() { int tmp; EA_ABX_P(); tmp = RDMEM((ushort)ea.d); SBC(tmp); }

        protected void m6502_0e() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ASL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_2e() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_4e() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); LSR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_6e() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_8e() { int tmp = 0; STX(ref tmp); EA_ABS(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ae() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); LDX(tmp); }
        protected void m6502_ce() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DEC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ee() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); INC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_1e() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ASL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_3e() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROL(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_5e() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); LSR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_7e() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ROR(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_9e() { int tmp = 0; EA_ABY_NP(); SXH(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_be() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); LDX(tmp); }
        protected void m6502_de() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DEC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_fe() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); INC(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_0f() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_2f() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_4f() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_6f() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_8f() { int tmp = 0; SAX(ref tmp); EA_ABS(); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_af() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); LAX(tmp); }
        protected void m6502_cf() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ef() { int tmp; EA_ABS(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }

        protected void m6502_1f() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SLO(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_3f() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RLA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_5f() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); SRE(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_7f() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); RRA(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_9f() { int tmp = 0; EA_ABY_NP(); SAH(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_bf() { int tmp; EA_ABY_P(); tmp = RDMEM((ushort)ea.d); LAX(tmp); }
        protected void m6502_df() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); DCP(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
        protected void m6502_ff() { int tmp; EA_ABX_NP(); tmp = RDMEM((ushort)ea.d); WRMEM((ushort)ea.d, (byte)tmp); ISB(ref tmp); WRMEM((ushort)ea.d, (byte)tmp); }
    }
}
