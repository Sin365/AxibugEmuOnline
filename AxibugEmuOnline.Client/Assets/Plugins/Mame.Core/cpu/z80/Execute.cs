namespace cpu.z80
{
    public partial class Z80A
    {
        public ushort PPC = 0;
        public ushort OP;
        /// <summary>
        /// Runs the CPU for a particular number of clock cycles.
        /// </summary>
        /// <param name="cycles">The number of cycles to run the CPU emulator for. Specify -1 to run for a single instruction.</param>
        public override int ExecuteCycles(int cycles)
        {
            pendingCycles = cycles;
            sbyte Displacement;

            bool TBOOL; byte TB; byte TBH; byte TBL; byte TB1; byte TB2; sbyte TSB; ushort TUS; int TI1; int TI2; int TIR;

            // Process interrupt requests.
            if (nonMaskableInterruptPending)
            {
                if (halted)
                {
                    halted = false;
                    RegPC.Word++;
                }
                totalExecutedCycles += 11; pendingCycles -= 11;
                nonMaskableInterruptPending = false;
                //iff2 = iff1;
                iff1 = false;
                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                RegPC.Word = 0x66;
                RegWZ.Word = RegPC.Word;
                NMICallback();
            }
            do
            {
                if (interrupt && iff1 && Interruptable) //Z80.irq_state iff1 !Z80.after_ei
                {
                    if (halted)
                    {
                        Halted = false;
                        RegPC.Word++;
                    }
                    iff1 = iff2 = false;
                    int irq_vector = IRQCallback();
                    switch (interruptMode)
                    {
                        case 0:
                            switch (irq_vector & 0xff0000)
                            {
                                case 0xcd0000:
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = (ushort)(irq_vector & 0xffff);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xc30000:
                                    RegPC.Word = (ushort)(irq_vector & 0xffff);
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                default:
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = (ushort)(irq_vector & 0x0038);
                                    totalExecutedCycles += (ulong)(cc_op[RegPC.Low] + cc_ex[RegPC.Low]); pendingCycles -= cc_op[RegPC.Low] + cc_ex[RegPC.Low];
                                    break;
                            }
                            break;
                        case 1:
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x38;
                            totalExecutedCycles += 13; pendingCycles -= 13;
                            break;
                        case 2:
                            TUS = (ushort)(RegI * 256 + irq_vector);
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Low = ReadMemory(TUS++); RegPC.High = ReadMemory(TUS);
                            totalExecutedCycles += 17; pendingCycles -= 17;
                            break;
                    }
                    RegWZ.Word = RegPC.Word;
                }

                {
                    Interruptable = true;

                    ++RegR;

                    PPC = RegPC.Word;
                    OP = ReadOp(PPC);
                    RegPC.Word++;
                    switch (OP)//ReadMemory(RegPC.Word++))
                    {
                        case 0x00: // NOP
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x01: // LD BC, nn
                            RegBC.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0x02: // LD (BC), A
                            WriteMemory(RegBC.Word, RegAF.High);
                            RegWZ.Low = (byte)((RegBC.Word + 1) & 0xFF);
                            RegWZ.High = RegAF.High;
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x03: // INC BC
                            ++RegBC.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x04: // INC B
                            RegAF.Low = (byte)(TableInc[++RegBC.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x05: // DEC B
                            RegAF.Low = (byte)(TableDec[--RegBC.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x06: // LD B, n
                            RegBC.High = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x07: // RLCA
                            RegAF.Word = TableRotShift[0, 0, RegAF.Word];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x08: // EX AF, AF'
                            TUS = RegAF.Word; RegAF.Word = RegAltAF.Word; RegAltAF.Word = TUS;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x09: // ADD HL, BC
                            TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                            TUS = (ushort)TIR;
                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                            RegFlagN = false;
                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                            RegHL.Word = TUS;
                            RegFlag3 = (TUS & 0x0800) != 0;
                            RegFlag5 = (TUS & 0x2000) != 0;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x0A: // LD A, (BC)
                            RegAF.High = ReadMemory(RegBC.Word);
                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x0B: // DEC BC
                            --RegBC.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x0C: // INC C
                            RegAF.Low = (byte)(TableInc[++RegBC.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x0D: // DEC C
                            RegAF.Low = (byte)(TableDec[--RegBC.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x0E: // LD C, n
                            RegBC.Low = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x0F: // RRCA
                            RegAF.Word = TableRotShift[0, 1, RegAF.Word];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x10: // DJNZ d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            if (--RegBC.High != 0)
                            {
                                RegPC.Word = (ushort)(RegPC.Word + TSB);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 13; pendingCycles -= 13;
                            }
                            else
                            {
                                totalExecutedCycles += 8; pendingCycles -= 8;
                            }
                            break;
                        case 0x11: // LD DE, nn
                            RegDE.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0x12: // LD (DE), A
                            WriteMemory(RegDE.Word, RegAF.High);
                            RegWZ.Low = (byte)((RegDE.Word + 1) & 0xFF);
                            RegWZ.High = RegAF.High;
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x13: // INC DE
                            ++RegDE.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x14: // INC D
                            RegAF.Low = (byte)(TableInc[++RegDE.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x15: // DEC D
                            RegAF.Low = (byte)(TableDec[--RegDE.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x16: // LD D, n
                            RegDE.High = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x17: // RLA
                            RegAF.Word = TableRotShift[0, 2, RegAF.Word];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x18: // JR d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            RegPC.Word = (ushort)(RegPC.Word + TSB);
                            RegWZ.Word = RegPC.Word;
                            totalExecutedCycles += 12; pendingCycles -= 12;
                            break;
                        case 0x19: // ADD HL, DE
                            TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                            TUS = (ushort)TIR;
                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                            RegFlagN = false;
                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                            RegHL.Word = TUS;
                            RegFlag3 = (TUS & 0x0800) != 0;
                            RegFlag5 = (TUS & 0x2000) != 0;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x1A: // LD A, (DE)
                            RegAF.High = ReadMemory(RegDE.Word);
                            RegWZ.Word = (ushort)(RegDE.Word + 1);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x1B: // DEC DE
                            --RegDE.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x1C: // INC E
                            RegAF.Low = (byte)(TableInc[++RegDE.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x1D: // DEC E
                            RegAF.Low = (byte)(TableDec[--RegDE.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x1E: // LD E, n
                            RegDE.Low = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x1F: // RRA
                            RegAF.Word = TableRotShift[0, 3, RegAF.Word];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x20: // JR NZ, d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            if (!RegFlagZ)
                            {
                                RegPC.Word = (ushort)(RegPC.Word + TSB);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 12; pendingCycles -= 12;
                            }
                            else
                            {
                                totalExecutedCycles += 7; pendingCycles -= 7;
                            }
                            break;
                        case 0x21: // LD HL, nn
                            RegHL.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0x22: // LD (nn), HL
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            WriteMemory(TUS++, RegHL.Low);
                            WriteMemory(TUS, RegHL.High);
                            RegWZ.Word = TUS;
                            totalExecutedCycles += 16; pendingCycles -= 16;
                            break;
                        case 0x23: // INC HL
                            ++RegHL.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x24: // INC H
                            RegAF.Low = (byte)(TableInc[++RegHL.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x25: // DEC H
                            RegAF.Low = (byte)(TableDec[--RegHL.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x26: // LD H, n
                            RegHL.High = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x27: // DAA
                            RegAF.Word = TableDaa[RegAF.Word];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x28: // JR Z, d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            if (RegFlagZ)
                            {
                                RegPC.Word = (ushort)(RegPC.Word + TSB);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 12; pendingCycles -= 12;
                            }
                            else
                            {
                                totalExecutedCycles += 7; pendingCycles -= 7;
                            }
                            break;
                        case 0x29: // ADD HL, HL
                            TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 + TI2;
                            TUS = (ushort)TIR;
                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                            RegFlagN = false;
                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                            RegHL.Word = TUS;
                            RegFlag3 = (TUS & 0x0800) != 0;
                            RegFlag5 = (TUS & 0x2000) != 0;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x2A: // LD HL, (nn)
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            RegHL.Low = ReadMemory(TUS++); RegHL.High = ReadMemory(TUS);
                            RegWZ.Word = TUS;
                            totalExecutedCycles += 16; pendingCycles -= 16;
                            break;
                        case 0x2B: // DEC HL
                            --RegHL.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x2C: // INC L
                            RegAF.Low = (byte)(TableInc[++RegHL.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x2D: // DEC L
                            RegAF.Low = (byte)(TableDec[--RegHL.Low] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x2E: // LD L, n
                            RegHL.Low = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x2F: // CPL
                            RegAF.High ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x30: // JR NC, d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            if (!RegFlagC)
                            {
                                RegPC.Word = (ushort)(RegPC.Word + TSB);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 12; pendingCycles -= 12;
                            }
                            else
                            {
                                totalExecutedCycles += 7; pendingCycles -= 7;
                            }
                            break;
                        case 0x31: // LD SP, nn
                            RegSP.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0x32: // LD (nn), A
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            WriteMemory(TUS, RegAF.High);
                            RegWZ.Low = (byte)((TUS + 1) & 0xFF);
                            RegWZ.High = RegAF.High;
                            totalExecutedCycles += 13; pendingCycles -= 13;
                            break;
                        case 0x33: // INC SP
                            ++RegSP.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x34: // INC (HL)
                            TB = ReadMemory(RegHL.Word); RegAF.Low = (byte)(TableInc[++TB] | (RegAF.Low & 1)); WriteMemory(RegHL.Word, TB);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x35: // DEC (HL)
                            TB = ReadMemory(RegHL.Word); RegAF.Low = (byte)(TableDec[--TB] | (RegAF.Low & 1)); WriteMemory(RegHL.Word, TB);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x36: // LD (HL), n
                            WriteMemory(RegHL.Word, ReadOpArg(RegPC.Word++));
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0x37: // SCF
                            RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x38: // JR C, d
                            TSB = (sbyte)ReadOpArg(RegPC.Word++);
                            if (RegFlagC)
                            {
                                RegPC.Word = (ushort)(RegPC.Word + TSB);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 12; pendingCycles -= 12;
                            }
                            else
                            {
                                totalExecutedCycles += 7; pendingCycles -= 7;
                            }
                            break;
                        case 0x39: // ADD HL, SP
                            TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                            TUS = (ushort)TIR;
                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                            RegFlagN = false;
                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                            RegHL.Word = TUS;
                            RegFlag3 = (TUS & 0x0800) != 0;
                            RegFlag5 = (TUS & 0x2000) != 0;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0x3A: // LD A, (nn)
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            RegAF.High = ReadMemory(TUS);
                            RegWZ.Word = (ushort)(TUS + 1);
                            totalExecutedCycles += 13; pendingCycles -= 13;//
                            break;
                        case 0x3B: // DEC SP
                            --RegSP.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0x3C: // INC A
                            RegAF.Low = (byte)(TableInc[++RegAF.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x3D: // DEC A
                            RegAF.Low = (byte)(TableDec[--RegAF.High] | (RegAF.Low & 1));
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x3E: // LD A, n
                            RegAF.High = ReadOpArg(RegPC.Word++);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x3F: // CCF
                            RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x40: // LD B, B
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x41: // LD B, C
                            RegBC.High = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x42: // LD B, D
                            RegBC.High = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x43: // LD B, E
                            RegBC.High = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x44: // LD B, H
                            RegBC.High = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x45: // LD B, L
                            RegBC.High = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x46: // LD B, (HL)
                            RegBC.High = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x47: // LD B, A
                            RegBC.High = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x48: // LD C, B
                            RegBC.Low = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x49: // LD C, C
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x4A: // LD C, D
                            RegBC.Low = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x4B: // LD C, E
                            RegBC.Low = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x4C: // LD C, H
                            RegBC.Low = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x4D: // LD C, L
                            RegBC.Low = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x4E: // LD C, (HL)
                            RegBC.Low = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x4F: // LD C, A
                            RegBC.Low = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x50: // LD D, B
                            RegDE.High = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x51: // LD D, C
                            RegDE.High = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x52: // LD D, D
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x53: // LD D, E
                            RegDE.High = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x54: // LD D, H
                            RegDE.High = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x55: // LD D, L
                            RegDE.High = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x56: // LD D, (HL)
                            RegDE.High = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x57: // LD D, A
                            RegDE.High = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x58: // LD E, B
                            RegDE.Low = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x59: // LD E, C
                            RegDE.Low = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x5A: // LD E, D
                            RegDE.Low = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x5B: // LD E, E
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x5C: // LD E, H
                            RegDE.Low = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x5D: // LD E, L
                            RegDE.Low = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x5E: // LD E, (HL)
                            RegDE.Low = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x5F: // LD E, A
                            RegDE.Low = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x60: // LD H, B
                            RegHL.High = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x61: // LD H, C
                            RegHL.High = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x62: // LD H, D
                            RegHL.High = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x63: // LD H, E
                            RegHL.High = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x64: // LD H, H
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x65: // LD H, L
                            RegHL.High = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x66: // LD H, (HL)
                            RegHL.High = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x67: // LD H, A
                            RegHL.High = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x68: // LD L, B
                            RegHL.Low = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x69: // LD L, C
                            RegHL.Low = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x6A: // LD L, D
                            RegHL.Low = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x6B: // LD L, E
                            RegHL.Low = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x6C: // LD L, H
                            RegHL.Low = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x6D: // LD L, L
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x6E: // LD L, (HL)
                            RegHL.Low = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x6F: // LD L, A
                            RegHL.Low = RegAF.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x70: // LD (HL), B
                            WriteMemory(RegHL.Word, RegBC.High);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x71: // LD (HL), C
                            WriteMemory(RegHL.Word, RegBC.Low);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x72: // LD (HL), D
                            WriteMemory(RegHL.Word, RegDE.High);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x73: // LD (HL), E
                            WriteMemory(RegHL.Word, RegDE.Low);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x74: // LD (HL), H
                            WriteMemory(RegHL.Word, RegHL.High);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x75: // LD (HL), L
                            WriteMemory(RegHL.Word, RegHL.Low);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x76: // HALT
                            Halt();
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x77: // LD (HL), A
                            WriteMemory(RegHL.Word, RegAF.High);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x78: // LD A, B
                            RegAF.High = RegBC.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x79: // LD A, C
                            RegAF.High = RegBC.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x7A: // LD A, D
                            RegAF.High = RegDE.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x7B: // LD A, E
                            RegAF.High = RegDE.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x7C: // LD A, H
                            RegAF.High = RegHL.High;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x7D: // LD A, L
                            RegAF.High = RegHL.Low;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x7E: // LD A, (HL)
                            RegAF.High = ReadMemory(RegHL.Word);
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x7F: // LD A, A
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x80: // ADD A, B
                            RegAF.Word = TableALU[0, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x81: // ADD A, C
                            RegAF.Word = TableALU[0, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x82: // ADD A, D
                            RegAF.Word = TableALU[0, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x83: // ADD A, E
                            RegAF.Word = TableALU[0, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x84: // ADD A, H
                            RegAF.Word = TableALU[0, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x85: // ADD A, L
                            RegAF.Word = TableALU[0, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x86: // ADD A, (HL)
                            RegAF.Word = TableALU[0, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x87: // ADD A, A
                            RegAF.Word = TableALU[0, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x88: // ADC A, B
                            RegAF.Word = TableALU[1, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x89: // ADC A, C
                            RegAF.Word = TableALU[1, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x8A: // ADC A, D
                            RegAF.Word = TableALU[1, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x8B: // ADC A, E
                            RegAF.Word = TableALU[1, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x8C: // ADC A, H
                            RegAF.Word = TableALU[1, RegAF.High, RegHL.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x8D: // ADC A, L
                            RegAF.Word = TableALU[1, RegAF.High, RegHL.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x8E: // ADC A, (HL)
                            RegAF.Word = TableALU[1, RegAF.High, ReadMemory(RegHL.Word), RegFlagC ? 1 : 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x8F: // ADC A, A
                            RegAF.Word = TableALU[1, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x90: // SUB B
                            RegAF.Word = TableALU[2, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x91: // SUB C
                            RegAF.Word = TableALU[2, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x92: // SUB D
                            RegAF.Word = TableALU[2, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x93: // SUB E
                            RegAF.Word = TableALU[2, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x94: // SUB H
                            RegAF.Word = TableALU[2, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x95: // SUB L
                            RegAF.Word = TableALU[2, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x96: // SUB (HL)
                            RegAF.Word = TableALU[2, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x97: // SUB A, A
                            RegAF.Word = TableALU[2, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x98: // SBC A, B
                            RegAF.Word = TableALU[3, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x99: // SBC A, C
                            RegAF.Word = TableALU[3, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x9A: // SBC A, D
                            RegAF.Word = TableALU[3, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x9B: // SBC A, E
                            RegAF.Word = TableALU[3, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x9C: // SBC A, H
                            RegAF.Word = TableALU[3, RegAF.High, RegHL.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x9D: // SBC A, L
                            RegAF.Word = TableALU[3, RegAF.High, RegHL.Low, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0x9E: // SBC A, (HL)
                            RegAF.Word = TableALU[3, RegAF.High, ReadMemory(RegHL.Word), RegFlagC ? 1 : 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0x9F: // SBC A, A
                            RegAF.Word = TableALU[3, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA0: // AND B
                            RegAF.Word = TableALU[4, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA1: // AND C
                            RegAF.Word = TableALU[4, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA2: // AND D
                            RegAF.Word = TableALU[4, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA3: // AND E
                            RegAF.Word = TableALU[4, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA4: // AND H
                            RegAF.Word = TableALU[4, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA5: // AND L
                            RegAF.Word = TableALU[4, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA6: // AND (HL)
                            RegAF.Word = TableALU[4, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xA7: // AND A
                            RegAF.Word = TableALU[4, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA8: // XOR B
                            RegAF.Word = TableALU[5, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xA9: // XOR C
                            RegAF.Word = TableALU[5, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xAA: // XOR D
                            RegAF.Word = TableALU[5, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xAB: // XOR E
                            RegAF.Word = TableALU[5, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xAC: // XOR H
                            RegAF.Word = TableALU[5, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xAD: // XOR L
                            RegAF.Word = TableALU[5, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xAE: // XOR (HL)
                            RegAF.Word = TableALU[5, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xAF: // XOR A
                            RegAF.Word = TableALU[5, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB0: // OR B
                            RegAF.Word = TableALU[6, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB1: // OR C
                            RegAF.Word = TableALU[6, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB2: // OR D
                            RegAF.Word = TableALU[6, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB3: // OR E
                            RegAF.Word = TableALU[6, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB4: // OR H
                            RegAF.Word = TableALU[6, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB5: // OR L
                            RegAF.Word = TableALU[6, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB6: // OR (HL)
                            RegAF.Word = TableALU[6, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xB7: // OR A
                            RegAF.Word = TableALU[6, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB8: // CP B
                            RegAF.Word = TableALU[7, RegAF.High, RegBC.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xB9: // CP C
                            RegAF.Word = TableALU[7, RegAF.High, RegBC.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xBA: // CP D
                            RegAF.Word = TableALU[7, RegAF.High, RegDE.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xBB: // CP E
                            RegAF.Word = TableALU[7, RegAF.High, RegDE.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xBC: // CP H
                            RegAF.Word = TableALU[7, RegAF.High, RegHL.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xBD: // CP L
                            RegAF.Word = TableALU[7, RegAF.High, RegHL.Low, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xBE: // CP (HL)
                            RegAF.Word = TableALU[7, RegAF.High, ReadMemory(RegHL.Word), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xBF: // CP A
                            RegAF.Word = TableALU[7, RegAF.High, RegAF.High, 0];
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xC0: // RET NZ
                            if (!RegFlagZ)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xC1: // POP BC
                            RegBC.Low = ReadMemory(RegSP.Word++); RegBC.High = ReadMemory(RegSP.Word++);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xC2: // JP NZ, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagZ)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xC3: // JP nn
                            RegPC.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            RegWZ.Word = RegPC.Word;
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xC4: // CALL NZ, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagZ)
                            {
                                totalExecutedCycles += 17; pendingCycles -= 17;
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xC5: // PUSH BC
                            WriteMemory(--RegSP.Word, RegBC.High); WriteMemory(--RegSP.Word, RegBC.Low);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xC6: // ADD A, n
                            RegAF.Word = TableALU[0, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xC7: // RST $00
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x00;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xC8: // RET Z
                            if (RegFlagZ)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xC9: // RET
                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                            RegWZ.Word = RegPC.Word;
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xCA: // JP Z, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagZ)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xCB: // (Prefix)
                            ++RegR;
                            switch (ReadOp(RegPC.Word++))
                            {
                                case 0x00: // RLC B
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x01: // RLC C
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x02: // RLC D
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x03: // RLC E
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x04: // RLC H
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x05: // RLC L
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x06: // RLC (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x07: // RLC A
                                    TUS = TableRotShift[1, 0, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x08: // RRC B
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x09: // RRC C
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x0A: // RRC D
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x0B: // RRC E
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x0C: // RRC H
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x0D: // RRC L
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x0E: // RRC (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x0F: // RRC A
                                    TUS = TableRotShift[1, 1, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x10: // RL B
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x11: // RL C
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x12: // RL D
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x13: // RL E
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x14: // RL H
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x15: // RL L
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x16: // RL (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x17: // RL A
                                    TUS = TableRotShift[1, 2, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x18: // RR B
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x19: // RR C
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x1A: // RR D
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x1B: // RR E
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x1C: // RR H
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x1D: // RR L
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x1E: // RR (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x1F: // RR A
                                    TUS = TableRotShift[1, 3, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x20: // SLA B
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x21: // SLA C
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x22: // SLA D
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x23: // SLA E
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x24: // SLA H
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x25: // SLA L
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x26: // SLA (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x27: // SLA A
                                    TUS = TableRotShift[1, 4, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x28: // SRA B
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x29: // SRA C
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x2A: // SRA D
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x2B: // SRA E
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x2C: // SRA H
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x2D: // SRA L
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x2E: // SRA (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x2F: // SRA A
                                    TUS = TableRotShift[1, 5, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x30: // SL1 B
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x31: // SL1 C
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x32: // SL1 D
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x33: // SL1 E
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x34: // SL1 H
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x35: // SL1 L
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x36: // SL1 (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x37: // SL1 A
                                    TUS = TableRotShift[1, 6, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x38: // SRL B
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegBC.High];
                                    RegBC.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x39: // SRL C
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegBC.Low];
                                    RegBC.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x3A: // SRL D
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegDE.High];
                                    RegDE.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x3B: // SRL E
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegDE.Low];
                                    RegDE.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x3C: // SRL H
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegHL.High];
                                    RegHL.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x3D: // SRL L
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegHL.Low];
                                    RegHL.Low = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x3E: // SRL (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegHL.Word)];
                                    WriteMemory(RegHL.Word, (byte)(TUS >> 8));
                                    RegAF.Low = (byte)TUS;
                                    break;
                                case 0x3F: // SRL A
                                    TUS = TableRotShift[1, 7, RegAF.Low + 256 * RegAF.High];
                                    RegAF.High = (byte)(TUS >> 8);
                                    RegAF.Low = (byte)TUS;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x40: // BIT 0, B
                                    RegFlagZ = (RegBC.High & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x41: // BIT 0, C
                                    RegFlagZ = (RegBC.Low & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x42: // BIT 0, D
                                    RegFlagZ = (RegDE.High & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x43: // BIT 0, E
                                    RegFlagZ = (RegDE.Low & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x44: // BIT 0, H
                                    RegFlagZ = (RegHL.High & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x45: // BIT 0, L
                                    RegFlagZ = (RegHL.Low & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x46: // BIT 0, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x47: // BIT 0, A
                                    RegFlagZ = (RegAF.High & 0x01) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x48: // BIT 1, B
                                    RegFlagZ = (RegBC.High & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x49: // BIT 1, C
                                    RegFlagZ = (RegBC.Low & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4A: // BIT 1, D
                                    RegFlagZ = (RegDE.High & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4B: // BIT 1, E
                                    RegFlagZ = (RegDE.Low & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4C: // BIT 1, H
                                    RegFlagZ = (RegHL.High & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4D: // BIT 1, L
                                    RegFlagZ = (RegHL.Low & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4E: // BIT 1, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x4F: // BIT 1, A
                                    RegFlagZ = (RegAF.High & 0x02) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x50: // BIT 2, B
                                    RegFlagZ = (RegBC.High & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x51: // BIT 2, C
                                    RegFlagZ = (RegBC.Low & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x52: // BIT 2, D
                                    RegFlagZ = (RegDE.High & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x53: // BIT 2, E
                                    RegFlagZ = (RegDE.Low & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x54: // BIT 2, H
                                    RegFlagZ = (RegHL.High & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x55: // BIT 2, L
                                    RegFlagZ = (RegHL.Low & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x56: // BIT 2, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x57: // BIT 2, A
                                    RegFlagZ = (RegAF.High & 0x04) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x58: // BIT 3, B
                                    RegFlagZ = (RegBC.High & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x59: // BIT 3, C
                                    RegFlagZ = (RegBC.Low & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5A: // BIT 3, D
                                    RegFlagZ = (RegDE.High & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5B: // BIT 3, E
                                    RegFlagZ = (RegDE.Low & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5C: // BIT 3, H
                                    RegFlagZ = (RegHL.High & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5D: // BIT 3, L
                                    RegFlagZ = (RegHL.Low & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5E: // BIT 3, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x5F: // BIT 3, A
                                    RegFlagZ = (RegAF.High & 0x08) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = !RegFlagZ;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x60: // BIT 4, B
                                    RegFlagZ = (RegBC.High & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x61: // BIT 4, C
                                    RegFlagZ = (RegBC.Low & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x62: // BIT 4, D
                                    RegFlagZ = (RegDE.High & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x63: // BIT 4, E
                                    RegFlagZ = (RegDE.Low & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x64: // BIT 4, H
                                    RegFlagZ = (RegHL.High & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x65: // BIT 4, L
                                    RegFlagZ = (RegHL.Low & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x66: // BIT 4, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x67: // BIT 4, A
                                    RegFlagZ = (RegAF.High & 0x10) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x68: // BIT 5, B
                                    RegFlagZ = (RegBC.High & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x69: // BIT 5, C
                                    RegFlagZ = (RegBC.Low & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6A: // BIT 5, D
                                    RegFlagZ = (RegDE.High & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6B: // BIT 5, E
                                    RegFlagZ = (RegDE.Low & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6C: // BIT 5, H
                                    RegFlagZ = (RegHL.High & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6D: // BIT 5, L
                                    RegFlagZ = (RegHL.Low & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6E: // BIT 5, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x6F: // BIT 5, A
                                    RegFlagZ = (RegAF.High & 0x20) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = !RegFlagZ;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x70: // BIT 6, B
                                    RegFlagZ = (RegBC.High & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x71: // BIT 6, C
                                    RegFlagZ = (RegBC.Low & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x72: // BIT 6, D
                                    RegFlagZ = (RegDE.High & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x73: // BIT 6, E
                                    RegFlagZ = (RegDE.Low & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x74: // BIT 6, H
                                    RegFlagZ = (RegHL.High & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x75: // BIT 6, L
                                    RegFlagZ = (RegHL.Low & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x76: // BIT 6, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x77: // BIT 6, A
                                    RegFlagZ = (RegAF.High & 0x40) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = false;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x78: // BIT 7, B
                                    RegFlagZ = (RegBC.High & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x79: // BIT 7, C
                                    RegFlagZ = (RegBC.Low & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7A: // BIT 7, D
                                    RegFlagZ = (RegDE.High & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7B: // BIT 7, E
                                    RegFlagZ = (RegDE.Low & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7C: // BIT 7, H
                                    RegFlagZ = (RegHL.High & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7D: // BIT 7, L
                                    RegFlagZ = (RegHL.Low & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7E: // BIT 7, (HL)
                                    RegFlagZ = (ReadMemory(RegHL.Word) & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = (RegWZ.High & 0x08) != 0;
                                    RegFlag5 = (RegWZ.High & 0x20) != 0;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x7F: // BIT 7, A
                                    RegFlagZ = (RegAF.High & 0x80) == 0;
                                    RegFlagP = RegFlagZ;
                                    RegFlagS = !RegFlagZ;
                                    RegFlag3 = false;
                                    RegFlag5 = false;
                                    RegFlagH = true;
                                    RegFlagN = false;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x80: // RES 0, B
                                    RegBC.High &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x81: // RES 0, C
                                    RegBC.Low &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x82: // RES 0, D
                                    RegDE.High &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x83: // RES 0, E
                                    RegDE.Low &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x84: // RES 0, H
                                    RegHL.High &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x85: // RES 0, L
                                    RegHL.Low &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x86: // RES 0, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x01)));
                                    break;
                                case 0x87: // RES 0, A
                                    RegAF.High &= unchecked((byte)~0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x88: // RES 1, B
                                    RegBC.High &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x89: // RES 1, C
                                    RegBC.Low &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x8A: // RES 1, D
                                    RegDE.High &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x8B: // RES 1, E
                                    RegDE.Low &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x8C: // RES 1, H
                                    RegHL.High &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x8D: // RES 1, L
                                    RegHL.Low &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x8E: // RES 1, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x02)));
                                    break;
                                case 0x8F: // RES 1, A
                                    RegAF.High &= unchecked((byte)~0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x90: // RES 2, B
                                    RegBC.High &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x91: // RES 2, C
                                    RegBC.Low &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x92: // RES 2, D
                                    RegDE.High &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x93: // RES 2, E
                                    RegDE.Low &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x94: // RES 2, H
                                    RegHL.High &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x95: // RES 2, L
                                    RegHL.Low &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x96: // RES 2, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x04)));
                                    break;
                                case 0x97: // RES 2, A
                                    RegAF.High &= unchecked((byte)~0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x98: // RES 3, B
                                    RegBC.High &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x99: // RES 3, C
                                    RegBC.Low &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x9A: // RES 3, D
                                    RegDE.High &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x9B: // RES 3, E
                                    RegDE.Low &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x9C: // RES 3, H
                                    RegHL.High &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x9D: // RES 3, L
                                    RegHL.Low &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x9E: // RES 3, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x08)));
                                    break;
                                case 0x9F: // RES 3, A
                                    RegAF.High &= unchecked((byte)~0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA0: // RES 4, B
                                    RegBC.High &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA1: // RES 4, C
                                    RegBC.Low &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA2: // RES 4, D
                                    RegDE.High &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA3: // RES 4, E
                                    RegDE.Low &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA4: // RES 4, H
                                    RegHL.High &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA5: // RES 4, L
                                    RegHL.Low &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA6: // RES 4, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x10)));
                                    break;
                                case 0xA7: // RES 4, A
                                    RegAF.High &= unchecked((byte)~0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA8: // RES 5, B
                                    RegBC.High &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xA9: // RES 5, C
                                    RegBC.Low &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xAA: // RES 5, D
                                    RegDE.High &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xAB: // RES 5, E
                                    RegDE.Low &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xAC: // RES 5, H
                                    RegHL.High &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xAD: // RES 5, L
                                    RegHL.Low &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xAE: // RES 5, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x20)));
                                    break;
                                case 0xAF: // RES 5, A
                                    RegAF.High &= unchecked((byte)~0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB0: // RES 6, B
                                    RegBC.High &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB1: // RES 6, C
                                    RegBC.Low &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB2: // RES 6, D
                                    RegDE.High &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB3: // RES 6, E
                                    RegDE.Low &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB4: // RES 6, H
                                    RegHL.High &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB5: // RES 6, L
                                    RegHL.Low &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB6: // RES 6, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x40)));
                                    break;
                                case 0xB7: // RES 6, A
                                    RegAF.High &= unchecked((byte)~0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB8: // RES 7, B
                                    RegBC.High &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xB9: // RES 7, C
                                    RegBC.Low &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xBA: // RES 7, D
                                    RegDE.High &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xBB: // RES 7, E
                                    RegDE.Low &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xBC: // RES 7, H
                                    RegHL.High &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xBD: // RES 7, L
                                    RegHL.Low &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xBE: // RES 7, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) & unchecked((byte)~0x80)));
                                    break;
                                case 0xBF: // RES 7, A
                                    RegAF.High &= unchecked((byte)~0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC0: // SET 0, B
                                    RegBC.High |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC1: // SET 0, C
                                    RegBC.Low |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC2: // SET 0, D
                                    RegDE.High |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC3: // SET 0, E
                                    RegDE.Low |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC4: // SET 0, H
                                    RegHL.High |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC5: // SET 0, L
                                    RegHL.Low |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC6: // SET 0, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x01)));
                                    break;
                                case 0xC7: // SET 0, A
                                    RegAF.High |= unchecked((byte)0x01);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC8: // SET 1, B
                                    RegBC.High |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xC9: // SET 1, C
                                    RegBC.Low |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xCA: // SET 1, D
                                    RegDE.High |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xCB: // SET 1, E
                                    RegDE.Low |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xCC: // SET 1, H
                                    RegHL.High |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xCD: // SET 1, L
                                    RegHL.Low |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xCE: // SET 1, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x02)));
                                    break;
                                case 0xCF: // SET 1, A
                                    RegAF.High |= unchecked((byte)0x02);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD0: // SET 2, B
                                    RegBC.High |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD1: // SET 2, C
                                    RegBC.Low |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD2: // SET 2, D
                                    RegDE.High |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD3: // SET 2, E
                                    RegDE.Low |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD4: // SET 2, H
                                    RegHL.High |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD5: // SET 2, L
                                    RegHL.Low |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD6: // SET 2, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x04)));
                                    break;
                                case 0xD7: // SET 2, A
                                    RegAF.High |= unchecked((byte)0x04);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD8: // SET 3, B
                                    RegBC.High |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xD9: // SET 3, C
                                    RegBC.Low |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xDA: // SET 3, D
                                    RegDE.High |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xDB: // SET 3, E
                                    RegDE.Low |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xDC: // SET 3, H
                                    RegHL.High |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xDD: // SET 3, L
                                    RegHL.Low |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xDE: // SET 3, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x08)));
                                    break;
                                case 0xDF: // SET 3, A
                                    RegAF.High |= unchecked((byte)0x08);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE0: // SET 4, B
                                    RegBC.High |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE1: // SET 4, C
                                    RegBC.Low |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE2: // SET 4, D
                                    RegDE.High |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE3: // SET 4, E
                                    RegDE.Low |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE4: // SET 4, H
                                    RegHL.High |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE5: // SET 4, L
                                    RegHL.Low |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE6: // SET 4, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x10)));
                                    break;
                                case 0xE7: // SET 4, A
                                    RegAF.High |= unchecked((byte)0x10);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE8: // SET 5, B
                                    RegBC.High |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xE9: // SET 5, C
                                    RegBC.Low |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEA: // SET 5, D
                                    RegDE.High |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEB: // SET 5, E
                                    RegDE.Low |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEC: // SET 5, H
                                    RegHL.High |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xED: // SET 5, L
                                    RegHL.Low |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEE: // SET 5, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x20)));
                                    break;
                                case 0xEF: // SET 5, A
                                    RegAF.High |= unchecked((byte)0x20);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF0: // SET 6, B
                                    RegBC.High |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF1: // SET 6, C
                                    RegBC.Low |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF2: // SET 6, D
                                    RegDE.High |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF3: // SET 6, E
                                    RegDE.Low |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF4: // SET 6, H
                                    RegHL.High |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF5: // SET 6, L
                                    RegHL.Low |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF6: // SET 6, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x40)));
                                    break;
                                case 0xF7: // SET 6, A
                                    RegAF.High |= unchecked((byte)0x40);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF8: // SET 7, B
                                    RegBC.High |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xF9: // SET 7, C
                                    RegBC.Low |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xFA: // SET 7, D
                                    RegDE.High |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xFB: // SET 7, E
                                    RegDE.Low |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xFC: // SET 7, H
                                    RegHL.High |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xFD: // SET 7, L
                                    RegHL.Low |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xFE: // SET 7, (HL)
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(RegHL.Word, (byte)(ReadMemory(RegHL.Word) | unchecked((byte)0x80)));
                                    break;
                                case 0xFF: // SET 7, A
                                    RegAF.High |= unchecked((byte)0x80);
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                            }
                            break;
                        case 0xCC: // CALL Z, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagZ)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xCD: // CALL nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = RegWZ.Word;
                            totalExecutedCycles += 17; pendingCycles -= 17;
                            break;
                        case 0xCE: // ADC A, n
                            RegAF.Word = TableALU[1, RegAF.High, ReadOpArg(RegPC.Word++), RegFlagC ? 1 : 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xCF: // RST $08
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x08;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xD0: // RET NC
                            if (!RegFlagC)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xD1: // POP DE
                            RegDE.Low = ReadMemory(RegSP.Word++); RegDE.High = ReadMemory(RegSP.Word++);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xD2: // JP NC, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagC)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xD3: // OUT n, A
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) | (RegAF.High << 8));
                            WriteHardware(TUS, RegAF.High);
                            RegWZ.Low = (byte)(((TUS & 0xFF) + 1) & 0xFF);
                            RegWZ.High = RegAF.High;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xD4: // CALL NC, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagC)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xD5: // PUSH DE
                            WriteMemory(--RegSP.Word, RegDE.High); WriteMemory(--RegSP.Word, RegDE.Low);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xD6: // SUB n
                            RegAF.Word = TableALU[2, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xD7: // RST $10
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x10;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xD8: // RET C
                            if (RegFlagC)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xD9: // EXX
                            TUS = RegBC.Word; RegBC.Word = RegAltBC.Word; RegAltBC.Word = TUS;
                            TUS = RegDE.Word; RegDE.Word = RegAltDE.Word; RegAltDE.Word = TUS;
                            TUS = RegHL.Word; RegHL.Word = RegAltHL.Word; RegAltHL.Word = TUS;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xDA: // JP C, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagC)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xDB: // IN A, n
                            TUS = (ushort)(ReadOpArg(RegPC.Word++) | (RegAF.High << 8));
                            RegAF.High = ReadHardware(TUS);
                            RegWZ.Word = (ushort)(TUS + 1);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xDC: // CALL C, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagC)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xDD: // (Prefix)
                            ++RegR;
                            switch (ReadOp(RegPC.Word++))
                            {
                                case 0x00: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x01: // LD BC, nn
                                    RegBC.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x02: // LD (BC), A
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    WriteMemory(RegBC.Word, RegAF.High);
                                    break;
                                case 0x03: // INC BC
                                    ++RegBC.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x04: // INC B
                                    RegAF.Low = (byte)(TableInc[++RegBC.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x05: // DEC B
                                    RegAF.Low = (byte)(TableDec[--RegBC.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x06: // LD B, n
                                    RegBC.High = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x07: // RLCA
                                    RegAF.Word = TableRotShift[0, 0, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x08: // EX AF, AF'
                                    TUS = RegAF.Word; RegAF.Word = RegAltAF.Word; RegAltAF.Word = TUS;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x09: // ADD IX, BC
                                    TI1 = (short)RegIX.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIX.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIX.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x0A: // LD A, (BC)
                                    RegAF.High = ReadMemory(RegBC.Word);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x0B: // DEC BC
                                    --RegBC.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x0C: // INC C
                                    RegAF.Low = (byte)(TableInc[++RegBC.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0D: // DEC C
                                    RegAF.Low = (byte)(TableDec[--RegBC.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0E: // LD C, n
                                    RegBC.Low = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x0F: // RRCA
                                    RegAF.Word = TableRotShift[0, 1, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x10: // DJNZ d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (--RegBC.High != 0)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 13; pendingCycles -= 13;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 8; pendingCycles -= 8;
                                    }
                                    break;
                                case 0x11: // LD DE, nn
                                    RegDE.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x12: // LD (DE), A
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    WriteMemory(RegDE.Word, RegAF.High);
                                    break;
                                case 0x13: // INC DE
                                    ++RegDE.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x14: // INC D
                                    RegAF.Low = (byte)(TableInc[++RegDE.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x15: // DEC D
                                    RegAF.Low = (byte)(TableDec[--RegDE.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x16: // LD D, n
                                    RegDE.High = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x17: // RLA
                                    RegAF.Word = TableRotShift[0, 2, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x18: // JR d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    RegPC.Word = (ushort)(RegPC.Word + TSB);
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x19: // ADD IX, DE
                                    TI1 = (short)RegIX.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIX.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIX.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x1A: // LD A, (DE)
                                    RegAF.High = ReadMemory(RegDE.Word);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x1B: // DEC DE
                                    --RegDE.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x1C: // INC E
                                    RegAF.Low = (byte)(TableInc[++RegDE.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1D: // DEC E
                                    RegAF.Low = (byte)(TableDec[--RegDE.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1E: // LD E, n
                                    RegDE.Low = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x1F: // RRA
                                    RegAF.Word = TableRotShift[0, 3, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x20: // JR NZ, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x21: // LD IX, nn
                                    RegIX.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    break;
                                case 0x22: // LD (nn), IX
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegIX.Low);
                                    WriteMemory(TUS, RegIX.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x23: // INC IX
                                    ++RegIX.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x24: // INC IXH
                                    RegAF.Low = (byte)(TableInc[++RegIX.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x25: // DEC IXH
                                    RegAF.Low = (byte)(TableDec[--RegIX.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x26: // LD IXH, n
                                    RegIX.High = ReadOpArg(RegPC.Word++);
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x27: // DAA
                                    RegAF.Word = TableDaa[RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x28: // JR Z, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (RegFlagZ)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x29: // ADD IX, IX
                                    TI1 = (short)RegIX.Word; TI2 = (short)RegIX.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIX.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIX.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x2A: // LD IX, (nn)
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegIX.Low = ReadMemory(TUS++); RegIX.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    break;
                                case 0x2B: // DEC IX
                                    --RegIX.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x2C: // INC IXL
                                    RegAF.Low = (byte)(TableInc[++RegIX.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2D: // DEC IXL
                                    RegAF.Low = (byte)(TableDec[--RegIX.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2E: // LD IXL, n
                                    RegIX.Low = ReadOpArg(RegPC.Word++);
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2F: // CPL
                                    RegAF.High ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x30: // JR NC, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (!RegFlagC)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x31: // LD SP, nn
                                    RegSP.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x32: // LD (nn), A
                                    totalExecutedCycles += 13; pendingCycles -= 13;
                                    WriteMemory((ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256), RegAF.High);
                                    break;
                                case 0x33: // INC SP
                                    ++RegSP.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x34: // INC (IX+d)
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    TB = ReadMemory(RegWZ.Word); RegAF.Low = (byte)(TableInc[++TB] | (RegAF.Low & 1)); WriteMemory(RegWZ.Word, TB);
                                    break;
                                case 0x35: // DEC (IX+d)
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    TB = ReadMemory(RegWZ.Word); RegAF.Low = (byte)(TableDec[--TB] | (RegAF.Low & 1)); WriteMemory(RegWZ.Word, TB);
                                    break;
                                case 0x36: // LD (IX+d), n
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, ReadOpArg(RegPC.Word++));
                                    break;
                                case 0x37: // SCF
                                    RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x38: // JR C, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (RegFlagC)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x39: // ADD IX, SP
                                    TI1 = (short)RegIX.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIX.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIX.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x3A: // LD A, (nn)
                                    RegAF.High = ReadMemory((ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256));
                                    totalExecutedCycles += 13; pendingCycles -= 13;
                                    break;
                                case 0x3B: // DEC SP
                                    --RegSP.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x3C: // INC A
                                    RegAF.Low = (byte)(TableInc[++RegAF.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3D: // DEC A
                                    RegAF.Low = (byte)(TableDec[--RegAF.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3E: // LD A, n
                                    RegAF.High = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x3F: // CCF
                                    RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x40: // LD B, B
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x41: // LD B, C
                                    RegBC.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x42: // LD B, D
                                    RegBC.High = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x43: // LD B, E
                                    RegBC.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x44: // LD B, IXH
                                    RegBC.High = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x45: // LD B, IXL
                                    RegBC.High = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x46: // LD B, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegBC.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x47: // LD B, A
                                    RegBC.High = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x48: // LD C, B
                                    RegBC.Low = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x49: // LD C, C
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4A: // LD C, D
                                    RegBC.Low = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4B: // LD C, E
                                    RegBC.Low = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4C: // LD C, IXH
                                    RegBC.Low = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x4D: // LD C, IXL
                                    RegBC.Low = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x4E: // LD C, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegBC.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x4F: // LD C, A
                                    RegBC.Low = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x50: // LD D, B
                                    RegDE.High = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x51: // LD D, C
                                    RegDE.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x52: // LD D, D
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x53: // LD D, E
                                    RegDE.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x54: // LD D, IXH
                                    RegDE.High = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x55: // LD D, IXL
                                    RegDE.High = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x56: // LD D, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegDE.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x57: // LD D, A
                                    RegDE.High = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x58: // LD E, B
                                    RegDE.Low = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x59: // LD E, C
                                    RegDE.Low = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5A: // LD E, D
                                    RegDE.Low = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5B: // LD E, E
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5C: // LD E, IXH
                                    RegDE.Low = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x5D: // LD E, IXL
                                    RegDE.Low = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x5E: // LD E, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegDE.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x5F: // LD E, A
                                    RegDE.Low = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x60: // LD IXH, B
                                    RegIX.High = RegBC.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x61: // LD IXH, C
                                    RegIX.High = RegBC.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x62: // LD IXH, D
                                    RegIX.High = RegDE.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x63: // LD IXH, E
                                    RegIX.High = RegDE.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x64: // LD IXH, IXH
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x65: // LD IXH, IXL
                                    RegIX.High = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x66: // LD H, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegHL.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x67: // LD IXH, A
                                    RegIX.High = RegAF.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x68: // LD IXL, B
                                    RegIX.Low = RegBC.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x69: // LD IXL, C
                                    RegIX.Low = RegBC.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6A: // LD IXL, D
                                    RegIX.Low = RegDE.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6B: // LD IXL, E
                                    RegIX.Low = RegDE.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6C: // LD IXL, IXH
                                    RegIX.Low = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6D: // LD IXL, IXL
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6E: // LD L, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegHL.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x6F: // LD IXL, A
                                    RegIX.Low = RegAF.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x70: // LD (IX+d), B
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegBC.High);
                                    break;
                                case 0x71: // LD (IX+d), C
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegBC.Low);
                                    break;
                                case 0x72: // LD (IX+d), D
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegDE.High);
                                    break;
                                case 0x73: // LD (IX+d), E
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegDE.Low);
                                    break;
                                case 0x74: // LD (IX+d), H
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegHL.High);
                                    break;
                                case 0x75: // LD (IX+d), L
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegHL.Low);
                                    break;
                                case 0x76: // HALT
                                    Halt();
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x77: // LD (IX+d), A
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegAF.High);
                                    break;
                                case 0x78: // LD A, B
                                    RegAF.High = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x79: // LD A, C
                                    RegAF.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7A: // LD A, D
                                    RegAF.High = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7B: // LD A, E
                                    RegAF.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7C: // LD A, IXH
                                    RegAF.High = RegIX.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x7D: // LD A, IXL
                                    RegAF.High = RegIX.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x7E: // LD A, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x7F: // LD A, A
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x80: // ADD A, B
                                    RegAF.Word = TableALU[0, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x81: // ADD A, C
                                    RegAF.Word = TableALU[0, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x82: // ADD A, D
                                    RegAF.Word = TableALU[0, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x83: // ADD A, E
                                    RegAF.Word = TableALU[0, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x84: // ADD A, IXH
                                    RegAF.Word = TableALU[0, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x85: // ADD A, IXL
                                    RegAF.Word = TableALU[0, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x86: // ADD A, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[0, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;//modify 16 to 19
                                    break;
                                case 0x87: // ADD A, A
                                    RegAF.Word = TableALU[0, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x88: // ADC A, B
                                    RegAF.Word = TableALU[1, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x89: // ADC A, C
                                    RegAF.Word = TableALU[1, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8A: // ADC A, D
                                    RegAF.Word = TableALU[1, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8B: // ADC A, E
                                    RegAF.Word = TableALU[1, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8C: // ADC A, IXH
                                    RegAF.Word = TableALU[1, RegAF.High, RegIX.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x8D: // ADC A, IXL
                                    RegAF.Word = TableALU[1, RegAF.High, RegIX.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x8E: // ADC A, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[1, RegAF.High, ReadMemory(RegWZ.Word), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x8F: // ADC A, A
                                    RegAF.Word = TableALU[1, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x90: // SUB B
                                    RegAF.Word = TableALU[2, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x91: // SUB C
                                    RegAF.Word = TableALU[2, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x92: // SUB D
                                    RegAF.Word = TableALU[2, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x93: // SUB E
                                    RegAF.Word = TableALU[2, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x94: // SUB IXH
                                    RegAF.Word = TableALU[2, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x95: // SUB IXL
                                    RegAF.Word = TableALU[2, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x96: // SUB (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[2, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x97: // SUB A, A
                                    RegAF.Word = TableALU[2, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x98: // SBC A, B
                                    RegAF.Word = TableALU[3, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x99: // SBC A, C
                                    RegAF.Word = TableALU[3, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9A: // SBC A, D
                                    RegAF.Word = TableALU[3, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9B: // SBC A, E
                                    RegAF.Word = TableALU[3, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9C: // SBC A, IXH
                                    RegAF.Word = TableALU[3, RegAF.High, RegIX.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x9D: // SBC A, IXL
                                    RegAF.Word = TableALU[3, RegAF.High, RegIX.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x9E: // SBC A, (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[3, RegAF.High, ReadMemory(RegWZ.Word), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x9F: // SBC A, A
                                    RegAF.Word = TableALU[3, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA0: // AND B
                                    RegAF.Word = TableALU[4, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA1: // AND C
                                    RegAF.Word = TableALU[4, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA2: // AND D
                                    RegAF.Word = TableALU[4, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA3: // AND E
                                    RegAF.Word = TableALU[4, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA4: // AND IXH
                                    RegAF.Word = TableALU[4, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xA5: // AND IXL
                                    RegAF.Word = TableALU[4, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xA6: // AND (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[4, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xA7: // AND A
                                    RegAF.Word = TableALU[4, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA8: // XOR B
                                    RegAF.Word = TableALU[5, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA9: // XOR C
                                    RegAF.Word = TableALU[5, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAA: // XOR D
                                    RegAF.Word = TableALU[5, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAB: // XOR E
                                    RegAF.Word = TableALU[5, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAC: // XOR IXH
                                    RegAF.Word = TableALU[5, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xAD: // XOR IXL
                                    RegAF.Word = TableALU[5, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xAE: // XOR (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[5, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xAF: // XOR A
                                    RegAF.Word = TableALU[5, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB0: // OR B
                                    RegAF.Word = TableALU[6, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB1: // OR C
                                    RegAF.Word = TableALU[6, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB2: // OR D
                                    RegAF.Word = TableALU[6, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB3: // OR E
                                    RegAF.Word = TableALU[6, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB4: // OR IXH
                                    RegAF.Word = TableALU[6, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xB5: // OR IXL
                                    RegAF.Word = TableALU[6, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xB6: // OR (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[6, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xB7: // OR A
                                    RegAF.Word = TableALU[6, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB8: // CP B
                                    RegAF.Word = TableALU[7, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB9: // CP C
                                    RegAF.Word = TableALU[7, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBA: // CP D
                                    RegAF.Word = TableALU[7, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBB: // CP E
                                    RegAF.Word = TableALU[7, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBC: // CP IXH
                                    RegAF.Word = TableALU[7, RegAF.High, RegIX.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xBD: // CP IXL
                                    RegAF.Word = TableALU[7, RegAF.High, RegIX.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xBE: // CP (IX+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    RegAF.Word = TableALU[7, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xBF: // CP A
                                    RegAF.Word = TableALU[7, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC0: // RET NZ
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xC1: // POP BC
                                    RegBC.Low = ReadMemory(RegSP.Word++); RegBC.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC2: // JP NZ, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC3: // JP nn
                                    RegPC.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC4: // CALL NZ, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagZ)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xC5: // PUSH BC
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegBC.High); WriteMemory(--RegSP.Word, RegBC.Low);
                                    break;
                                case 0xC6: // ADD A, n
                                    RegAF.Word = TableALU[0, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xC7: // RST $00
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x00;
                                    break;
                                case 0xC8: // RET Z
                                    if (RegFlagZ)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xC9: // RET
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xCA: // JP Z, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagZ)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xCB: // (Prefix)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    //++RegR;
                                    RegWZ.Word = (ushort)(RegIX.Word + Displacement);
                                    switch (ReadOpArg(RegPC.Word++))
                                    {
                                        case 0x00: // RLC (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x01: // RLC (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x02: // RLC (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x03: // RLC (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x04: // RLC (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x05: // RLC (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x06: // RLC (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x07: // RLC (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x08: // RRC (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x09: // RRC (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x0A: // RRC (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x0B: // RRC (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x0C: // RRC (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x0D: // RRC (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x0E: // RRC (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0F: // RRC (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x10: // RL (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x11: // RL (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x12: // RL (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x13: // RL (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x14: // RL (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x15: // RL (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x16: // RL (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x17: // RL (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x18: // RR (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x19: // RR (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x1A: // RR (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x1B: // RR (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x1C: // RR (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x1D: // RR (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x1E: // RR (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1F: // RR (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x20: // SLA (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x21: // SLA (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x22: // SLA (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x23: // SLA (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x24: // SLA (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x25: // SLA (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x26: // SLA (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x27: // SLA (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x28: // SRA (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x29: // SRA (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x2A: // SRA (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x2B: // SRA (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x2C: // SRA (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x2D: // SRA (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x2E: // SRA (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2F: // SRA (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x30: // SL1 (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x31: // SL1 (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x32: // SL1 (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x33: // SL1 (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x34: // SL1 (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x35: // SL1 (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x36: // SL1 (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x37: // SL1 (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x38: // SRL (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.High = (byte)TUS;
                                            break;
                                        case 0x39: // SRL (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegBC.Low = (byte)TUS;
                                            break;
                                        case 0x3A: // SRL (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.High = (byte)TUS;
                                            break;
                                        case 0x3B: // SRL (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegDE.Low = (byte)TUS;
                                            break;
                                        case 0x3C: // SRL (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.High = (byte)TUS;
                                            break;
                                        case 0x3D: // SRL (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegHL.Low = (byte)TUS;
                                            break;
                                        case 0x3E: // SRL (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3F: // SRL (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            RegAF.High = (byte)TUS;
                                            break;
                                        case 0x40: // BIT 0, (IX+d)
                                        case 0x41: // BIT 0, (IX+d)
                                        case 0x42: // BIT 0, (IX+d)
                                        case 0x43: // BIT 0, (IX+d)
                                        case 0x44: // BIT 0, (IX+d)
                                        case 0x45: // BIT 0, (IX+d)
                                        case 0x46: // BIT 0, (IX+d)
                                        case 0x47: // BIT 0, (IX+d)
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x01) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            break;
                                        case 0x48: // BIT 1, (IX+d)
                                        case 0x49: // BIT 1, (IX+d)
                                        case 0x4A: // BIT 1, (IX+d)
                                        case 0x4B: // BIT 1, (IX+d)
                                        case 0x4C: // BIT 1, (IX+d)
                                        case 0x4D: // BIT 1, (IX+d)
                                        case 0x4E: // BIT 1, (IX+d)
                                        case 0x4F: // BIT 1, (IX+d)
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x02) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            break;
                                        case 0x50: // BIT 2, (IX+d)
                                        case 0x51: // BIT 2, (IX+d)
                                        case 0x52: // BIT 2, (IX+d)
                                        case 0x53: // BIT 2, (IX+d)
                                        case 0x54: // BIT 2, (IX+d)
                                        case 0x55: // BIT 2, (IX+d)
                                        case 0x56: // BIT 2, (IX+d)
                                        case 0x57: // BIT 2, (IX+d)
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x04) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            break;
                                        case 0x58: // BIT 3, (IX+d)
                                        case 0x59: // BIT 3, (IX+d)
                                        case 0x5A: // BIT 3, (IX+d)
                                        case 0x5B: // BIT 3, (IX+d)
                                        case 0x5C: // BIT 3, (IX+d)
                                        case 0x5D: // BIT 3, (IX+d)
                                        case 0x5E: // BIT 3, (IX+d)
                                        case 0x5F: // BIT 3, (IX+d)
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x08) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            break;
                                        case 0x60: // BIT 4, (IX+d)
                                        case 0x61: // BIT 4, (IX+d)
                                        case 0x62: // BIT 4, (IX+d)
                                        case 0x63: // BIT 4, (IX+d)
                                        case 0x64: // BIT 4, (IX+d)
                                        case 0x65: // BIT 4, (IX+d)
                                        case 0x66: // BIT 4, (IX+d)
                                        case 0x67: // BIT 4, (IX+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x10) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x68: // BIT 5, (IX+d)
                                        case 0x69: // BIT 5, (IX+d)
                                        case 0x6A: // BIT 5, (IX+d)
                                        case 0x6B: // BIT 5, (IX+d)
                                        case 0x6C: // BIT 5, (IX+d)
                                        case 0x6D: // BIT 5, (IX+d)
                                        case 0x6E: // BIT 5, (IX+d)
                                        case 0x6F: // BIT 5, (IX+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x20) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x70: // BIT 6, (IX+d)
                                        case 0x71: // BIT 6, (IX+d)
                                        case 0x72: // BIT 6, (IX+d)
                                        case 0x73: // BIT 6, (IX+d)
                                        case 0x74: // BIT 6, (IX+d)
                                        case 0x75: // BIT 6, (IX+d)
                                        case 0x76: // BIT 6, (IX+d)
                                        case 0x77: // BIT 6, (IX+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x40) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x78: // BIT 7, (IX+d)
                                        case 0x79: // BIT 7, (IX+d)
                                        case 0x7A: // BIT 7, (IX+d)
                                        case 0x7B: // BIT 7, (IX+d)
                                        case 0x7C: // BIT 7, (IX+d)
                                        case 0x7D: // BIT 7, (IX+d)
                                        case 0x7E: // BIT 7, (IX+d)
                                        case 0x7F: // BIT 7, (IX+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x80) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = !TBOOL;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x80: // RES 0, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0x81: // RES 0, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0x82: // RES 0, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0x83: // RES 0, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0x84: // RES 0, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0x85: // RES 0, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0x86: // RES 0, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x87: // RES 0, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0x88: // RES 1, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0x89: // RES 1, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0x8A: // RES 1, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0x8B: // RES 1, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0x8C: // RES 1, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0x8D: // RES 1, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0x8E: // RES 1, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8F: // RES 1, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0x90: // RES 2, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0x91: // RES 2, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0x92: // RES 2, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0x93: // RES 2, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0x94: // RES 2, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0x95: // RES 2, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0x96: // RES 2, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x97: // RES 2, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0x98: // RES 3, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0x99: // RES 3, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0x9A: // RES 3, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0x9B: // RES 3, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0x9C: // RES 3, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0x9D: // RES 3, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0x9E: // RES 3, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9F: // RES 3, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xA0: // RES 4, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xA1: // RES 4, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xA2: // RES 4, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xA3: // RES 4, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xA4: // RES 4, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xA5: // RES 4, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xA6: // RES 4, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA7: // RES 4, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xA8: // RES 5, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xA9: // RES 5, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xAA: // RES 5, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xAB: // RES 5, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xAC: // RES 5, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xAD: // RES 5, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xAE: // RES 5, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAF: // RES 5, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xB0: // RES 6, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xB1: // RES 6, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xB2: // RES 6, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xB3: // RES 6, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xB4: // RES 6, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xB5: // RES 6, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xB6: // RES 6, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB7: // RES 6, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xB8: // RES 7, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xB9: // RES 7, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xBA: // RES 7, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xBB: // RES 7, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xBC: // RES 7, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xBD: // RES 7, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xBE: // RES 7, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBF: // RES 7, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xC0: // SET 0, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xC1: // SET 0, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xC2: // SET 0, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xC3: // SET 0, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xC4: // SET 0, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xC5: // SET 0, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xC6: // SET 0, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC7: // SET 0, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xC8: // SET 1, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xC9: // SET 1, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xCA: // SET 1, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xCB: // SET 1, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xCC: // SET 1, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xCD: // SET 1, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xCE: // SET 1, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCF: // SET 1, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xD0: // SET 2, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xD1: // SET 2, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xD2: // SET 2, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xD3: // SET 2, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xD4: // SET 2, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xD5: // SET 2, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xD6: // SET 2, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD7: // SET 2, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xD8: // SET 3, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xD9: // SET 3, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xDA: // SET 3, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xDB: // SET 3, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xDC: // SET 3, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xDD: // SET 3, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xDE: // SET 3, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDF: // SET 3, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xE0: // SET 4, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xE1: // SET 4, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xE2: // SET 4, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xE3: // SET 4, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xE4: // SET 4, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xE5: // SET 4, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xE6: // SET 4, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE7: // SET 4, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xE8: // SET 5, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xE9: // SET 5, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xEA: // SET 5, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xEB: // SET 5, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xEC: // SET 5, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xED: // SET 5, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xEE: // SET 5, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEF: // SET 5, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xF0: // SET 6, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xF1: // SET 6, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xF2: // SET 6, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xF3: // SET 6, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xF4: // SET 6, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xF5: // SET 6, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xF6: // SET 6, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF7: // SET 6, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                        case 0xF8: // SET 7, (IX+d)→B
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegBC.High);
                                            break;
                                        case 0xF9: // SET 7, (IX+d)→C
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegBC.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegBC.Low);
                                            break;
                                        case 0xFA: // SET 7, (IX+d)→D
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegDE.High);
                                            break;
                                        case 0xFB: // SET 7, (IX+d)→E
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegDE.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegDE.Low);
                                            break;
                                        case 0xFC: // SET 7, (IX+d)→H
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegHL.High);
                                            break;
                                        case 0xFD: // SET 7, (IX+d)→L
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegHL.Low = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegHL.Low);
                                            break;
                                        case 0xFE: // SET 7, (IX+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFF: // SET 7, (IX+d)→A
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            RegAF.High = (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80));
                                            WriteMemory(RegWZ.Word, RegAF.High);
                                            break;
                                    }
                                    break;
                                case 0xCC: // CALL Z, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagZ)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xCD: // CALL nn
                                    totalExecutedCycles += 17; pendingCycles -= 17;
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = TUS;
                                    break;
                                case 0xCE: // ADC A, n
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegAF.Word = TableALU[1, RegAF.High, ReadMemory(RegPC.Word++), RegFlagC ? 1 : 0];
                                    break;
                                case 0xCF: // RST $08
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x08;
                                    break;
                                case 0xD0: // RET NC
                                    if (!RegFlagC)
                                    {
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xD1: // POP DE
                                    RegDE.Low = ReadMemory(RegSP.Word++); RegDE.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xD2: // JP NC, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagC)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xD3: // OUT n, A
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteHardware(ReadMemory(RegPC.Word++), RegAF.High);
                                    break;
                                case 0xD4: // CALL NC, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xD5: // PUSH DE
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegDE.High); WriteMemory(--RegSP.Word, RegDE.Low);
                                    break;
                                case 0xD6: // SUB n
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegAF.Word = TableALU[2, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    break;
                                case 0xD7: // RST $10
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x10;
                                    break;
                                case 0xD8: // RET C
                                    if (RegFlagC)
                                    {
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xD9: // EXX
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    TUS = RegBC.Word; RegBC.Word = RegAltBC.Word; RegAltBC.Word = TUS;
                                    TUS = RegDE.Word; RegDE.Word = RegAltDE.Word; RegAltDE.Word = TUS;
                                    TUS = RegHL.Word; RegHL.Word = RegAltHL.Word; RegAltHL.Word = TUS;
                                    break;
                                case 0xDA: // JP C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xDB: // IN A, n
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) | (RegAF.High << 8));
                                    RegAF.High = ReadHardware(TUS);
                                    RegWZ.Word = (ushort)(TUS + 1);
                                    break;
                                case 0xDC: // CALL C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xDD: // <-
                                    // Invalid sequence.
                                    totalExecutedCycles += 1337; pendingCycles -= 1337;
                                    break;
                                case 0xDE: // SBC A, n
                                    RegAF.Word = TableALU[3, RegAF.High, ReadMemory(RegPC.Word++), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xDF: // RST $18
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x18;
                                    break;
                                case 0xE0: // RET PO
                                    if (!RegFlagP)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xE1: // POP IX
                                    RegIX.Low = ReadMemory(RegSP.Word++); RegIX.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    break;
                                case 0xE2: // JP PO, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagP)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xE3: // EX (SP), IX
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    TUS = RegSP.Word; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
                                    WriteMemory(TUS++, RegIX.Low); WriteMemory(TUS, RegIX.High);
                                    RegIX.Low = TBL; RegIX.High = TBH;
                                    RegWZ.Word = RegIX.Word;
                                    break;
                                case 0xE4: // CALL C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xE5: // PUSH IX
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(--RegSP.Word, RegIX.High); WriteMemory(--RegSP.Word, RegIX.Low);
                                    break;
                                case 0xE6: // AND n
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegAF.Word = TableALU[4, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    break;
                                case 0xE7: // RST $20
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x20;
                                    break;
                                case 0xE8: // RET PE
                                    if (RegFlagP)
                                    {
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xE9: // JP IX
                                    RegPC.Word = RegIX.Word;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEA: // JP PE, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagP)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xEB: // EX DE, HL
                                    TUS = RegDE.Word; RegDE.Word = RegHL.Word; RegHL.Word = TUS;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEC: // CALL PE, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagP)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xED: // (Prefix)
                                    ++RegR;
                                    switch (ReadOp(RegPC.Word++))
                                    {
                                        case 0x00: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x01: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x02: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x03: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x04: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x05: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x06: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x07: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x08: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x09: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x10: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x11: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x12: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x13: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x14: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x15: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x16: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x17: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x18: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x19: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x20: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x21: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x22: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x23: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x24: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x25: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x26: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x27: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x28: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x29: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x30: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x31: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x32: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x33: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x34: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x35: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x36: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x37: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x38: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x39: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x40: // IN B, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegBC.High = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegBC.High > 127;
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlag5 = (RegBC.High & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegBC.High & 0x08) != 0;
                                            RegFlagP = TableParity[RegBC.High];
                                            RegFlagN = false;
                                            break;
                                        case 0x41: // OUT C, B
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegBC.High);
                                            break;
                                        case 0x42: // SBC HL, BC
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegBC.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegBC.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x43: // LD (nn), BC
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegBC.Low);
                                            WriteMemory(TUS, RegBC.High);
                                            break;
                                        case 0x44: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x45: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x46: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x47: // LD I, A
                                            RegI = RegAF.High;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x48: // IN C, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegBC.Low = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegBC.Low > 127;
                                            RegFlagZ = RegBC.Low == 0;
                                            RegFlag5 = (RegBC.Low & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegBC.Low & 0x08) != 0;
                                            RegFlagP = TableParity[RegBC.Low];
                                            RegFlagN = false;
                                            break;
                                        case 0x49: // OUT C, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegBC.Low);
                                            break;
                                        case 0x4A: // ADC HL, BC
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x4B: // LD BC, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegBC.Low = ReadMemory(TUS++); RegBC.High = ReadMemory(TUS);
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x4C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x4D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x4E: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x4F: // LD R, A
                                            RegR = RegAF.High;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x50: // IN D, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegDE.High = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegDE.High > 127;
                                            RegFlagZ = RegDE.High == 0;
                                            RegFlag5 = (RegDE.High & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegDE.High & 0x08) != 0;
                                            RegFlagP = TableParity[RegDE.High];
                                            RegFlagN = false;
                                            break;
                                        case 0x51: // OUT C, D
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegDE.High);
                                            break;
                                        case 0x52: // SBC HL, DE
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegDE.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegDE.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x53: // LD (nn), DE
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegDE.Low);
                                            WriteMemory(TUS, RegDE.High);
                                            break;
                                        case 0x54: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x55: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x56: // IM $1
                                            interruptMode = 1;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x57: // LD A, I
                                            RegAF.High = RegI;
                                            RegFlagS = RegI > 127;
                                            RegFlagZ = RegI == 0;
                                            RegFlag5 = ((RegI & 0x20) != 0);
                                            RegFlagH = false;
                                            RegFlag3 = ((RegI & 0x08) != 0);
                                            RegFlagN = false;
                                            RegFlagP = IFF2;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x58: // IN E, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegDE.Low = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegDE.Low > 127;
                                            RegFlagZ = RegDE.Low == 0;
                                            RegFlag5 = (RegDE.Low & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegDE.Low & 0x08) != 0;
                                            RegFlagP = TableParity[RegDE.Low];
                                            RegFlagN = false;
                                            break;
                                        case 0x59: // OUT C, E
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegDE.Low);
                                            break;
                                        case 0x5A: // ADC HL, DE
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x5B: // LD DE, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegDE.Low = ReadMemory(TUS++); RegDE.High = ReadMemory(TUS);
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x5C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x5D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x5E: // IM $2
                                            interruptMode = 2;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x5F: // LD A, R
                                            RegAF.High = (byte)((RegR & 0x7F) | RegR2);
                                            RegFlagS = (RegR2 == 0x80);
                                            RegFlagZ = (byte)((RegR & 0x7F) | RegR2) == 0;
                                            RegFlagH = false;
                                            RegFlag5 = ((RegR & 0x20) != 0);
                                            RegFlagN = false;
                                            RegFlag3 = ((RegR & 0x08) != 0);
                                            RegFlagP = IFF2;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x60: // IN H, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegHL.High = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegHL.High > 127;
                                            RegFlagZ = RegHL.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegHL.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegHL.High & 0x08) != 0;
                                            RegFlag5 = (RegHL.High & 0x20) != 0;
                                            break;
                                        case 0x61: // OUT C, H
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegHL.High);
                                            break;
                                        case 0x62: // SBC HL, HL
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegHL.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegHL.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x63: // LD (nn), HL
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegHL.Low);
                                            WriteMemory(TUS, RegHL.High);
                                            break;
                                        case 0x64: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x65: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x66: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x67: // RRD
                                            totalExecutedCycles += 18; pendingCycles -= 18;
                                            TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                            WriteMemory(RegHL.Word, (byte)((TB2 >> 4) + (TB1 << 4)));
                                            RegAF.High = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            break;
                                        case 0x68: // IN L, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegHL.Low = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegHL.Low > 127;
                                            RegFlagZ = RegHL.Low == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegHL.Low];
                                            RegFlagN = false;
                                            RegFlag3 = (RegHL.Low & 0x08) != 0;
                                            RegFlag5 = (RegHL.Low & 0x20) != 0;
                                            break;
                                        case 0x69: // OUT C, L
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegHL.Low);
                                            break;
                                        case 0x6A: // ADC HL, HL
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x6B: // LD HL, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegHL.Low = ReadMemory(TUS++); RegHL.High = ReadMemory(TUS);
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0x6C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x6D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x6E: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x6F: // RLD
                                            totalExecutedCycles += 18; pendingCycles -= 18;
                                            TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                            WriteMemory(RegHL.Word, (byte)((TB1 & 0x0F) + (TB2 << 4)));
                                            RegAF.High = (byte)((TB1 & 0xF0) + (TB2 >> 4));
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            break;
                                        case 0x70: // IN 0, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            TB = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = TB > 127;
                                            RegFlagZ = TB == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[TB];
                                            RegFlagN = false;
                                            RegFlag3 = (TB & 0x08) != 0;
                                            RegFlag5 = (TB & 0x20) != 0;
                                            break;
                                        case 0x71: // OUT C, 0
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, 0);
                                            break;
                                        case 0x72: // SBC HL, SP
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegSP.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegSP.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x73: // LD (nn), SP
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegSP.Low);
                                            WriteMemory(TUS, RegSP.High);
                                            break;
                                        case 0x74: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x75: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x76: // IM $1
                                            interruptMode = 1;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x77: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x78: // IN A, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegAF.High = ReadHardware((ushort)RegBC.Word);
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            break;
                                        case 0x79: // OUT C, A
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegAF.High);
                                            break;
                                        case 0x7A: // ADC HL, SP
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x7B: // LD SP, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegSP.Low = ReadMemory(TUS++); RegSP.High = ReadMemory(TUS);
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x7C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x7D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x7E: // IM $2
                                            interruptMode = 2;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x7F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x80: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x81: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x82: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x83: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x84: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x85: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x86: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x87: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x88: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x89: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x90: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x91: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x92: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x93: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x94: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x95: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x96: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x97: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x98: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x99: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA0: // LDI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            break;
                                        case 0xA1: // CPI
                                            TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0xA2: // INI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word++, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xA3: // OUTI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word++);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            WriteHardware(RegBC.Word, TB);
                                            RegFlagC = false;
                                            RegFlagN = false;
                                            RegFlag3 = IsX(RegBC.High);
                                            RegFlagH = false;
                                            RegFlag5 = IsY(RegBC.High);
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlagS = IsS(RegBC.High);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            if (IsS(TB))
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagC = true;
                                                RegFlagH = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xA4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA8: // LDD
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            break;
                                        case 0xA9: // CPD
                                            TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                            RegWZ.Word--;
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0xAA: // IND
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word--, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xAB: // OUTD
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word--);
                                            WriteHardware(RegBC.Word, TB);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            RegFlagZ = RegBC.High == 0;
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xAC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB0: // LDIR
                                            WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            if (RegBC.Word != 0)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB1: // CPIR
                                            TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            if (RegBC.Word != 0 && !RegFlagZ)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB2: // INIR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word++, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xB3: // OTIR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word++);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            WriteHardware(RegBC.Word, TB);
                                            RegFlagC = false;
                                            RegFlagN = false;
                                            RegFlag3 = IsX(RegBC.High);
                                            RegFlagH = false;
                                            RegFlag5 = IsY(RegBC.High);
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlagS = IsS(RegBC.High);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            if (IsS(TB))
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagC = true;
                                                RegFlagH = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xB4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB8: // LDDR
                                            WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            if (RegBC.Word != 0)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB9: // CPDR
                                            TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            if (RegBC.Word != 0 && !RegFlagZ)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xBA: // INDR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word--, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xBB: // OTDR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word--);
                                            WriteHardware(RegBC.Word, TB);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            RegFlagZ = RegBC.High == 0;
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xBC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xED: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                    }
                                    break;
                                case 0xEE: // XOR n
                                    RegAF.Word = TableALU[5, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xEF: // RST $28
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x28;
                                    break;
                                case 0xF0: // RET P
                                    if (!RegFlagS)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xF1: // POP AF
                                    RegAF.Low = ReadMemory(RegSP.Word++); RegAF.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xF2: // JP P, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagS)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xF3: // DI
                                    IFF1 = IFF2 = false;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF4: // CALL P, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagS)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xF5: // PUSH AF
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegAF.High); WriteMemory(--RegSP.Word, RegAF.Low);
                                    break;
                                case 0xF6: // OR n
                                    RegAF.Word = TableALU[6, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xF7: // RST $30
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x30;
                                    break;
                                case 0xF8: // RET M
                                    if (RegFlagS)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xF9: // LD SP, IX
                                    RegSP.Word = RegIX.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xFA: // JP M, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagS)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xFB: // EI
                                    IFF1 = IFF2 = true;
                                    Interruptable = false;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFC: // CALL M, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagS)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xFD: // <-
                                    // Invalid sequence.
                                    totalExecutedCycles += 1337; pendingCycles -= 1337;
                                    break;
                                case 0xFE: // CP n
                                    RegAF.Word = TableALU[7, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xFF: // RST $38
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x38;
                                    break;
                            }
                            break;
                        case 0xDE: // SBC A, n
                            RegAF.Word = TableALU[3, RegAF.High, ReadOpArg(RegPC.Word++), RegFlagC ? 1 : 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xDF: // RST $18
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x18;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xE0: // RET PO
                            if (!RegFlagP)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xE1: // POP HL
                            RegHL.Low = ReadMemory(RegSP.Word++); RegHL.High = ReadMemory(RegSP.Word++);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xE2: // JP PO, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagP)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xE3: // EX (SP), HL
                            TUS = RegSP.Word; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
                            WriteMemory(TUS++, RegHL.Low); WriteMemory(TUS, RegHL.High);
                            RegHL.Low = TBL; RegHL.High = TBH;
                            RegWZ.Word = RegHL.Word;
                            totalExecutedCycles += 19; pendingCycles -= 19;
                            break;
                        case 0xE4: // CALL C, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagC)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xE5: // PUSH HL
                            WriteMemory(--RegSP.Word, RegHL.High); WriteMemory(--RegSP.Word, RegHL.Low);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xE6: // AND n
                            RegAF.Word = TableALU[4, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xE7: // RST $20
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x20;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xE8: // RET PE
                            if (RegFlagP)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xE9: // JP HL
                            RegPC.Word = RegHL.Word;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xEA: // JP PE, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagP)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xEB: // EX DE, HL
                            TUS = RegDE.Word; RegDE.Word = RegHL.Word; RegHL.Word = TUS;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xEC: // CALL PE, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagP)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xED: // (Prefix)
                            ++RegR;
                            switch (ReadOp(RegPC.Word++))
                            {
                                case 0x00: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x01: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x02: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x03: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x04: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x05: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x06: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x07: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x08: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x09: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x10: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x11: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x12: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x13: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x14: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x15: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x16: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x17: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x18: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x19: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x20: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x21: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x22: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x23: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x24: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x25: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x26: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x27: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x28: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x29: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x2F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x30: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x31: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x32: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x33: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x34: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x35: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x36: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x37: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x38: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x39: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x40: // IN B, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegBC.High = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = RegBC.High > 127;
                                    RegFlagZ = RegBC.High == 0;
                                    RegFlag5 = (RegBC.High & 0x20) != 0;
                                    RegFlagH = false;
                                    RegFlag3 = (RegBC.High & 0x08) != 0;
                                    RegFlagP = TableParity[RegBC.High];
                                    RegFlagN = false;
                                    break;
                                case 0x41: // OUT C, B
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegBC.High);
                                    break;
                                case 0x42: // SBC HL, BC
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 - TI2;
                                    if (RegFlagC) { --TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((RegHL.Word ^ RegBC.Word ^ TUS) & 0x1000) != 0;
                                    RegFlagN = true;
                                    RegFlagC = (((int)RegHL.Word - (int)RegBC.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x43: // LD (nn), BC
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegBC.Low);
                                    WriteMemory(TUS, RegBC.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x44: // NEG
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    break;
                                case 0x45: // RETN
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x46: // IM $0
                                    interruptMode = 0;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x47: // LD I, A
                                    RegI = RegAF.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x48: // IN C, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegBC.Low = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = RegBC.Low > 127;
                                    RegFlagZ = RegBC.Low == 0;
                                    RegFlag5 = (RegBC.Low & 0x20) != 0;
                                    RegFlagH = false;
                                    RegFlag3 = (RegBC.Low & 0x08) != 0;
                                    RegFlagP = TableParity[RegBC.Low];
                                    RegFlagN = false;
                                    break;
                                case 0x49: // OUT C, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegBC.Low);
                                    break;
                                case 0x4A: // ADC HL, BC
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                                    if (RegFlagC) { ++TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x4B: // LD BC, (nn)
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegBC.Low = ReadMemory(TUS++); RegBC.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x4C: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4D: // RETI
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x4E: // IM $0
                                    interruptMode = 0;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x4F: // LD R, A
                                    RegR = RegAF.High;
                                    RegR2 = (byte)(RegAF.High & 0x80);
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x50: // IN D, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegDE.High = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = RegDE.High > 127;
                                    RegFlagZ = RegDE.High == 0;
                                    RegFlag5 = (RegDE.High & 0x20) != 0;
                                    RegFlagH = false;
                                    RegFlag3 = (RegDE.High & 0x08) != 0;
                                    RegFlagP = TableParity[RegDE.High];
                                    RegFlagN = false;
                                    break;
                                case 0x51: // OUT C, D
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegDE.High);
                                    break;
                                case 0x52: // SBC HL, DE
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 - TI2;
                                    if (RegFlagC) { --TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((RegHL.Word ^ RegDE.Word ^ TUS) & 0x1000) != 0;
                                    RegFlagN = true;
                                    RegFlagC = (((int)RegHL.Word - (int)RegDE.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x53: // LD (nn), DE
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegDE.Low);
                                    WriteMemory(TUS, RegDE.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x54: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x55: // RETN
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    break;
                                case 0x56: // IM $1
                                    interruptMode = 1;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x57: // LD A, I
                                    RegAF.High = RegI;
                                    RegFlagS = RegI > 127;
                                    RegFlagZ = RegI == 0;
                                    RegFlag5 = ((RegI & 0x20) != 0);
                                    RegFlagH = false;
                                    RegFlag3 = ((RegI & 0x08) != 0);
                                    RegFlagN = false;
                                    RegFlagP = IFF2;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x58: // IN E, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegDE.Low = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = RegDE.Low > 127;
                                    RegFlagZ = RegDE.Low == 0;
                                    RegFlag5 = (RegDE.Low & 0x20) != 0;
                                    RegFlagH = false;
                                    RegFlag3 = (RegDE.Low & 0x08) != 0;
                                    RegFlagP = TableParity[RegDE.Low];
                                    RegFlagN = false;
                                    break;
                                case 0x59: // OUT C, E
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegDE.Low);
                                    break;
                                case 0x5A: // ADC HL, DE
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                                    if (RegFlagC) { ++TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x5B: // LD DE, (nn)
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegDE.Low = ReadMemory(TUS++); RegDE.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x5C: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5D: // RETI
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x5E: // IM $2
                                    interruptMode = 2;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x5F: // LD A, R
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    RegAF.High = (byte)((RegR & 0x7F) | RegR2);
                                    RegFlagS = (RegR2 == 0x80);
                                    RegFlagZ = (byte)((RegR & 0x7F) | RegR2) == 0;
                                    RegFlagH = false;
                                    RegFlag5 = ((RegR & 0x20) != 0);
                                    RegFlagN = false;
                                    RegFlag3 = ((RegR & 0x08) != 0);
                                    RegFlagP = IFF2;
                                    break;
                                case 0x60: // IN H, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegHL.High = ReadHardware(RegBC.Word);
                                    RegFlagS = RegHL.High > 127;
                                    RegFlagZ = RegHL.High == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[RegHL.High];
                                    RegFlagN = false;
                                    RegFlag3 = (RegHL.High & 0x08) != 0;
                                    RegFlag5 = (RegHL.High & 0x20) != 0;
                                    break;
                                case 0x61: // OUT C, H
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegHL.High);
                                    break;
                                case 0x62: // SBC HL, HL
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 - TI2;
                                    if (RegFlagC) { --TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((RegHL.Word ^ RegHL.Word ^ TUS) & 0x1000) != 0;
                                    RegFlagN = true;
                                    RegFlagC = (((int)RegHL.Word - (int)RegHL.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x63: // LD (nn), HL
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegHL.Low);
                                    WriteMemory(TUS, RegHL.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x64: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x65: // RETN
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x66: // IM $0
                                    interruptMode = 0;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x67: // RRD
                                    totalExecutedCycles += 18; pendingCycles -= 18;
                                    TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    WriteMemory(RegHL.Word, (byte)((TB2 >> 4) + (TB1 << 4)));
                                    RegAF.High = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
                                    RegFlagS = RegAF.High > 127;
                                    RegFlagZ = RegAF.High == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[RegAF.High];
                                    RegFlagN = false;
                                    RegFlag3 = (RegAF.High & 0x08) != 0;
                                    RegFlag5 = (RegAF.High & 0x20) != 0;
                                    break;
                                case 0x68: // IN L, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegHL.Low = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = RegHL.Low > 127;
                                    RegFlagZ = RegHL.Low == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[RegHL.Low];
                                    RegFlagN = false;
                                    RegFlag3 = (RegHL.Low & 0x08) != 0;
                                    RegFlag5 = (RegHL.Low & 0x20) != 0;
                                    break;
                                case 0x69: // OUT C, L
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegHL.Low);
                                    break;
                                case 0x6A: // ADC HL, HL
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 + TI2;
                                    if (RegFlagC) { ++TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x6B: // LD HL, (nn)
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegHL.Low = ReadMemory(TUS++); RegHL.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x6C: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6D: // RETI
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x6E: // IM $0
                                    interruptMode = 0;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x6F: // RLD
                                    totalExecutedCycles += 18; pendingCycles -= 18;
                                    TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    WriteMemory(RegHL.Word, (byte)((TB1 & 0x0F) + (TB2 << 4)));
                                    RegAF.High = (byte)((TB1 & 0xF0) + (TB2 >> 4));
                                    RegFlagS = RegAF.High > 127;
                                    RegFlagZ = RegAF.High == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[RegAF.High];
                                    RegFlagN = false;
                                    RegFlag3 = (RegAF.High & 0x08) != 0;
                                    RegFlag5 = (RegAF.High & 0x20) != 0;
                                    break;
                                case 0x70: // IN 0, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    TB = ReadHardware((ushort)RegBC.Word);
                                    RegFlagS = TB > 127;
                                    RegFlagZ = TB == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[TB];
                                    RegFlagN = false;
                                    RegFlag3 = (TB & 0x08) != 0;
                                    RegFlag5 = (TB & 0x20) != 0;
                                    break;
                                case 0x71: // OUT C, 0
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, 0);
                                    break;
                                case 0x72: // SBC HL, SP
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 - TI2;
                                    if (RegFlagC) { --TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((RegHL.Word ^ RegSP.Word ^ TUS) & 0x1000) != 0;
                                    RegFlagN = true;
                                    RegFlagC = (((int)RegHL.Word - (int)RegSP.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x73: // LD (nn), SP
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegSP.Low);
                                    WriteMemory(TUS, RegSP.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x74: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x75: // RETN
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x76: // IM $1
                                    interruptMode = 1;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x77: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x78: // IN A, C
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    RegAF.High = ReadHardware(RegBC.Word);
                                    RegFlagS = RegAF.High > 127;
                                    RegFlagZ = RegAF.High == 0;
                                    RegFlagH = false;
                                    RegFlagP = TableParity[RegAF.High];
                                    RegFlagN = false;
                                    RegFlag3 = (RegAF.High & 0x08) != 0;
                                    RegFlag5 = (RegAF.High & 0x20) != 0;
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    break;
                                case 0x79: // OUT C, A
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    WriteHardware(RegBC.Word, RegAF.High);
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    break;
                                case 0x7A: // ADC HL, SP
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                                    if (RegFlagC) { ++TIR; ++TI2; }
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegHL.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegFlagP = TIR > 32767 || TIR < -32768;
                                    RegFlagS = TUS > 32767;
                                    RegFlagZ = TUS == 0;
                                    RegHL.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    break;
                                case 0x7B: // LD SP, (nn)
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegSP.Low = ReadMemory(TUS++); RegSP.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x7C: // NEG
                                    RegAF.Word = TableNeg[RegAF.Word];
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7D: // RETI
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    RegWZ.Word = RegPC.Word;
                                    IFF1 = IFF2;
                                    break;
                                case 0x7E: // IM $2
                                    interruptMode = 2;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0x7F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x80: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x81: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x82: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x83: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x84: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x85: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x86: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x87: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x88: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x89: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x90: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x91: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x92: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x93: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x94: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x95: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x96: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x97: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x98: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x99: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9A: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9B: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9C: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9D: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9E: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9F: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA0: // LDI
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                    TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    RegFlagH = false;
                                    RegFlagN = false;
                                    break;
                                case 0xA1: // CPI
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                    RegWZ.Word++;
                                    RegFlagN = true;
                                    RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                    RegFlagZ = TB2 == 0;
                                    RegFlagS = TB2 > 127;
                                    TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    break;
                                case 0xA2: // INI
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadHardware(RegBC.Word);
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    --RegBC.High;
                                    WriteMemory(RegHL.Word++, TB);
                                    RegFlagZ = RegBC.High == 0;
                                    TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    break;
                                case 0xA3: // OUTI
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadMemory(RegHL.Word++);
                                    --RegBC.High;
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    WriteHardware(RegBC.Word, TB);
                                    RegFlagC = false;
                                    RegFlagN = false;
                                    RegFlag3 = IsX(RegBC.High);
                                    RegFlagH = false;
                                    RegFlag5 = IsY(RegBC.High);
                                    RegFlagZ = RegBC.High == 0;
                                    RegFlagS = IsS(RegBC.High);
                                    TUS = (ushort)(RegHL.Low + TB);
                                    if (IsS(TB))
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagC = true;
                                        RegFlagH = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    break;
                                case 0xA4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA8: // LDD
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                    TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    RegFlagH = false;
                                    RegFlagN = false;
                                    break;
                                case 0xA9: // CPD
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                    RegWZ.Word--;
                                    RegFlagN = true;
                                    RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                    RegFlagZ = TB2 == 0;
                                    RegFlagS = TB2 > 127;
                                    TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    break;
                                case 0xAA: // IND
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadHardware(RegBC.Word);
                                    RegWZ.Word = (ushort)(RegBC.Word - 1);
                                    --RegBC.High;
                                    WriteMemory(RegHL.Word--, TB);
                                    RegFlagZ = RegBC.High == 0;
                                    TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    break;
                                case 0xAB: // OUTD
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadMemory(RegHL.Word--);
                                    WriteHardware(RegBC.Word, TB);
                                    --RegBC.High;
                                    RegWZ.Word = (ushort)(RegBC.Word - 1);
                                    TUS = (ushort)(RegHL.Low + TB);
                                    RegFlagZ = RegBC.High == 0;
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    break;
                                case 0xAC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAD: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB0: // LDIR
                                    WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                    TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    RegFlagH = false;
                                    RegFlagN = false;
                                    if (RegBC.Word != 0)
                                    {
                                        RegPC.Word -= 2;
                                        RegWZ.Word = (ushort)(RegPC.Word + 1);
                                        totalExecutedCycles += 21; pendingCycles -= 21;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 16; pendingCycles -= 16;
                                    }
                                    break;
                                case 0xB1: // CPIR
                                    TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                    RegWZ.Word++;
                                    RegFlagN = true;
                                    RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                    RegFlagZ = TB2 == 0;
                                    RegFlagS = TB2 > 127;
                                    TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    if (RegBC.Word != 0 && !RegFlagZ)
                                    {
                                        RegPC.Word -= 2;
                                        RegWZ.Word = (ushort)(RegPC.Word + 1);
                                        totalExecutedCycles += 21; pendingCycles -= 21;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 16; pendingCycles -= 16;
                                    }
                                    break;
                                case 0xB2: // INIR
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadHardware(RegBC.Word);
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    --RegBC.High;
                                    WriteMemory(RegHL.Word++, TB);
                                    RegFlagZ = RegBC.High == 0;
                                    TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    if (RegBC.High != 0)
                                    {
                                        RegPC.Word -= 2;
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xB3: // OTIR
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadMemory(RegHL.Word++);
                                    --RegBC.High;
                                    RegWZ.Word = (ushort)(RegBC.Word + 1);
                                    WriteHardware(RegBC.Word, TB);
                                    RegFlagC = false;
                                    RegFlagN = false;
                                    RegFlag3 = IsX(RegBC.High);
                                    RegFlagH = false;
                                    RegFlag5 = IsY(RegBC.High);
                                    RegFlagZ = RegBC.High == 0;
                                    RegFlagS = IsS(RegBC.High);
                                    TUS = (ushort)(RegHL.Low + TB);
                                    if (IsS(TB))
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagC = true;
                                        RegFlagH = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    if (RegBC.High != 0)
                                    {
                                        RegPC.Word -= 2;
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xB4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB8: // LDDR
                                    WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                    TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    RegFlagH = false;
                                    RegFlagN = false;
                                    if (RegBC.Word != 0)
                                    {
                                        RegPC.Word -= 2;
                                        RegWZ.Word = (ushort)(RegPC.Word + 1);
                                        totalExecutedCycles += 21; pendingCycles -= 21;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 16; pendingCycles -= 16;
                                    }
                                    break;
                                case 0xB9: // CPDR
                                    TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                    RegWZ.Word--;
                                    RegFlagN = true;
                                    RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                    RegFlagZ = TB2 == 0;
                                    RegFlagS = TB2 > 127;
                                    TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                    --RegBC.Word;
                                    RegFlagP = RegBC.Word != 0;
                                    if (RegBC.Word != 0 && !RegFlagZ)
                                    {
                                        RegPC.Word -= 2;
                                        RegWZ.Word = (ushort)(RegPC.Word + 1);
                                        totalExecutedCycles += 21; pendingCycles -= 21;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 16; pendingCycles -= 16;
                                    }
                                    break;
                                case 0xBA: // INDR
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadHardware(RegBC.Word);
                                    RegWZ.Word = (ushort)(RegBC.Word - 1);
                                    --RegBC.High;
                                    WriteMemory(RegHL.Word--, TB);
                                    RegFlagZ = RegBC.High == 0;
                                    TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    if (RegBC.High != 0)
                                    {
                                        RegPC.Word -= 2;
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xBB: // OTDR
                                    totalExecutedCycles += 16; pendingCycles -= 16;
                                    TB = ReadMemory(RegHL.Word--);
                                    WriteHardware(RegBC.Word, TB);
                                    --RegBC.High;
                                    RegWZ.Word = (ushort)(RegBC.Word - 1);
                                    TUS = (ushort)(RegHL.Low + TB);
                                    RegFlagZ = RegBC.High == 0;
                                    if ((TB & 0x80) != 0)
                                    {
                                        RegFlagN = true;
                                    }
                                    if ((TUS & 0x100) != 0)
                                    {
                                        RegFlagH = RegFlagC = true;
                                    }
                                    RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                    if (RegBC.High != 0)
                                    {
                                        RegPC.Word -= 2;
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xBC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBD: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC0: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC1: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC2: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC3: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC8: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC9: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCA: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCB: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCD: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xCF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD0: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD1: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD2: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD3: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD8: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xD9: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDA: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDB: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDD: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE0: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE1: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE2: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE3: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE8: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xE9: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEA: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEB: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xED: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF0: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF1: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF2: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF3: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF4: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF5: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF6: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF7: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF8: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF9: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFA: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFB: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFC: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFD: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFE: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFF: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                            }
                            break;
                        case 0xEE: // XOR n
                            RegAF.Word = TableALU[5, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xEF: // RST $28
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x28;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xF0: // RET P
                            if (!RegFlagS)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xF1: // POP AF
                            RegAF.Low = ReadMemory(RegSP.Word++); RegAF.High = ReadMemory(RegSP.Word++);
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xF2: // JP P, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagS)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xF3: // DI
                            IFF1 = IFF2 = false;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xF4: // CALL P, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (!RegFlagS)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xF5: // PUSH AF
                            WriteMemory(--RegSP.Word, RegAF.High); WriteMemory(--RegSP.Word, RegAF.Low);
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xF6: // OR n
                            RegAF.Word = TableALU[6, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xF7: // RST $30
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x30;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                        case 0xF8: // RET M
                            if (RegFlagS)
                            {
                                RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                RegWZ.Word = RegPC.Word;
                                totalExecutedCycles += 11; pendingCycles -= 11;
                            }
                            else
                            {
                                totalExecutedCycles += 5; pendingCycles -= 5;
                            }
                            break;
                        case 0xF9: // LD SP, HL
                            RegSP.Word = RegHL.Word;
                            totalExecutedCycles += 6; pendingCycles -= 6;
                            break;
                        case 0xFA: // JP M, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagS)
                            {
                                RegPC.Word = RegWZ.Word;
                            }
                            totalExecutedCycles += 10; pendingCycles -= 10;
                            break;
                        case 0xFB: // EI
                            IFF1 = IFF2 = true;
                            Interruptable = false;
                            totalExecutedCycles += 4; pendingCycles -= 4;
                            break;
                        case 0xFC: // CALL M, nn
                            RegWZ.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                            if (RegFlagS)
                            {
                                WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                RegPC.Word = RegWZ.Word;
                                totalExecutedCycles += 17; pendingCycles -= 17;
                            }
                            else
                            {
                                totalExecutedCycles += 10; pendingCycles -= 10;
                            }
                            break;
                        case 0xFD: // (Prefix)
                            ++RegR;
                            switch (ReadOp(RegPC.Word++))
                            {
                                case 0x00: // NOP
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x01: // LD BC, nn
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    RegBC.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    break;
                                case 0x02: // LD (BC), A
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    WriteMemory(RegBC.Word, RegAF.High);
                                    break;
                                case 0x03: // INC BC
                                    ++RegBC.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x04: // INC B
                                    RegAF.Low = (byte)(TableInc[++RegBC.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x05: // DEC B
                                    RegAF.Low = (byte)(TableDec[--RegBC.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x06: // LD B, n
                                    RegBC.High = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x07: // RLCA
                                    RegAF.Word = TableRotShift[0, 0, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x08: // EX AF, AF'
                                    TUS = RegAF.Word; RegAF.Word = RegAltAF.Word; RegAltAF.Word = TUS;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x09: // ADD IY, BC
                                    TI1 = (short)RegIY.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIY.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIY.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x0A: // LD A, (BC)
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegAF.High = ReadMemory(RegBC.Word);
                                    break;
                                case 0x0B: // DEC BC
                                    --RegBC.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x0C: // INC C
                                    RegAF.Low = (byte)(TableInc[++RegBC.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0D: // DEC C
                                    RegAF.Low = (byte)(TableDec[--RegBC.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x0E: // LD C, n
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegBC.Low = ReadMemory(RegPC.Word++);
                                    break;
                                case 0x0F: // RRCA
                                    RegAF.Word = TableRotShift[0, 1, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x10: // DJNZ d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (--RegBC.High != 0)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 13; pendingCycles -= 13;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 8; pendingCycles -= 8;
                                    }
                                    break;
                                case 0x11: // LD DE, nn
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    RegDE.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    break;
                                case 0x12: // LD (DE), A
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    WriteMemory(RegDE.Word, RegAF.High);
                                    break;
                                case 0x13: // INC DE
                                    ++RegDE.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x14: // INC D
                                    RegAF.Low = (byte)(TableInc[++RegDE.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x15: // DEC D
                                    RegAF.Low = (byte)(TableDec[--RegDE.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x16: // LD D, n
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    RegDE.High = ReadMemory(RegPC.Word++);
                                    break;
                                case 0x17: // RLA
                                    RegAF.Word = TableRotShift[0, 2, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x18: // JR d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    RegPC.Word = (ushort)(RegPC.Word + TSB);
                                    totalExecutedCycles += 12; pendingCycles -= 12;
                                    break;
                                case 0x19: // ADD IY, DE
                                    TI1 = (short)RegIY.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIY.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIY.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x1A: // LD A, (DE)
                                    RegAF.High = ReadMemory(RegDE.Word);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x1B: // DEC DE
                                    --RegDE.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x1C: // INC E
                                    RegAF.Low = (byte)(TableInc[++RegDE.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1D: // DEC E
                                    RegAF.Low = (byte)(TableDec[--RegDE.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x1E: // LD E, n
                                    RegDE.Low = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x1F: // RRA
                                    RegAF.Word = TableRotShift[0, 3, RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x20: // JR NZ, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x21: // LD IY, nn
                                    RegIY.Word = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    break;
                                case 0x22: // LD (nn), IY
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    WriteMemory(TUS++, RegIY.Low);
                                    WriteMemory(TUS, RegIY.High);
                                    RegWZ.Word = TUS;
                                    break;
                                case 0x23: // INC IY
                                    ++RegIY.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x24: // INC IYH
                                    RegAF.Low = (byte)(TableInc[++RegIY.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x25: // DEC IYH
                                    RegAF.Low = (byte)(TableDec[--RegIY.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x26: // LD IYH, n
                                    RegIY.High = ReadOpArg(RegPC.Word++);
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x27: // DAA
                                    RegAF.Word = TableDaa[RegAF.Word];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x28: // JR Z, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (RegFlagZ)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x29: // ADD IY, IY
                                    TI1 = (short)RegIY.Word; TI2 = (short)RegIY.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIY.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIY.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x2A: // LD IY, (nn)
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) + ReadOpArg(RegPC.Word++) * 256);
                                    RegIY.Low = ReadMemory(TUS++); RegIY.High = ReadMemory(TUS);
                                    RegWZ.Word = TUS;
                                    totalExecutedCycles += 20; pendingCycles -= 20;
                                    break;
                                case 0x2B: // DEC IY
                                    --RegIY.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x2C: // INC IYL
                                    RegAF.Low = (byte)(TableInc[++RegIY.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2D: // DEC IYL
                                    RegAF.Low = (byte)(TableDec[--RegIY.Low] | (RegAF.Low & 1));
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2E: // LD IYL, n
                                    RegIY.Low = ReadOpArg(RegPC.Word++);
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x2F: // CPL
                                    RegAF.High ^= 0xFF; RegFlagH = true; RegFlagN = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x30: // JR NC, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (!RegFlagC)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x31: // LD SP, nn
                                    RegSP.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0x32: // LD (nn), A
                                    totalExecutedCycles += 13; pendingCycles -= 13;
                                    WriteMemory((ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256), RegAF.High);
                                    break;
                                case 0x33: // INC SP
                                    ++RegSP.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x34: // INC (IY+d)
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    TB = ReadMemory(RegWZ.Word); RegAF.Low = (byte)(TableInc[++TB] | (RegAF.Low & 1)); WriteMemory(RegWZ.Word, TB);
                                    break;
                                case 0x35: // DEC (IY+d)
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    TB = ReadMemory(RegWZ.Word); RegAF.Low = (byte)(TableDec[--TB] | (RegAF.Low & 1)); WriteMemory(RegWZ.Word, TB);
                                    break;
                                case 0x36: // LD (IY+d), n
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, ReadOpArg(RegPC.Word++));
                                    break;
                                case 0x37: // SCF
                                    RegFlagH = false; RegFlagN = false; RegFlagC = true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x38: // JR C, d
                                    TSB = (sbyte)ReadMemory(RegPC.Word++);
                                    if (RegFlagC)
                                    {
                                        RegPC.Word = (ushort)(RegPC.Word + TSB);
                                        totalExecutedCycles += 12; pendingCycles -= 12;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 7; pendingCycles -= 7;
                                    }
                                    break;
                                case 0x39: // ADD IY, SP
                                    TI1 = (short)RegIY.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                                    TUS = (ushort)TIR;
                                    RegWZ.Word = (ushort)(RegIY.Word + 1);
                                    RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                    RegFlagN = false;
                                    RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                    RegIY.Word = TUS;
                                    RegFlag3 = (TUS & 0x0800) != 0;
                                    RegFlag5 = (TUS & 0x2000) != 0;
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    break;
                                case 0x3A: // LD A, (nn)
                                    RegAF.High = ReadMemory((ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256));
                                    totalExecutedCycles += 13; pendingCycles -= 13;
                                    break;
                                case 0x3B: // DEC SP
                                    --RegSP.Word;
                                    totalExecutedCycles += 6; pendingCycles -= 6;
                                    break;
                                case 0x3C: // INC A
                                    RegAF.Low = (byte)(TableInc[++RegAF.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3D: // DEC A
                                    RegAF.Low = (byte)(TableDec[--RegAF.High] | (RegAF.Low & 1));
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x3E: // LD A, n
                                    RegAF.High = ReadMemory(RegPC.Word++);
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0x3F: // CCF
                                    RegFlagH = RegFlagC; RegFlagN = false; RegFlagC ^= true; RegFlag3 = (RegAF.High & 0x08) != 0; RegFlag5 = (RegAF.High & 0x20) != 0;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x40: // LD B, B
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x41: // LD B, C
                                    RegBC.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x42: // LD B, D
                                    RegBC.High = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x43: // LD B, E
                                    RegBC.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x44: // LD B, IYH
                                    RegBC.High = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x45: // LD B, IYL
                                    RegBC.High = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x46: // LD B, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegBC.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x47: // LD B, A
                                    RegBC.High = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x48: // LD C, B
                                    RegBC.Low = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x49: // LD C, C
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4A: // LD C, D
                                    RegBC.Low = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4B: // LD C, E
                                    RegBC.Low = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x4C: // LD C, IYH
                                    RegBC.Low = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x4D: // LD C, IYL
                                    RegBC.Low = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x4E: // LD C, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegBC.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x4F: // LD C, A
                                    RegBC.Low = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x50: // LD D, B
                                    RegDE.High = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x51: // LD D, C
                                    RegDE.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x52: // LD D, D
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x53: // LD D, E
                                    RegDE.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x54: // LD D, IYH
                                    RegDE.High = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x55: // LD D, IYL
                                    RegDE.High = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x56: // LD D, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegDE.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x57: // LD D, A
                                    RegDE.High = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x58: // LD E, B
                                    RegDE.Low = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x59: // LD E, C
                                    RegDE.Low = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5A: // LD E, D
                                    RegDE.Low = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5B: // LD E, E
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x5C: // LD E, IYH
                                    RegDE.Low = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x5D: // LD E, IYL
                                    RegDE.Low = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x5E: // LD E, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegDE.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x5F: // LD E, A
                                    RegDE.Low = RegAF.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x60: // LD IYH, B
                                    RegIY.High = RegBC.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x61: // LD IYH, C
                                    RegIY.High = RegBC.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x62: // LD IYH, D
                                    RegIY.High = RegDE.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x63: // LD IYH, E
                                    RegIY.High = RegDE.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x64: // LD IYH, IYH
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x65: // LD IYH, IYL
                                    RegIY.High = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x66: // LD H, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegHL.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x67: // LD IYH, A
                                    RegIY.High = RegAF.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x68: // LD IYL, B
                                    RegIY.Low = RegBC.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x69: // LD IYL, C
                                    RegIY.Low = RegBC.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6A: // LD IYL, D
                                    RegIY.Low = RegDE.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6B: // LD IYL, E
                                    RegIY.Low = RegDE.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6C: // LD IYL, IYH
                                    RegIY.Low = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6D: // LD IYL, IYL
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x6E: // LD L, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegHL.Low = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x6F: // LD IYL, A
                                    RegIY.Low = RegAF.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x70: // LD (IY+d), B
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegBC.High);
                                    break;
                                case 0x71: // LD (IY+d), C
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegBC.Low);
                                    break;
                                case 0x72: // LD (IY+d), D
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegDE.High);
                                    break;
                                case 0x73: // LD (IY+d), E
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegDE.Low);
                                    break;
                                case 0x74: // LD (IY+d), H
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegHL.High);
                                    break;
                                case 0x75: // LD (IY+d), L
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegHL.Low);
                                    break;
                                case 0x76: // HALT
                                    Halt();
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x77: // LD (IY+d), A
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    WriteMemory(RegWZ.Word, RegAF.High);
                                    break;
                                case 0x78: // LD A, B
                                    RegAF.High = RegBC.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x79: // LD A, C
                                    RegAF.High = RegBC.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7A: // LD A, D
                                    RegAF.High = RegDE.High;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7B: // LD A, E
                                    RegAF.High = RegDE.Low;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x7C: // LD A, IYH
                                    RegAF.High = RegIY.High;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x7D: // LD A, IYL
                                    RegAF.High = RegIY.Low;
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x7E: // LD A, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.High = ReadMemory(RegWZ.Word);
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x7F: // LD A, A
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x80: // ADD A, B
                                    RegAF.Word = TableALU[0, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x81: // ADD A, C
                                    RegAF.Word = TableALU[0, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x82: // ADD A, D
                                    RegAF.Word = TableALU[0, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x83: // ADD A, E
                                    RegAF.Word = TableALU[0, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x84: // ADD A, IYH
                                    RegAF.Word = TableALU[0, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x85: // ADD A, IYL
                                    RegAF.Word = TableALU[0, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x86: // ADD A, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[0, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;//16
                                    break;
                                case 0x87: // ADD A, A
                                    RegAF.Word = TableALU[0, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x88: // ADC A, B
                                    RegAF.Word = TableALU[1, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x89: // ADC A, C
                                    RegAF.Word = TableALU[1, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8A: // ADC A, D
                                    RegAF.Word = TableALU[1, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8B: // ADC A, E
                                    RegAF.Word = TableALU[1, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x8C: // ADC A, IYH
                                    RegAF.Word = TableALU[1, RegAF.High, RegIY.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x8D: // ADC A, IYL
                                    RegAF.Word = TableALU[1, RegAF.High, RegIY.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x8E: // ADC A, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[1, RegAF.High, ReadMemory(RegWZ.Word), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x8F: // ADC A, A
                                    RegAF.Word = TableALU[1, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x90: // SUB B
                                    RegAF.Word = TableALU[2, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x91: // SUB C
                                    RegAF.Word = TableALU[2, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x92: // SUB D
                                    RegAF.Word = TableALU[2, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x93: // SUB E
                                    RegAF.Word = TableALU[2, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x94: // SUB IYH
                                    RegAF.Word = TableALU[2, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x95: // SUB IYL
                                    RegAF.Word = TableALU[2, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x96: // SUB (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[2, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x97: // SUB A, A
                                    RegAF.Word = TableALU[2, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x98: // SBC A, B
                                    RegAF.Word = TableALU[3, RegAF.High, RegBC.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x99: // SBC A, C
                                    RegAF.Word = TableALU[3, RegAF.High, RegBC.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9A: // SBC A, D
                                    RegAF.Word = TableALU[3, RegAF.High, RegDE.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9B: // SBC A, E
                                    RegAF.Word = TableALU[3, RegAF.High, RegDE.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0x9C: // SBC A, IYH
                                    RegAF.Word = TableALU[3, RegAF.High, RegIY.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x9D: // SBC A, IYL
                                    RegAF.Word = TableALU[3, RegAF.High, RegIY.Low, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0x9E: // SBC A, (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[3, RegAF.High, ReadMemory(RegWZ.Word), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0x9F: // SBC A, A
                                    RegAF.Word = TableALU[3, RegAF.High, RegAF.High, RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA0: // AND B
                                    RegAF.Word = TableALU[4, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA1: // AND C
                                    RegAF.Word = TableALU[4, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA2: // AND D
                                    RegAF.Word = TableALU[4, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA3: // AND E
                                    RegAF.Word = TableALU[4, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA4: // AND IYH
                                    RegAF.Word = TableALU[4, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xA5: // AND IYL
                                    RegAF.Word = TableALU[4, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xA6: // AND (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[4, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xA7: // AND A
                                    RegAF.Word = TableALU[4, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA8: // XOR B
                                    RegAF.Word = TableALU[5, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xA9: // XOR C
                                    RegAF.Word = TableALU[5, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAA: // XOR D
                                    RegAF.Word = TableALU[5, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAB: // XOR E
                                    RegAF.Word = TableALU[5, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xAC: // XOR IYH
                                    RegAF.Word = TableALU[5, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xAD: // XOR IYL
                                    RegAF.Word = TableALU[5, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xAE: // XOR (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[5, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xAF: // XOR A
                                    RegAF.Word = TableALU[5, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB0: // OR B
                                    RegAF.Word = TableALU[6, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB1: // OR C
                                    RegAF.Word = TableALU[6, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB2: // OR D
                                    RegAF.Word = TableALU[6, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB3: // OR E
                                    RegAF.Word = TableALU[6, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB4: // OR IYH
                                    RegAF.Word = TableALU[6, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xB5: // OR IYL
                                    RegAF.Word = TableALU[6, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xB6: // OR (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[6, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xB7: // OR A
                                    RegAF.Word = TableALU[6, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB8: // CP B
                                    RegAF.Word = TableALU[7, RegAF.High, RegBC.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xB9: // CP C
                                    RegAF.Word = TableALU[7, RegAF.High, RegBC.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBA: // CP D
                                    RegAF.Word = TableALU[7, RegAF.High, RegDE.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBB: // CP E
                                    RegAF.Word = TableALU[7, RegAF.High, RegDE.Low, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xBC: // CP IYH
                                    RegAF.Word = TableALU[7, RegAF.High, RegIY.High, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xBD: // CP IYL
                                    RegAF.Word = TableALU[7, RegAF.High, RegIY.Low, 0];
                                    totalExecutedCycles += 9; pendingCycles -= 9;
                                    break;
                                case 0xBE: // CP (IY+d)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    RegAF.Word = TableALU[7, RegAF.High, ReadMemory(RegWZ.Word), 0];
                                    totalExecutedCycles += 19; pendingCycles -= 19;
                                    break;
                                case 0xBF: // CP A
                                    RegAF.Word = TableALU[7, RegAF.High, RegAF.High, 0];
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xC0: // RET NZ
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xC1: // POP BC
                                    RegBC.Low = ReadMemory(RegSP.Word++); RegBC.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC2: // JP NZ, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagZ)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC3: // JP nn
                                    RegPC.Word = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xC4: // CALL NZ, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagZ)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xC5: // PUSH BC
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegBC.High); WriteMemory(--RegSP.Word, RegBC.Low);
                                    break;
                                case 0xC6: // ADD A, n
                                    RegAF.Word = TableALU[0, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xC7: // RST $00
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x00;
                                    break;
                                case 0xC8: // RET Z
                                    if (RegFlagZ)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xC9: // RET
                                    RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xCA: // JP Z, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagZ)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xCB: // (Prefix)
                                    Displacement = (sbyte)ReadOpArg(RegPC.Word++);
                                    //++RegR;
                                    RegWZ.Word = (ushort)(RegIY.Word + Displacement);
                                    switch (ReadOpArg(RegPC.Word++))
                                    {
                                        case 0x00: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x01: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x02: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x03: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x04: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x05: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x06: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x07: // RLC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 0, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x08: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x09: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0A: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0B: // RRC (IY+d)
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            break;
                                        case 0x0C: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0D: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0E: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x0F: // RRC (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 1, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x10: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x11: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x12: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x13: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x14: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x15: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x16: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x17: // RL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 2, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x18: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x19: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1A: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1B: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1C: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1D: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1E: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x1F: // RR (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 3, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x20: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x21: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x22: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x23: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x24: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x25: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x26: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x27: // SLA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 4, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x28: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x29: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2A: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2B: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2C: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2D: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2E: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x2F: // SRA (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 5, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x30: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x31: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x32: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x33: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x34: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x35: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x36: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x37: // SL1 (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 6, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x38: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x39: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3A: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3B: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3C: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3D: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3E: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x3F: // SRL (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            TUS = TableRotShift[1, 7, RegAF.Low + 256 * ReadMemory(RegWZ.Word)];
                                            WriteMemory(RegWZ.Word, (byte)(TUS >> 8));
                                            RegAF.Low = (byte)TUS;
                                            break;
                                        case 0x40: // BIT 0, (IY+d)
                                        case 0x41: // BIT 0, (IY+d)
                                        case 0x42: // BIT 0, (IY+d)
                                        case 0x43: // BIT 0, (IY+d)
                                        case 0x44: // BIT 0, (IY+d)
                                        case 0x45: // BIT 0, (IY+d)
                                        case 0x46: // BIT 0, (IY+d)
                                        case 0x47: // BIT 0, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x01) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x48: // BIT 1, (IY+d)
                                        case 0x49: // BIT 1, (IY+d)
                                        case 0x4A: // BIT 1, (IY+d)
                                        case 0x4B: // BIT 1, (IY+d)
                                        case 0x4C: // BIT 1, (IY+d)
                                        case 0x4D: // BIT 1, (IY+d)
                                        case 0x4E: // BIT 1, (IY+d)
                                        case 0x4F: // BIT 1, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x02) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x50: // BIT 2, (IY+d)
                                        case 0x51: // BIT 2, (IY+d)
                                        case 0x52: // BIT 2, (IY+d)
                                        case 0x53: // BIT 2, (IY+d)
                                        case 0x54: // BIT 2, (IY+d)
                                        case 0x55: // BIT 2, (IY+d)
                                        case 0x56: // BIT 2, (IY+d)
                                        case 0x57: // BIT 2, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x04) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x58: // BIT 3, (IY+d)
                                        case 0x59: // BIT 3, (IY+d)
                                        case 0x5A: // BIT 3, (IY+d)
                                        case 0x5B: // BIT 3, (IY+d)
                                        case 0x5C: // BIT 3, (IY+d)
                                        case 0x5D: // BIT 3, (IY+d)
                                        case 0x5E: // BIT 3, (IY+d)
                                        case 0x5F: // BIT 3, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x08) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x60: // BIT 4, (IY+d)
                                        case 0x61: // BIT 4, (IY+d)
                                        case 0x62: // BIT 4, (IY+d)
                                        case 0x63: // BIT 4, (IY+d)
                                        case 0x64: // BIT 4, (IY+d)
                                        case 0x65: // BIT 4, (IY+d)
                                        case 0x66: // BIT 4, (IY+d)
                                        case 0x67: // BIT 4, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x10) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x68: // BIT 5, (IY+d)
                                        case 0x69: // BIT 5, (IY+d)
                                        case 0x6A: // BIT 5, (IY+d)
                                        case 0x6B: // BIT 5, (IY+d)
                                        case 0x6C: // BIT 5, (IY+d)
                                        case 0x6D: // BIT 5, (IY+d)
                                        case 0x6E: // BIT 5, (IY+d)
                                        case 0x6F: // BIT 5, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x20) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x70: // BIT 6, (IY+d)
                                        case 0x71: // BIT 6, (IY+d)
                                        case 0x72: // BIT 6, (IY+d)
                                        case 0x73: // BIT 6, (IY+d)
                                        case 0x74: // BIT 6, (IY+d)
                                        case 0x75: // BIT 6, (IY+d)
                                        case 0x76: // BIT 6, (IY+d)
                                        case 0x77: // BIT 6, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x40) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = false;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x78: // BIT 7, (IY+d)
                                        case 0x79: // BIT 7, (IY+d)
                                        case 0x7A: // BIT 7, (IY+d)
                                        case 0x7B: // BIT 7, (IY+d)
                                        case 0x7C: // BIT 7, (IY+d)
                                        case 0x7D: // BIT 7, (IY+d)
                                        case 0x7E: // BIT 7, (IY+d)
                                        case 0x7F: // BIT 7, (IY+d)
                                            TBOOL = (ReadMemory(RegWZ.Word) & 0x80) == 0;
                                            RegFlagN = false;
                                            RegFlagP = TBOOL;
                                            RegFlag3 = ((RegWZ.Word >> 8) & 0x08) != 0;
                                            RegFlagH = true;
                                            RegFlag5 = ((RegWZ.Word >> 8) & 0x20) != 0;
                                            RegFlagZ = TBOOL;
                                            RegFlagS = !TBOOL;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x80: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x81: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x82: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x83: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x84: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x85: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x86: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x87: // RES 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x01)));
                                            break;
                                        case 0x88: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x89: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8A: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8B: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8C: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8D: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8E: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x8F: // RES 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x02)));
                                            break;
                                        case 0x90: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x91: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x92: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x93: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x94: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x95: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x96: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x97: // RES 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x04)));
                                            break;
                                        case 0x98: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x99: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9A: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9B: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9C: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9D: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9E: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0x9F: // RES 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x08)));
                                            break;
                                        case 0xA0: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA1: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA2: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA3: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA4: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA5: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA6: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA7: // RES 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x10)));
                                            break;
                                        case 0xA8: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xA9: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAA: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAB: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAC: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAD: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAE: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xAF: // RES 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x20)));
                                            break;
                                        case 0xB0: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB1: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB2: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB3: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB4: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB5: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB6: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB7: // RES 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x40)));
                                            break;
                                        case 0xB8: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xB9: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBA: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBB: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBC: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBD: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBE: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xBF: // RES 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) & unchecked((byte)~0x80)));
                                            break;
                                        case 0xC0: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC1: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC2: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC3: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC4: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC5: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC6: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC7: // SET 0, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x01)));
                                            break;
                                        case 0xC8: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xC9: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCA: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCB: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCC: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCD: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCE: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xCF: // SET 1, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x02)));
                                            break;
                                        case 0xD0: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD1: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD2: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD3: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD4: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD5: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD6: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD7: // SET 2, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x04)));
                                            break;
                                        case 0xD8: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xD9: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDA: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDB: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDC: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDD: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDE: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xDF: // SET 3, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x08)));
                                            break;
                                        case 0xE0: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE1: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE2: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE3: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE4: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE5: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE6: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE7: // SET 4, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x10)));
                                            break;
                                        case 0xE8: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xE9: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEA: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEB: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEC: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xED: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEE: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xEF: // SET 5, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x20)));
                                            break;
                                        case 0xF0: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF1: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF2: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF3: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF4: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF5: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF6: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF7: // SET 6, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x40)));
                                            break;
                                        case 0xF8: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xF9: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFA: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFB: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFC: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFD: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFE: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                        case 0xFF: // SET 7, (IY+d)
                                            totalExecutedCycles += 23; pendingCycles -= 23;
                                            WriteMemory(RegWZ.Word, (byte)(ReadMemory(RegWZ.Word) | unchecked((byte)0x80)));
                                            break;
                                    }
                                    break;
                                case 0xCC: // CALL Z, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagZ)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xCD: // CALL nn
                                    totalExecutedCycles += 17; pendingCycles -= 17;
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = TUS;
                                    break;
                                case 0xCE: // ADC A, n
                                    RegAF.Word = TableALU[1, RegAF.High, ReadMemory(RegPC.Word++), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xCF: // RST $08
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x08;
                                    break;
                                case 0xD0: // RET NC
                                    if (!RegFlagC)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xD1: // POP DE
                                    RegDE.Low = ReadMemory(RegSP.Word++); RegDE.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xD2: // JP NC, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagC)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xD3: // OUT n, A
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteHardware(ReadMemory(RegPC.Word++), RegAF.High);
                                    break;
                                case 0xD4: // CALL NC, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xD5: // PUSH DE
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegDE.High); WriteMemory(--RegSP.Word, RegDE.Low);
                                    break;
                                case 0xD6: // SUB n
                                    RegAF.Word = TableALU[2, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xD7: // RST $10
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x10;
                                    break;
                                case 0xD8: // RET C
                                    if (RegFlagC)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xD9: // EXX
                                    TUS = RegBC.Word; RegBC.Word = RegAltBC.Word; RegAltBC.Word = TUS;
                                    TUS = RegDE.Word; RegDE.Word = RegAltDE.Word; RegAltDE.Word = TUS;
                                    TUS = RegHL.Word; RegHL.Word = RegAltHL.Word; RegAltHL.Word = TUS;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xDA: // JP C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xDB: // IN A, n
                                    TUS = (ushort)(ReadOpArg(RegPC.Word++) | (RegAF.High << 8));
                                    RegAF.High = ReadHardware(TUS);
                                    RegWZ.Word = (ushort)(TUS + 1);
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    break;
                                case 0xDC: // CALL C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xDD: // <-
                                    // Invalid sequence.
                                    totalExecutedCycles += 1337; pendingCycles -= 1337;
                                    break;
                                case 0xDE: // SBC A, n
                                    RegAF.Word = TableALU[3, RegAF.High, ReadMemory(RegPC.Word++), RegFlagC ? 1 : 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xDF: // RST $18
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x18;
                                    break;
                                case 0xE0: // RET PO
                                    if (!RegFlagP)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xE1: // POP IY
                                    RegIY.Low = ReadMemory(RegSP.Word++); RegIY.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 14; pendingCycles -= 14;
                                    break;
                                case 0xE2: // JP PO, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagP)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xE3: // EX (SP), IY
                                    totalExecutedCycles += 23; pendingCycles -= 23;
                                    TUS = RegSP.Word; TBL = ReadMemory(TUS++); TBH = ReadMemory(TUS--);
                                    WriteMemory(TUS++, RegIY.Low); WriteMemory(TUS, RegIY.High);
                                    RegIY.Low = TBL; RegIY.High = TBH;
                                    RegWZ.Word = RegIY.Word;
                                    break;
                                case 0xE4: // CALL C, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagC)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xE5: // PUSH IY
                                    totalExecutedCycles += 15; pendingCycles -= 15;
                                    WriteMemory(--RegSP.Word, RegIY.High); WriteMemory(--RegSP.Word, RegIY.Low);
                                    break;
                                case 0xE6: // AND n
                                    RegAF.Word = TableALU[4, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xE7: // RST $20
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x20;
                                    break;
                                case 0xE8: // RET PE
                                    if (RegFlagP)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xE9: // JP IY
                                    RegPC.Word = RegIY.Word;
                                    totalExecutedCycles += 8; pendingCycles -= 8;
                                    break;
                                case 0xEA: // JP PE, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagP)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xEB: // EX DE, HL
                                    TUS = RegDE.Word; RegDE.Word = RegHL.Word; RegHL.Word = TUS;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xEC: // CALL PE, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagP)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xED: // (Prefix)
                                    ++RegR;
                                    switch (ReadOp(RegPC.Word++))
                                    {
                                        case 0x00: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x01: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x02: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x03: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x04: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x05: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x06: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x07: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x08: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x09: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x0F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x10: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x11: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x12: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x13: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x14: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x15: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x16: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x17: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x18: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x19: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x1F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x20: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x21: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x22: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x23: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x24: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x25: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x26: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x27: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x28: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x29: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x2F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x30: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x31: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x32: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x33: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x34: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x35: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x36: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x37: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x38: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x39: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x3F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x40: // IN B, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegBC.High = ReadHardware(RegBC.Word);
                                            RegFlagS = RegBC.High > 127;
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlag5 = (RegBC.High & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegBC.High & 0x08) != 0;
                                            RegFlagP = TableParity[RegBC.High];
                                            RegFlagN = false;
                                            break;
                                        case 0x41: // OUT C, B
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegBC.High);
                                            break;
                                        case 0x42: // SBC HL, BC
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegBC.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegBC.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x43: // LD (nn), BC
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegBC.Low);
                                            WriteMemory(TUS, RegBC.High);
                                            RegWZ.Word = TUS;
                                            break;
                                        case 0x44: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x45: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            RegWZ.Word = RegPC.Word;
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x46: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x47: // LD I, A
                                            RegI = RegAF.High;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x48: // IN C, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegBC.Low = ReadHardware(RegBC.Word);
                                            RegFlagS = RegBC.Low > 127;
                                            RegFlagZ = RegBC.Low == 0;
                                            RegFlag5 = (RegBC.Low & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegBC.Low & 0x08) != 0;
                                            RegFlagP = TableParity[RegBC.Low];
                                            RegFlagN = false;
                                            break;
                                        case 0x49: // OUT C, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegBC.Low);
                                            break;
                                        case 0x4A: // ADC HL, BC
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegBC.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x4B: // LD BC, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegBC.Low = ReadMemory(TUS++); RegBC.High = ReadMemory(TUS);
                                            RegWZ.Word = TUS;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x4C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x4D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x4E: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x4F: // LD R, A
                                            RegR = RegAF.High;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x50: // IN D, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegDE.High = ReadHardware(RegBC.Word);
                                            RegFlagS = RegDE.High > 127;
                                            RegFlagZ = RegDE.High == 0;
                                            RegFlag5 = (RegDE.High & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegDE.High & 0x08) != 0;
                                            RegFlagP = TableParity[RegDE.High];
                                            RegFlagN = false;
                                            break;
                                        case 0x51: // OUT C, D
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegDE.High);
                                            break;
                                        case 0x52: // SBC HL, DE
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                                            RegFlagH = ((RegHL.Word ^ RegDE.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegDE.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x53: // LD (nn), DE
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegDE.Low);
                                            WriteMemory(TUS, RegDE.High);
                                            RegWZ.Word = TUS;
                                            break;
                                        case 0x54: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x55: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x56: // IM $1
                                            interruptMode = 1;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x57: // LD A, I
                                            RegAF.High = RegI;
                                            RegFlagS = RegI > 127;
                                            RegFlagZ = RegI == 0;
                                            RegFlag5 = ((RegI & 0x20) != 0);
                                            RegFlagH = false;
                                            RegFlag3 = ((RegI & 0x08) != 0);
                                            RegFlagN = false;
                                            RegFlagP = IFF2;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x58: // IN E, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegDE.Low = ReadHardware(RegBC.Word);
                                            RegFlagS = RegDE.Low > 127;
                                            RegFlagZ = RegDE.Low == 0;
                                            RegFlag5 = (RegDE.Low & 0x20) != 0;
                                            RegFlagH = false;
                                            RegFlag3 = (RegDE.Low & 0x08) != 0;
                                            RegFlagP = TableParity[RegDE.Low];
                                            RegFlagN = false;
                                            break;
                                        case 0x59: // OUT C, E
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegDE.Low);
                                            break;
                                        case 0x5A: // ADC HL, DE
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegDE.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x5B: // LD DE, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegDE.Low = ReadMemory(TUS++); RegDE.High = ReadMemory(TUS);
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x5C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x5D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x5E: // IM $2
                                            interruptMode = 2;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x5F: // LD A, R
                                            RegAF.High = (byte)((RegR & 0x7F) | RegR2);
                                            RegFlagS = (RegR2 == 0x80);
                                            RegFlagZ = (byte)((RegR & 0x7F) | RegR2) == 0;
                                            RegFlagH = false;
                                            RegFlag5 = ((RegR & 0x20) != 0);
                                            RegFlagN = false;
                                            RegFlag3 = ((RegR & 0x08) != 0);
                                            RegFlagP = IFF2;
                                            totalExecutedCycles += 9; pendingCycles -= 9;
                                            break;
                                        case 0x60: // IN H, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegHL.High = ReadHardware(RegBC.Word);
                                            RegFlagS = RegHL.High > 127;
                                            RegFlagZ = RegHL.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegHL.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegHL.High & 0x08) != 0;
                                            RegFlag5 = (RegHL.High & 0x20) != 0;
                                            break;
                                        case 0x61: // OUT C, H
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegHL.High);
                                            break;
                                        case 0x62: // SBC HL, HL
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((RegHL.Word ^ RegHL.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegHL.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x63: // LD (nn), HL
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegHL.Low);
                                            WriteMemory(TUS, RegHL.High);
                                            break;
                                        case 0x64: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x65: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x66: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x67: // RRD
                                            totalExecutedCycles += 18; pendingCycles -= 18;
                                            TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                            WriteMemory(RegHL.Word, (byte)((TB2 >> 4) + (TB1 << 4)));
                                            RegAF.High = (byte)((TB1 & 0xF0) + (TB2 & 0x0F));
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            break;
                                        case 0x68: // IN L, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegHL.Low = ReadHardware(RegBC.Word);
                                            RegFlagS = RegHL.Low > 127;
                                            RegFlagZ = RegHL.Low == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegHL.Low];
                                            RegFlagN = false;
                                            RegFlag3 = (RegHL.Low & 0x08) != 0;
                                            RegFlag5 = (RegHL.Low & 0x20) != 0;
                                            break;
                                        case 0x69: // OUT C, L
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegHL.Low);
                                            break;
                                        case 0x6A: // ADC HL, HL
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegHL.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x6B: // LD HL, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegHL.Low = ReadMemory(TUS++); RegHL.High = ReadMemory(TUS);
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0x6C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x6D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x6E: // IM $0
                                            interruptMode = 0;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x6F: // RLD
                                            totalExecutedCycles += 18; pendingCycles -= 18;
                                            TB1 = RegAF.High; TB2 = ReadMemory(RegHL.Word);
                                            WriteMemory(RegHL.Word, (byte)((TB1 & 0x0F) + (TB2 << 4)));
                                            RegAF.High = (byte)((TB1 & 0xF0) + (TB2 >> 4));
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            break;
                                        case 0x70: // IN 0, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            TB = ReadHardware(RegBC.Word);
                                            RegFlagS = TB > 127;
                                            RegFlagZ = TB == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[TB];
                                            RegFlagN = false;
                                            RegFlag3 = (TB & 0x08) != 0;
                                            RegFlag5 = (TB & 0x20) != 0;
                                            break;
                                        case 0x71: // OUT C, 0
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, 0);
                                            break;
                                        case 0x72: // SBC HL, SP
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 - TI2;
                                            if (RegFlagC) { --TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                                            RegFlagH = ((RegHL.Word ^ RegSP.Word ^ TUS) & 0x1000) != 0;
                                            RegFlagN = true;
                                            RegFlagC = (((int)RegHL.Word - (int)RegSP.Word - (RegFlagC ? 1 : 0)) & 0x10000) != 0;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x73: // LD (nn), SP
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            WriteMemory(TUS++, RegSP.Low);
                                            WriteMemory(TUS, RegSP.High);
                                            RegWZ.Word = TUS;
                                            break;
                                        case 0x74: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x75: // RETN
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            RegWZ.Word = RegPC.Word;
                                            IFF1 = IFF2;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x76: // IM $1
                                            interruptMode = 1;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x77: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x78: // IN A, C
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            RegAF.High = ReadHardware(RegBC.Word);
                                            RegFlagS = RegAF.High > 127;
                                            RegFlagZ = RegAF.High == 0;
                                            RegFlagH = false;
                                            RegFlagP = TableParity[RegAF.High];
                                            RegFlagN = false;
                                            RegFlag3 = (RegAF.High & 0x08) != 0;
                                            RegFlag5 = (RegAF.High & 0x20) != 0;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            break;
                                        case 0x79: // OUT C, A
                                            totalExecutedCycles += 12; pendingCycles -= 12;
                                            WriteHardware(RegBC.Low, RegAF.High);
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            break;
                                        case 0x7A: // ADC HL, SP
                                            TI1 = (short)RegHL.Word; TI2 = (short)RegSP.Word; TIR = TI1 + TI2;
                                            if (RegFlagC) { ++TIR; ++TI2; }
                                            TUS = (ushort)TIR;
                                            RegWZ.Word = (ushort)(RegHL.Word + 1);
                                            RegFlagH = ((TI1 & 0xFFF) + (TI2 & 0xFFF)) > 0xFFF;
                                            RegFlagN = false;
                                            RegFlagC = ((ushort)TI1 + (ushort)TI2) > 0xFFFF;
                                            RegFlagP = TIR > 32767 || TIR < -32768;
                                            RegFlagS = TUS > 32767;
                                            RegFlagZ = TUS == 0;
                                            RegHL.Word = TUS;
                                            RegFlag3 = (TUS & 0x0800) != 0;
                                            RegFlag5 = (TUS & 0x2000) != 0;
                                            totalExecutedCycles += 15; pendingCycles -= 15;
                                            break;
                                        case 0x7B: // LD SP, (nn)
                                            TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                            RegSP.Low = ReadMemory(TUS++); RegSP.High = ReadMemory(TUS);
                                            RegWZ.Word = TUS;
                                            totalExecutedCycles += 20; pendingCycles -= 20;
                                            break;
                                        case 0x7C: // NEG
                                            RegAF.Word = TableNeg[RegAF.Word];
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x7D: // RETI
                                            RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                            RegWZ.Word = RegPC.Word;
                                            totalExecutedCycles += 14; pendingCycles -= 14;
                                            break;
                                        case 0x7E: // IM $2
                                            interruptMode = 2;
                                            totalExecutedCycles += 8; pendingCycles -= 8;
                                            break;
                                        case 0x7F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x80: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x81: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x82: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x83: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x84: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x85: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x86: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x87: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x88: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x89: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x8F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x90: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x91: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x92: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x93: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x94: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x95: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x96: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x97: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x98: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x99: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9A: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9B: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9C: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9D: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9E: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0x9F: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA0: // LDI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            break;
                                        case 0xA1: // CPI
                                            TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0xA2: // INI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word++, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xA3: // OUTI
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word++);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            WriteHardware(RegBC.Word, TB);
                                            RegFlagC = false;
                                            RegFlagN = false;
                                            RegFlag3 = IsX(RegBC.High);
                                            RegFlagH = false;
                                            RegFlag5 = IsY(RegBC.High);
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlagS = IsS(RegBC.High);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            if (IsS(TB))
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagC = true;
                                                RegFlagH = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xA4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xA8: // LDD
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            break;
                                        case 0xA9: // CPD
                                            TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            break;
                                        case 0xAA: // IND
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word--, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xAB: // OUTD
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word--);
                                            WriteHardware(RegBC.Word, TB);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            RegFlagZ = RegBC.High == 0;
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            break;
                                        case 0xAC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xAF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB0: // LDIR
                                            WriteMemory(RegDE.Word++, TB1 = ReadMemory(RegHL.Word++));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            if (RegBC.Word != 0)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB1: // CPIR
                                            TB1 = ReadMemory(RegHL.Word++); TB2 = (byte)(RegAF.High - TB1);
                                            RegWZ.Word++;
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            if (RegBC.Word != 0 && !RegFlagZ)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB2: // INIR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word++, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low + 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xB3: // OTIR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word++);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word + 1);
                                            WriteHardware(RegBC.Word, TB);
                                            RegFlagC = false;
                                            RegFlagN = false;
                                            RegFlag3 = IsX(RegBC.High);
                                            RegFlagH = false;
                                            RegFlag5 = IsY(RegBC.High);
                                            RegFlagZ = RegBC.High == 0;
                                            RegFlagS = IsS(RegBC.High);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            if (IsS(TB))
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagC = true;
                                                RegFlagH = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xB4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xB8: // LDDR
                                            WriteMemory(RegDE.Word--, TB1 = ReadMemory(RegHL.Word--));
                                            TB1 += RegAF.High; RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            RegFlagH = false;
                                            RegFlagN = false;
                                            if (RegBC.Word != 0)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xB9: // CPDR
                                            TB1 = ReadMemory(RegHL.Word--); TB2 = (byte)(RegAF.High - TB1);
                                            RegWZ.Word--;
                                            RegFlagN = true;
                                            RegFlagH = TableHalfBorrow[RegAF.High, TB1];
                                            RegFlagZ = TB2 == 0;
                                            RegFlagS = TB2 > 127;
                                            TB1 = (byte)(RegAF.High - TB1 - (RegFlagH ? 1 : 0)); RegFlag5 = (TB1 & 0x02) != 0; RegFlag3 = (TB1 & 0x08) != 0;
                                            --RegBC.Word;
                                            RegFlagP = RegBC.Word != 0;
                                            if (RegBC.Word != 0 && !RegFlagZ)
                                            {
                                                RegPC.Word -= 2;
                                                RegWZ.Word = (ushort)(RegPC.Word + 1);
                                                totalExecutedCycles += 21; pendingCycles -= 21;
                                            }
                                            else
                                            {
                                                totalExecutedCycles += 16; pendingCycles -= 16;
                                            }
                                            break;
                                        case 0xBA: // INDR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadHardware(RegBC.Word);
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            --RegBC.High;
                                            WriteMemory(RegHL.Word--, TB);
                                            RegFlagZ = RegBC.High == 0;
                                            TUS = (ushort)(((RegBC.Low - 1) & 0xff) + TB);
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xBB: // OTDR
                                            totalExecutedCycles += 16; pendingCycles -= 16;
                                            TB = ReadMemory(RegHL.Word--);
                                            WriteHardware(RegBC.Word, TB);
                                            --RegBC.High;
                                            RegWZ.Word = (ushort)(RegBC.Word - 1);
                                            TUS = (ushort)(RegHL.Low + TB);
                                            RegFlagZ = RegBC.High == 0;
                                            if ((TB & 0x80) != 0)
                                            {
                                                RegFlagN = true;
                                            }
                                            if ((TUS & 0x100) != 0)
                                            {
                                                RegFlagH = RegFlagC = true;
                                            }
                                            RegFlagP = TableParity[(TUS & 0x07) ^ RegBC.High];
                                            if (RegBC.High != 0)
                                            {
                                                RegPC.Word -= 2;
                                                totalExecutedCycles += 5; pendingCycles -= 5;
                                            }
                                            break;
                                        case 0xBC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xBF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xC9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xCF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xD9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xDF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xE9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xED: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xEF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF0: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF1: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF2: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF3: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF4: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF5: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF6: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF7: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF8: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xF9: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFA: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFB: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFC: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFD: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFE: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                        case 0xFF: // NOP
                                            totalExecutedCycles += 4; pendingCycles -= 4;
                                            break;
                                    }
                                    break;
                                case 0xEE: // XOR n
                                    RegAF.Word = TableALU[5, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xEF: // RST $28
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x28;
                                    break;
                                case 0xF0: // RET P
                                    if (!RegFlagS)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xF1: // POP AF
                                    RegAF.Low = ReadMemory(RegSP.Word++); RegAF.High = ReadMemory(RegSP.Word++);
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xF2: // JP P, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagS)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xF3: // DI
                                    IFF1 = IFF2 = false;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xF4: // CALL P, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (!RegFlagS)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xF5: // PUSH AF
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegAF.High); WriteMemory(--RegSP.Word, RegAF.Low);
                                    break;
                                case 0xF6: // OR n
                                    RegAF.Word = TableALU[6, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xF7: // RST $30
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x30;
                                    break;
                                case 0xF8: // RET M
                                    if (RegFlagS)
                                    {
                                        RegPC.Low = ReadMemory(RegSP.Word++); RegPC.High = ReadMemory(RegSP.Word++);
                                        totalExecutedCycles += 11; pendingCycles -= 11;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 5; pendingCycles -= 5;
                                    }
                                    break;
                                case 0xF9: // LD SP, IY
                                    RegSP.Word = RegIY.Word;
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xFA: // JP M, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagS)
                                    {
                                        RegPC.Word = TUS;
                                    }
                                    totalExecutedCycles += 10; pendingCycles -= 10;
                                    break;
                                case 0xFB: // EI
                                    IFF1 = IFF2 = true;
                                    Interruptable = false;
                                    totalExecutedCycles += 4; pendingCycles -= 4;
                                    break;
                                case 0xFC: // CALL M, nn
                                    TUS = (ushort)(ReadMemory(RegPC.Word++) + ReadMemory(RegPC.Word++) * 256);
                                    if (RegFlagS)
                                    {
                                        totalExecutedCycles += 17; pendingCycles -= 17;
                                        WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                        RegPC.Word = TUS;
                                    }
                                    else
                                    {
                                        totalExecutedCycles += 10; pendingCycles -= 10;
                                    }
                                    break;
                                case 0xFD: // <-
                                    // Invalid sequence.
                                    totalExecutedCycles += 1337; pendingCycles -= 1337;
                                    break;
                                case 0xFE: // CP n
                                    RegAF.Word = TableALU[7, RegAF.High, ReadMemory(RegPC.Word++), 0];
                                    totalExecutedCycles += 7; pendingCycles -= 7;
                                    break;
                                case 0xFF: // RST $38
                                    totalExecutedCycles += 11; pendingCycles -= 11;
                                    WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                                    RegPC.Word = 0x38;
                                    break;
                            }
                            break;
                        case 0xFE: // CP n
                            RegAF.Word = TableALU[7, RegAF.High, ReadOpArg(RegPC.Word++), 0];
                            totalExecutedCycles += 7; pendingCycles -= 7;
                            break;
                        case 0xFF: // RST $38
                            WriteMemory(--RegSP.Word, RegPC.High); WriteMemory(--RegSP.Word, RegPC.Low);
                            RegPC.Word = 0x38;
                            totalExecutedCycles += 11; pendingCycles -= 11;
                            break;
                    }
                }
            }
            while (pendingCycles > 0);
            return cycles - pendingCycles;
        }
    }
}
