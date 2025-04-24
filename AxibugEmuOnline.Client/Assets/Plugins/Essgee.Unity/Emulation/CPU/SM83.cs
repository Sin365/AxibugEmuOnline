using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Linq;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.CPU
{
    public partial class SM83 : ICPU
    {
        [Flags]
        enum Flags : byte
        {
            UnusedBit0 = (1 << 0),          /* (0) */
            UnusedBit1 = (1 << 1),          /* (0) */
            UnusedBit2 = (1 << 2),          /* (0) */
            UnusedBit3 = (1 << 3),          /* (0) */
            Carry = (1 << 4),               /* C */
            HalfCarry = (1 << 5),           /* H */
            Subtract = (1 << 6),            /* N */
            Zero = (1 << 7)                 /* Z */
        }

        [Flags]
        public enum InterruptSource : byte
        {
            VBlank = 0,
            LCDCStatus = 1,
            TimerOverflow = 2,
            SerialIO = 3,
            Keypad = 4
        }

        public delegate byte MemoryReadDelegate(ushort address);
        public delegate void MemoryWriteDelegate(ushort address, byte value);

        public delegate void RequestInterruptDelegate(InterruptSource source);

        delegate void SimpleOpcodeDelegate(SM83 c);

        protected MemoryReadDelegate memoryReadDelegate;
        protected MemoryWriteDelegate memoryWriteDelegate;

        [StateRequired]
        protected Register af, bc, de, hl;
        [StateRequired]
        protected ushort sp, pc;

        [StateRequired]
        protected bool ime, imeDelay, halt, doHaltBug;

        [StateRequired]
        protected byte op;

        [StateRequired]
        protected int currentCycles;

        string logFile;
        int numLogEntries;
        string[] logEntries;

        public SM83(MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite)
        {
            af = bc = de = hl = new Register();

            memoryReadDelegate = memoryRead;
            memoryWriteDelegate = memoryWrite;

            if (AppEnvironment.EnableSuperSlowCPULogger)
            {
                //logFile = @"D:\Temp\Essgee\log-lr35902.txt";
                //numLogEntries = 0;
                //logEntries = new string[2000];
            }
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            af.axi_AllData = BitConverter.ToUInt16(data.MemberData[nameof(af)]);
            bc.axi_AllData = BitConverter.ToUInt16(data.MemberData[nameof(bc)]);
            de.axi_AllData = BitConverter.ToUInt16(data.MemberData[nameof(de)]);
            hl.axi_AllData = BitConverter.ToUInt16(data.MemberData[nameof(hl)]);

            sp = BitConverter.ToUInt16(data.MemberData[nameof(sp)]);
            pc = BitConverter.ToUInt16(data.MemberData[nameof(pc)]);

            ime = BitConverter.ToBoolean(data.MemberData[nameof(ime)]);
            imeDelay = BitConverter.ToBoolean(data.MemberData[nameof(imeDelay)]);
            halt = BitConverter.ToBoolean(data.MemberData[nameof(halt)]);
            doHaltBug = BitConverter.ToBoolean(data.MemberData[nameof(doHaltBug)]);

            op = data.MemberData[nameof(op)].First();

            currentCycles = BitConverter.ToInt32(data.MemberData[nameof(currentCycles)]);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();

            data.MemberData[nameof(af)] = BitConverter.GetBytes(af.axi_AllData);
            data.MemberData[nameof(bc)] = BitConverter.GetBytes(bc.axi_AllData);
            data.MemberData[nameof(de)] = BitConverter.GetBytes(de.axi_AllData);
            data.MemberData[nameof(hl)] = BitConverter.GetBytes(hl.axi_AllData);

            data.MemberData[nameof(sp)] = BitConverter.GetBytes(sp);
            data.MemberData[nameof(pc)] = BitConverter.GetBytes(pc);

            data.MemberData[nameof(ime)] = BitConverter.GetBytes(ime);
            data.MemberData[nameof(imeDelay)] = BitConverter.GetBytes(imeDelay);
            data.MemberData[nameof(halt)] = BitConverter.GetBytes(halt);
            data.MemberData[nameof(doHaltBug)] = BitConverter.GetBytes(doHaltBug);

            data.MemberData[nameof(op)] = BitConverter.GetBytes(op);

            data.MemberData[nameof(currentCycles)] = BitConverter.GetBytes(currentCycles);
            return data;
        }
        #endregion
        public virtual void Startup()
        {
            Reset();

            if (memoryReadDelegate == null) throw new EmulationException("SM83: Memory read method is null");
            if (memoryWriteDelegate == null) throw new EmulationException("SM83: Memory write method is null");
        }

        public virtual void Shutdown()
        {
            if (AppEnvironment.EnableSuperSlowCPULogger && logEntries != null)
            {
                //TODO 暂时不要日志看后续是否需要加
                //System.IO.File.AppendAllText(logFile, string.Join("", logEntries.Take(numLogEntries)));
            }

            //
        }

        public virtual void Reset()
        {
            af.Word = bc.Word = de.Word = hl.Word = 0;
            pc = 0;
            sp = 0;

            ime = imeDelay = halt = doHaltBug = false;

            currentCycles = 0;
        }

        public int Step()
        {
            currentCycles = 0;

            /* Handle delayed interrupt enable */
            if (imeDelay)
            {
                ime = true;
                imeDelay = false;
            }

            /* Check interrupts */
            HandleInterrupts();

            if (halt)
            {
                /* CPU halted */
                currentCycles = 4;
            }
            else
            {
                if (AppEnvironment.EnableSuperSlowCPULogger && logEntries != null)
                {
                    string disasm = string.Format("{0} | {1} | {2} | {3}\n", DisassembleOpcode(this, pc).PadRight(48), PrintRegisters(this), PrintFlags(this), PrintInterrupt(this));
                    logEntries[numLogEntries++] = disasm;
                    if (numLogEntries >= logEntries.Length)
                    {
                        //TODO 暂时不要日志看后续是否需要加
                        //System.IO.File.AppendAllText(logFile, string.Join("", logEntries));
                        numLogEntries = 0;
                    }
                }

                /* Do HALT bug */
                if (doHaltBug)
                {
                    pc--;
                    doHaltBug = false;
                }

                /* Fetch and execute opcode */
                op = ReadMemory8(pc++);
                switch (op)
                {
                    case 0xCB: ExecuteOpCB(); break;
                    default: ExecuteOpcodeNoPrefix(op); break;
                }
            }

            return currentCycles;
        }

        #region Opcode Execution and Cycle Management

        private void ExecuteOpcodeNoPrefix(byte op)
        {
            opcodesNoPrefix[op](this);
            currentCycles += CycleCounts.NoPrefix[op];
        }

        private void ExecuteOpCB()
        {
            byte cbOp = ReadMemory8(pc++);
            opcodesPrefixCB[cbOp](this);
            currentCycles += CycleCounts.PrefixCB[cbOp];
        }

        #endregion

        #region Helpers (Flags, etc.)

        public void SetStackPointer(ushort value)
        {
            sp = value;
        }

        public void SetProgramCounter(ushort value)
        {
            pc = value;
        }

        public void SetRegisterAF(ushort value)
        {
            af.Word = value;
        }

        public void SetRegisterBC(ushort value)
        {
            bc.Word = value;
        }

        public void SetRegisterDE(ushort value)
        {
            de.Word = value;
        }

        public void SetRegisterHL(ushort value)
        {
            hl.Word = value;
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

        #endregion

        #region Interrupt, Halt and Stop State Handling

        public void RequestInterrupt(InterruptSource source)
        {
            memoryWriteDelegate(0xFF0F, (byte)(memoryReadDelegate(0xFF0F) | (byte)(1 << (byte)source)));
        }

        private void HandleInterrupts()
        {
            var intEnable = memoryReadDelegate(0xFFFF);
            var intFlags = memoryReadDelegate(0xFF0F);

            if ((intEnable & intFlags) != 0)
            {
                LeaveHaltState();

                if (ime)
                {
                    if (ServiceInterrupt(InterruptSource.VBlank, intEnable, intFlags)) return;
                    if (ServiceInterrupt(InterruptSource.LCDCStatus, intEnable, intFlags)) return;
                    if (ServiceInterrupt(InterruptSource.TimerOverflow, intEnable, intFlags)) return;
                    if (ServiceInterrupt(InterruptSource.SerialIO, intEnable, intFlags)) return;
                    if (ServiceInterrupt(InterruptSource.Keypad, intEnable, intFlags)) return;
                }
            }
        }

        private bool ServiceInterrupt(InterruptSource intSource, byte intEnable, byte intFlags)
        {
            var intSourceBit = (byte)(1 << (byte)intSource);

            if (((intEnable & intSourceBit) == intSourceBit) && ((intFlags & intSourceBit) == intSourceBit))
            {
                ime = false;

                currentCycles += 20;

                return RestartFromInterrupt(intSource);
            }

            return false;
        }

        protected virtual void EnterHaltState()
        {
            if (ime)
            {
                halt = true;
                pc--;
            }
            else
            {
                if ((memoryReadDelegate(0xFF0F) & memoryReadDelegate(0xFFFF) & 0x1F) != 0)
                    doHaltBug = true;
                else
                    halt = true;
            }
        }

        private void LeaveHaltState()
        {
            if (halt)
            {
                halt = false;
                if (ime)
                    pc++;
            }
        }

        #endregion

        #region Memory Access Functions

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

        #region Opcodes: 8-Bit Arithmetic Group

        protected void Add8(byte operand, bool withCarry)
        {
            int operandWithCarry = (operand + (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));
            int result = (af.High + operandWithCarry);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.Carry, (result > 0xFF));

            af.High = (byte)result;
        }

        protected void Subtract8(byte operand, bool withCarry)
        {
            int operandWithCarry = (operand + (withCarry && IsFlagSet(Flags.Carry) ? 1 : 0));
            int result = (af.High - operandWithCarry);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.Carry, (af.High < operandWithCarry));

            af.High = (byte)result;
        }

        protected void And8(byte operand)
        {
            int result = (af.High & operand);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            ClearFlag(Flags.Subtract);
            SetFlag(Flags.HalfCarry);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Or8(byte operand)
        {
            int result = (af.High | operand);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Xor8(byte operand)
        {
            int result = (af.High ^ operand);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            ClearFlag(Flags.Carry);

            af.High = (byte)result;
        }

        protected void Cp8(byte operand)
        {
            int result = (af.High - operand);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((af.High ^ result ^ operand) & 0x10) != 0));
            SetClearFlagConditional(Flags.Carry, (af.High < operand));
        }

        protected void Increment8(ref byte register)
        {
            byte result = (byte)(register + 1);

            SetClearFlagConditional(Flags.Zero, (result == 0x00));
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, ((register & 0x0F) == 0x0F));
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

            SetClearFlagConditional(Flags.Zero, (result == 0x00));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, ((register & 0x0F) == 0x00));
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
            int value = af.High;

            if (!IsFlagSet(Flags.Subtract))
            {
                if (IsFlagSet(Flags.HalfCarry) || ((value & 0x0F) > 9))
                    value += 0x06;
                if (IsFlagSet(Flags.Carry) || (value > 0x9F))
                    value += 0x60;
            }
            else
            {
                if (IsFlagSet(Flags.HalfCarry))
                    value = (value - 0x06) & 0xFF;
                if (IsFlagSet(Flags.Carry))
                    value -= 0x60;
            }

            ClearFlag(Flags.HalfCarry);
            ClearFlag(Flags.Zero);

            if ((value & 0x100) != 0) SetFlag(Flags.Carry);

            value &= 0xFF;

            if (value == 0) SetFlag(Flags.Zero);

            af.High = (byte)value;
        }

        protected void Negate()
        {
            int result = (0 - af.High);

            SetClearFlagConditional(Flags.Zero, ((result & 0xFF) == 0x00));
            SetFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, ((0 - (af.High & 0x0F)) < 0));
            SetClearFlagConditional(Flags.Carry, (af.High != 0x00));

            af.High = (byte)result;
        }

        protected void EnableInterrupts()
        {
            ime = false;
            imeDelay = true;
        }

        protected void DisableInterrupts()
        {
            ime = false;
        }

        #endregion

        #region Opcodes: 16-Bit Arithmetic Group

        protected void Add16(ref Register dest, ushort operand)
        {
            int operandWithCarry = (short)operand;
            int result = (dest.Word + operandWithCarry);

            // Z
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((dest.Word & 0x0FFF) + (operandWithCarry & 0x0FFF)) > 0x0FFF));
            SetClearFlagConditional(Flags.Carry, (((dest.Word & 0xFFFF) + (operandWithCarry & 0xFFFF)) > 0xFFFF));

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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected void RotateLeftAccumulator()
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isMsbSet = IsBitSet(af.High, 7);
            af.High <<= 1;
            if (isCarrySet) SetBit(ref af.High, 0);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected void RotateLeftAccumulatorCircular()
        {
            bool isMsbSet = IsBitSet(af.High, 7);
            af.High <<= 1;
            if (isMsbSet) SetBit(ref af.High, 0);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isMsbSet);
        }

        protected void RotateRightAccumulator()
        {
            bool isCarrySet = IsFlagSet(Flags.Carry);
            bool isLsbSet = IsBitSet(af.High, 0);
            af.High >>= 1;
            if (isCarrySet) SetBit(ref af.High, 7);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
        }

        protected void RotateRightAccumulatorCircular()
        {
            bool isLsbSet = IsBitSet(af.High, 0);
            af.High >>= 1;
            if (isLsbSet) SetBit(ref af.High, 7);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            SetClearFlagConditional(Flags.Carry, isLsbSet);
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

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
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
        }

        protected void TestBit(byte value, int bit)
        {
            bool isBitSet = ((value & (1 << bit)) != 0);

            SetClearFlagConditional(Flags.Zero, !isBitSet);
            ClearFlag(Flags.Subtract);
            SetFlag(Flags.HalfCarry);
            // C
        }

        #endregion

        #region Opcodes: Jump Group

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

        protected bool RestartFromInterrupt(InterruptSource intSource)
        {
            // https://github.com/Gekkio/mooneye-gb/blob/ca7ff30/tests/acceptance/interrupts/ie_push.s

            var address = (ushort)(0x0040 + (byte)((int)intSource << 3));
            var intSourceBit = (byte)(1 << (byte)intSource);

            WriteMemory8(--sp, (byte)(pc >> 8));

            var newIntEnable = memoryReadDelegate(0xFFFF);
            var continueRestart = (newIntEnable & intSourceBit) != 0;

            WriteMemory8(--sp, (byte)(pc & 0xFF));

            if (continueRestart)
            {
                pc = address;
                memoryWriteDelegate(0xFF0F, (byte)(memoryReadDelegate(0xFF0F) & (byte)~intSourceBit));
            }
            else
                pc = 0x0000;

            return continueRestart;
        }

        #endregion

        #region Opcodes: SM83-specific Opcodes

        protected void PopAF()
        {
            af.Low = (byte)(ReadMemory8(sp++) & 0xF0);
            af.High = ReadMemory8(sp++);
        }

        protected void Swap(ushort address)
        {
            byte value = ReadMemory8(address);
            Swap(ref value);
            WriteMemory8(address, value);
        }

        protected void Swap(ref byte value)
        {
            value = (byte)((value & 0xF0) >> 4 | (value & 0x0F) << 4);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            ClearFlag(Flags.Subtract);
            ClearFlag(Flags.HalfCarry);
            ClearFlag(Flags.Carry);
        }

        protected virtual void Stop()
        {
            pc++;
        }

        private void AddSPNN()
        {
            byte offset = ReadMemory8(pc++);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((sp & 0x0F) + (offset & 0x0F)) > 0x0F));
            SetClearFlagConditional(Flags.Carry, (((sp & 0xFF) + (byte)(offset & 0xFF)) > 0xFF));

            sp = (ushort)(sp + (sbyte)offset);
        }

        private void LoadHLSPNN()
        {
            byte offset = ReadMemory8(pc++);

            ClearFlag(Flags.Zero);
            ClearFlag(Flags.Subtract);
            SetClearFlagConditional(Flags.HalfCarry, (((sp & 0x0F) + (offset & 0x0F)) > 0x0F));
            SetClearFlagConditional(Flags.Carry, (((sp & 0xFF) + (byte)(offset & 0xFF)) > 0xFF));

            hl.Word = (ushort)(sp + (sbyte)offset);
        }

        #endregion
    }
}
