using System;

namespace VirtualNes.Core
{
    public class TRIANGLE
    {
        public byte[] reg = new byte[4];

        public byte enable;
        public byte holdnote;
        public byte counter_start;
        public byte dummy0;

        public int phaseacc;
        public int freq;
        public int len_count;
        public int lin_count;
        public int adder;

        public int nowvolume;

        // For sync;
        public byte[] sync_reg = new byte[4];
        public byte sync_enable;
        public byte sync_holdnote;
        public byte sync_counter_start;
        //		public byte	dummy1;
        public int sync_len_count;
        public int sync_lin_count;

        internal void ZeroMemory()
        {
            Array.Clear(reg, 0, reg.Length);

            enable = 0;
            holdnote = 0;
            counter_start = 0;
            dummy0 = 0;
            phaseacc = 0;
            freq = 0;
            len_count = 0;
            lin_count = 0;
            adder = 0;
            nowvolume = 0;
            Array.Clear(sync_reg, 0, sync_reg.Length);
            sync_enable = 0;
            sync_holdnote = 0;
            sync_counter_start = 0;

            sync_len_count = 0;
            sync_lin_count = 0;
        }
    }
}
