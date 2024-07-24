namespace VirtualNes.Core
{
    public class PPU
    {
        private NES m_nes;
        private byte[] lpScreen;
        private byte[] lpColormode;
        private bool bVSMode;
        private int nVSColorMap;
        private byte VSSecurityData;
        private byte[] Bit2Rev = new byte[256];
        public PPU(NES nes)
        {
            m_nes = nes;
            lpScreen = null;
            lpColormode = null;

            bVSMode = false;
            nVSColorMap = -1;
            VSSecurityData = 0;

            for (int i = 0; i < 256; i++)
            {
                byte m = 0x80;
                byte c = 0;
                for (int j = 0; j < 8; j++)
                {
                    if ((i & (1 << j)) > 0) c |= m;
                    m >>= 1;
                }
                Bit2Rev[i] = c;
            }
        }
    }
}
