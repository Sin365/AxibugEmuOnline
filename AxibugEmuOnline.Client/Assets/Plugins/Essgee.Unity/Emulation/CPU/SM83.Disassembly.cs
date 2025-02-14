using System.Linq;

namespace Essgee.Emulation.CPU
{
    public partial class SM83
    {
        static readonly string[] opcodeMnemonicNoPrefix =
        {
		/*  +00                         +01                         +02                         +03                         +04                         +05                         +06                         +07                         */
			"NOP",                      "LD BC, 0x{0:X4}",          "LD (BC), A",               "INC BC",                   "INC B",                    "DEC B",                    "LD B, 0x{0:X2}",           "RLCA",                     /* 0x00 */
			"LD (0x{0:X4}), SP",        "ADD HL, BC",               "LD A, (BC)",               "DEC BC",                   "INC C",                    "DEC C",                    "LD C, 0x{0:X2}",           "RRCA",                     /* 0x08 */
			"STOP",                     "LD DE, 0x{0:X4}",          "LD (DE), A",               "INC DE",                   "INC D",                    "DEC D",                    "LD D, 0x{0:X2}",           "RLA",                      /* 0x10 */
			"JR 0x{0:X2}",              "ADD HL, DE",               "LD A, (DE)",               "DEC DE",                   "INC E",                    "DEC E",                    "LD E, 0x{0:X2}",           "RRA",                      /* 0x18 */
			"JR NZ, 0x{0:X2}",          "LD HL, 0x{0:X4}",          "LDI (HL), A",              "INC HL",                   "INC H",                    "DEC H",                    "LD H, 0x{0:X2}",           "DAA",                      /* 0x20 */
			"JR Z, 0x{0:X2}",           "ADD HL, HL",               "LDI A, (HL)",              "DEC HL",                   "INC L",                    "DEC L",                    "LD L, 0x{0:X2}",           "CPL",                      /* 0x28 */
			"JR NC, 0x{0:X2}",          "LD SP, 0x{0:X4}",          "LDD (HL), A",              "INC SP",                   "INC (HL)",                 "DEC (HL)",                 "LD (HL), 0x{0:X2}",        "SCF",                      /* 0x30 */
			"JR C, 0x{0:X2}",           "ADD HL, SP",               "LDD A, (HL)",              "DEC SP",                   "INC A",                    "DEC A",                    "LD A, 0x{0:X2}",           "CCF",                      /* 0x38 */
			"LD B, B",                  "LD B, C",                  "LD B, D",                  "LD B, E",                  "LD B, H",                  "LD B, L",                  "LD B, (HL)",               "LD B, A",                  /* 0x40 */
			"LD C, B",                  "LD C, C",                  "LD C, D",                  "LD C, E",                  "LD C, H",                  "LD C, L",                  "LD C, (HL)",               "LD C, A",                  /* 0x48 */
			"LD D, B",                  "LD D, C",                  "LD D, D",                  "LD D, E",                  "LD D, H",                  "LD D, L",                  "LD D, (HL)",               "LD D, A",                  /* 0x50 */
			"LD E, B",                  "LD E, C",                  "LD E, D",                  "LD E, E",                  "LD E, H",                  "LD E, L",                  "LD E, (HL)",               "LD E, A",                  /* 0x58 */
			"LD H, B",                  "LD H, C",                  "LD H, D",                  "LD H, E",                  "LD H, H",                  "LD H, L",                  "LD H, (HL)",               "LD H, A",                  /* 0x60 */
			"LD L, B",                  "LD L, C",                  "LD L, D",                  "LD L, E",                  "LD L, H",                  "LD L, L",                  "LD L, (HL)",               "LD L, A",                  /* 0x68 */
			"LD (HL), B",               "LD (HL), C",               "LD (HL), D",               "LD (HL), E",               "LD (HL), H",               "LD (HL), L",               "HALT",                     "LD (HL), A",               /* 0x70 */
			"LD A, B",                  "LD A, C",                  "LD A, D",                  "LD A, E",                  "LD A, H",                  "LD A, L",                  "LD A, (HL)",               "LD A, A",                  /* 0x78 */
			"ADD B",                    "ADD C",                    "ADD D",                    "ADD E",                    "ADD H",                    "ADD L",                    "ADD (HL)",                 "ADD A",                    /* 0x80 */
			"ADC B",                    "ADC C",                    "ADC D",                    "ADC E",                    "ADC H",                    "ADC L",                    "ADC (HL)",                 "ADC A",                    /* 0x88 */
			"SUB B",                    "SUB C",                    "SUB D",                    "SUB E",                    "SUB H",                    "SUB L",                    "SUB (HL)",                 "SUB A",                    /* 0x90 */
			"SBC B",                    "SBC C",                    "SBC D",                    "SBC E",                    "SBC H",                    "SBC L",                    "SBC (HL)",                 "SBC A",                    /* 0x98 */
			"AND B",                    "AND C",                    "AND D",                    "AND E",                    "AND H",                    "AND L",                    "AND (HL)",                 "AND A",                    /* 0xA0 */
			"XOR B",                    "XOR C",                    "XOR D",                    "XOR E",                    "XOR H",                    "XOR L",                    "XOR (HL)",                 "XOR A",                    /* 0xA8 */
			"OR B",                     "OR C",                     "OR D",                     "OR E",                     "OR H",                     "OR L",                     "OR (HL)",                  "OR A",                     /* 0xB0 */
			"CP B",                     "CP C",                     "CP D",                     "CP E",                     "CP H",                     "CP L",                     "CP (HL)",                  "CP A",                     /* 0xB8 */
			"RET NZ",                   "POP BC",                   "JP NZ, 0x{0:X4}",          "JP 0x{0:X4}",              "CALL NZ, 0x{0:X4}",        "PUSH BC",                  "ADD 0x{0:X2}",             "RST 00",                   /* 0xC0 */
			"RET Z",                    "RET",                      "JP Z, 0x{0:X4}",           string.Empty,               "CALL Z, 0x{0:X4}",         "CALL 0x{0:X4}",            "ADC 0x{0:X2}",             "RST 08",                   /* 0xC8 */
			"RET NC",                   "POP DE",                   "JP NC, 0x{0:X4}",          ".DB 0xD3",                 "CALL NC, 0x{0:X4}",        "PUSH DE",                  "SUB 0x{0:X2}",             "RST 10",                   /* 0xD0 */
			"RET C",                    "RETI",                     "JP C, 0x{0:X4}",           ".DB 0xDB",                 "CALL C, 0x{0:X4}",         ".DB 0xDD",                 "SBC 0x{0:X2}",             "RST 18",                   /* 0xD8 */
			"LD (FF00+0x{0:X2}), A",    "POP HL",                   "LD (FF00+C), A",           ".DB 0xE3",                 ".DB 0xE4",                 "PUSH HL",                  "AND 0x{0:X2}",             "RST 20",                   /* 0xE0 */
			"ADD SP, 0x{0:X2}",         "LD PC, HL",                "LD (0x{0:X4}), A",         ".DB 0xEB",                 ".DB 0xEC",                 ".DB 0xED",                 "XOR 0x{0:X2}",             "RST 28",                   /* 0xE8 */
			"LD A, (FF00+0x{0:X2})",    "POP AF",                   "LD A, (FF00+C)",           "DI",                       ".DB 0xF4",                 "PUSH AF",                  "OR 0x{0:X2}",              "RST 30",                   /* 0xF0 */
			"LD HL, SP+0x{0:X2}",       "LD SP, HL",                "LD A, (0x{0:X4})",         "EI",                       ".DB 0xFC",                 ".DB 0xFD",                 "CP 0x{0:X2}",              "RST 38"                    /* 0xF8 */
		};

