namespace VirtualNes.Core
{
    public class APU_INTERNAL : APU_INTERFACE
    {
        private NES nes;

        public void SetParent(NES parent)
        {
            nes = parent;
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
