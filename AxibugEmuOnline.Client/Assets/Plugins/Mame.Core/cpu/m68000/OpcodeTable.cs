using System;
using System.Collections.Generic;

namespace cpu.m68000
{
    partial class MC68000
    {
        void BuildOpcodeTable()
        {
            Assign("ill", ILL, "", "Data8", "Data8");//
            Assign("ori2ccr", ORI_CCR, "0000000000111100");//
            Assign("ori2sr", ORI_SR, "0000000001111100");
            Assign("ori", ORI, "00000000", "Size2_1", "OAmXn");
            Assign("andi2ccr", ANDI_CCR, "0000001000111100");//
            Assign("andi2sr", ANDI_SR, "0000001001111100");
            Assign("andi", ANDI, "00000010", "Size2_1", "OAmXn");
            Assign("subi", SUBI, "00000100", "Size2_1", "OAmXn");
            Assign("addi", ADDI, "00000110", "Size2_1", "OAmXn");
            Assign("eori2ccr", EORI_CCR, "0000101000111100");//
            Assign("eori2sr", EORI_SR, "0000101001111100");
            Assign("eori", EORI, "00001010", "Size2_1", "OAmXn");
            Assign("cmpi", CMPI, "00001100", "Size2_1", "OAmXn");
            Assign("btst", BTSTi, "0000100000", "BAmXn");
            Assign("bchg", BCHGi, "0000100001", "OAmXn");
            Assign("bclr", BCLRi, "0000100010", "OAmXn");
            Assign("bset", BSETi, "0000100011", "OAmXn");
            Assign("btst", BTSTr, "0000", "Xn", "100", "AmXn");
            Assign("bchg", BCHGr, "0000", "Xn", "101", "OAmXn");
            Assign("bclr", BCLRr, "0000", "Xn", "110", "OAmXn");
            Assign("bset", BSETr, "0000", "Xn", "111", "OAmXn");
            Assign("movep", MOVEP, "0000", "Xn", "1", "Data1", "Size1", "001", "Xn");//
            Assign("movea", MOVEA, "00", "Size2_2", "Xn", "001", "AmXn");
            Assign("move", MOVE, "00", "01", "OXnAm", "MAmXn");
            Assign("move", MOVE, "00", "Size2_2", "OXnAm", "AmXn");
            Assign("movefsr", MOVEfSR, "0100000011", "OAmXn");
            Assign("moveccr", MOVECCR, "0100010011", "MAmXn");
            Assign("move2sr", MOVEtSR, "0100011011", "MAmXn");
            Assign("negx", NEGX, "01000000", "Size2_1", "OAmXn");//
            Assign("clr", CLR, "01000010", "Size2_1", "OAmXn");
            Assign("neg", NEG, "01000100", "Size2_1", "OAmXn");
            Assign("not", NOT, "01000110", "Size2_1", "OAmXn");
            Assign("ext", EXT, "010010001", "Size1", "000", "Xn");
            Assign("nbcd", NBCD, "0100100000", "OAmXn");//
            Assign("swap", SWAP, "0100100001000", "Xn");
            Assign("pea", PEA, "0100100001", "LAmXn");
            Assign("illegal", ILLEGAL, "0100101011111100");//
            Assign("tas", TAS, "0100101011", "OAmXn");//
            Assign("tst", TST, "01001010", "Size2_1", "OAmXn");
            Assign("trap", TRAP, "010011100100", "Data4");
            Assign("link", LINK, "0100111001010", "Xn");
            Assign("unlk", UNLK, "0100111001011", "Xn");
            Assign("moveusp", MOVEUSP, "010011100110", "Data1", "Xn");
            Assign("reset", RESET, "0100111001110000");//
            Assign("nop", NOP, "0100111001110001");
            Assign("stop", STOP, "0100111001110010");//
            Assign("rte", RTE, "0100111001110011");
            Assign("rts", RTS, "0100111001110101");
            Assign("trapv", TRAPV, "0100111001110110");//
            Assign("rtr", RTR, "0100111001110111");
            Assign("jsr", JSR, "0100111010", "LAmXn");
            Assign("jmp", JMP, "0100111011", "LAmXn");
            Assign("movem", MOVEM0, "010010001", "Size1", "M2AmXn");
            Assign("movem", MOVEM1, "010011001", "Size1", "M3AmXn");
            Assign("lea", LEA, "0100", "Xn", "111", "LAmXn");
            Assign("chk", CHK, "0100", "Xn", "110", "MAmXn");//
            Assign("addq", ADDQ, "0101", "Data3", "0", "00", "OAmXn");
            Assign("addq", ADDQ, "0101", "Data3", "0", "Size2_3", "A2AmXn");
            Assign("subq", SUBQ, "0101", "Data3", "1", "00", "OAmXn");
            Assign("subq", SUBQ, "0101", "Data3", "1", "Size2_3", "A2AmXn");
            Assign("scc", Scc, "0101", "CondAll", "11", "OAmXn");
            Assign("dbcc", DBcc, "0101", "CondAll", "11001", "Xn");
            Assign("bra", BRA, "01100000", "Data8");
            Assign("bsr", BSR, "01100001", "Data8");
            Assign("bcc", Bcc, "0110", "CondMain", "Data8");
            Assign("moveq", MOVEQ, "0111", "Xn", "0", "Data8");
            Assign("divu", DIVU, "1000", "Xn", "011", "MAmXn");
            Assign("divs", DIVS, "1000", "Xn", "111", "MAmXn");
            Assign("sbcd", SBCD0, "1000", "Xn", "100000", "Xn");//
            Assign("sbcd", SBCD1, "1000", "Xn", "100001", "Xn");//
            Assign("or", OR0, "1000", "Xn", "0", "Size2_1", "MAmXn");
            Assign("or", OR1, "1000", "Xn", "1", "Size2_1", "O2AmXn");
            Assign("sub", SUB0, "1001", "Xn", "0", "00", "MAmXn");
            Assign("sub", SUB0, "1001", "Xn", "0", "Size2_3", "AmXn");
            Assign("sub", SUB1, "1001", "Xn", "1", "00", "O2AmXn");
            Assign("sub", SUB1, "1001", "Xn", "1", "Size2_3", "A2AmXn");
            Assign("subx", SUBX0, "1001", "Xn", "1", "Size2_1", "000", "Xn");//
            Assign("subx", SUBX1, "1001", "Xn", "1", "Size2_1", "001", "Xn");//
            Assign("suba", SUBA, "1001", "Xn", "Size1", "11", "AmXn");
            Assign("eor", EOR, "1011", "Xn", "1", "Size2_1", "OAmXn");
            Assign("cmpm", CMPM, "1011", "Xn", "1", "Size2_1", "001", "Xn");
            Assign("cmp", CMP, "1011", "Xn", "0", "00", "MAmXn");
            Assign("cmp", CMP, "1011", "Xn", "0", "Size2_3", "AmXn");
            Assign("cmpa", CMPA, "1011", "Xn", "Size1", "11", "AmXn");
            Assign("mulu", MULU, "1100", "Xn", "011", "MAmXn");
            Assign("muls", MULS, "1100", "Xn", "111", "MAmXn");
            Assign("abcd", ABCD0, "1100", "Xn", "100000", "Xn");//
            Assign("abcd", ABCD1, "1100", "Xn", "100001", "Xn");//
            Assign("exg", EXGdd, "1100", "Xn", "101000", "Xn");//
            Assign("exg", EXGaa, "1100", "Xn", "101001", "Xn");//
            Assign("exg", EXGda, "1100", "Xn", "110001", "Xn");//
            Assign("and", AND0, "1100", "Xn", "0", "Size2_1", "MAmXn");
            Assign("and", AND1, "1100", "Xn", "1", "Size2_1", "O2AmXn");
            Assign("add", ADD0, "1101", "Xn", "0", "00", "MAmXn");
            Assign("add", ADD0, "1101", "Xn", "0", "Size2_3", "AmXn");
            Assign("add", ADD1, "1101", "Xn", "1", "Size2_1", "O2AmXn");
            Assign("addx", ADDX0, "1101", "Xn", "1", "Size2_1", "000", "Xn");//
            Assign("addx", ADDX1, "1101", "Xn", "1", "Size2_1", "001", "Xn");//
            Assign("adda", ADDA, "1101", "Xn", "Size1", "11", "AmXn");
            Assign("asl", ASLd0, "1110000111", "O2AmXn");//
            Assign("asr", ASRd0, "1110000011", "O2AmXn");//
            Assign("lsl", LSLd0, "1110001111", "O2AmXn");//
            Assign("lsr", LSRd0, "1110001011", "O2AmXn");//
            Assign("roxl", ROXLd0, "1110010111", "O2AmXn");//
            Assign("roxr", ROXRd0, "1110010011", "O2AmXn");//
            Assign("rol", ROLd0, "1110011111", "O2AmXn");//
            Assign("ror", RORd0, "1110011011", "O2AmXn");//
            Assign("asl", ASLd, "1110", "Data3", "1", "Size2_1", "Data1", "00", "Xn");
            Assign("asr", ASRd, "1110", "Data3", "0", "Size2_1", "Data1", "00", "Xn");
            Assign("lsl", LSLd, "1110", "Data3", "1", "Size2_1", "Data1", "01", "Xn");
            Assign("lsr", LSRd, "1110", "Data3", "0", "Size2_1", "Data1", "01", "Xn");
            Assign("roxl", ROXLd, "1110", "Data3", "1", "Size2_1", "Data1", "10", "Xn");
            Assign("roxr", ROXRd, "1110", "Data3", "0", "Size2_1", "Data1", "10", "Xn");
            Assign("rol", ROLd, "1110", "Data3", "1", "Size2_1", "Data1", "11", "Xn");
            Assign("ror", RORd, "1110", "Data3", "0", "Size2_1", "Data1", "11", "Xn");
        }

