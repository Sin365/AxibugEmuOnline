using System;

namespace VirtualNes.Core
{
    public class PAD
    {
        protected NES nes;
        protected int excontroller_select;
        protected EXPAD expad;
        protected bool bStrobe;
        protected bool bSwapButton;
        protected bool bSwapPlayer;
        protected bool bZapperMode;
        protected VSType nVSSwapType;
        protected byte[] padbit = new byte[4];
        protected byte micbit;
        protected byte[] padbitsync = new byte[4];
        protected byte micbitsync;

        public PAD(NES parent)
        {
            nes = parent;
            excontroller_select = 0;
            expad = null;
            bStrobe = false;
            bSwapButton = false;
            bSwapPlayer = false;
            bZapperMode = false;
            nVSSwapType = VSType.VS_TYPE0;

            padbit[0] = padbit[1] = padbit[2] = padbit[3] = 0;
            micbit = 0;

            padbitsync[0] = padbitsync[1] = padbitsync[2] = padbitsync[3] = 0;
            micbitsync = 0;
        }

        public void Dispose()
        {
        }
    }

    public enum VSType
    {
        VS_TYPE0 = 0,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P No reverse
        VS_TYPE1,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse
        VS_TYPE2,   // SELECT1P=START1P/START1P =START2P 1P/2P No reverse
        VS_TYPE3,   // SELECT1P=START1P/START1P =START2P 1P/2P Reverse
        VS_TYPE4,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P No reverse (Protection)
        VS_TYPE5,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse    (Protection)
        VS_TYPE6,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse	(For Golf)
        VS_TYPEZ,	// ZAPPER
    }
}
