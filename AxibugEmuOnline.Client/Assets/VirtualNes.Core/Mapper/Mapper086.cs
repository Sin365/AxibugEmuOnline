//////////////////////////////////////////////////////////////////////////
// Mapper086  Jaleco Early Mapper #2                                    //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper086 : Mapper
    {
        BYTE reg, cnt;
        public Mapper086(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
            SetVROM_8K_Bank(0);
            reg = 0xFF;
            cnt = 0;
        }

        //void Mapper086::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr == 0x6000)
            {
                SetPROM_32K_Bank((data & 0x30) >> 4);

                SetVROM_8K_Bank((data & 0x03) | ((data & 0x40) >> 4));
            }
            if (addr == 0x7000)
            {
                if ((reg & 0x10) == 0 && ((data & 0x10) != 0) && cnt == 0)
                {
                    //DEBUGOUT( "WR:$%02X\n", data );
                    if ((data & 0x0F) == 0      // Strike
                     || (data & 0x0F) == 5)
                    {   // Foul
                        cnt = 60;       // 次の発声を1秒程禁止する
                    }

                    // OSDにするべきか…
                    if (Supporter.S.Config.sound.bExtraSoundEnable)
                    {
                        //TODO : 似乎VirtuaNES有直接播放某个音频文件的功能
                        //DirectSound.EsfAllStop();
                        //DirectSound.EsfPlay(ESF_MOEPRO_STRIKE + (data & 0x0F));
                    }
                }
                reg = data;
            }
        }

        //void Mapper086::VSync()
        public override void VSync()
        {
            if (cnt != 0)
            {
                cnt--;
            }
        }



    }
}
