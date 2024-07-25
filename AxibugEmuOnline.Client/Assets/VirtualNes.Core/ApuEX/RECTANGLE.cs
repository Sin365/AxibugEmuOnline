using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
