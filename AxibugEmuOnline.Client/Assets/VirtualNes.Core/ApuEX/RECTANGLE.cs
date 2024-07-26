using System;

namespace VirtualNes.Core
{
    public class RECTANGLE
    {
        public byte[] reg = new byte[4];        // register

        public byte enable;        // enable
        public byte holdnote;  // holdnote
        public byte volume;        // volume
        public byte complement;

        // For Render
        public int phaseacc;
        public int freq;
        public int freqlimit;
        public int adder;
        public int duty;
        public int len_count;

        public int nowvolume;

        // For Envelope
        public byte env_fixed;
        public byte env_decay;
        public byte env_count;
        public byte dummy0;
        public int env_vol;

        // For Sweep
        public byte swp_on;
        public byte swp_inc;
        public byte swp_shift;
        public byte swp_decay;
        public byte swp_count;
        public byte[] dummy1 = new byte[3];

        // For sync;
        public byte[] sync_reg = new byte[4];
        public byte sync_output_enable;
        public byte sync_enable;
        public byte sync_holdnote;
        public byte dummy2;
        public int sync_len_count;

        public void ZeroMemory()
        {
            Array.Clear(reg, 0, reg.Length);
            enable = 0;
            holdnote = 0;
            volume = 0;
            complement = 0;

            phaseacc = 0;
            freq = 0;
            freqlimit = 0;
            adder = 0;
            duty = 0;
            len_count = 0;

            nowvolume = 0;

            env_fixed = 0;
            env_decay = 0;
            env_count = 0;
            dummy0 = 0;
            env_vol = 0;

            swp_on = 0;
            swp_inc = 0;
            swp_shift = 0;
            swp_decay = 0;
            swp_count = 0;
            Array.Clear(dummy1, 0, dummy1.Length);

            Array.Clear(sync_reg, 0, sync_reg.Length);
            sync_output_enable = 0;
            sync_enable = 0;
            sync_holdnote = 0;
            dummy2 = 0;
            sync_len_count = 0;
        }
    }
}
