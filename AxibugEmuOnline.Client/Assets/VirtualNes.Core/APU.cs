using System;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class APU
    {
        public const uint QUEUE_LENGTH = 8192;

        private NES nes;
        private byte exsound_select;
        private APU_INTERNAL @internal = new APU_INTERNAL();
        private int last_data;
        private int last_diff;
        protected short[] m_SoundBuffer = new short[256];
        protected int[] lowpass_filter = new int[4];
        protected QUEUE queue = new QUEUE();
        protected QUEUE exqueue = new QUEUE();
        protected bool[] m_bMute = new bool[16];

        public APU(NES parent)
        {
            exsound_select = 0;

            nes = parent;
            @internal.SetParent(parent);

            last_data = last_diff = 0;

            Array.Clear(m_SoundBuffer, 0, m_SoundBuffer.Length);
            Array.Clear(lowpass_filter, 0, lowpass_filter.Length);

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

        internal void Write(ushort addr, byte data)
        {
            // $4018偼VirtuaNES屌桳億乕僩
            if (addr >= 0x4000 && addr <= 0x401F)
            {
                @internal.SyncWrite(addr, data);
                SetQueue(nes.cpu.GetTotalCycles(), addr, data);
            }
        }

        private void SetQueue(int writetime, ushort addr, byte data)
        {
            queue.data[queue.wrptr].time = writetime;
            queue.data[queue.wrptr].addr = addr;
            queue.data[queue.wrptr].data = data;
            queue.wrptr++;

            var newwrptr = (int)(queue.wrptr & (QUEUE_LENGTH - 1));
            queue.wrptr = newwrptr;

            if (queue.wrptr == queue.rdptr)
            {
                Debuger.LogError("queue overflow.");
            }
        }

        private bool GetQueue(int writetime, ref QUEUEDATA ret)
        {
            if (queue.wrptr == queue.rdptr)
            {
                return false;
            }
            if (queue.data[queue.rdptr].time <= writetime)
            {
                ret = queue.data[queue.rdptr];
                queue.rdptr++;
                var newrdptr = (int)(queue.rdptr & (QUEUE_LENGTH - 1));
                queue.rdptr = newrdptr;
                return true;
            }
            return false;
        }
    }

    public struct QUEUEDATA
    {
        public int time;
        public ushort addr;
        public byte data;
        public byte reserved;
    }

    public class QUEUE
    {
        public int rdptr;
        public int wrptr;
        public QUEUEDATA[] data = new QUEUEDATA[8192];
    }
}
