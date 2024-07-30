using System;

namespace VirtualNes.Core
{
    public class APU_FDS : APU_INTERFACE
    {
        private FDSSOUND fds = new FDSSOUND();
        private FDSSOUND fds_sync = new FDSSOUND();

        public override void Reset(float fClock, int nRate)
        {
            //todo : 实现
        }

        public override void Setup(float fClock, int nRate)
        {
            //todo : 实现
        }

        public override void Write(ushort addr, byte data)
        {
            //todo : 实现
        }

        public override int Process(int channel)
        {
            //todo : 实现
            return 0;
        }

        internal void SyncWrite(ushort addr, byte data)
        {
            WriteSub(addr, data, fds_sync, 1789772.5d);
        }

        private void WriteSub(ushort addr, byte data, FDSSOUND ch, double rate)
        {
            //todo : 实现
        }

        private class FDSSOUND
        {
            //todo : 实现
        }
    }
}
