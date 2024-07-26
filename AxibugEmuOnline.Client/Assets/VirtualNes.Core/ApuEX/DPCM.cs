namespace VirtualNes.Core
{
    public class DPCM
    {
        public byte[] reg = new byte[4];
        public byte enable;
        public byte looping;
        public byte cur_byte;
        public byte dpcm_value;

        public int freq;
        public int phaseacc;
        public int output;

        ushort address, cache_addr;
        public int dmalength, cache_dmalength;
        public int dpcm_output_real, dpcm_output_fake, dpcm_output_old, dpcm_output_offset;

        // For sync
        public byte[] sync_reg = new byte[4];
        public byte sync_enable;
        public byte sync_looping;
        public byte sync_irq_gen;
        public byte sync_irq_enable;
        public int sync_cycles, sync_cache_cycles;
        public int sync_dmalength, sync_cache_dmalength;
    }
}