        void Assign(string instr, Action exec, string root, params string[] bitfield)
        {
            List<string> opList = new List<string>();
            opList.Add(root);
            foreach (var component in bitfield)
            {
                if (IsBinary(component)) AppendConstant(opList, component);
                else if (component == "Size1") opList = AppendPermutations(opList, Size1);
                else if (component == "Size2_0") opList = AppendPermutations(opList, Size2_0);
                else if (component == "Size2_1") opList = AppendPermutations(opList, Size2_1);
                else if (component == "Size2_2") opList = AppendPermutations(opList, Size2_2);
                else if (component == "Size2_3") opList = AppendPermutations(opList, Size2_3);
                else if (component == "OXnAm") opList = AppendPermutations(opList, OXn3Am3);//0,2-6,7_0-7_1
                else if (component == "AmXn") opList = AppendPermutations(opList, Am3Xn3);//0-6,7_0-7_4
                else if (component == "OAmXn") opList = AppendPermutations(opList, OAm3Xn3);//0,2-6,7_0-7_1
                else if (component == "BAmXn") opList = AppendPermutations(opList, BAm3Xn3);//0,2-6,7_0-7_3
                else if (component == "MAmXn") opList = AppendPermutations(opList, MAm3Xn3);//0,2-6,7_0-7_4
                else if (component == "AAmXn") opList = AppendPermutations(opList, AAm3Xn3);//1-6,7_0-7_4
                else if (component == "LAmXn") opList = AppendPermutations(opList, LAm3Xn3);//2,5-6,7_0-7_3
                else if (component == "M2AmXn") opList = AppendPermutations(opList, M2Am3Xn3);//2,4-6,7_0-7_1
                else if (component == "M3AmXn") opList = AppendPermutations(opList, M3Am3Xn3);//2-3,5-6,7_0-7_3
                else if (component == "A2AmXn") opList = AppendPermutations(opList, A2Am3Xn3);//0-6,7_0-7_1
                else if (component == "O2AmXn") opList = AppendPermutations(opList, O2Am3Xn3);//2-6,7_0-7_1
                else if (component == "Xn") opList = AppendPermutations(opList, Xn3);
                else if (component == "CondMain") opList = AppendPermutations(opList, ConditionMain);
                else if (component == "CondAll") opList = AppendPermutations(opList, ConditionAll);
                else if (component == "Data1") opList = AppendData(opList, 1);
                else if (component == "Data3") opList = AppendData(opList, 3);
                else if (component == "Data4") opList = AppendData(opList, 4);
                else if (component == "Data8") opList = AppendData(opList, 8);
            }
            foreach (var opcode in opList)
            {
                int opc = Convert.ToInt32(opcode, 2);
                Opcodes[opc] = exec;
            }
        }

