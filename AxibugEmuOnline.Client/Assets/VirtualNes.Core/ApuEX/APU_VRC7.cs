using System;
using VirtualNes.Core.Emu2413;

namespace VirtualNes.Core
{
    public class APU_VRC7 : APU_INTERFACE
    {
        OPLL VRC7_OPLL;
        byte address;

        public APU_VRC7()
        {
            Emu2413API.OPLL_init(3579545, 22050);  // 仮のサンプリングレート
            VRC7_OPLL = Emu2413API.OPLL_new();

            if (VRC7_OPLL != null)
            {
                Emu2413API.OPLL_reset(VRC7_OPLL);
                Emu2413API.OPLL_reset_patch(VRC7_OPLL, Emu2413API.OPLL_VRC7_TONE);
                VRC7_OPLL.masterVolume = 128;
            }

            // 仮設定
            Reset(APU_CLOCK, 22050);
        }

        public override void Dispose()
        {
            if (VRC7_OPLL != null)
            {
                Emu2413API.OPLL_delete(VRC7_OPLL);
                VRC7_OPLL = null;
                //		OPLL_close();	// 無くても良い(中身無し)
            }
        }

        public override void Reset(float fClock, int nRate)
        {
            if (VRC7_OPLL != null)
            {
                Emu2413API.OPLL_reset(VRC7_OPLL);
                Emu2413API.OPLL_reset_patch(VRC7_OPLL, Emu2413API.OPLL_VRC7_TONE);
                VRC7_OPLL.masterVolume = 128;
            }

            address = 0;

            Setup(fClock, nRate);
        }

        public override void Setup(float fClock, int nRate)
        {
            Emu2413API.OPLL_setClock((UInt32)(fClock * 2.0f), (UInt32)nRate);
        }

        public override void Write(ushort addr, byte data)
        {
            if (VRC7_OPLL != null)
            {
                if (addr == 0x9010)
                {
                    address = data;
                }
                else if (addr == 0x9030)
                {
                    Emu2413API.OPLL_writeReg(VRC7_OPLL, address, data);
                }
            }
        }

        public override int Process(int channel)
        {
            if (VRC7_OPLL != null)
                return Emu2413API.OPLL_calc(VRC7_OPLL);

            return 0;
        }

        float[] blkmul = { 0.5f, 1.0f, 2.0f, 4.0f, 8.0f, 16.0f, 32.0f, 64.0f };
        public override int GetFreq(int channel)
        {
            if (VRC7_OPLL != null && channel < 8)
            {
                int fno = ((VRC7_OPLL.reg[0x20 + channel] & 0x01) << 8) + VRC7_OPLL.reg[0x10 + channel];
                int blk = (VRC7_OPLL.reg[0x20 + channel] >> 1) & 0x07;

                if ((VRC7_OPLL.reg[0x20 + channel] & 0x10) != 0)
                {
                    return (int)((256.0d * fno * blkmul[blk]) / ((1 << 18) / (3579545.0 / 72.0)));
                }
            }

            return 0;
        }

        public override uint GetSize()
        {
            return 0;
        }

        public override void SaveState(StateBuffer buffer)
        {
            //not impl
        }
    }
}
