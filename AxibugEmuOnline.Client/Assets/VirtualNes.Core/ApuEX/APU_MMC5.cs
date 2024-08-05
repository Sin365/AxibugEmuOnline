using System;

namespace VirtualNes.Core
{
    public class APU_MMC5 : APU_INTERFACE
    {
        SYNCRECTANGLE sch0 = new SYNCRECTANGLE();
        SYNCRECTANGLE sch1 = new SYNCRECTANGLE();

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
            //todo : 实现
        }

        internal byte SyncRead(ushort addr)
        {
            byte data = 0;

            if (addr == 0x5015)
            {
                if ((sch0.enable != 0) && sch0.vbl_length > 0) data |= (1 << 0);
                if ((sch1.enable != 0) && sch1.vbl_length > 0) data |= (1 << 1);
            }

            return data;
        }

        public class SYNCRECTANGLE
        {
            // For sync
            public byte[] reg = new byte[4];
            public byte enable;
            public byte holdnote;
            public byte[] dummy = new byte[2];
            public int vbl_length;
        }
    }
}