        static readonly int[] opcodeLengthNoPrefix =
        {
            1, 3, 1, 1, 1, 1, 2, 1, 3, 1, 1, 1, 1, 1, 2, 1,
            2, 3, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1,
            2, 3, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1,
            2, 3, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 3, 3, 3, 1, 2, 1, 1, 1, 3, 2, 3, 3, 2, 1,
            1, 1, 3, 1, 3, 1, 2, 1, 1, 1, 3, 1, 3, 1, 2, 1,
            2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 1, 1, 2, 1,
            2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 1, 1, 2, 1
        };

        static readonly string[] opcodeMnemonicPrefixCB = new string[]
        {
		/*  +00                         +01                         +02                         +03                         +04                         +05                         +06                         +07                         */
			"RLC B",                    "RLC C",                    "RLC D",                    "RLC E",                    "RLC H",                    "RLC L",                    "RLC (HL)",                 "RLC A",                    /* 0x00 */
			"RRC B",                    "RRC C",                    "RRC D",                    "RRC E",                    "RRC H",                    "RRC L",                    "RRC (HL)",                 "RRC A",                    /* 0x08 */
			"RL B",                     "RL C",                     "RL D",                     "RL E",                     "RL H",                     "RL L",                     "RL (HL)",                  "RL A",                     /* 0x10 */
			"RR B",                     "RR C",                     "RR D",                     "RR E",                     "RR H",                     "RR L",                     "RR (HL)",                  "RR A",                     /* 0x18 */
			"SLA B",                    "SLA C",                    "SLA D",                    "SLA E",                    "SLA H",                    "SLA L",                    "SLA (HL)",                 "SLA A",                    /* 0x20 */
			"SRA B",                    "SRA C",                    "SRA D",                    "SRA E",                    "SRA H",                    "SRA L",                    "SRA (HL)",                 "SRA A",                    /* 0x28 */
			"SWAP B",                   "SWAP C",                   "SWAP D",                   "SWAP E",                   "SWAP H",                   "SWAP L",                   "SWAP (HL)",                "SWAP A",                   /* 0x30 */
			"SRL B",                    "SRL C",                    "SRL D",                    "SRL E",                    "SRL H",                    "SRL L",                    "SRL (HL)",                 "SRL A",                    /* 0x38 */
			"BIT 0, B",                 "BIT 0, C",                 "BIT 0, D",                 "BIT 0, E",                 "BIT 0, H",                 "BIT 0, L",                 "BIT 0, (HL)",              "BIT 0, A",                 /* 0x40 */
			"BIT 1, B",                 "BIT 1, C",                 "BIT 1, D",                 "BIT 1, E",                 "BIT 1, H",                 "BIT 1, L",                 "BIT 1, (HL)",              "BIT 1, A",                 /* 0x48 */
			"BIT 2, B",                 "BIT 2, C",                 "BIT 2, D",                 "BIT 2, E",                 "BIT 2, H",                 "BIT 2, L",                 "BIT 2, (HL)",              "BIT 2, A",                 /* 0x50 */
			"BIT 3, B",                 "BIT 3, C",                 "BIT 3, D",                 "BIT 3, E",                 "BIT 3, H",                 "BIT 3, L",                 "BIT 3, (HL)",              "BIT 3, A",                 /* 0x58 */
			"BIT 4, B",                 "BIT 4, C",                 "BIT 4, D",                 "BIT 4, E",                 "BIT 4, H",                 "BIT 4, L",                 "BIT 4, (HL)",              "BIT 4, A",                 /* 0x60 */
			"BIT 5, B",                 "BIT 5, C",                 "BIT 5, D",                 "BIT 5, E",                 "BIT 5, H",                 "BIT 5, L",                 "BIT 5, (HL)",              "BIT 5, A",                 /* 0x68 */
			"BIT 6, B",                 "BIT 6, C",                 "BIT 6, D",                 "BIT 6, E",                 "BIT 6, H",                 "BIT 6, L",                 "BIT 6, (HL)",              "BIT 6, A",                 /* 0x70 */
			"BIT 7, B",                 "BIT 7, C",                 "BIT 7, D",                 "BIT 7, E",                 "BIT 7, H",                 "BIT 7, L",                 "BIT 7, (HL)",              "BIT 7, A",                 /* 0x78 */
			"RES 0, B",                 "RES 0, C",                 "RES 0, D",                 "RES 0, E",                 "RES 0, H",                 "RES 0, L",                 "RES 0, (HL)",              "RES 0, A",                 /* 0x80 */
			"RES 1, B",                 "RES 1, C",                 "RES 1, D",                 "RES 1, E",                 "RES 1, H",                 "RES 1, L",                 "RES 1, (HL)",              "RES 1, A",                 /* 0x88 */
			"RES 2, B",                 "RES 2, C",                 "RES 2, D",                 "RES 2, E",                 "RES 2, H",                 "RES 2, L",                 "RES 2, (HL)",              "RES 2, A",                 /* 0x90 */
			"RES 3, B",                 "RES 3, C",                 "RES 3, D",                 "RES 3, E",                 "RES 3, H",                 "RES 3, L",                 "RES 3, (HL)",              "RES 3, A",                 /* 0x98 */
			"RES 4, B",                 "RES 4, C",                 "RES 4, D",                 "RES 4, E",                 "RES 4, H",                 "RES 4, L",                 "RES 4, (HL)",              "RES 4, A",                 /* 0xA0 */
			"RES 5, B",                 "RES 5, C",                 "RES 5, D",                 "RES 5, E",                 "RES 5, H",                 "RES 5, L",                 "RES 5, (HL)",              "RES 5, A",                 /* 0xA8 */
			"RES 6, B",                 "RES 6, C",                 "RES 6, D",                 "RES 6, E",                 "RES 6, H",                 "RES 6, L",                 "RES 6, (HL)",              "RES 6, A",                 /* 0xB0 */
			"RES 7, B",                 "RES 7, C",                 "RES 7, D",                 "RES 7, E",                 "RES 7, H",                 "RES 7, L",                 "RES 7, (HL)",              "RES 7, A",                 /* 0xB8 */
			"SET 0, B",                 "SET 0, C",                 "SET 0, D",                 "SET 0, E",                 "SET 0, H",                 "SET 0, L",                 "SET 0, (HL)",              "SET 0, A",                 /* 0xC0 */
			"SET 1, B",                 "SET 1, C",                 "SET 1, D",                 "SET 1, E",                 "SET 1, H",                 "SET 1, L",                 "SET 1, (HL)",              "SET 1, A",                 /* 0xC8 */
			"SET 2, B",                 "SET 2, C",                 "SET 2, D",                 "SET 2, E",                 "SET 2, H",                 "SET 2, L",                 "SET 2, (HL)",              "SET 2, A",                 /* 0xD0 */
			"SET 3, B",                 "SET 3, C",                 "SET 3, D",                 "SET 3, E",                 "SET 3, H",                 "SET 3, L",                 "SET 3, (HL)",              "SET 3, A",                 /* 0xD8 */
			"SET 4, B",                 "SET 4, C",                 "SET 4, D",                 "SET 4, E",                 "SET 4, H",                 "SET 4, L",                 "SET 4, (HL)",              "SET 4, A",                 /* 0xE0 */
			"SET 5, B",                 "SET 5, C",                 "SET 5, D",                 "SET 5, E",                 "SET 5, H",                 "SET 5, L",                 "SET 5, (HL)",              "SET 5, A",                 /* 0xE8 */
			"SET 6, B",                 "SET 6, C",                 "SET 6, D",                 "SET 6, E",                 "SET 6, H",                 "SET 6, L",                 "SET 6, (HL)",              "SET 6, A",                 /* 0xF0 */
			"SET 7, B",                 "SET 7, C",                 "SET 7, D",                 "SET 7, E",                 "SET 7, H",                 "SET 7, L",                 "SET 7, (HL)",              "SET 7, A"                  /* 0xF8 */
		};

