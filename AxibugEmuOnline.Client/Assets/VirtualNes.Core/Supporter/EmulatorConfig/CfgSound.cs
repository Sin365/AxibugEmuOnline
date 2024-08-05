namespace VirtualNes.Core
{
    public class CfgSound
    {
        public bool bEnable { get; set; } = true;
        public int nRate { get; set; } = 48000;
        public int nBits { get; set; } = 8;
        public int nBufferSize { get; set; } = 4;
        public int nFilterType { get; set; } = 0;
        public bool bChangeTone { get; set; } = false;
        public bool bDisableVolumeEffect { get; set; } = false;
        public bool bExtraSoundEnable { get; set; } = true;
        public short[] nVolume { get; set; } = new short[16]
        {
            100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,
        };

    }
}