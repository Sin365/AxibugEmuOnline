using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.CPU
{
    public partial class Z80A : ICPU
    {
        [Flags]
        enum Flags : byte
        {
            Carry = (1 << 0),               /* C */
            Subtract = (1 << 1),            /* N */
            ParityOrOverflow = (1 << 2),    /* P */
            UnusedBitX = (1 << 3),          /* (X) */
            HalfCarry = (1 << 4),           /* H */
            UnusedBitY = (1 << 5),          /* (Y) */
            Zero = (1 << 6),                /* Z */
            Sign = (1 << 7)                 /* S */
        }

        public delegate byte MemoryReadDelegate(ushort address);
        public delegate void MemoryWriteDelegate(ushort address, byte value);
        public delegate byte PortReadDelegate(byte port);
        public delegate void PortWriteDelegate(byte port, byte value);

        delegate void SimpleOpcodeDelegate(Z80A c);
        delegate void DDFDOpcodeDelegate(Z80A c, ref Register register);
        delegate void DDFDCBOpcodeDelegate(Z80A c, ref Register register, ushort address);

        MemoryReadDelegate memoryReadDelegate;
        MemoryWriteDelegate memoryWriteDelegate;
        PortReadDelegate portReadDelegate;
        PortWriteDelegate portWriteDelegate;

        [StateRequired]
        protected Register af, bc, de, hl;
        [StateRequired]
        protected Register af_, bc_, de_, hl_;
        [StateRequired]
        protected Register ix, iy;
        [StateRequired]
        protected byte i, r;
        [StateRequired]
        protected ushort sp, pc;

        [StateRequired]
        protected bool iff1, iff2, eiDelay, halt;
        [StateRequired]
        protected byte im;

        [StateRequired]
        protected byte op;

        [StateRequired]
        InterruptState intState, nmiState;

        [StateRequired]
        int currentCycles;

        public Z80A(MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite, PortReadDelegate portRead, PortWriteDelegate portWrite)
        {
            af = bc = de = hl = new Register();
            af_ = bc_ = de_ = hl_ = new Register();
            ix = iy = new Register();

            memoryReadDelegate = memoryRead;
            memoryWriteDelegate = memoryWrite;
            portReadDelegate = portRead;
            portWriteDelegate = portWrite;
        }

        public virtual void Startup()
        {
            Reset();

            if (memoryReadDelegate == null) throw new EmulationException("Z80A: Memory read method is null");
            if (memoryWriteDelegate == null) throw new EmulationException("Z80A: Memory write method is null");
            if (portReadDelegate == null) throw new EmulationException("Z80A: Port read method is null");
            if (portWriteDelegate == null) throw new EmulationException("Z80A: Port write method is null");
        }

        public virtual void Shutdown()
        {
            //
        }

        public virtual void Reset()
        {
            af.Word = bc.Word = de.Word = hl.Word = 0;
            af_.Word = bc_.Word = de_.Word = hl_.Word = 0;
            ix.Word = iy.Word = 0;
            i = r = 0;
            pc = 0;
            sp = 0;

            iff1 = iff2 = eiDelay = halt = false;
            im = 0;

            intState = nmiState = InterruptState.Clear;

            currentCycles = 0;
        }

        public int Step()
        {
            currentCycles = 0;

            /* Handle delayed interrupt enable */
            if (eiDelay)
            {
                eiDelay = false;
                iff1 = iff2 = true;
            }
            else
            {
                /* Check INT line */
                if (intState == InterruptState.Assert)
                {
                    ServiceInterrupt();
                }

                /* Check NMI line */
                if (nmiState == InterruptState.Assert)
                {
                    nmiState = InterruptState.Clear;
                    ServiceNonMaskableInterrupt();
                }
            }


            if (AppEnvironment.EnableSuperSlowCPULogger)
            {
                string disasm = string.Format("{0} | {1} | {2} | {3}\n", DisassembleOpcode(this, pc).PadRight(48), PrintRegisters(this), PrintFlags(this), PrintInterrupt(this));
                System.IO.File.AppendAllText(@"D:\Temp\Essgee\log.txt", disasm);
            }

            /* Fetch and execute opcode */
            op = ReadMemory8(pc++);
            switch (op)
            {
                case 0xCB: ExecuteOpCB(); break;
                case 0xDD: ExecuteOpDD(); break;
                case 0xED: ExecuteOpED(); break;
                case 0xFD: ExecuteOpFD(); break;
                default: ExecuteOpcodeNoPrefix(op); break;
            }

            return currentCycles;
        }

        #region Opcode Execution and Cycle Management

        private void ExecuteOpcodeNoPrefix(byte op)
        {
            IncrementRefresh();
            opcodesNoPrefix[op](this);

            currentCycles += CycleCounts.NoPrefix[op];
        }

        private void ExecuteOpED()
        {
            IncrementRefresh();
            byte edOp = ReadMemory8(pc++);

            IncrementRefresh();
            opcodesPrefixED[edOp](this);

            currentCycles += CycleCounts.PrefixED[edOp];
        }

        private void ExecuteOpCB()
        {
            IncrementRefresh();
            byte cbOp = ReadMemory8(pc++);

            IncrementRefresh();
            opcodesPrefixCB[cbOp](this);

            currentCycles += CycleCounts.PrefixCB[cbOp];
        }

        private void ExecuteOpDD()
        {
            IncrementRefresh();
            byte ddOp = ReadMemory8(pc++);

            if (ddOp != 0xDD)
            {
                IncrementRefresh();
                opcodesPrefixDDFD[ddOp](this, ref ix);
            }

            currentCycles += (CycleCounts.PrefixDDFD[ddOp] != 0 ? CycleCounts.PrefixDDFD[ddOp] : CycleCounts.NoPrefix[ddOp] + CycleCounts.AdditionalDDFDOps);
        }

        private void ExecuteOpFD()
        {
            IncrementRefresh();
            byte fdOp = ReadMemory8(pc++);

            if (fdOp != 0xFD)
            {
                IncrementRefresh();
                opcodesPrefixDDFD[fdOp](this, ref iy);
            }

            currentCycles += (CycleCounts.PrefixDDFD[fdOp] != 0 ? CycleCounts.PrefixDDFD[fdOp] : CycleCounts.NoPrefix[fdOp] + CycleCounts.AdditionalDDFDOps);
        }

        private void ExecuteOpDDFDCB(byte op, ref Register register)
        {
            IncrementRefresh();
            sbyte operand = (sbyte)ReadMemory8(pc);
            ushort address = (ushort)(register.Word + operand);
            pc += 2;

            IncrementRefresh();
            opcodesPrefixDDFDCB[op](this, ref register, address);

            currentCycles += (CycleCounts.PrefixCB[op] + CycleCounts.AdditionalDDFDCBOps);
        }

        #endregion

        #region Helpers (Refresh Register, Flags, etc.)

        public void SetStackPointer(ushort value)
        {
            sp = value;
        }

        public void SetProgramCounter(ushort value)
        {
            pc = value;
        }

        private void IncrementRefresh()
        {
            r = (byte)(((r + 1) & 0x7F) | (r & 0x80));
        }

        private void SetFlag(Flags flags)
        {
            af.Low |= (byte)flags;
        }

        private void ClearFlag(Flags flags)
        {
            af.Low &= (byte)~flags;
        }

        private void SetClearFlagConditional(Flags flags, bool condition)
        {
            if (condition)
                af.Low |= (byte)flags;
            else
                af.Low &= (byte)~flags;
        }

        private bool IsFlagSet(Flags flags)
        {
            return (((Flags)af.Low & flags) == flags);
        }

        private void CalculateAndSetParity(byte value)
        {
            int bitsSet = 0;
            while (value != 0) { bitsSet += (value & 0x01); value >>= 1; }
            SetClearFlagConditional(Flags.ParityOrOverflow, (bitsSet == 0 || (bitsSet % 2) == 0));
        }

        private ushort CalculateIXIYAddress(Register register)
        {
            return (ushort)(register.Word + (sbyte)ReadMemory8(pc++));
        }

        #endregion

        #region Interrupt and Halt State Handling

        public void SetInterruptLine(InterruptType type, InterruptState state)
        {
            switch (type)
            {
                case InterruptType.Maskable:
                    intState = state;
                    break;

                case InterruptType.NonMaskable:
                    nmiState = state;
                    break;

                default: throw new EmulationException("Z80A: Unknown interrupt type");
            }
        }

        private void ServiceInterrupt()
        {
            if (!iff1) return;

            LeaveHaltState();
            iff1 = iff2 = false;

            switch (im)
            {
                case 0x00:
                    /* Execute opcode(s) from data bus */
                    /* TODO: no real data bus emulation, just execute opcode 0xFF instead (Xenon 2 SMS, http://www.smspower.org/forums/1172-EmulatingInterrupts#5395) */
                    ExecuteOpcodeNoPrefix(0xFF);
                    currentCycles += 30;
                    break;

                case 0x01:
                    /* Restart to location 0x0038, same as opcode 0xFF */
                    ExecuteOpcodeNoPrefix(0xFF);
                    currentCycles += 30;
                    break;

                case 0x02:
                    /* Indirect call via I register */
                    /* TODO: unsupported at the moment, not needed in currently emulated systems */
                    IncrementRefresh();
                    break;
            }
        }

        private void ServiceNonMaskableInterrupt()
        {
            IncrementRefresh();
            Restart(0x0066);

            iff2 = iff1;
            iff1 = halt = false;

            currentCycles += 11;
        }

        private void EnterHaltState()
        {
            halt = true;
            pc--;
        }

        private void LeaveHaltState()
        {
            if (halt)
            {
                halt = false;
                pc++;
            }
        }

        #endregion

        #region Memory and Port Access Functions

        private byte ReadMemory8(ushort address)
        {
            return memoryReadDelegate(address);
        }

        private void WriteMemory8(ushort address, byte value)
        {
            memoryWriteDelegate(address, value);
        }

        private ushort ReadMemory16(ushort address)
        {
            return (ushort)((memoryReadDelegate((ushort)(address + 1)) << 8) | memoryReadDelegate(address));
        }

        private void WriteMemory16(ushort address, ushort value)
        {
            memoryWriteDelegate(address, (byte)(value & 0xFF));
            memoryWriteDelegate((ushort)(address + 1), (byte)(value >> 8));
        }

        private byte ReadPort(byte port)
        {
            return portReadDelegate(port);
        }

        private void WritePort(byte port, byte value)
        {
            portWriteDelegate(port, value);
        }

        #endregion

        #region Opcodes: 8-Bit Load Group

        protected void LoadRegisterFromMemory8(ref byte register, ushort address, bool specialRegs)
        {
            LoadRegister8(ref register, ReadMemory8(address), specialRegs);
        }

        protected void LoadRegisterImmediate8(ref byte register, bool specialRegs)
        {
            LoadRegister8(ref register, ReadMemory8(pc++), specialRegs);
        }

        protected void LoadRegister8(ref byte register, byte value, bool specialRegs)
        {
            register = value;

            // Register is I or R?
            if (specialRegs)
            {
                SetClearFlagConditional(Flags.Sign, IsBitSet(register, 7));
                SetClearFlagConditional(Flags.Zero, (register == 0x00));
                ClearFlag(Flags.HalfCarry);
                SetClearFlagConditional(Flags.ParityOrOverflow, (iff2));
                ClearFlag(Flags.Subtract);
                // C
            }
        }

        protected void LoadMemory8(ushort address, byte value)
        {
            WriteMemory8(address, value);
        }

        #endregion

        #region Opcodes: 16-Bit Load Group

        protected void LoadRegisterImmediate16(ref ushort register)
        {
            LoadRegister16(ref register, ReadMemory16(pc));
            pc += 2;
        }

        protected void LoadRegister16(ref ushort register, ushort value)
        {
            register = value;
        }

        protected void LoadMemory16(ushort address, ushort value)
        {
            WriteMemory16(address, value);
        }

        protected void Push(Register register)
        {
            WriteMemory8(--sp, register.High);
            WriteMemory8(--sp, register.Low);
        }

        protected void Pop(ref Register register)
        {
            register.Low = ReadMemory8(sp++);
            register.High = ReadMemory8(sp++);
        }

        #endregion

        #region Opcodes: Exchange, Block Transfer and Search Group

        protected void ExchangeRegisters16(ref Register reg1, ref Register reg2)
        {
            ushort tmp = reg1.Word;
            reg1.Word = reg2.Word;
            reg2.Word = tmp;
        }

        protected void ExchangeStackRegister16(ref Register reg)
        {
            byte sl = ReadMemory8(sp);
            byte sh = ReadMemory8((ushort)(sp + 1));

            WriteMemory8(sp, reg.Low);
            WriteMemory8((ushort)(sp + 1), reg.High);

            reg.Low = sl;
            reg.High = sh;
        }

        protected void LoadIncrement()
        {
            byte hlValue = ReadMemory8(hl.Word);
            WriteMemory8(de.Word, hlValue);
            Increment16(ref de.Word);
            Increment16(ref hl.Word);
            Decrement16(ref bc.Word);

            byte n = (byte)(hlValue + af.High);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(n, 1));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(n, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (bc.Word != 0));
            ClearFlag(Flags.Subtract);
            // C
        }

        protected void LoadIncrementRepeat()
        {
            LoadIncrement();

            if (bc.Word != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
        }

        protected void LoadDecrement()
        {
            byte hlValue = ReadMemory8(hl.Word);
            WriteMemory8(de.Word, hlValue);
            Decrement16(ref de.Word);
            Decrement16(ref hl.Word);
            Decrement16(ref bc.Word);

            byte n = (byte)(hlValue + af.High);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(n, 1));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(n, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (bc.Word != 0));
            ClearFlag(Flags.Subtract);
            // C
        }

        protected void LoadDecrementRepeat()
        {
            LoadDecrement();

            if (bc.Word != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
        }

        protected void CompareIncrement()
        {
            byte operand = ReadMemory8(hl.Word);
            int result = (af.High - (sbyte)operand);

            hl.Word++;
            bc.Word--;

            bool halfCarry = (((af.High ^ result ^ operand) & 0x10) != 0);
            byte n = (byte)(result - (halfCarry ? 1 : 0));

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, (af.High == operand));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(n, 1));
            SetClearFlagConditional(Flags.HalfCarry, halfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(n, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (bc.Word != 0));
            SetFlag(Flags.Subtract);
            // C
        }

        protected void CompareIncrementRepeat()
        {
            CompareIncrement();

            if (bc.Word != 0 && !IsFlagSet(Flags.Zero))
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
        }

        protected void CompareDecrement()
        {
            byte operand = ReadMemory8(hl.Word);
            int result = (af.High - (sbyte)operand);

            hl.Word--;
            bc.Word--;

            bool halfCarry = (((af.High ^ result ^ operand) & 0x10) != 0);
            byte n = (byte)(result - (halfCarry ? 1 : 0));

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, (af.High == operand));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(n, 1));
            SetClearFlagConditional(Flags.HalfCarry, halfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(n, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (bc.Word != 0));
            SetFlag(Flags.Subtract);
            // C
        }

        protected void CompareDecrementRepeat()
        {
            CompareDecrement();

            if (bc.Word != 0 && !IsFlagSet(Flags.Zero))
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
        }

        #endregion

        #region Opcodes: 8-Bit Arithmetic Group

        protected void Add8(byte operand, bool withCarry)
        {
            int operandWithCarry = (operand + (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));
            int result = (af.High + operandWithCarry);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)result, 5));
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)result, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (((operand ^ af.High ^ 0x80) & (af.High ^ result) & 0x80) != 0));
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, (result > 0xFF));

            af.High = (byte)result;
        }

        protected void Subtract8(byte operand, bool withCarry)
        {
            int operandWithCarry = (operand + (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));
            int result = (af.High - operandWithCarry);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)result, 5));
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)result, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (((operand ^ af.High) & (af.High ^ result) & 0x80) != 0));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, (af.High < operandWithCarry));

            af.High = (byte)result;
        }

        protected void And8(byte operand)
        {
            int result = (af.High & operand);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)result, 5));
            SetFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)result, 3));
            CalculateAndSetParity((byte)result);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Or8(byte operand)
        {
            int result = (af.High | operand);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)result, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)result, 3));
            CalculateAndSetParity((byte)result);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Xor8(byte operand)
        {
            int result = (af.High ^ operand);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)result, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)result, 3));
            CalculateAndSetParity((byte)result);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Cp8(byte operand)
        {
            int result = (af.High - operand);

            SetClearFlagConditional(Flags.Sign, IsBitSet((byte)result, 7));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(operand, 5));
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(operand, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (((operand ^ af.High) & (af.High ^ result) & 0x80) != 0));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, (af.High < operand));
        }

        protected void Increment8(ref byte register)
        {
            byte result = (byte)(register + 1);

            SetClearFlagConditional(Flags.Sign, IsBitSet(result, 7));
            SetClearFlagConditional(Flags.Zero, (result == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(result, 5));
            SetClearFlagConditional(Flags.HalfCarry, ((register & 0x0F) == 0x0F));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(result, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (register == 0x7F));
            ClearFlag(Flags.Subtract);
            // C

            register = result;
        }

        protected void IncrementMemory8(ushort address)
        {
            byte value = ReadMemory8(address);
            Increment8(ref value);
            WriteMemory8(address, value);
        }

        protected void Decrement8(ref byte register)
        {
            byte result = (byte)(register - 1);

            SetClearFlagConditional(Flags.Sign, IsBitSet(result, 7));
            SetClearFlagConditional(Flags.Zero, (result == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(result, 5));
            SetClearFlagConditional(Flags.HalfCarry, ((register & 0x0F) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(result, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (register == 0x80));
            SetFlag(Flags.Subtract);
            // C

            register = result;
        }

        protected void DecrementMemory8(ushort address)
        {
            byte value = ReadMemory8(address);
            Decrement8(ref value);
            WriteMemory8(address, value);
        }

        #endregion

        #region Opcodes: General-Purpose Arithmetic and CPU Control Group

        protected void DecimalAdjustAccumulator()
        {
            /* "The Undocumented Z80 Documented" by Sean Young, chapter 4.7, http://www.z80.info/zip/z80-documented.pdf */

            byte before = af.High, diff = 0x00, result;
            bool carry = IsFlagSet(Flags.Carry), halfCarry = IsFlagSet(Flags.HalfCarry);
            byte highNibble = (byte)((before & 0xF0) >> 4), lowNibble = (byte)(before & 0x0F);

            if (carry)
            {
                diff |= 0x60;
                if ((halfCarry && lowNibble <= 0x09) || lowNibble >= 0x0A)
                    diff |= 0x06;
            }
            else
            {
                if (lowNibble >= 0x0A && lowNibble <= 0x0F)
                {
                    diff |= 0x06;
                    if (highNibble >= 0x09 && highNibble <= 0x0F)
                        diff |= 0x60;
                }
                else
                {
                    if (highNibble >= 0x0A && highNibble <= 0x0F)
                        diff |= 0x60;
                    if (halfCarry)
                        diff |= 0x06;
                }

                SetClearFlagConditional(Flags.Carry, (
                    ((highNibble >= 0x09 && highNibble <= 0x0F) && (lowNibble >= 0x0A && lowNibble <= 0x0F)) ||
                    ((highNibble >= 0x0A && highNibble <= 0x0F) && (lowNibble >= 0x00 && lowNibble <= 0x09))));
            }

            if (!IsFlagSet(Flags.Subtract))
                SetClearFlagConditional(Flags.HalfCarry, (lowNibble >= 0x0A && lowNibble <= 0x0F));
            else
                SetClearFlagConditional(Flags.HalfCarry, (halfCarry && (lowNibble >= 0x00 && lowNibble <= 0x05)));

            if (!IsFlagSet(Flags.Subtract))
                result = (byte)(before + diff);
            else
                result = (byte)(before - diff);

            SetClearFlagConditional(Flags.Sign, IsBitSet(result, 7));
            SetClearFlagConditional(Flags.Zero, (result == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(result, 5));
            // H (set above)
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(result, 3));
            CalculateAndSetParity(result);
            // N
            // C (set above)

            af.High = result;
        }

        protected void Negate()
        {
            int result = (0 - af.High);

            SetClearFlagConditional(Flags.Sign, ((result & 0xFF) >= 0x80));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)(result & 0xFF), 5));
            SetClearFlagConditional(Flags.HalfCarry, ((0 - (af.High & 0x0F)) < 0));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)(result & 0xFF), 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (af.High == 0x80));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, (af.High != 0x00));

            af.High = (byte)result;
        }

        #endregion

        #region Opcodes: 16-Bit Arithmetic Group

        protected void Add16(ref Register dest, ushort operand, bool withCarry)
        {
            int operandWithCarry = ((short)operand + (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));
            int result = (dest.Word + operandWithCarry);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)(result >> 8), 5));
            SetClearFlagConditional(Flags.HalfCarry, (((dest.Word & 0x0FFF) + (operandWithCarry & 0x0FFF)) > 0x0FFF));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)(result >> 8), 3));
            // PV
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, (((dest.Word & 0xFFFF) + (operandWithCarry & 0xFFFF)) > 0xFFFF));

            if (withCarry)
            {
                SetClearFlagConditional(Flags.Sign, ((result & 0x8000) != 0x0000));
                SetClearFlagConditional(Flags.Zero, ((result & 0xFFFF) == 0x0000));
                SetClearFlagConditional(Flags.ParityOrOverflow, (((dest.Word ^ operandWithCarry) & 0x8000) == 0 && ((dest.Word ^ (result & 0xFFFF)) & 0x8000) != 0));
            }

            dest.Word = (ushort)result;
        }

        protected void Subtract16(ref Register dest, ushort operand, bool withCarry)
        {
            int result = (dest.Word - operand - (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Sign, ((result & 0x8000) != 0x0000));
            SetClearFlagConditional(Flags.Zero, ((result & 0xFFFF) == 0x0000));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)(result >> 8), 5));
            SetClearFlagConditional(Flags.HalfCarry, ((((dest.Word ^ result ^ operand) >> 8) & 0x10) != 0));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)(result >> 8), 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, (((operand ^ dest.Word) & (dest.Word ^ result) & 0x8000) != 0));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, ((result & 0x10000) != 0));

            dest.Word = (ushort)result;
        }

        protected void Increment16(ref ushort register)
        {
            register++;
        }

        protected void Decrement16(ref ushort register)
        {
            register--;
        }

        #endregion

        #region Opcodes: Rotate and Shift Group

        protected byte RotateLeft(ushort address)
        {
            byte value = ReadMemory8(address);
            RotateLeft(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void RotateLeft(ref byte value)
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isMsbSet = IsBitSet(value, 7);
            value <<= 1;
            if (isCarrySet) SetBit(ref value, 0);

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected byte RotateLeftCircular(ushort address)
        {
            byte value = ReadMemory8(address);
            RotateLeftCircular(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void RotateLeftCircular(ref byte value)
        {
            bool isMsbSet = IsBitSet(value, 7);
            value <<= 1;
            if (isMsbSet) SetBit(ref value, 0);

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected byte RotateRight(ushort address)
        {
            byte value = ReadMemory8(address);
            RotateRight(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void RotateRight(ref byte value)
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isLsbSet = IsBitSet(value, 0);
            value >>= 1;
            if (isCarrySet) SetBit(ref value, 7);

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected byte RotateRightCircular(ushort address)
        {
            byte value = ReadMemory8(address);
            RotateRightCircular(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void RotateRightCircular(ref byte value)
        {
            bool isLsbSet = IsBitSet(value, 0);
            value >>= 1;
            if (isLsbSet) SetBit(ref value, 7);

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected void RotateLeftAccumulator()
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isMsbSet = IsBitSet(af.High, 7);
            af.High <<= 1;
            if (isCarrySet) SetBit(ref af.High, 0);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            // PV
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected void RotateLeftAccumulatorCircular()
        {
            bool isMsbSet = IsBitSet(af.High, 7);
            af.High <<= 1;
            if (isMsbSet) SetBit(ref af.High, 0);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            // PV
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected void RotateRightAccumulator()
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isLsbSet = IsBitSet(af.High, 0);
            af.High >>= 1;
            if (isCarrySet) SetBit(ref af.High, 7);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            // PV
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected void RotateRightAccumulatorCircular()
        {
            bool isLsbSet = IsBitSet(af.High, 0);
            af.High >>= 1;
            if (isLsbSet) SetBit(ref af.High, 7);

            // S
            // Z
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            // PV
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected void RotateRight4B()
        {
            byte hlValue = ReadMemory8(hl.Word);

            // A=WX  (HL)=YZ
            // A=WZ  (HL)=XY
            byte a1 = (byte)(af.High >> 4);     //W
            byte a2 = (byte)(af.High & 0xF);    //X
            byte hl1 = (byte)(hlValue >> 4);    //Y
            byte hl2 = (byte)(hlValue & 0xF);   //Z

            af.High = (byte)((a1 << 4) | hl2);
            hlValue = (byte)((a2 << 4) | hl1);

            WriteMemory8(hl.Word, hlValue);

            SetClearFlagConditional(Flags.Sign, IsBitSet(af.High, 7));
            SetClearFlagConditional(Flags.Zero, (af.High == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            CalculateAndSetParity(af.High);
            ClearFlag(Flags.Subtract);
            // C
        }

        protected void RotateLeft4B()
        {
            byte hlValue = ReadMemory8(hl.Word);

            // A=WX  (HL)=YZ
            // A=WY  (HL)=ZX
            byte a1 = (byte)(af.High >> 4);     //W
            byte a2 = (byte)(af.High & 0xF);    //X
            byte hl1 = (byte)(hlValue >> 4);    //Y
            byte hl2 = (byte)(hlValue & 0xF);   //Z

            af.High = (byte)((a1 << 4) | hl1);
            hlValue = (byte)((hl2 << 4) | a2);

            WriteMemory8(hl.Word, hlValue);

            SetClearFlagConditional(Flags.Sign, IsBitSet(af.High, 7));
            SetClearFlagConditional(Flags.Zero, (af.High == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(af.High, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(af.High, 3));
            CalculateAndSetParity(af.High);
            ClearFlag(Flags.Subtract);
            // C
        }

        protected byte ShiftLeftArithmetic(ushort address)
        {
            byte value = ReadMemory8(address);
            ShiftLeftArithmetic(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void ShiftLeftArithmetic(ref byte value)
        {
            bool isMsbSet = IsBitSet(value, 7);
            value <<= 1;

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected byte ShiftRightArithmetic(ushort address)
        {
            byte value = ReadMemory8(address);
            ShiftRightArithmetic(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void ShiftRightArithmetic(ref byte value)
        {
            bool isLsbSet = IsBitSet(value, 0);
            bool isMsbSet = IsBitSet(value, 7);
            value >>= 1;
            if (isMsbSet) SetBit(ref value, 7);

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected byte ShiftLeftLogical(ushort address)
        {
            byte value = ReadMemory8(address);
            ShiftLeftLogical(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void ShiftLeftLogical(ref byte value)
        {
            bool isMsbSet = IsBitSet(value, 7);
            value <<= 1;
            value |= 0x01;

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected byte ShiftRightLogical(ushort address)
        {
            byte value = ReadMemory8(address);
            ShiftRightLogical(ref value);
            WriteMemory8(address, value);
            return value;
        }

        protected void ShiftRightLogical(ref byte value)
        {
            bool isLsbSet = IsBitSet(value, 0);
            value >>= 1;

            SetClearFlagConditional(Flags.Sign, IsBitSet(value, 7));
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            CalculateAndSetParity(value);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        #endregion

        #region Opcodes: Bit Set, Reset and Test Group

        protected byte SetBit(ushort address, int bit)
        {
            byte value = ReadMemory8(address);
            SetBit(ref value, bit);
            WriteMemory8(address, value);
            return value;
        }

        protected void SetBit(ref byte value, int bit)
        {
            value |= (byte)(1 << bit);
        }

        protected byte ResetBit(ushort address, int bit)
        {
            byte value = ReadMemory8(address);
            ResetBit(ref value, bit);
            WriteMemory8(address, value);
            return value;
        }

        protected void ResetBit(ref byte value, int bit)
        {
            value &= (byte)~(1 << bit);
        }

        protected void TestBit(ushort address, int bit)
        {
            byte value = ReadMemory8(address);

            TestBit(value, bit);

            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet((byte)(address >> 8), 5));
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet((byte)(address >> 8), 3));
        }

        protected void TestBit(byte value, int bit)
        {
            bool isBitSet = ((value & (1 << bit)) != 0);

            SetClearFlagConditional(Flags.Sign, (bit == 7 && isBitSet));
            SetClearFlagConditional(Flags.Zero, !isBitSet);
            SetClearFlagConditional(Flags.UnusedBitY, IsBitSet(value, 5));
            SetFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.UnusedBitX, IsBitSet(value, 3));
            SetClearFlagConditional(Flags.ParityOrOverflow, !isBitSet);
            ClearFlag(Flags.Subtract);
            // C
        }

        #endregion

        #region Opcodes: Jump Group

        protected void DecrementJumpNonZero()
        {
            bc.High--;
            JumpConditional8(bc.High != 0);
        }

        protected void Jump8()
        {
            pc += (ushort)(((sbyte)ReadMemory8(pc)) + 1);
        }

        protected void JumpConditional8(bool condition)
        {
            if (condition)
            {
                Jump8();
                currentCycles += CycleCounts.AdditionalJumpCond8Taken;
            }
            else
                pc++;
        }

        protected void JumpConditional16(bool condition)
        {
            if (condition)
                pc = ReadMemory16(pc);
            else
                pc += 2;
        }

        #endregion

        #region Opcodes: Call and Return Group

        protected void Call16()
        {
            WriteMemory8(--sp, (byte)((pc + 2) >> 8));
            WriteMemory8(--sp, (byte)((pc + 2) & 0xFF));
            pc = ReadMemory16(pc);
        }

        protected void CallConditional16(bool condition)
        {
            if (condition)
            {
                Call16();
                currentCycles += CycleCounts.AdditionalCallCondTaken;
            }
            else
                pc += 2;
        }

        protected void Return()
        {
            pc = ReadMemory16(sp);
            sp += 2;
        }

        protected void ReturnConditional(bool condition)
        {
            if (condition)
            {
                Return();
                currentCycles += CycleCounts.AdditionalRetCondTaken;
            }
        }

        protected void Restart(ushort address)
        {
            WriteMemory8(--sp, (byte)(pc >> 8));
            WriteMemory8(--sp, (byte)(pc & 0xFF));
            pc = address;
        }

        #endregion

        #region Opcodes: Input and Output Group

        protected void PortInput(ref byte dest, byte port)
        {
            dest = ReadPort(port);

            SetClearFlagConditional(Flags.Sign, IsBitSet(dest, 7));
            SetClearFlagConditional(Flags.Zero, (dest == 0x00));
            ClearFlag(Flags.HalfCarry);
            CalculateAndSetParity(dest);
            ClearFlag(Flags.Subtract);
            // C
        }

        protected void PortInputFlagsOnly(byte port)
        {
            byte temp = 0;

            PortInput(ref temp, port);
        }

        protected void PortInputIncrement()
        {
            WriteMemory8(hl.Word, ReadPort(bc.Low));
            Increment16(ref hl.Word);
            Decrement8(ref bc.High);

            // S
            SetClearFlagConditional(Flags.Zero, (bc.High == 0x00));
            // H
            // PV
            SetFlag(Flags.Subtract);
            // C
        }

        protected void PortInputIncrementRepeat()
        {
            PortInputIncrement();

            if (bc.High != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
            else
            {
                // S
                SetFlag(Flags.Zero);
                // H
                // PV
                SetFlag(Flags.Subtract);
                // C
            }
        }

        protected void PortInputDecrement()
        {
            WriteMemory8(hl.Word, ReadPort(bc.Low));
            Decrement16(ref hl.Word);
            Decrement8(ref bc.High);

            // S
            SetClearFlagConditional(Flags.Zero, (bc.High == 0x00));
            // H
            // PV
            SetFlag(Flags.Subtract);
            // C
        }

        protected void PortInputDecrementRepeat()
        {
            PortInputDecrement();

            if (bc.High != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
            else
            {
                // S
                SetFlag(Flags.Zero);
                // H
                // PV
                SetFlag(Flags.Subtract);
                // C
            }
        }

        protected void PortOutputIncrement()
        {
            byte value = ReadMemory8(hl.Word);
            WritePort(bc.Low, value);
            Increment16(ref hl.Word);
            Decrement8(ref bc.High);

            bool setHC = ((value + hl.Low) > 255);

            // S
            SetClearFlagConditional(Flags.Zero, (bc.High == 0x00));
            SetClearFlagConditional(Flags.HalfCarry, setHC);
            CalculateAndSetParity((byte)(((value + hl.Low) & 0x07) ^ bc.High));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, setHC);
        }

        protected void PortOutputIncrementRepeat()
        {
            PortOutputIncrement();

            if (bc.High != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
            else
            {
                // S
                SetFlag(Flags.Zero);
                // H
                // PV
                SetFlag(Flags.Subtract);
                // C
            }
        }

        protected void PortOutputDecrement()
        {
            byte value = ReadMemory8(hl.Word);
            WritePort(bc.Low, value);
            Decrement16(ref hl.Word);
            Decrement8(ref bc.High);

            bool setHC = ((value + hl.Low) > 255);

            // S
            SetClearFlagConditional(Flags.Zero, (bc.High == 0x00));
            SetClearFlagConditional(Flags.HalfCarry, setHC);
            CalculateAndSetParity((byte)(((value + hl.Low) & 0x07) ^ bc.High));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.Carry, setHC);
        }

        protected void PortOutputDecrementRepeat()
        {
            PortOutputDecrement();

            if (bc.High != 0)
            {
                currentCycles += CycleCounts.AdditionalRepeatByteOps;
                pc -= 2;
            }
            else
            {
                // S
                SetFlag(Flags.Zero);
                // H
                // PV
                SetFlag(Flags.Subtract);
                // C
            }
        }

        #endregion
    }
}
