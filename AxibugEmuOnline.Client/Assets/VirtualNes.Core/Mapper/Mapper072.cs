//////////////////////////////////////////////////////////////////////////
// Mapper072  Jaleco/Type1 lower bank switch                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class Mapper072 : Mapper
    {
        public Mapper072(NES parent) : base(parent)
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

        //void Mapper072::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((data & 0x80) != 0)
            {
                SetPROM_16K_Bank(4, data & 0x0F);
            }
            else if ((data & 0x40) != 0)
            {
                SetVROM_8K_Bank(data & 0x0F);
            }
            else
            {
                if (addr >= 0xC100 && addr <= 0xC11F && data == 0x20)
                {
                    Debuger.Log($"SOUND CODE:{addr & 0x1F:X2}");

                    // OSDにするべきか…
                    if (Supporter.Config.sound.bExtraSoundEnable)
                    {
                        //TODO : 似乎VirtuaNES有直接播放某个音频文件的功能
                        //DirectSound.EsfAllStop();
                        //DirectSound.EsfPlay(ESF_MOETENNIS_00 + (addr & 0x1F));
                    }
                }
            }
        }


    }
}
