using System;

namespace VirtualNes.Core
{
    public class NOISE
    {
        public byte[] reg = new byte[4];        // register

        public byte enable;        // enable
        public byte holdnote;  // holdnote
        public byte volume;        // volume
        public byte xor_tap;
        public int shift_reg;

        // For Render
        public int phaseacc;
        public int freq;
        public int len_count;

        public int nowvolume;
        public int output;

        // For Envelope
        public byte env_fixed;
        public byte env_decay;
        public byte env_count;
        public byte dummy0;
        public int env_vol;

        // For sync;
        public byte[] sync_reg = new byte[4];
        public byte sync_output_enable;
        public byte sync_enable;
        public byte sync_holdnote;
        public byte dummy1;
        public int sync_len_count;

        internal void ZeroMemory()
        {
            Array.Clear(reg, 0, reg.Length);

            enable = 0;
            holdnote = 0;
            volume = 0;
            xor_tap = 0;
            shift_reg = 0;

            phaseacc = 0;
            freq = 0;
            len_count = 0;
            nowvolume = 0;
            output = 0;

            env_fixed = 0;
            env_decay = 0;
            env_count = 0;
            dummy0 = 0;
            env_vol = 0;

            Array.Clear(sync_reg, 0, sync_reg.Length);
            sync_output_enable = 0;
            sync_enable = 0;
            sync_holdnote = 0;
            dummy1 = 0;
            sync_len_count = 0;
        }
    }
}
