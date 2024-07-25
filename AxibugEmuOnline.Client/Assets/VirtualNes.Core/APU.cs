using Codice.CM.Common;
using System;
using System.Collections;

namespace VirtualNes.Core
{
    public class APU
    {
        private NES nes;
        private byte exsound_select;
        private APU_INTERNAL @internal = new APU_INTERNAL();
        private int last_data;
        private int last_diff;
        protected short[] m_SoundBuffer = new short[256];
        protected int[] lowpass_filter = new int[4];
        protected QUEUE queue;
        protected QUEUE exqueue;
        protected bool[] m_bMute = new bool[16];

        public APU(NES parent)
        {
            exsound_select = 0;

            nes = parent;
            @internal.SetParent(parent);

            last_data = last_diff = 0;

            Array.Clear(m_SoundBuffer, 0, m_SoundBuffer.Length);
            Array.Clear(lowpass_filter, 0, lowpass_filter.Length);
            queue = QUEUE.GetDefault();
            exqueue = QUEUE.GetDefault();

            for (int i = 0; i < m_bMute.Length; i++)
                m_bMute[i] = true;
        }

        public void Dispose()
        {
        }

        internal void SyncDPCM(int cycles)
        {
            @internal.Sync(cycles);
        }

        internal byte Read(ushort addr)
        {
            return @internal.SyncRead(addr);
        }
    }

    public struct QUEUEDATA
    {
        public int time;
        public ushort addr;
        public byte data;
        public byte reserved;
    }

    public struct QUEUE
    {
        public int rdptr;
        public int wrptr;
        QUEUEDATA[] data;

        public static QUEUE GetDefault()
        {
            var res = new QUEUE();
            res.rdptr = 0;
            res.wrptr = 0;
            res.data = new QUEUEDATA[8192];
            return res;
        }
    }
}
