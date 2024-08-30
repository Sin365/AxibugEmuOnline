namespace VirtualNes.Core
{
    public struct MMUSTAT : IStateBufferObject
    {
        public byte[] CPU_MEM_TYPE;
        public ushort[] CPU_MEM_PAGE;
        public byte[] PPU_MEM_TYPE;
        public ushort[] PPU_MEM_PAGE;
        public byte[] CRAM_USED;

        public static MMUSTAT GetDefault()
        {
            var res = new MMUSTAT();

            res.CPU_MEM_TYPE = new byte[8];
            res.CPU_MEM_PAGE = new ushort[8];
            res.PPU_MEM_TYPE = new byte[12];
            res.PPU_MEM_PAGE = new ushort[12];
            res.CRAM_USED = new byte[8];

            return res;
        }

        public uint GetSize()
        {
            return (uint)(CPU_MEM_TYPE.Length + CPU_MEM_PAGE.Length + PPU_MEM_TYPE.Length + PPU_MEM_PAGE.Length + CRAM_USED.Length);
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(CPU_MEM_TYPE);
            buffer.Write(CPU_MEM_PAGE);
            buffer.Write(PPU_MEM_TYPE);
            buffer.Write(PPU_MEM_PAGE);
            buffer.Write(CRAM_USED);
        }
    }
}
