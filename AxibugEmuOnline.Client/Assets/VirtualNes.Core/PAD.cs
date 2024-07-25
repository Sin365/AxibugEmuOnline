using System;

namespace VirtualNes.Core
{
    public class PAD
    {
        private NES nes;
        private int excontroller_select;
        private EXPAD expad;
        private bool bStrobe;
        private bool bSwapButton;
        private bool bSwapPlayer;
        private bool bZapperMode;
        private VSType nVSSwapType;
        private byte[] padbit = new byte[4];
        private byte micbit;
        private byte[] padbitsync = new byte[4];
        private byte micbitsync;
        private bool bBarcodeWorld;

        public uint pad1bit, pad2bit, pad3bit, pad4bit;

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

        internal byte Read(ushort addr)
        {
            byte data = 0x00;

            if (addr == 0x4016)
            {
                data = (byte)(pad1bit & 1);
                pad1bit >>= 1;
                data |= (byte)(((pad3bit & 1)) << 1);
                pad3bit >>= 1;
                // Mic
                if (!nes.rom.IsVSUNISYSTEM())
                {
                    data |= micbitsync;
                }
                if (expad != null)
                {
                    data |= expad.Read4016();
                }
            }
            if (addr == 0x4017)
            {
                data = (byte)(pad2bit & 1);
                pad2bit >>= 1;
                data |= (byte)((pad4bit & 1) << 1);
                pad4bit >>= 1;

                if (expad != null)
                {
                    data |= expad.Read4017();
                }

                if (bBarcodeWorld)
                {
                    data |= nes.Barcode2();
                }
            }

            return data;
        }
        public void Dispose() { }
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
