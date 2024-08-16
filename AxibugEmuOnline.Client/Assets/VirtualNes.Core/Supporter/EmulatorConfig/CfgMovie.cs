namespace VirtualNes.Core
{
    public class CfgMovie
    {
        public byte[] bUsePlayer = new byte[4] { 0xFF, 0x00, 0x00, 0x00 };
        public bool bRerecord = true;
        public bool bLoopPlay = false;
        public bool bResetRec = false;
        public bool bPadDisplay = false;
        public bool bTimeDisplay = false;
    }
}