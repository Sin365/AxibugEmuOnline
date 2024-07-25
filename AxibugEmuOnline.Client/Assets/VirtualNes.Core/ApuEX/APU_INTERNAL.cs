namespace VirtualNes.Core
{
    public class APU_INTERNAL : APU_INTERFACE
    {
        private NES nes;
        private int FrameCycle;
        private byte FrameIRQoccur;

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
    }
}
