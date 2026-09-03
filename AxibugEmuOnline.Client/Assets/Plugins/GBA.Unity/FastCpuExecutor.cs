// FastCpuExecutor — 巨型 switch 解释器实验
//
// 目标：把 CPU 指令分发从"delegate 数组间接调用"改为"单个巨型 switch 直接执行"，
//       让 IL2CPP 有机会把指令实现内联进主循环，消除每指令一次的间接跳转 + 分支预测 miss。
//
// 这是一个可回退的实验：本文件是全新文件，不改动 Arm.cs / Thumb.cs / Arm7.cs。
// 如果 IL2CPP 实测没有收益，删掉本文件并把 Gba.cs 的调用切回 ExecuteArm/ExecuteThumb 即可。
//
// 注意：本文件是手写转录，与原 Arm.cs / Thumb.cs 一一对应。
//       LineDebug 调用（[Conditional("DONT")]）已省略，发布时无运行时开销。

using static OptimeGBA.Bits;
using static Util;

namespace OptimeGBA
{
    public unsafe sealed class FastCpuExecutor
    {
        private readonly Arm7 arm7;
        private Memory Mem => arm7.Mem;

        // 缓存常用字段，避免每次解引用 this
        public uint* R;

        public FastCpuExecutor(Arm7 arm7)
        {
            this.arm7 = arm7;
            R = arm7.R;
        }

