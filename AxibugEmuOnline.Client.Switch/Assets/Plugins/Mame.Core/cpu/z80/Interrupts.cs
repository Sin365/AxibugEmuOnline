using System;

namespace cpu.z80
{
    public partial class Z80A
    {
        private bool iff1;
        public bool IFF1 { get { return iff1; } set { iff1 = value; } }

        private bool iff2;
        public bool IFF2 { get { return iff2; } set { iff2 = value; } }

        private bool interrupt;
        public bool Interrupt { get { return interrupt; } set { interrupt = value; } }

        private bool nonMaskableInterrupt;
        public bool NonMaskableInterrupt
        {
            get { return nonMaskableInterrupt; }
            set { if (value && !nonMaskableInterrupt) NonMaskableInterruptPending = true; nonMaskableInterrupt = value; }
        }

        private bool nonMaskableInterruptPending;
        public bool NonMaskableInterruptPending { get { return nonMaskableInterruptPending; } set { nonMaskableInterruptPending = value; } }

        private int interruptMode;
        public int InterruptMode
        {
            get { return interruptMode; }
            set { if (value < 0 || value > 2) throw new ArgumentOutOfRangeException(); interruptMode = value; }
        }

        private bool halted;
        public bool Halted { get { return halted; } set { halted = value; } }

        public Func<int> IRQCallback = delegate () { return 0; };
        public Action NMICallback = delegate () { };

        private void ResetInterrupts()
        {
            Interrupt = false;
            NonMaskableInterrupt = false;
            NonMaskableInterruptPending = false;
            Interruptable = true;
            IFF1 = IFF2 = false;
        }

        private void Halt()
        {
            RegPC.Word--;
            Halted = true;
        }
    }
}