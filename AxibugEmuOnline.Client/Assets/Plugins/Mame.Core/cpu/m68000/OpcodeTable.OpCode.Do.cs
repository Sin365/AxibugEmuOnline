//namespace cpu.m68000
//{
//    partial class MC68000
//    {

//        /// <summary>
//        /// 重写的68000指令调度
//        /// </summary>
//        /// <param name="op"></param>
//        void DoOpCode(ushort op)
//        {
//            MC68000Code opType = MC68000CodeArr[op];
//            switch (opType)
//            {
//                case MC68000Code.ORI: ORI(); return;
//                case MC68000Code.ILL: ILL(); return;
//                case MC68000Code.ORI_CCR: ORI_CCR(); return;
//                case MC68000Code.ORI_SR: ORI_SR(); return;
//                case MC68000Code.BTSTr: BTSTr(); return;
//                case MC68000Code.MOVEP: MOVEP(); return;
//                case MC68000Code.BCHGr: BCHGr(); return;
//                case MC68000Code.BCLRr: BCLRr(); return;
//                case MC68000Code.BSETr: BSETr(); return;
//                case MC68000Code.ANDI: ANDI(); return;
//                case MC68000Code.ANDI_CCR: ANDI_CCR(); return;
//                case MC68000Code.ANDI_SR: ANDI_SR(); return;
//                case MC68000Code.SUBI: SUBI(); return;
//                case MC68000Code.ADDI: ADDI(); return;
//                case MC68000Code.BTSTi: BTSTi(); return;
//                case MC68000Code.BCHGi: BCHGi(); return;
//                case MC68000Code.BCLRi: BCLRi(); return;
//                case MC68000Code.BSETi: BSETi(); return;
//                case MC68000Code.EORI: EORI(); return;
//                case MC68000Code.EORI_CCR: EORI_CCR(); return;
//                case MC68000Code.EORI_SR: EORI_SR(); return;
//                case MC68000Code.CMPI: CMPI(); return;
//                case MC68000Code.MOVE: MOVE(); return;
//                case MC68000Code.MOVEA: MOVEA(); return;
//                case MC68000Code.NEGX: NEGX(); return;
//                case MC68000Code.MOVEfSR: MOVEfSR(); return;
//                case MC68000Code.CHK: CHK(); return;
//                case MC68000Code.LEA: LEA(); return;
//                case MC68000Code.CLR: CLR(); return;
//                case MC68000Code.NEG: NEG(); return;
//                case MC68000Code.MOVECCR: MOVECCR(); return;
//                case MC68000Code.NOT: NOT(); return;
//                case MC68000Code.MOVEtSR: MOVEtSR(); return;
//                case MC68000Code.NBCD: NBCD(); return;
//                case MC68000Code.SWAP: SWAP(); return;
//                case MC68000Code.PEA: PEA(); return;
//                case MC68000Code.EXT: EXT(); return;
//                case MC68000Code.MOVEM0: MOVEM0(); return;
//                case MC68000Code.TST: TST(); return;
//                case MC68000Code.TAS: TAS(); return;
//                case MC68000Code.ILLEGAL: ILLEGAL(); return;
//                case MC68000Code.MOVEM1: MOVEM1(); return;
//                case MC68000Code.TRAP: TRAP(); return;
//                case MC68000Code.LINK: LINK(); return;
//                case MC68000Code.UNLK: UNLK(); return;
//                case MC68000Code.MOVEUSP: MOVEUSP(); return;
//                case MC68000Code.RESET: RESET(); return;
//                case MC68000Code.NOP: NOP(); return;
//                case MC68000Code.STOP: STOP(); return;
//                case MC68000Code.RTE: RTE(); return;
//                case MC68000Code.RTS: RTS(); return;
//                case MC68000Code.TRAPV: TRAPV(); return;
//                case MC68000Code.RTR: RTR(); return;
//                case MC68000Code.JSR: JSR(); return;
//                case MC68000Code.JMP: JMP(); return;
//                case MC68000Code.ADDQ: ADDQ(); return;
//                case MC68000Code.Scc: Scc(); return;
//                case MC68000Code.DBcc: DBcc(); return;
//                case MC68000Code.SUBQ: SUBQ(); return;
//                case MC68000Code.BRA: BRA(); return;
//                case MC68000Code.BSR: BSR(); return;
//                case MC68000Code.Bcc: Bcc(); return;
//                case MC68000Code.MOVEQ: MOVEQ(); return;
//                case MC68000Code.OR0: OR0(); return;
//                case MC68000Code.DIVU: DIVU(); return;
//                case MC68000Code.SBCD0: SBCD0(); return;
//                case MC68000Code.SBCD1: SBCD1(); return;
//                case MC68000Code.OR1: OR1(); return;
//                case MC68000Code.DIVS: DIVS(); return;
//                case MC68000Code.SUB0: SUB0(); return;
//                case MC68000Code.SUBA: SUBA(); return;
//                case MC68000Code.SUBX0: SUBX0(); return;
//                case MC68000Code.SUBX1: SUBX1(); return;
//                case MC68000Code.SUB1: SUB1(); return;
//                case MC68000Code.CMP: CMP(); return;
//                case MC68000Code.CMPA: CMPA(); return;
//                case MC68000Code.EOR: EOR(); return;
//                case MC68000Code.CMPM: CMPM(); return;
//                case MC68000Code.AND0: AND0(); return;
//                case MC68000Code.MULU: MULU(); return;
//                case MC68000Code.ABCD0: ABCD0(); return;
//                case MC68000Code.ABCD1: ABCD1(); return;
//                case MC68000Code.AND1: AND1(); return;
//                case MC68000Code.EXGdd: EXGdd(); return;
//                case MC68000Code.EXGaa: EXGaa(); return;
//                case MC68000Code.EXGda: EXGda(); return;
//                case MC68000Code.MULS: MULS(); return;
//                case MC68000Code.ADD0: ADD0(); return;
//                case MC68000Code.ADDA: ADDA(); return;
//                case MC68000Code.ADDX0: ADDX0(); return;
//                case MC68000Code.ADDX1: ADDX1(); return;
//                case MC68000Code.ADD1: ADD1(); return;
//                case MC68000Code.ASRd: ASRd(); return;
//                case MC68000Code.LSRd: LSRd(); return;
//                case MC68000Code.ROXRd: ROXRd(); return;
//                case MC68000Code.RORd: RORd(); return;
//                case MC68000Code.ASRd0: ASRd0(); return;
//                case MC68000Code.ASLd: ASLd(); return;
//                case MC68000Code.LSLd: LSLd(); return;
//                case MC68000Code.ROXLd: ROXLd(); return;
//                case MC68000Code.ROLd: ROLd(); return;
//                case MC68000Code.ASLd0: ASLd0(); return;
//                case MC68000Code.LSRd0: LSRd0(); return;
//                case MC68000Code.LSLd0: LSLd0(); return;
//                case MC68000Code.ROXRd0: ROXRd0(); return;
//                case MC68000Code.ROXLd0: ROXLd0(); return;
//                case MC68000Code.RORd0: RORd0(); return;
//                case MC68000Code.ROLd0: ROLd0(); return;
//            }
//        }
//    }
//}