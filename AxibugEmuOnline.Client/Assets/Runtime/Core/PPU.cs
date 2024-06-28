namespace AxibugEmuOnline.Client.UNES
{
    public sealed partial class PPU : Addressable
    {   
        public PPU(Emulator emulator) : base(emulator, 0x3FFF)
        {
            InitializeMemoryMap();
        }
    }
}
