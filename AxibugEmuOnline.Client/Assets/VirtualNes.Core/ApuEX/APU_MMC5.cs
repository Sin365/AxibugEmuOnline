using System;

namespace VirtualNes.Core
{
    public class APU_MMC5 : APU_INTERFACE
    {
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

        internal void SyncWrite(ushort addr, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
