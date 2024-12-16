/////////////////////////////////
// Mapper092  Jaleco/Type1 Higher bank switch                           //
//////////////////////////////////////////////////////////////////////////
using VirtualNes.Core.Debug;
using static VirtualNes.MMU;
using INT = System.Int32;

namespace VirtualNes.Core
{
    public class Mapper092 : Mapper
    {
        public Mapper092(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            if (VROM_8K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper092::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            //DEBUGOUT( "A:%04X D:%02X\n", addr, data );

            data = (byte)(addr & 0xFF);

            if (addr >= 0x9000)
            {
                if ((data & 0xF0) == 0xD0)
                {
                    SetPROM_16K_Bank(6, data & 0x0F);
                }
                else if ((data & 0xF0) == 0xE0)
                {
                    SetVROM_8K_Bank(data & 0x0F);
                }
            }
            else
            {
                if ((data & 0xF0) == 0xB0)
                {
                    SetPROM_16K_Bank(6, data & 0x0F);
                }
                else if ((data & 0xF0) == 0x70)
                {
                    SetVROM_8K_Bank(data & 0x0F);
                }
                else if ((data & 0xF0) == 0xC0)
                {
                    INT[] tbl = new int[]{ 3, 4, 5, 6, 0, 1, 2, 7,
                      9,10, 8,11,13,12,14,15 };

                    // OSDにするべきか…
                    if (Supporter.Config.sound.bExtraSoundEnable)
                    {
                        //TODO : 似乎VirtuaNES有直接播放某个音频文件的功能
                        Debuger.Log($"CODE {data:X2}");
                        //DirectSound.EsfAllStop();
                        //DirectSound.EsfPlay(ESF_MOEPRO_STRIKE + tbl[data & 0x0F]);
                    }
                }
            }
        }

    }
}
