using System;

namespace VirtualNes.Core
{
    public class APU_FDS : APU_INTERFACE
    {
        private FDSSOUND fds = new FDSSOUND();
        private FDSSOUND fds_sync = new FDSSOUND();

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
            WriteSub(addr, data, fds_sync, 1789772.5d);
        }

        private void WriteSub(ushort addr, byte data, FDSSOUND ch, double rate)
        {
            throw new NotImplementedException();
        }

        private class FDSSOUND
        {
            //todo : 实现
        }
    }
}
