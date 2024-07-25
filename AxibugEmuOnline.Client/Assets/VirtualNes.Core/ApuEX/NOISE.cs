using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