        static readonly int[] opcodeLengthPrefixCB = new int[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        public static string PrintRegisters(SM83 cpu)
        {
            return $"AF:{cpu.af.Word:X4} BC:{cpu.bc.Word:X4} DE:{cpu.de.Word:X4} HL:{cpu.hl.Word:X4} SP:{cpu.sp:X4}";
        }

        public static string PrintFlags(SM83 cpu)
        {
            return $"{(cpu.IsFlagSet(Flags.Zero) ? "Z" : "-")}{(cpu.IsFlagSet(Flags.Subtract) ? "N" : "-")}{(cpu.IsFlagSet(Flags.HalfCarry) ? "H" : "-")}{(cpu.IsFlagSet(Flags.Carry) ? "C" : "-")}";
        }

        public static string PrintInterrupt(SM83 cpu)
        {
            var intFlags = (InterruptSource)cpu.memoryReadDelegate(0xFF0F);
            var intEnable = (InterruptSource)cpu.memoryReadDelegate(0xFFFF);

            var intFlagsString =
                $"{((intFlags & InterruptSource.VBlank) != 0 ? "V" : "-")}" +
                $"{((intFlags & InterruptSource.LCDCStatus) != 0 ? "L" : "-")}" +
                $"{((intFlags & InterruptSource.TimerOverflow) != 0 ? "T" : "-")}" +
                $"{((intFlags & InterruptSource.SerialIO) != 0 ? "S" : "-")}" +
                $"{((intFlags & InterruptSource.Keypad) != 0 ? "K" : "-")}";

            var intEnableString =
                $"{((intEnable & InterruptSource.VBlank) != 0 ? "V" : "-")}" +
                $"{((intEnable & InterruptSource.LCDCStatus) != 0 ? "L" : "-")}" +
                $"{((intEnable & InterruptSource.TimerOverflow) != 0 ? "T" : "-")}" +
                $"{((intEnable & InterruptSource.SerialIO) != 0 ? "S" : "-")}" +
                $"{((intEnable & InterruptSource.Keypad) != 0 ? "K" : "-")}";

            return $"IME:{(cpu.ime ? "1" : "0")} IF:{intFlagsString} IE:{intEnableString}";
        }

        public static string DisassembleOpcode(SM83 cpu, ushort address)
        {
            var opcode = DisassembleGetOpcodeBytes(cpu, address);
            return $"0x{address:X4} | {DisassembleMakeByteString(cpu, opcode).PadRight(15)} | {DisassembleMakeMnemonicString(cpu, opcode)}";
        }

        public static byte[] DisassembleGetOpcodeBytes(SM83 cpu, ushort address)
        {
            var opcode = new byte[3];
            for (int i = 0; i < opcode.Length; i++)
                opcode[i] = address + i <= 0xFFFF ? cpu.memoryReadDelegate((ushort)(address + i)) : (byte)0;
            return opcode;
        }

        public static int DisassembleGetOpcodeLen(SM83 cpu, byte[] opcode)
        {
            if (opcode[0] == 0xCB)
                return opcodeLengthPrefixCB[opcode[1]];
            else
                return opcodeLengthNoPrefix[opcode[0]];
        }

        public static string DisassembleMakeByteString(SM83 cpu, byte[] opcode)
        {
            return string.Join(" ", opcode.Select(x => $"{x:X2}").Take(DisassembleGetOpcodeLen(cpu, opcode)));
        }

        public static string DisassembleMakeMnemonicString(SM83 cpu, byte[] opcode)
        {
            var len = DisassembleGetOpcodeLen(cpu, opcode);
            var start = opcode[0] == 0xCB ? 1 : 0;
            var mnemonics = opcode[0] == 0xCB ? opcodeMnemonicPrefixCB : opcodeMnemonicNoPrefix;

            switch (len - start)
            {
                case 1: return mnemonics[opcode[start]];
                case 2: return string.Format(mnemonics[opcode[start]], opcode[start + 1]);
                case 3: return string.Format(mnemonics[opcode[start]], (opcode[start + 2] << 8 | opcode[start + 1]));
                default: return string.Empty;
            }
        }

        private string MakeUnimplementedOpcodeString(string prefix, ushort address)
        {
            var opcode = DisassembleGetOpcodeBytes(this, address);
            return $"Unimplemented {(prefix != string.Empty ? prefix + " " : prefix)}opcode {DisassembleMakeByteString(this, opcode)} ({DisassembleMakeMnemonicString(this, opcode)})";
        }
    }
}