        void AppendConstant(List<string> ops, string constant)
        {
            for (int i = 0; i < ops.Count; i++)
                ops[i] = ops[i] + constant;
        }

        List<string> AppendPermutations(List<string> ops, string[] permutations)
        {
            List<string> output = new List<string>();

            foreach (var input in ops)
                foreach (var perm in permutations)
                    output.Add(input + perm);

            return output;
        }

        List<string> AppendData(List<string> ops, int bits)
        {
            List<string> output = new List<string>();

            foreach (var input in ops)
                for (int i = 0; i < BinaryExp(bits); i++)
                    output.Add(input + Convert.ToString(i, 2).PadLeft(bits, '0'));

            return output;
        }

        int BinaryExp(int bits)
        {
            int res = 1;
            for (int i = 0; i < bits; i++)
                res *= 2;
            return res;
        }

        bool IsBinary(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == '0' || c == '1')
                    continue;
                return false;
            }
            return true;
        }

        #region Tables

        static readonly string[] Size2_0 = { "01", "11", "10" };
        static readonly string[] Size2_1 = { "00", "01", "10" };
        static readonly string[] Size2_2 = { "11", "10" };
        static readonly string[] Size2_3 = { "01", "10" };
        static readonly string[] Size1 = { "0", "1" };
        static readonly string[] Xn3 = { "000", "001", "010", "011", "100", "101", "110", "111" };

        static readonly string[] OXn3Am3 = {
            "000000", // Dn   Data register
            "001000",
            "010000",
            "011000",
            "100000",
            "101000",
            "110000",
            "111000",

            "000010", // (An) Address
            "001010",
            "010010",
            "011010",
            "100010",
            "101010",
            "110010",
            "111010",

            "000011", // (An)+ Address with Postincrement
            "001011",
            "010011",
            "011011",
            "100011",
            "101011",
            "110011",
            "111011",

            "000100", // -(An) Address with Predecrement
            "001100",
            "010100",
            "011100",
            "100100",
            "101100",
            "110100",
            "111100",

            "000101", // (d16, An) Address with Displacement
            "001101",
            "010101",
            "011101",
            "100101",
            "101101",
            "110101",
            "111101",

            "000110", // (d8, An, Xn) Address with Index
            "001110",
            "010110",
            "011110",
            "100110",
            "101110",
            "110110",
            "111110",

            "000111", // (xxx).W       Absolute Short
            "001111", // (xxx).L       Absolute Long
        };

        static readonly string[] Am3Xn3 = {
            "000000", // Dn   Data register
            "000001",
            "000010",
            "000011",
            "000100",
            "000101",
            "000110",
            "000111",

            "001000", // An    Address register
            "001001",
            "001010",
            "001011",
            "001100",
            "001101",
            "001110",
            "001111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
            "111100", // #imm          Immediate
        };

        static readonly string[] OAm3Xn3 = {
            "000000", // Dn   Data register
            "000001",
            "000010",
            "000011",
            "000100",
            "000101",
            "000110",
            "000111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] BAm3Xn3 = {
            "000000", // Dn   Data register
            "000001",
            "000010",
            "000011",
            "000100",
            "000101",
            "000110",
            "000111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] MAm3Xn3 = {
            "000000", // Dn   Data register
            "000001",
            "000010",
            "000011",
            "000100",
            "000101",
            "000110",
            "000111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
            "111100", // #imm          Immediate
        };

        static readonly string[] AAm3Xn3 = {
            "001000", // An    Address register
            "001001",
            "001010",
            "001011",
            "001100",
            "001101",
            "001110",
            "001111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
            "111100", // #imm          Immediate
        };

        static readonly string[] LAm3Xn3 = {
            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] M2Am3Xn3 = {
            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] M3Am3Xn3 = {
            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111010", // (d16, PC)     PC with Displacement
            "111011", // (d8, PC, Xn)  PC with Index
            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] A2Am3Xn3 = {
            "000000", // Dn   Data register
            "000001",
            "000010",
            "000011",
            "000100",
            "000101",
            "000110",
            "000111",

            "001000", // An    Address register
            "001001",
            "001010",
            "001011",
            "001100",
            "001101",
            "001110",
            "001111",

            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] O2Am3Xn3 = {
            "010000", // (An) Address
            "010001",
            "010010",
            "010011",
            "010100",
            "010101",
            "010110",
            "010111",

            "011000", // (An)+ Address with Postincrement
            "011001",
            "011010",
            "011011",
            "011100",
            "011101",
            "011110",
            "011111",

            "100000", // -(An) Address with Predecrement
            "100001",
            "100010",
            "100011",
            "100100",
            "100101",
            "100110",
            "100111",

            "101000", // (d16, An) Address with Displacement
            "101001",
            "101010",
            "101011",
            "101100",
            "101101",
            "101110",
            "101111",

            "110000", // (d8, An, Xn) Address with Index
            "110001",
            "110010",
            "110011",
            "110100",
            "110101",
            "110110",
            "110111",

            "111000", // (xxx).W       Absolute Short
            "111001", // (xxx).L       Absolute Long
        };

        static readonly string[] ConditionMain = {
            "0010", // HI  Higher (unsigned)
            "0011", // LS  Lower or Same (unsigned)
            "0100", // CC  Carry Clear (aka Higher or Same, unsigned)
            "0101", // CS  Carry Set (aka Lower, unsigned)
            "0110", // NE  Not Equal
            "0111", // EQ  Equal
            "1000", // VC  Overflow Clear
            "1001", // VS  Overflow Set
            "1010", // PL  Plus
            "1011", // MI  Minus
            "1100", // GE  Greater or Equal (signed)
            "1101", // LT  Less Than (signed)
            "1110", // GT  Greater Than (signed)
            "1111"  // LE  Less or Equal (signed)
        };

        static readonly string[] ConditionAll = {
            "0000", // T   True 
            "0001", // F   False            
            "0010", // HI  Higher (unsigned)
            "0011", // LS  Lower or Same (unsigned)
            "0100", // CC  Carry Clear (aka Higher or Same, unsigned)
            "0101", // CS  Carry Set (aka Lower, unsigned)
            "0110", // NE  Not Equal
            "0111", // EQ  Equal
            "1000", // VC  Overflow Clear
            "1001", // VS  Overflow Set
            "1010", // PL  Plus
            "1011", // MI  Minus
            "1100", // GE  Greater or Equal (signed)
            "1101", // LT  Less Than (signed)
            "1110", // GT  Greater Than (signed)
            "1111"  // LE  Less or Equal (signed)
        };

        #endregion
    }
}