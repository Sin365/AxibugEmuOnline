using System;

namespace VirtualNes.Core
{
    public class APU_INTERNAL : APU_INTERFACE
    {
        private NES nes;
        // Frame Counter
        private int FrameCycle;
        private int FrameCount;
        private int FrameType;
        private byte FrameIRQ;
        private byte FrameIRQoccur;

        // Channels
        private RECTANGLE ch0 = new RECTANGLE();
        private RECTANGLE ch1 = new RECTANGLE();
        private TRIANGLE ch2 = new TRIANGLE();
        private NOISE ch3 = new NOISE();
        private DPCM ch4 = new DPCM();



        public void SetParent(NES parent)
        {
            nes = parent;
        }

        public override bool Sync(int cycles)
        {
            FrameCycle -= cycles * 2;
            if (FrameCycle <= 0)
            {
                FrameCycle += 14915;

                UpdateFrame();
            }

            var result = FrameIRQoccur | (SyncUpdateDPCM(cycles) ? 1 : 0);
            return result != 0;
        }

        private bool SyncUpdateDPCM(int cycles)
        {
            //TODO : 实现
            return false;
        }

        private void UpdateFrame()
        {
            //TODO : 实现
        }

        public override void Reset(float fClock, int nRate)
        {
            throw new System.NotImplementedException();
        }

        public override void Setup(float fClock, int nRate)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(ushort addr, byte data)
        {
            throw new System.NotImplementedException();
        }

        public override int Process(int channel)
        {
            throw new System.NotImplementedException();
        }

        internal byte SyncRead(ushort addr)
        {
            byte data = (byte)(addr >> 8);

            if (addr == 0x4015)
            {
                data = 0;
                if ((ch0.sync_enable != 0) && ch0.sync_len_count > 0) data |= (1 << 0);
                if ((ch1.sync_enable != 0) && ch1.sync_len_count > 0) data |= (1 << 1);
                if ((ch2.sync_enable != 0) && ch2.sync_len_count > 0) data |= (1 << 2);
                if ((ch3.sync_enable != 0) && ch3.sync_len_count > 0) data |= (1 << 3);
                if ((ch4.sync_enable != 0) && (ch4.sync_dmalength != 0)) data |= (1 << 4);
                if (FrameIRQoccur != 0) data |= (1 << 6);
                if (ch4.sync_irq_enable != 0) data |= (1 << 7);
                FrameIRQoccur = 0;

                nes.cpu.ClrIRQ(CPU.IRQ_FRAMEIRQ);
            }
            if (addr == 0x4017)
            {
                if (FrameIRQoccur != 0)
                {
                    data = 0;
                }
                else
                {
                    data |= (1 << 6);
                }
            }
            return data;
        }
    }
}