        // 转发到 Arm7 的辅助方法（这些是冷路径，内联与否无所谓）
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private uint GetCPSR()
        {
            uint val = 0;
            if (arm7.Negative) val = BitSet(val, 31);
            if (arm7.Zero) val = BitSet(val, 30);
            if (arm7.Carry) val = BitSet(val, 29);
            if (arm7.Overflow) val = BitSet(val, 28);
            if (arm7.Sticky) val = BitSet(val, 27);
            if (arm7.IRQDisable) val = BitSet(val, 7);
            if (arm7.FIQDisable) val = BitSet(val, 6);
            if (arm7.ThumbState) val = BitSet(val, 5);
            val |= (uint)arm7.Mode;
            return val;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SetCPSR(uint val)
        {
            arm7.Negative = BitTest(val, 31);
            arm7.Zero = BitTest(val, 30);
            arm7.Carry = BitTest(val, 29);
            arm7.Overflow = BitTest(val, 28);
            arm7.Sticky = BitTest(val, 27);
            arm7.IRQDisable = BitTest(val, 7);
            arm7.FIQDisable = BitTest(val, 6);
            bool newThumbState = BitTest(val, 5);
            if (newThumbState != arm7.ThumbState)
            {
                arm7.StateChange();
            }
            arm7.ThumbState = newThumbState;
            SetMode((Arm7Mode)(val & 0b01111));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private uint GetSPSR()
        {
            switch (arm7.Mode)
            {
                case Arm7Mode.FIQ:
                case Arm7Mode.OldFIQ:
                    return arm7.SPSR_fiq;
                case Arm7Mode.SVC:
                case Arm7Mode.OldSVC:
                    return arm7.SPSR_svc;
                case Arm7Mode.ABT:
                    return arm7.SPSR_abt;
                case Arm7Mode.IRQ:
                case Arm7Mode.OldIRQ:
                    return arm7.SPSR_irq;
                case Arm7Mode.UND:
                    return arm7.SPSR_und;
            }
            return GetCPSR();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SetSPSR(uint set)
        {
            switch (arm7.Mode)
            {
                case Arm7Mode.FIQ:
                case Arm7Mode.OldFIQ:
                    arm7.SPSR_fiq = set;
                    return;
                case Arm7Mode.SVC:
                case Arm7Mode.OldSVC:
                    arm7.SPSR_svc = set;
                    return;
                case Arm7Mode.ABT:
                    arm7.SPSR_abt = set;
                    return;
                case Arm7Mode.IRQ:
                case Arm7Mode.OldIRQ:
                    arm7.SPSR_irq = set;
                    return;
                case Arm7Mode.UND:
                    arm7.SPSR_und = set;
                    return;
            }
            SetCPSR(set);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SetMode(Arm7Mode mode)
        {
            mode |= (Arm7Mode)0b10000;
            switch (arm7.Mode)
            {
                case Arm7Mode.USR:
                case Arm7Mode.SYS: for (uint i = 0; i < 7; i++) arm7.Rusr[i] = R[8 + i]; break;
                case Arm7Mode.FIQ: for (uint i = 0; i < 7; i++) arm7.Rfiq[i] = R[8 + i]; break;
                case Arm7Mode.SVC: for (uint i = 0; i < 2; i++) arm7.Rsvc[i] = R[13 + i]; break;
                case Arm7Mode.ABT: for (uint i = 0; i < 2; i++) arm7.Rabt[i] = R[13 + i]; break;
                case Arm7Mode.IRQ: for (uint i = 0; i < 2; i++) arm7.Rirq[i] = R[13 + i]; break;
                case Arm7Mode.UND: for (uint i = 0; i < 2; i++) arm7.Rund[i] = R[13 + i]; break;
            }
            switch (mode)
            {
                case Arm7Mode.USR:
                case Arm7Mode.SYS: for (uint i = 5; i < 7; i++) R[8 + i] = arm7.Rusr[i]; break;
                case Arm7Mode.FIQ: for (uint i = 0; i < 7; i++) R[8 + i] = arm7.Rfiq[i]; break;
                case Arm7Mode.SVC: for (uint i = 0; i < 2; i++) R[13 + i] = arm7.Rsvc[i]; break;
                case Arm7Mode.ABT: for (uint i = 0; i < 2; i++) R[13 + i] = arm7.Rabt[i]; break;
                case Arm7Mode.IRQ: for (uint i = 0; i < 2; i++) R[13 + i] = arm7.Rirq[i]; break;
                case Arm7Mode.UND: for (uint i = 0; i < 2; i++) R[13 + i] = arm7.Rund[i]; break;
            }
            if (arm7.Mode == Arm7Mode.FIQ)
                for (uint i = 0; i < 5; i++) R[8 + i] = arm7.Rusr[i];
            arm7.Mode = mode;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool CheckCondition(uint code)
        {
            if (code == 0xE)
            {
                return true;
            }
            switch (code)
            {
                case 0x0: return arm7.Zero;
                case 0x1: return !arm7.Zero;
                case 0x2: return arm7.Carry;
                case 0x3: return !arm7.Carry;
                case 0x4: return arm7.Negative;
                case 0x5: return !arm7.Negative;
                case 0x6: return arm7.Overflow;
                case 0x7: return !arm7.Overflow;
                case 0x8: return arm7.Carry && !arm7.Zero;
                case 0x9: return !arm7.Carry || arm7.Zero;
                case 0xA: return arm7.Negative == arm7.Overflow;
                case 0xB: return arm7.Negative != arm7.Overflow;
                case 0xC: return !arm7.Zero && arm7.Negative == arm7.Overflow;
                case 0xD: return arm7.Zero || (arm7.Negative != arm7.Overflow);
                case 0xE: return true;
                case 0xF: return true;
            }
            return false;
        }

        // 内存读取（与 Arm7 中手内联版本一致：页表快速路径）
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private uint Read32InstrFetch(uint addr)
        {
            uint addr_12 = addr >> 12;
            uint addr_24 = addr >> 24;
            arm7.InstructionCycles += arm7.Timing32InstrFetch[(addr_24) & 0xF];
            var page = Mem.PageTableRead[addr_12];
            if (page != null)
                return *(uint*)(page + (addr & Mem.MemoryRegionMasks[addr_12]));
            return Mem.Read32Unregistered(false, addr);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private ushort Read16InstrFetch(uint addr)
        {
            arm7.InstructionCycles += arm7.Timing8And16InstrFetch[(addr >> 24) & 0xF];
            return Mem.Read16(addr);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private uint Read32(uint addr)
        {
            arm7.InstructionCycles += arm7.Timing32[(addr >> 24) & 0xF];
            return Mem.Read32(addr);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private ushort Read16(uint addr)
        {
            arm7.InstructionCycles += arm7.Timing8And16[(addr >> 24) & 0xF];
            return Mem.Read16(addr);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private byte Read8(uint addr)
        {
            arm7.InstructionCycles += arm7.Timing8And16[(addr >> 24) & 0xF];
            return Mem.Read8(addr);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Write32(uint addr, uint val)
        {
            arm7.InstructionCycles += arm7.Timing32[(addr >> 24) & 0xF];
            Mem.Write32(addr, val);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Write16(uint addr, ushort val)
        {
            arm7.InstructionCycles += arm7.Timing8And16[(addr >> 24) & 0xF];
            Mem.Write16(addr, val);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Write8(uint addr, byte val)
        {
            arm7.InstructionCycles += arm7.Timing8And16[(addr >> 24) & 0xF];
            Mem.Write8(addr, val);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ICycle()
        {
            arm7.InstructionCycles += 1;
        }

        // 分支/流水线（与 Arm7 一致）
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void InitFlushPipeline()
        {
            if (arm7.ThumbState)
            {
                R[15] += 4;
                arm7.InstructionCycles += arm7.Timing8And16InstrFetch[(R[15] >> 24) & 0xF] * 2U;
            }
            else
            {
                R[15] += 8;
                arm7.InstructionCycles += arm7.Timing32InstrFetch[(R[15] >> 24) & 0xF] * 2U;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void FlushPipeline()
        {
            if (arm7.ThumbState)
            {
                R[15] &= ~1U;
                R[15] += 2;
                arm7.InstructionCycles += arm7.Timing8And16InstrFetch[(R[15] >> 24) & 0xF] * 2U;
            }
            else
            {
                R[15] &= ~3U;
                R[15] += 4;
                arm7.InstructionCycles += arm7.Timing32InstrFetch[(R[15] >> 24) & 0xF] * 2U;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static uint popcount_8(uint n)
        {
            uint m = 0x01010101;
            uint c = n & m;
            int i;
            for (i = 0; i < 7; i++)
            {
                n >>= 1;
                c += n & m;
            }
            c += c >> 8;
            c += c >> 16;
            return c & 0x3f;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool CheckOverflowSub(uint val1, uint val2, uint result)
        {
            return ((val1 ^ val2) & ((val1 ^ result)) & 0x80000000) != 0;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool CheckOverflowAdd(uint val1, uint val2, uint result)
        {
            return (~(val1 ^ val2) & ((val1 ^ result)) & 0x80000000) != 0;
        }

        // ============ 主循环：一次执行一条指令（返回消耗周期） ============
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public uint Execute()
        {
            arm7.InstructionsRan++;
            arm7.InstructionCycles = 0;

            if (!arm7.ThumbState)
            {
                ExecuteArmInlined();
            }
            else
            {
                ExecuteThumbInlined();
            }

            return arm7.InstructionCycles;
        }

        // ============ THUMB ============
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ExecuteThumbInlined()
        {
            ushort ins = (ushort)Read16InstrFetch(R[15] - 4);
            int decodeBits = ins >> 6;

            // Thumb: decodeBits = ins>>6（0-1023），按 top-3-bits（(ins>>13)&0b111）分 8 组，
            // 每组覆盖 128 个连续 decodeBits 值。
            switch (decodeBits)
            {
                // ---- top 0b000 (decodeBits 0x000-0x07F) ----
                case >= 0x000 and <= 0x07F:
                    {
                        switch ((ins >> 11) & 0b11)
                        {
                            case 0b00: // LSL (1)
                                {
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rmValue = R[(uint)((ins >> 3) & 0b111)];
                                    if (immed5 == 0)
                                    {
                                        R[rd] = rmValue;
                                    }
                                    else
                                    {
                                        arm7.Carry = BitTest(rmValue, (byte)(32 - immed5));
                                        R[rd] = LogicalShiftLeft32(rmValue, (byte)immed5);
                                    }
                                    arm7.Negative = BitTest(R[rd], 31);
                                    arm7.Zero = R[rd] == 0;
                                }
                                break;
                            case 0b01: // LSR (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rm = (uint)((ins >> 3) & 0b111);
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint rmVal = R[rm];
                                    uint final;
                                    if (immed5 == 0)
                                    {
                                        arm7.Carry = BitTest(rmVal, 31);
                                        final = 0;
                                    }
                                    else
                                    {
                                        arm7.Carry = BitTest(rmVal, (byte)(immed5 - 1));
                                        final = LogicalShiftRight32(rmVal, (byte)immed5);
                                    }
                                    R[rd] = final;
                                    arm7.Negative = BitTest(final, 31);
                                    arm7.Zero = final == 0;
                                }
                                break;
                            case 0b10: // ASR (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rmValue = R[(uint)((ins >> 3) & 0b111)];
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    if (immed5 == 0)
                                    {
                                        arm7.Carry = BitTest(rmValue, 31);
                                        if (BitTest(rmValue, 31))
                                        {
                                            R[rd] = 0xFFFFFFFF;
                                        }
                                        else
                                        {
                                            R[rd] = 0;
                                        }
                                    }
                                    else
                                    {
                                        arm7.Carry = BitTest(rmValue, (byte)(immed5 - 1));
                                        R[rd] = ArithmeticShiftRight32(rmValue, (byte)immed5);
                                    }
                                    arm7.Negative = BitTest(R[rd], 31);
                                    arm7.Zero = R[rd] == 0;
                                }
                                break;
                            case 0b11: // Add/subtract/compare/move immediate
                                {
                                    switch ((ins >> 9) & 0b11)
                                    {
                                        case 0b00: // ADD (3)
                                            {
                                                uint rd = (uint)((ins >> 0) & 0b111);
                                                uint rnVal = R[(uint)((ins >> 3) & 0b111)];
                                                uint rmVal = R[(uint)((ins >> 6) & 0b111)];
                                                uint final = rnVal + rmVal;
                                                R[rd] = final;
                                                arm7.Negative = BitTest(final, 31);
                                                arm7.Zero = final == 0;
                                                arm7.Carry = (long)rnVal + (long)rmVal > 0xFFFFFFFF;
                                                arm7.Overflow = CheckOverflowAdd(rnVal, rmVal, final);
                                            }
                                            break;
                                        case 0b01: // SUB (3)
                                            {
                                                uint rd = (uint)((ins >> 0) & 0b111);
                                                uint rnValue = R[(uint)((ins >> 3) & 0b111)];
                                                uint rmValue = R[(uint)((ins >> 6) & 0b111)];
                                                uint final = rnValue - rmValue;
                                                R[rd] = final;
                                                arm7.Negative = BitTest(final, 31);
                                                arm7.Zero = final == 0;
                                                arm7.Carry = !(rmValue > rnValue);
                                                arm7.Overflow = CheckOverflowSub(rnValue, rmValue, final);
                                            }
                                            break;
                                        case 0b10: // ADD (1)
                                            {
                                                uint rd = (uint)((ins >> 0) & 0b111);
                                                uint rnVal = R[(uint)((ins >> 3) & 0b111)];
                                                uint immed3 = (uint)((ins >> 6) & 0b111);
                                                uint final = rnVal + immed3;
                                                R[rd] = final;
                                                arm7.Negative = BitTest(final, 31);
                                                arm7.Zero = final == 0;
                                                arm7.Carry = (long)rnVal + (long)immed3 > 0xFFFFFFFF;
                                                arm7.Overflow = CheckOverflowAdd(rnVal, immed3, final);
                                                if (rd == 15) FlushPipeline();
                                            }
                                            break;
                                        case 0b11: // SUB (1)
                                            {
                                                uint rd = (uint)((ins >> 0) & 0b111);
                                                uint rn = (uint)((ins >> 3) & 0b111);
                                                uint immed3 = (uint)((ins >> 6) & 0b111);
                                                uint rdVal = R[rd];
                                                uint rnVal = R[rn];
                                                uint final = rnVal - immed3;
                                                R[rd] = final;
                                                arm7.Negative = BitTest(final, 31);
                                                arm7.Zero = final == 0;
                                                arm7.Carry = !(immed3 > rnVal);
                                                arm7.Overflow = CheckOverflowSub(rnVal, immed3, final);
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // ---- top 0b001 (decodeBits 0x080-0x0FF) ----
                case >= 0x080 and <= 0x0FF:
                    {
                        switch ((ins >> 11) & 0b11)
                        {
                            case 0b00: // MOV (1)
                                {
                                    uint rd = (uint)((ins >> 8) & 0b111);
                                    uint immed8 = ins & 0xFFu;
                                    R[rd] = immed8;
                                    arm7.Negative = false;
                                    arm7.Zero = immed8 == 0;
                                }
                                break;
                            case 0b01: // CMP (1)
                                {
                                    uint rd = (uint)((ins >> 8) & 0b111);
                                    uint immed8 = ins & 0xFFu;
                                    uint rnVal = R[rd];
                                    uint alu_out = rnVal - immed8;
                                    arm7.Negative = BitTest(alu_out, 31);
                                    arm7.Zero = alu_out == 0;
                                    arm7.Carry = !(immed8 > rnVal);
                                    arm7.Overflow = CheckOverflowSub(rnVal, immed8, alu_out);
                                }
                                break;
                            case 0b10: // ADD (2)
                                {
                                    uint rd = (uint)((ins >> 8) & 0b111);
                                    uint immed8 = ins & 0xFFu;
                                    uint rdVal = R[rd];
                                    uint final = rdVal + immed8;
                                    R[rd] = final;
                                    arm7.Negative = BitTest(final, 31);
                                    arm7.Zero = final == 0;
                                    arm7.Carry = (long)rdVal + (long)immed8 > 0xFFFFFFFF;
                                    arm7.Overflow = CheckOverflowAdd(rdVal, immed8, final);
                                }
                                break;
                            case 0b11: // SUB (2)
                                {
                                    uint rd = (uint)((ins >> 8) & 0b111);
                                    uint immed8 = ins & 0xFFu;
                                    uint rdVal = R[rd];
                                    uint final = rdVal - immed8;
                                    R[rd] = final;
                                    arm7.Negative = BitTest(final, 31);
                                    arm7.Zero = final == 0;
                                    arm7.Carry = !(immed8 > rdVal);
                                    arm7.Overflow = CheckOverflowSub(rdVal, immed8, final);
                                }
                                break;
                        }
                    }
                    break;

                // ---- top 0b010 (decodeBits 0x100-0x17F) ----
                case >= 0x100 and <= 0x17F:
                    {
                        if ((ins & 0b1111110000000000) == 0b0100000000000000) // Data Processing
                        {
                            uint opcode = (uint)((ins >> 6) & 0xFU);
                            uint rd = (uint)((ins >> 0) & 0b111);
                            uint rm = (uint)((ins >> 3) & 0b111);
                            uint rdVal = R[rd];
                            uint rmVal = R[rm];
                            switch (opcode)
                            {
                                case 0x0: // AND
                                    {
                                        uint final = rdVal & rmVal;
                                        R[rd] = final;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = final == 0;
                                    }
                                    break;
                                case 0x1: // EOR
                                    {
                                        rdVal = rdVal ^ rmVal;
                                        R[rd] = rdVal;
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0x2: // LSL (2)
                                    {
                                        uint rsValue = R[rm];
                                        if ((rsValue & 0xFF) == 0)
                                        {
                                        }
                                        else if ((rsValue & 0xFF) < 32)
                                        {
                                            arm7.Carry = BitTest(rdVal, (byte)(32 - (rsValue & 0xFF)));
                                            rdVal = LogicalShiftLeft32(rdVal, (byte)(rsValue & 0xFF));
                                        }
                                        else if ((rsValue & 0xFF) == 32)
                                        {
                                            arm7.Carry = BitTest(rdVal, 0);
                                            rdVal = 0;
                                        }
                                        else
                                        {
                                            arm7.Carry = false;
                                            rdVal = 0;
                                        }
                                        R[rd] = rdVal;
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0x3: // LSR (2)
                                    {
                                        uint rsVal = R[rm];
                                        if ((rsVal & 0xFF) == 0)
                                        {
                                        }
                                        else if ((rsVal & 0xFF) < 32)
                                        {
                                            arm7.Carry = BitTest(rdVal, (byte)((rsVal & 0xFF) - 1));
                                            R[rd] = LogicalShiftRight32(rdVal, (byte)(rsVal & 0xFF));
                                        }
                                        else if ((rsVal & 0xFF) == 32)
                                        {
                                            arm7.Carry = BitTest(rdVal, 31);
                                            R[rd] = 0;
                                        }
                                        else
                                        {
                                            arm7.Carry = false;
                                            R[rd] = 0;
                                        }
                                        rdVal = R[rd];
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0x4: // ASR (2)
                                    {
                                        uint rsVal = R[rm];
                                        if ((rsVal & 0xFF) == 0)
                                        {
                                        }
                                        else if ((rsVal & 0xFF) < 32)
                                        {
                                            arm7.Carry = BitTest(rdVal, (byte)((rsVal & 0xFF) - 1));
                                            rdVal = ArithmeticShiftRight32(rdVal, (byte)(rsVal & 0xFF));
                                        }
                                        else
                                        {
                                            arm7.Carry = BitTest(rdVal, 31);
                                            if (!arm7.Carry)
                                            {
                                                rdVal = 0;
                                            }
                                            else
                                            {
                                                rdVal = 0xFFFFFFFF;
                                            }
                                        }
                                        R[rd] = rdVal;
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0x5: // ADC
                                    {
                                        uint final = rdVal + rmVal + (arm7.Carry ? 1U : 0);
                                        R[rd] = final;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = rdVal == 0;
                                        arm7.Carry = (long)rdVal + (long)rmVal + (arm7.Carry ? 1U : 0) > 0xFFFFFFFF;
                                        arm7.Overflow = CheckOverflowAdd(rdVal, rmVal, final);
                                    }
                                    break;
                                case 0x6: // SBC
                                    {
                                        uint final = rdVal - rmVal - (!arm7.Carry ? 1U : 0);
                                        R[rd] = final;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = final == 0;
                                        arm7.Carry = !((long)rmVal + (!arm7.Carry ? 1U : 0) > rdVal);
                                        arm7.Overflow = CheckOverflowSub(rdVal, rmVal, final);
                                    }
                                    break;
                                case 0x7: // ROR
                                    {
                                        uint rsVal = R[rm];
                                        if ((rsVal & 0xFF) == 0)
                                        {
                                        }
                                        else if ((rsVal & 0b11111) == 0)
                                        {
                                            arm7.Carry = BitTest(rdVal, 31);
                                        }
                                        else
                                        {
                                            arm7.Carry = BitTest(rdVal, (byte)((rsVal & 0b11111) - 1));
                                            rdVal = RotateRight32(rdVal, (byte)(rsVal & 0b11111));
                                            R[rd] = rdVal;
                                        }
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0x8: // TST
                                    {
                                        uint final = rdVal & rmVal;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = final == 0;
                                    }
                                    break;
                                case 0x9: // NEG / RSB
                                    {
                                        uint final = 0 - rmVal;
                                        R[rd] = final;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = final == 0;
                                        arm7.Carry = !(rmVal > 0);
                                        arm7.Overflow = CheckOverflowSub(0, rmVal, final);
                                    }
                                    break;
                                case 0xA: // CMP (2)
                                    {
                                        uint alu_out = rdVal - rmVal;
                                        arm7.Negative = BitTest(alu_out, 31);
                                        arm7.Zero = alu_out == 0;
                                        arm7.Carry = !(rmVal > rdVal);
                                        arm7.Overflow = CheckOverflowSub(rdVal, rmVal, alu_out);
                                    }
                                    break;
                                case 0xB: // CMN
                                    {
                                        uint alu_out = rdVal + rmVal;
                                        arm7.Negative = BitTest(alu_out, 31);
                                        arm7.Zero = alu_out == 0;
                                        arm7.Carry = (long)rmVal + (long)rdVal > 0xFFFFFFFF;
                                        arm7.Overflow = CheckOverflowAdd(rdVal, rmVal, alu_out);
                                    }
                                    break;
                                case 0xC: // ORR
                                    {
                                        R[rd] = R[rd] | R[rm];
                                        arm7.Negative = BitTest(R[rd], 31);
                                        arm7.Zero = R[rd] == 0;
                                    }
                                    break;
                                case 0xD: // MUL
                                    {
                                        rdVal = (rmVal * rdVal);
                                        R[rd] = rdVal;
                                        arm7.Negative = BitTest(rdVal, 31);
                                        arm7.Zero = rdVal == 0;
                                    }
                                    break;
                                case 0xE: // BIC
                                    {
                                        uint final = rdVal & (~rmVal);
                                        R[rd] = final;
                                        arm7.Negative = BitTest(final, 31);
                                        arm7.Zero = final == 0;
                                    }
                                    break;
                                case 0xF: // MVN
                                    {
                                        R[rd] = ~R[rm];
                                        arm7.Negative = BitTest(R[rd], 31);
                                        arm7.Zero = R[rd] == 0;
                                    }
                                    break;
                            }
                        }
                        else if ((ins & 0b1111110000000000) == 0b0100010000000000) // Special Data Processing / Branch-exchange
                        {
                            switch ((ins >> 8) & 0b11)
                            {
                                case 0b00: // ADD (4)
                                    {
                                        uint rd = (uint)((ins >> 0) & 0b111) | ((ins & BIT_7) >> 4);
                                        uint rm = (uint)((ins >> 3) & 0b111) | ((ins & BIT_6) >> 3);
                                        uint rdVal = R[rd];
                                        uint rmVal = R[rm];
                                        uint final = rdVal + rmVal;
                                        R[rd] = final;
                                        if (rd == 15)
                                        {
                                            FlushPipeline();
                                        }
                                    }
                                    break;
                                case 0b01: // CMP (3)
                                    {
                                        uint rn = (uint)((ins >> 0) & 0b111) | ((ins & BIT_7) >> 4);
                                        uint rm = (uint)((ins >> 3) & 0b111) | ((ins & BIT_6) >> 3);
                                        uint rnVal = R[rn];
                                        uint rmVal = R[rm];
                                        uint alu_out = rnVal - rmVal;
                                        arm7.Negative = BitTest(alu_out, 31);
                                        arm7.Zero = alu_out == 0;
                                        arm7.Carry = !(rmVal > rnVal);
                                        arm7.Overflow = CheckOverflowSub(rnVal, rmVal, alu_out);
                                    }
                                    break;
                                case 0b10: // MOV (3)
                                    {
                                        uint rd = (uint)((ins >> 0) & 0b111) | ((ins & BIT_7) >> 4);
                                        uint rm = (uint)((ins >> 3) & 0b111) | ((ins & BIT_6) >> 3);
                                        R[rd] = R[rm];
                                        if (rd == 15)
                                        {
                                            R[15] &= 0xFFFFFFFE;
                                            FlushPipeline();
                                        }
                                    }
                                    break;
                                case 0b11: // BX
                                    {
                                        uint rm = (uint)((ins >> 3) & 0xF);
                                        uint val = R[rm];
                                        if (BitTest(ins, 7))
                                        {
                                            R[14] = (R[15] - 2) | 1;
                                        }
                                        arm7.ThumbState = BitTest(val, 0);
                                        R[15] = val & 0xFFFFFFFE;
                                        FlushPipeline();
                                        if (!arm7.ThumbState)
                                        {
                                            arm7.StateChange();
                                        }
                                    }
                                    break;
                            }
                        }
                        else if ((ins & 0b1111100000000000) == 0b0100100000000000) // LDR (3) - Load from literal pool
                        {
                            uint rd = (uint)((ins >> 8) & 0b111);
                            uint immed8 = (uint)((ins >> 0) & 0xFF);
                            uint addr = (R[15] & 0xFFFFFFFC) + (immed8 * 4);
                            uint readAddr = addr & ~0b11U;
                            uint readVal = Read32(readAddr);
                            R[rd] = RotateRight32(readVal, (byte)((addr & 0b11) * 8));
                        }
                        else if ((ins & 0b1111000000000000) == 0b0101000000000000) // Load/store register offset
                        {
                            uint rd = (uint)((ins >> 0) & 0b111);
                            uint rn = (uint)((ins >> 3) & 0b111);
                            uint rm = (uint)((ins >> 6) & 0b111);
                            uint rnVal = R[rn];
                            uint rmVal = R[rm];
                            uint addr = rnVal + rmVal;
                            switch ((ins >> 9) & 0b111)
                            {
                                case 0b000: // STR (2)
                                    Write32(addr & ~0b11U, R[rd]);
                                    break;
                                case 0b001: // STRH (2)
                                    Write16(addr & ~1u, (ushort)R[rd]);
                                    break;
                                case 0b010: // STRB (2)
                                    Write8(addr, (byte)R[rd]);
                                    break;
                                case 0b011: // LDRSB
                                    {
                                        int readVal = (sbyte)Read8(addr);
                                        R[rd] = (uint)readVal;
                                        ICycle();
                                    }
                                    break;
                                case 0b100: // LDR (2)
                                    {
                                        uint readAddr = addr & ~0b11U;
                                        uint readVal = Read32(readAddr);
                                        R[rd] = RotateRight32(readVal, (byte)((addr & 0b11) * 8));
                                        ICycle();
                                    }
                                    break;
                                case 0b101: // LDRH (2)
                                    {
                                        R[rd] = RotateRight32(Read16(addr & ~1u), (byte)(8 * (addr & 1)));
                                        ICycle();
                                    }
                                    break;
                                case 0b110: // LDRB (2)
                                    {
                                        R[rd] = Read8(addr);
                                        ICycle();
                                    }
                                    break;
                                case 0b111: // LDRSH
                                    {
                                        int readVal;
                                        if ((addr & 1) != 0)
                                        {
                                            readVal = (sbyte)Read8(addr);
                                        }
                                        else
                                        {
                                            readVal = (short)Read16(addr);
                                        }
                                        R[rd] = (uint)readVal;
                                        ICycle();
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                // ---- top 0b011 (decodeBits 0x180-0x1FF) ----
                case >= 0x180 and <= 0x1FF:
                    {
                        switch ((ins >> 11) & 0b11)
                        {
                            case 0b01: // LDR (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rnValue = R[(uint)((ins >> 3) & 0b111)];
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint addr = rnValue + (immed5 * 4);
                                    uint readAddr = addr & ~0b11U;
                                    uint readVal = Read32(readAddr);
                                    R[rd] = RotateRight32(readVal, (byte)((addr & 0b11) * 8));
                                    ICycle();
                                }
                                break;
                            case 0b00: // STR (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rnValue = R[(uint)((ins >> 3) & 0b111)];
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint addr = rnValue + (immed5 * 4);
                                    Write32(addr & ~3U, R[rd]);
                                }
                                break;
                            case 0b10: // STRB (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rdVal = R[rd];
                                    uint rn = (uint)((ins >> 3) & 0b111);
                                    uint rnVal = R[rn];
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint addr = rnVal + immed5;
                                    Write8(addr, (byte)rdVal);
                                }
                                break;
                            case 0b11: // LDRB (1)
                                {
                                    uint rd = (uint)((ins >> 0) & 0b111);
                                    uint rn = (uint)((ins >> 3) & 0b111);
                                    uint rnVal = R[rn];
                                    uint immed5 = (uint)((ins >> 6) & 0b11111);
                                    uint addr = rnVal + immed5;
                                    R[rd] = Read8(addr);
                                    ICycle();
                                }
                                break;
                        }
                    }
                    break;

                // ---- top 0b100 (decodeBits 0x200-0x27F) ----
                case >= 0x200 and <= 0x27F:
                    {
                        if ((ins & 0b1111000000000000) == 0b1000000000000000) // STRH (1) / LDRH (1)
                        {
                            bool load = BitTest(ins, 11);
                            uint rd = (uint)((ins >> 0) & 0b111);
                            uint rn = (uint)((ins >> 3) & 0b111);
                            uint rnVal = R[rn];
                            uint immed5 = (uint)((ins >> 6) & 0b11111);
                            uint addr = rnVal + (immed5 * 2);
                            if (load)
                            {
                                R[rd] = RotateRight32(Read16(addr & ~1u), (byte)(8 * (addr & 1)));
                                ICycle();
                            }
                            else
                            {
                                Write16(addr & ~1u, (ushort)R[rd]);
                            }
                        }
                        else if ((ins & 0b1111100000000000) == 0b1001100000000000) // LDR (4) - Load from stack
                        {
                            uint immed8 = (uint)((ins >> 0) & 0xFF);
                            uint rd = (uint)((ins >> 8) & 0b111);
                            uint addr = R[13] + (immed8 * 4);
                            uint readAddr = addr & ~0b11U;
                            uint readVal = Read32(readAddr);
                            R[rd] = RotateRight32(readVal, (byte)((addr & 0b11) * 8));
                            ICycle();
                        }
                        else if ((ins & 0b1111100000000000) == 0b1001000000000000) // STR (3) - Store to stack
                        {
                            uint immed8 = (uint)((ins >> 0) & 0xFF);
                            uint rd = (uint)((ins >> 8) & 0b111);
                            uint addr = R[13] + (immed8 * 4);
                            Write32(addr & ~3U, R[rd]);
                            ICycle();
                        }
                    }
                    break;

                // ---- top 0b101 (decodeBits 0x280-0x2FF) ----
                case >= 0x280 and <= 0x2FF:
                    {
                        if ((ins & 0b1111000000000000) == 0b1011000000000000) // Miscellaneous
                        {
                            if ((ins & 0b1111011000000000) == 0b1011010000000000) // POP & PUSH
                            {
                                if (BitTest(ins, 11))
                                {
                                    // POP
                                    uint addr = R[13];
                                    uint registerCount = popcount_8(ins & 0x1FFU);
                                    R[13] = addr + registerCount * 4;
                                    if (BitTest(ins, 0)) { R[0] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 1)) { R[1] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 2)) { R[2] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 3)) { R[3] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 4)) { R[4] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 5)) { R[5] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 6)) { R[6] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 7)) { R[7] = Read32(addr & ~3u); addr += 4; }
                                    if (BitTest(ins, 8))
                                    {
                                        R[15] = Read32(addr);
                                        // GBA: armv5=false, 不做 ThumbState 切换
                                        FlushPipeline();
                                        addr += 4;
                                    }
                                    if ((ins & 0x1FF) == 0)
                                    {
                                        // GBA: armv5=false
                                        R[15] = Read32(addr & ~3u);
                                        FlushPipeline();
                                        R[13] += 0x40;
                                    }
                                    ICycle();
                                }
                                else
                                {
                                    // PUSH
                                    uint addr = R[13];
                                    addr -= 4 * popcount_8((uint)ins & 0x1FF);
                                    if (BitTest(ins, 0)) { Write32(addr & ~3u, R[0]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 1)) { Write32(addr & ~3u, R[1]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 2)) { Write32(addr & ~3u, R[2]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 3)) { Write32(addr & ~3u, R[3]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 4)) { Write32(addr & ~3u, R[4]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 5)) { Write32(addr & ~3u, R[5]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 6)) { Write32(addr & ~3u, R[6]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 7)) { Write32(addr & ~3u, R[7]); addr += 4; R[13] -= 4; }
                                    if (BitTest(ins, 8))
                                    {
                                        Write32(addr, R[14]);
                                        addr += 4;
                                        R[13] -= 4;
                                    }
                                    if ((ins & 0x1FF) == 0)
                                    {
                                        // GBA: armv5=false
                                        Write32(addr & ~3u, R[15]);
                                        R[13] += 0x40;
                                    }
                                }
                            }
                            else if ((ins & 0b1111111110000000) == 0b1011000000000000) // ADD (7)
                            {
                                uint immed7 = (uint)(ins & 0b1111111);
                                R[13] = R[13] + (immed7 << 2);
                            }
                            else if ((ins & 0b1111111110000000) == 0b1011000010000000) // SUB (4)
                            {
                                uint immed7 = (uint)(ins & 0b1111111);
                                R[13] = R[13] - (immed7 << 2);
                            }
                            else if ((ins & 0b1111111111000000) == 0b1011101011000000) // REVSH
                            {
                                uint rd = (uint)((ins >> 0) & 0b111);
                                uint rdVal = R[rd];
                                uint rn = (uint)((ins >> 3) & 0b111);
                                uint rnVal = R[rn];
                                uint rnValHalfLower = ((rnVal >> 0) & 0xFFFF);
                                uint rnValHalfUpper = ((rnVal >> 8) & 0xFFFF);
                                rdVal &= 0xFFFF0000;
                                rdVal |= (rnValHalfUpper << 0);
                                rdVal |= (rnValHalfLower << 8);
                                if (BitTest(rn, 7))
                                {
                                    rdVal |= 0xFFFF0000;
                                }
                                else
                                {
                                    rdVal &= 0x0000FFFF;
                                }
                                R[rd] = rdVal;
                            }
                        }
                        else if ((ins & 0b1111100000000000) == 0b1010000000000000) // ADD (5) - Add to PC
                        {
                            uint immed8 = (uint)(ins & 0xFF);
                            uint rd = (uint)((ins >> 8) & 0b111);
                            R[rd] = (R[15] & 0xFFFFFFFC) + (immed8 * 4);
                        }
                        else if ((ins & 0b1111100000000000) == 0b1010100000000000) // ADD (6) - Add to SP
                        {
                            uint immed8 = (uint)(ins & 0xFF);
                            uint rd = (uint)((ins >> 8) & 0b111);
                            R[rd] = R[13] + (immed8 << 2);
                        }
                    }
                    break;

                // ---- top 0b110 (decodeBits 0x300-0x37F) ----
                case >= 0x300 and <= 0x37F:
                    {
                        if ((ins & 0b1111000000000000) == 0b1100000000000000) // LDMIA, STMIA
                        {
                            if (BitTest(ins, 11))
                            {
                                // LDMIA
                                uint rn = (uint)((ins >> 8) & 0b111);
                                uint addr = R[rn];
                                uint registerList = ins & 0xFFU;
                                uint registerCount = popcount_8(registerList);
                                uint writebackVal = R[rn] + registerCount * 4;
                                R[rn] = writebackVal;
                                uint register = 0;
                                for (; registerList != 0; registerList >>= 1)
                                {
                                    if (BitTest(registerList, 0))
                                    {
                                        R[register] = Read32(addr & ~3u);
                                        addr += 4;
                                    }
                                    register++;
                                }
                                if ((ins & 0xFF) == 0)
                                {
                                    // GBA: armv5=false
                                    R[15] = Read32(addr & ~3u);
                                    FlushPipeline();
                                    R[rn] += 0x40;
                                }
                                ICycle();
                            }
                            else
                            {
                                // STMIA
                                uint rn = (uint)((ins >> 8) & 0b111);
                                uint addr = R[rn];
                                R[15] += 2;
                                uint registerList = ins & 0xFFU;
                                uint registerCount = popcount_8(registerList);
                                uint writebackVal = R[rn] + registerCount * 4;
                                uint register = 0;
                                for (; registerList != 0; registerList >>= 1)
                                {
                                    if (BitTest(registerList, 0))
                                    {
                                        Write32(addr & ~3u, R[register]);
                                        addr += 4;
                                        // 原版：if (!Armv5) R[rn] = writebackVal —— GBA(armv5=false) 每次写入后回写基址
                                        R[rn] = writebackVal;
                                    }
                                    register++;
                                }
                                if ((ins & 0xFF) == 0)
                                {
                                    // GBA: armv5=false
                                    Write32(addr & ~3u, R[15]);
                                    R[rn] += 0x40;
                                }
                                R[15] -= 2;
                            }
                        }
                        else if ((ins & 0b1111111100000000) == 0b1101111100000000) // SWI
                        {
                            arm7.SPSR_svc = GetCPSR();
                            SetMode(Arm7Mode.SVC);
                            R[14] = R[15] - 2;
                            arm7.ThumbState = false;
                            arm7.IRQDisable = true;
                            R[15] = arm7.VectorSoftwareInterrupt;
                            FlushPipeline();
                            arm7.StateChange();
                        }
                        else if ((ins & 0b1111000000000000) == 0b1101000000000000) // B (1) - Conditional
                        {
                            uint cond = (uint)((ins >> 8) & 0xF);
                            bool condition = CheckCondition(cond);
                            if (condition)
                            {
                                int offset = (int)(ins & 0xFF) << 1;
                                offset = (offset << 23) >> 23;
                                R[15] = (uint)(R[15] + offset);
                                FlushPipeline();
                            }
                        }
                    }
                    break;

                // ---- top 0b111 (decodeBits 0x380-0x3FF) ----
                case >= 0x380 and <= 0x3FF:
                    {
                        if ((ins & 0b1111100000000000) == 0b1110000000000000) // B (2) - Unconditional
                        {
                            int signedImmed11 = (int)(ins & 0b11111111111) << 1;
                            signedImmed11 = (signedImmed11 << 20) >> 20;
                            R[15] = (uint)(R[15] + signedImmed11);
                            FlushPipeline();
                        }
                        else if ((ins & 0b1110000000000000) == 0b1110000000000000) // BL, BLX
                        {
                            uint H = (uint)((ins >> 11) & 0b11);
                            switch (H)
                            {
                                case 0b10: // BLUpperFill
                                    {
                                        int offset11 = ins & 0b11111111111;
                                        offset11 <<= 12;
                                        offset11 = ((int)offset11 << 9) >> 9;
                                        R[14] = (uint)(R[15] + offset11);
                                    }
                                    break;
                                case 0b11: // BLToThumb
                                    {
                                        int offset11 = ins & 0b11111111111;
                                        uint oldR14 = R[14];
                                        R[14] = (R[15] - 2) | 1;
                                        R[15] = (uint)(oldR14 + (offset11 << 1));
                                        R[15] &= 0xFFFFFFFE;
                                        FlushPipeline();
                                    }
                                    break;
                                case 0b01: // BLToArm
                                    {
                                        int offset11 = ins & 0b11111111111;
                                        uint oldR14 = R[14];
                                        R[14] = (R[15] - 2) | 1;
                                        R[15] = (uint)((oldR14 + (offset11 << 1)) & ~3);
                                        arm7.ThumbState = false;
                                        FlushPipeline();
                                        arm7.StateChange();
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }

            if (arm7.ThumbState)
            {
                R[15] += 2;
            }
            else
            {
                R[15] += 4;
            }
        }

        // ============ ARM 辅助：DataDecode / LDRSTR / LDMSTM ============

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _DataDecode(uint ins, bool useImmediate32, out uint shifterOperand, out bool shifterCarryOut, out uint rnVal, out uint rd)
        {
            rd = (ins >> 12) & 0xF;
            shifterOperand = 0;
            shifterCarryOut = false;

            if (useImmediate32)
            {
                uint rn = (ins >> 16) & 0xF;
                rnVal = R[rn];
                uint rotateBits = ((ins >> 8) & 0xF) * 2;
                uint constant = ins & 0xFF;
                shifterOperand = (constant >> (byte)rotateBits) | (constant << -(byte)rotateBits);
                if (rotateBits == 0)
                {
                    shifterCarryOut = arm7.Carry;
                }
                else
                {
                    shifterCarryOut = (shifterOperand & (1 << 31)) != 0;
                }
                return;
            }
            else
            {
                bool regShift = (ins & BIT_4) != 0;
                byte shiftBits;
                uint shiftType = (ins >> 5) & 0b11;

                if (!regShift)
                {
                    shiftBits = (byte)((ins >> 7) & 0b11111);
                    uint rn = (ins >> 16) & 0xF;
                    uint rm = ins & 0xF;
                    rnVal = R[rn];
                    uint rmVal = R[rm];

                    switch (shiftType)
                    {
                        case 0b00: // LSL
                            if (shiftBits == 0)
                            {
                                shifterOperand = rmVal;
                                shifterCarryOut = arm7.Carry;
                            }
                            else
                            {
                                shifterOperand = rmVal << shiftBits;
                                shifterCarryOut = (rmVal & (1 << (32 - shiftBits))) != 0;
                            }
                            break;
                        case 0b01: // LSR
                            if (shiftBits == 0)
                            {
                                shifterOperand = 0;
                                shifterCarryOut = (rmVal & (1 << 31)) != 0;
                            }
                            else
                            {
                                shifterOperand = rmVal >> shiftBits;
                                shifterCarryOut = (rmVal & (1 << (shiftBits - 1))) != 0;
                            }
                            break;
                        case 0b10: // ASR
                            if (shiftBits == 0)
                            {
                                shifterOperand = (uint)((int)rmVal >> 31);
                                shifterCarryOut = (rmVal & (1 << 31)) != 0;
                            }
                            else
                            {
                                shifterOperand = (uint)((int)rmVal >> shiftBits);
                                shifterCarryOut = (rmVal & (1 << (shiftBits - 1))) != 0;
                            }
                            break;
                        case 0b11: // ROR
                            if (shiftBits == 0)
                            {
                                shifterOperand = (arm7.Carry ? 1U : 0) << (31) | rmVal >> 1;
                                shifterCarryOut = (rmVal & (1 << 0)) != 0;
                            }
                            else
                            {
                                shifterOperand = (rmVal >> shiftBits) | (rmVal << -shiftBits);
                                shifterCarryOut = (rmVal & (1 << (shiftBits - 1))) != 0;
                            }
                            break;
                    }
                    return;
                }
                else
                {
                    // Register shift
                    uint rn = (ins >> 16) & 0xF;
                    uint rs = (ins >> 8) & 0xF;
                    uint rm = ins & 0xF;
                    arm7.InstructionCycles += 1;
                    R[15] += 4;
                    rnVal = R[rn];
                    uint rsVal = R[rs];
                    uint rmVal = R[rm];
                    R[15] -= 4;
                    shiftBits = (byte)rsVal;

                    switch (shiftType)
                    {
                        case 0b00:
                            if (shiftBits == 0)
                            {
                                shifterOperand = rmVal;
                                shifterCarryOut = arm7.Carry;
                                break;
                            }
                            if (shiftBits >= 32)
                            {
                                if (shiftBits > 32)
                                {
                                    shifterCarryOut = false;
                                }
                                else
                                {
                                    shifterCarryOut = (rmVal & (1 << 0)) != 0;
                                }
                                shifterOperand = 0;
                                break;
                            }
                            shifterOperand = rmVal << shiftBits;
                            shifterCarryOut = (rmVal & (1 << (byte)(32 - shiftBits))) != 0;
                            break;
                        case 0b01:
                            if (shiftBits == 0)
                            {
                                shifterOperand = rmVal;
                                shifterCarryOut = arm7.Carry;
                            }
                            else if (shiftBits < 32)
                            {
                                shifterOperand = rmVal >> shiftBits;
                                shifterCarryOut = (rmVal & (1 << (byte)(shiftBits - 1))) != 0;
                            }
                            else if (shiftBits == 32)
                            {
                                shifterOperand = 0;
                                shifterCarryOut = (rmVal & (1 << 31)) != 0;
                            }
                            else
                            {
                                shifterOperand = 0;
                                shifterCarryOut = false;
                            }
                            break;
                        case 0b10:
                            if (shiftBits == 0)
                            {
                                shifterOperand = rmVal;
                                shifterCarryOut = arm7.Carry;
                            }
                            else if (shiftBits < 32)
                            {
                                shifterOperand = (uint)((int)rmVal >> shiftBits);
                                shifterCarryOut = (rmVal & (1 << (byte)(shiftBits - 1))) != 0;
                            }
                            else if (shiftBits >= 32)
                            {
                                shifterOperand = (uint)((int)rmVal >> 31);
                                shifterCarryOut = (rmVal & (1 << 31)) != 0;
                            }
                            break;
                        case 0b11:
                            if (shiftBits == 0)
                            {
                                shifterOperand = rmVal;
                                shifterCarryOut = arm7.Carry;
                            }
                            else
                            {
                                shifterOperand = (rmVal >> (byte)(shiftBits & 0b11111)) | (rmVal << -(byte)(shiftBits & 0b11111));
                                shifterCarryOut = (rmVal & (1 << (byte)((shiftBits & 0b11111) - 1))) != 0;
                            }
                            break;
                    }
                    return;
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _DataOp(uint ins, uint opcode, bool useImmediate32, bool setFlags)
        {
            _DataDecode(ins, useImmediate32, out uint shifterOperand, out bool shifterCarryOut, out uint rnValue, out uint rd);

            switch (opcode)
            {
                case 0x0: // AND
                    {
                        uint final = rnValue & shifterOperand;
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x1: // EOR
                    {
                        uint final = rnValue ^ shifterOperand;
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x2: // SUB
                    {
                        uint aluOut = rnValue - shifterOperand;
                        R[rd] = aluOut;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(aluOut, 31);
                            arm7.Zero = aluOut == 0;
                            arm7.Carry = shifterOperand <= rnValue;
                            arm7.Overflow = CheckOverflowSub(rnValue, shifterOperand, aluOut);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x3: // RSB
                    {
                        uint aluOut = shifterOperand - rnValue;
                        R[rd] = aluOut;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(aluOut, 31);
                            arm7.Zero = aluOut == 0;
                            arm7.Carry = rnValue <= shifterOperand;
                            arm7.Overflow = CheckOverflowSub(shifterOperand, rnValue, aluOut);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x4: // ADD
                    {
                        uint final = rnValue + shifterOperand;
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = (long)rnValue + (long)shifterOperand > 0xFFFFFFFFL;
                            arm7.Overflow = CheckOverflowAdd(rnValue, shifterOperand, final);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x5: // ADC
                    {
                        uint final = rnValue + shifterOperand + (arm7.Carry ? 1U : 0);
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = (long)rnValue + (long)shifterOperand + (arm7.Carry ? 1U : 0) > 0xFFFFFFFFL;
                            arm7.Overflow = CheckOverflowAdd(rnValue, shifterOperand, final);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x6: // SBC
                    {
                        uint aluOut = rnValue - shifterOperand - (!arm7.Carry ? 1U : 0U);
                        R[rd] = aluOut;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(aluOut, 31);
                            arm7.Zero = aluOut == 0;
                            arm7.Carry = (long)shifterOperand + (arm7.Carry ? 0 : 1L) <= rnValue;
                            arm7.Overflow = CheckOverflowSub(rnValue, shifterOperand, aluOut);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x7: // RSC
                    {
                        uint aluOut = shifterOperand - rnValue - (!arm7.Carry ? 1U : 0U);
                        R[rd] = aluOut;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(aluOut, 31);
                            arm7.Zero = aluOut == 0;
                            arm7.Carry = (long)rnValue + (long)(!arm7.Carry ? 1U : 0) <= shifterOperand;
                            arm7.Overflow = CheckOverflowSub(shifterOperand, rnValue, aluOut);
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0x8: // TST
                    {
                        uint final = rnValue & shifterOperand;
                        arm7.Negative = BitTest(final, 31);
                        arm7.Zero = final == 0;
                        arm7.Carry = shifterCarryOut;
                    }
                    break;
                case 0x9: // TEQ
                    {
                        uint aluOut = rnValue ^ shifterOperand;
                        arm7.Negative = BitTest(aluOut, 31);
                        arm7.Zero = aluOut == 0;
                        arm7.Carry = shifterCarryOut;
                    }
                    break;
                case 0xA: // CMP
                    {
                        uint aluOut = rnValue - shifterOperand;
                        arm7.Negative = BitTest(aluOut, 31);
                        arm7.Zero = aluOut == 0;
                        arm7.Carry = rnValue >= shifterOperand;
                        arm7.Overflow = CheckOverflowSub(rnValue, shifterOperand, aluOut);
                    }
                    break;
                case 0xB: // CMN
                    {
                        uint aluOut = rnValue + shifterOperand;
                        arm7.Negative = BitTest(aluOut, 31);
                        arm7.Zero = aluOut == 0;
                        arm7.Carry = (long)rnValue + (long)shifterOperand > 0xFFFFFFFF;
                        arm7.Overflow = CheckOverflowAdd(rnValue, shifterOperand, aluOut);
                    }
                    break;
                case 0xC: // ORR
                    {
                        uint final = rnValue | shifterOperand;
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0xD: // MOV
                    {
                        R[rd] = shifterOperand;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(shifterOperand, 31);
                            arm7.Zero = shifterOperand == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0xE: // BIC
                    {
                        uint final = rnValue & ~shifterOperand;
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
                case 0xF: // MVN
                    {
                        R[rd] = ~shifterOperand;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(~shifterOperand, 31);
                            arm7.Zero = ~shifterOperand == 0;
                            arm7.Carry = shifterCarryOut;
                            if (rd == 15)
                            {
                                SetCPSR(GetSPSR());
                                FlushPipeline();
                            }
                        }
                        else
                        {
                            if (rd == 15) FlushPipeline();
                        }
                    }
                    break;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _RegularLDRSTR(uint ins, bool L, bool useRegister)
        {
            uint rn = (ins >> 16) & 0xF;
            uint rd = (ins >> 12) & 0xF;
            uint rnValue = R[rn];
            bool P = BitTest(ins, 24);
            bool U = BitTest(ins, 23);
            bool B = BitTest(ins, 22);
            bool W = BitTest(ins, 21);
            uint offset = 0;

            if (useRegister)
            {
                uint rmVal = R[ins & 0xF];
                if ((ins & 0b111111110000) == 0b000000000000)
                {
                    offset = rmVal;
                }
                else
                {
                    uint shiftType = (ins >> 5) & 0b11;
                    byte shiftBits = (byte)((ins >> 7) & 0b11111);
                    switch (shiftType)
                    {
                        case 0b00:
                            offset = LogicalShiftLeft32(rmVal, shiftBits);
                            break;
                        case 0b01:
                            if (shiftBits == 0)
                            {
                                offset = 0;
                            }
                            else
                            {
                                offset = LogicalShiftRight32(rmVal, shiftBits);
                            }
                            break;
                        case 0b10:
                            if (shiftBits == 0)
                            {
                                offset = (uint)((int)rmVal >> 31);
                            }
                            else
                            {
                                offset = ArithmeticShiftRight32(rmVal, shiftBits);
                            }
                            break;
                        default:
                        case 0b11:
                            if (shiftBits == 0)
                            {
                                offset = LogicalShiftLeft32(arm7.Carry ? 1U : 0, 31) | (LogicalShiftRight32(rmVal, 1));
                            }
                            else
                            {
                                offset = RotateRight32(rmVal, shiftBits);
                            }
                            break;
                    }
                }
            }
            else
            {
                offset = ins & 0b111111111111;
            }

            uint addr = rnValue;
            if (P)
            {
                if (U)
                {
                    addr += offset;
                }
                else
                {
                    addr -= offset;
                }
            }

            if (L)
            {
                uint loadVal = 0;
                if (B)
                {
                    loadVal = Read8(addr);
                }
                else
                {
                    if ((addr & 0b11) != 0)
                    {
                        uint data = Read32(addr & ~3U);
                        loadVal = RotateRight32(data, (byte)(8 * (addr & 0b11)));
                    }
                    else
                    {
                        loadVal = Read32(addr);
                    }
                }

                if (!P)
                {
                    if (U)
                    {
                        addr += offset;
                    }
                    else
                    {
                        addr -= offset;
                    }
                    R[rn] = addr;
                }
                else if (W)
                {
                    R[rn] = addr;
                }
                R[rd] = loadVal;

                if (rd == 15)
                {
                    // GBA armv5=false，不做 ThumbState 切换
                    FlushPipeline();
                }
                ICycle();
            }
            else
            {
                R[15] += 4;
                uint storeVal = R[rd];
                if (B)
                {
                    Write8(addr, (byte)storeVal);
                }
                else
                {
                    Write32(addr & 0xFFFFFFFC, storeVal);
                }
                R[15] -= 4;

                if (!P)
                {
                    if (U)
                    {
                        addr += offset;
                    }
                    else
                    {
                        addr -= offset;
                    }
                    R[rn] = addr;
                }
                else if (W)
                {
                    R[rn] = addr;
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _SpecialLDRSTR(uint ins, bool L, bool S, bool H)
        {
            bool W = BitTest(ins, 21);
            bool immediateOffset = BitTest(ins, 22);
            bool U = BitTest(ins, 23);
            bool P = BitTest(ins, 24);
            uint rd = (ins >> 12) & 0xF;
            uint rn = (ins >> 16) & 0xF;
            uint baseAddr = R[rn];

            uint offset;
            if (immediateOffset)
            {
                uint immed = (ins & 0xF) | ((ins >> 4) & 0xF0);
                offset = immed;
            }
            else
            {
                uint rm = ins & 0xF;
                offset = R[rm];
            }

            uint addr = baseAddr;
            if (P)
            {
                if (U)
                {
                    addr += offset;
                }
                else
                {
                    addr -= offset;
                }
            }

            uint loadVal = 0;
            if (L)
            {
                if (S)
                {
                    if (H)
                    {
                        // LDRSH
                        int readVal;
                        if ((addr & 1) != 0)
                        {
                            readVal = (sbyte)Read8(addr);
                        }
                        else
                        {
                            readVal = (short)Read16(addr);
                        }
                        loadVal = (uint)readVal;
                    }
                    else
                    {
                        // LDRSB
                        int val = (sbyte)Read8(addr);
                        loadVal = (uint)val;
                    }
                }
                else
                {
                    if (H)
                    {
                        // LDRH
                        loadVal = RotateRight32(Read16(addr & ~1u), (byte)((addr & 1) * 8));
                    }
                }
            }
            else
            {
                if (S)
                {
                    // GBA armv5=false：STRD/LDRD 不可用
                    return;
                }
                else
                {
                    if (H)
                    {
                        // STRH
                        Write16(addr & ~1u, (ushort)R[rd]);
                    }
                }
            }

            if (!P)
            {
                if (U)
                {
                    addr = baseAddr + offset;
                }
                else
                {
                    addr = baseAddr - offset;
                }
            }

            if (W || !P)
            {
                R[rn] = addr;
            }

            if (L)
            {
                R[rd] = loadVal;
                ICycle();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void _LDMSTM(uint ins, bool L)
        {
            bool P = BitTest(ins, 24);
            bool U = BitTest(ins, 23);
            bool S = BitTest(ins, 22);
            bool W = BitTest(ins, 21);
            bool loadsPc = BitTest(ins, 15);

            uint oldMode = 0;
            if (S && (!L || !loadsPc))
            {
                oldMode = (uint)arm7.Mode;
                SetMode(Arm7Mode.USR);
            }

            uint rn = (ins >> 16) & 0xF;
            uint addr = R[rn];
            uint bitsSet = popcount_8(ins & 0xFFFF);
            uint writebackValue;
            if (U)
            {
                if (W)
                {
                    writebackValue = addr + bitsSet * 4;
                }
                else
                {
                    writebackValue = addr;
                }
            }
            else
            {
                if (W)
                {
                    writebackValue = addr - bitsSet * 4;
                }
                else
                {
                    writebackValue = addr;
                }
                if (P)
                {
                    addr = addr - bitsSet * 4 - 4;
                }
                else
                {
                    addr = addr - bitsSet * 4 + 4;
                }
            }

            if (L)
            {
                if (W)
                {
                    R[rn] = writebackValue;
                }
            }
            else
            {
                R[15] += 4;
            }

            for (byte r = 0; r < 16; r++)
            {
                if (BitTest(ins, r))
                {
                    if (L)
                    {
                        if (P) addr += 4;
                        if (r != 15)
                        {
                            R[r] = Read32(addr & ~3U);
                        }
                        else
                        {
                            R[15] = Read32(addr & ~3U) & ~3U;
                            FlushPipeline();
                        }
                        if (!P) addr += 4;
                    }
                    else
                    {
                        if (P) addr += 4;
                        Write32(addr & ~3U, R[r]);
                        if (!P) addr += 4;
                        R[rn] = writebackValue;
                    }
                }
            }

            bool emptyRlist = (ins & 0xFFFF) == 0;
            if (emptyRlist)
            {
                if (L)
                {
                    R[15] = Read32(addr & ~3U);
                    FlushPipeline();
                    if (U)
                    {
                        R[rn] += 0x40;
                    }
                    else
                    {
                        R[rn] -= 0x40;
                    }
                }
                else
                {
                    if (P)
                    {
                        if (U)
                        {
                            Write32(R[rn] + 4, R[15]);
                            R[rn] += 0x40;
                        }
                        else
                        {
                            R[rn] -= 0x40;
                            Write32(R[rn], R[15]);
                        }
                    }
                    else
                    {
                        if (U)
                        {
                            Write32(R[rn], R[15]);
                            R[rn] += 0x40;
                        }
                        else
                        {
                            R[rn] -= 0x40;
                            Write32(R[rn] + 4, R[15]);
                        }
                    }
                }
            }

            if (!L)
            {
                R[15] -= 4;
            }

            if (S)
            {
                if (L && loadsPc)
                {
                    SetCPSR(GetSPSR());
                }
                else
                {
                    SetMode((Arm7Mode)oldMode);
                }
            }

            ICycle();
        }

        // ============ ARM ============
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ExecuteArmInlined()
        {
            uint ins = Read32InstrFetch(R[15] - 8);
            uint condition = (ins >> 28) & 0xF;

            if (CheckCondition(condition))
            {
                uint decodeBits = ((ins >> 16) & 0xFF0) | ((ins >> 4) & 0xF);

                // decodeBits 分类（armv5=false，由校验器确认无冲突）：
                //   0x0000-0x03FF  Data Processing（内部再分 BX/CLZ/SWP/MRS/MSR/MUL/MULL/HLF/ALU）
                //   0x0400-0x07FF  LDR / STR
                //   0x0800-0x09FF  LDM / STM
                //   0x0A00-0x0AFF  B
                //   0x0B00-0x0BFF  BL
                //   0x0C00-0x0EFF  Invalid
                //   0x0F00-0x0FFF  SWI
                switch (decodeBits)
                {
                    case >= 0x0F00 and <= 0x0FFF: // SWI
                        {
                            arm7.SPSR_svc = GetCPSR();
                            SetMode(Arm7Mode.SVC);
                            R[14] = R[15] - 4;
                            arm7.IRQDisable = true;
                            R[15] = arm7.VectorSoftwareInterrupt;
                            FlushPipeline();
                        }
                        break;

                    case >= 0x0B00 and <= 0x0BFF: // BL
                        {
                            int offset = (int)(ins & 0b111111111111111111111111) << 2;
                            offset = (offset << 6) >> 6;
                            R[14] = R[15] - 4;
                            R[15] = (uint)(R[15] + offset);
                            FlushPipeline();
                        }
                        break;

                    case >= 0x0A00 and <= 0x0AFF: // B
                        {
                            int offset = (int)(ins & 0b111111111111111111111111) << 2;
                            offset = (offset << 6) >> 6;
                            R[15] = (uint)(R[15] + offset);
                            FlushPipeline();
                        }
                        break;

                    case >= 0x0800 and <= 0x09FF: // LDM / STM
                        {
                            bool L = BitTest(ins, 20);
                            _LDMSTM(ins, L);
                        }
                        break;

                    case >= 0x0400 and <= 0x07FF: // LDR / STR
                        {
                            bool L = BitTest(ins, 20);
                            bool useRegister = BitTest(ins, 25);
                            _RegularLDRSTR(ins, L, useRegister);
                        }
                        break;

                    case >= 0x0000 and <= 0x03FF: // Data Processing（按 GetInstructionArm 顺序子分类）
                        {
                            ExecuteArmDP(ins);
                        }
                        break;

                    default:
                        {
                            arm7.Error($"Invalid ARM Instruction: {Hex(ins, 8)}");
                        }
                        break;
                }
            }

            // 与原 ExecuteArm 尾部一致：按 ThumbState 条件推进（BX 切到 Thumb 时 +2）
            if (arm7.ThumbState)
            {
                R[15] += 2;
            }
            else
            {
                R[15] += 4;
            }
        }

        // Data Processing 内部子分类：BX / CLZ / SWP / MRS / MSR / MUL / MULL / HLF / 纯 ALU
        // 与原 GetInstructionArm 顺序一致；armv5=false 下 QADD/COP 落入纯 ALU（与原版行为等价）
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ExecuteArmDP(uint ins)
        {
            uint decodeBits = ((ins >> 16) & 0xFF0) | ((ins >> 4) & 0xF);

            switch (decodeBits)
            {
                case 0x0121:
                case 0x0123: // BX
                    {
                        uint rm = ins & 0xF;
                        uint rmValue = R[rm];
                        arm7.ThumbState = BitTest(rmValue, 0);
                        if (arm7.ThumbState)
                        {
                            arm7.StateChange();
                        }
                        // BLX register
                        if (((ins >> 4) & 0xF) == 0b0011)
                        {
                            R[14] = R[15] - 4;
                        }
                        R[15] = rmValue & ~1U;
                        FlushPipeline();
                    }
                    break;

                case 0x0161: // CLZ（armv5 无 gate，GBA 可用）
                    {
                        uint rd = (ins >> 12) & 0xF;
                        uint rm = ins & 0xF;
                        uint rmVal = R[rm];
                        if (rmVal == 0)
                        {
                            R[rd] = 32;
                        }
                        else
                        {
                            R[rd] = 32 - popcount_8(rmVal);
                        }
                    }
                    break;

                case 0x0109:
                case 0x0149: // SWP / SWPB（armv5 无 gate，GBA 可用）
                    {
                        bool useByte = BitTest(ins, 22);
                        uint rm = ins & 0xF;
                        uint rd = (ins >> 12) & 0xF;
                        uint rn = (ins >> 16) & 0xF;
                        uint addr = R[rn];
                        uint storeValue = R[rm];
                        if (useByte)
                        {
                            R[rd] = Read8(addr);
                            Write8(addr, (byte)storeValue);
                        }
                        else
                        {
                            R[rd] = RotateRight32(Read32(addr & ~3u), (byte)((addr & 3u) * 8));
                            Write32(addr & ~3u, storeValue);
                        }
                        ICycle();
                    }
                    break;

                case 0x0100:
                case 0x0140: // MRS
                    {
                        bool useSPSR = BitTest(ins, 22);
                        uint rd = (ins >> 12) & 0xF;
                        R[rd] = useSPSR ? GetSPSR() : GetCPSR();
                    }
                    break;

                case 0x0120:
                case 0x0160:
                case >= 0x0320 and <= 0x032F:
                case >= 0x0360 and <= 0x036F: // MSR（寄存器 + 立即数）
                    {
                        bool useSPSR = BitTest(ins, 22);
                        bool setControl = BitTest(ins, 16);
                        bool setExtension = BitTest(ins, 17);
                        bool setStatus = BitTest(ins, 18);
                        bool setFlags = BitTest(ins, 19);
                        uint operand;
                        if (BitTest(ins, 25))
                        {
                            uint rotateBits = ((ins >> 8) & 0xF) * 2;
                            operand = RotateRight32(ins & 0xFF, (byte)rotateBits);
                        }
                        else
                        {
                            operand = R[ins & 0xF];
                        }
                        uint byteMask =
                            (setControl ? 0x000000FFu : 0) |
                            (setExtension ? 0x0000FF00u : 0) |
                            (setStatus ? 0x00FF0000u : 0) |
                            (setFlags ? 0xFF000000u : 0);
                        if (!useSPSR)
                        {
                            if (arm7.Mode == Arm7Mode.USR)
                            {
                                byteMask &= 0xFF000000;
                            }
                            SetCPSR((GetCPSR() & ~byteMask) | (operand & byteMask));
                        }
                        else
                        {
                            SetSPSR((GetSPSR() & ~byteMask) | (operand & byteMask));
                        }
                    }
                    break;

                case 0x0009:
                case 0x0019:
                case 0x0029:
                case 0x0039: // MUL
                    {
                        uint rd = (ins >> 16) & 0xF;
                        uint rs = (ins >> 8) & 0xF;
                        uint rm = ins & 0xF;
                        uint rsValue = R[rs];
                        uint rmValue = R[rm];
                        bool setFlags = (ins & (1 << 20)) != 0;
                        uint final;
                        if ((ins & (1 << 21)) != 0)
                        {
                            uint rnValue = R[(ins >> 12) & 0xF];
                            final = (rsValue * rmValue) + rnValue;
                        }
                        else
                        {
                            final = rsValue * rmValue;
                        }
                        R[rd] = final;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest(final, 31);
                            arm7.Zero = final == 0;
                        }
                    }
                    break;

                case 0x0089:
                case 0x0099:
                case 0x00A9:
                case 0x00B9:
                case 0x00C9:
                case 0x00D9:
                case 0x00E9:
                case 0x00F9: // MULL
                    {
                        bool signed = BitTest(ins, 22);
                        bool accumulate = BitTest(ins, 21);
                        bool setFlags = BitTest(ins, 20);
                        uint rdHi = (ins >> 16) & 0xF;
                        uint rdLo = (ins >> 12) & 0xF;
                        uint rs = (ins >> 8) & 0xF;
                        uint rm = ins & 0xF;
                        ulong rsVal = R[rs];
                        ulong rmVal = R[rm];
                        ulong longLo;
                        ulong longHi;
                        if (accumulate)
                        {
                            if (signed)
                            {
                                long rmValExt = (long)(rmVal << 32) >> 32;
                                long rsValExt = (long)(rsVal << 32) >> 32;
                                longLo = (ulong)(((rsValExt * rmValExt) & 0xFFFFFFFF) + R[rdLo]);
                                longHi = (ulong)((rsValExt * rmValExt) >> 32) + R[rdHi] + (longLo > 0xFFFFFFFF ? 1U : 0);
                            }
                            else
                            {
                                longLo = ((rsVal * rmVal) & 0xFFFFFFFF) + R[rdLo];
                                longHi = ((rsVal * rmVal) >> 32) + R[rdHi] + (longLo > 0xFFFFFFFF ? 1U : 0);
                            }
                        }
                        else
                        {
                            if (signed)
                            {
                                long rmValExt = (long)(rmVal << 32) >> 32;
                                long rsValExt = (long)(rsVal << 32) >> 32;
                                longLo = (ulong)((rsValExt * rmValExt));
                                longHi = (ulong)((rsValExt * rmValExt) >> 32);
                            }
                            else
                            {
                                longLo = (rmVal * rsVal);
                                longHi = ((rmVal * rsVal) >> 32);
                            }
                        }
                        R[rdLo] = (uint)longLo;
                        R[rdHi] = (uint)longHi;
                        if (setFlags)
                        {
                            arm7.Negative = BitTest((uint)longHi, 31);
                            arm7.Zero = R[rdLo] == 0 && R[rdHi] == 0;
                        }
                    }
                    break;

                default:
                    {
                        // ARMv5 特有乘法（SMLALxy/SMULxy/SMLAxy）：GBA(armv5=false) 上不存在，原版返回 Invalid
                        if ((ins & 0b1111100100000000000010010000) == 0b0001000000000000000010000000)
                        {
                            arm7.Error($"Invalid ARM Instruction: {Hex(ins, 8)}");
                            break;
                        }
                        // HLF 114 个 + 纯 ALU 841 个（含 QADD 4 个落点），按 mask 区分
                        if ((ins & 0b1110000000000000000010010000) == 0b0000000000000000000010010000)
                        {
                            // Halfword, Signed Byte Loads and Stores
                            bool L = BitTest(ins, 20);
                            bool S = BitTest(ins, 6);
                            bool H = BitTest(ins, 5);
                            if (!L && !S && H) { _SpecialLDRSTR(ins, false, false, true); }
                            else if (L && !S && H) { _SpecialLDRSTR(ins, true, false, true); }
                            else if (L && S && !H) { _SpecialLDRSTR(ins, true, true, false); }
                            else if (L && S && H) { _SpecialLDRSTR(ins, true, true, true); }
                        }
                        else
                        {
                            // 纯 ALU（Data Processing）
                            uint opcode = (ins >> 21) & 0xF;
                            bool setFlags = (ins & BIT_20) != 0;
                            bool useImmediate32 = (ins & BIT_25) != 0;
                            _DataOp(ins, opcode, useImmediate32, setFlags);
                        }
                    }
                    break;
            }
        }
    }
}
