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
    }
}
