//////////////////////////////////////////////////////////////////////////
// Mapper243  PC-Sachen/Hacker                                          //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{

    public class Mapper243 : Mapper
    {
        BYTE[] reg = new byte[4];
        public Mapper243(NES parent) : base(parent)
        {
        }

        //void Mapper243::Reset()
        public override void Reset()
        {
            SetPROM_32K_Bank(0);
            if (VROM_8K_SIZE > 4)
            {
                SetVROM_8K_Bank(4);
            }
            else if (VROM_8K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }

            SetVRAM_Mirror(VRAM_HMIRROR);

            reg[0] = 0;
            reg[1] = 0;
            reg[2] = 3;
            reg[3] = 0;
        }

        //void Mapper243::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if ((addr & 0x4101) == 0x4100)
            {
                reg[0] = data;
            }
            else if ((addr & 0x4101) == 0x4101)
            {
                switch (reg[0] & 0x07)
                {
                    case 0:
                        reg[1] = 0;
                        reg[2] = 3;
                        break;
                    case 4:
                        reg[2] = (byte)((reg[2] & 0x06) | (data & 0x01));
                        break;
                    case 5:
                        reg[1] = (byte)(data & 0x01);
                        break;
                    case 6:
                        reg[2] = (byte)((reg[2] & 0x01) | ((data & 0x03) << 1));
                        break;
                    case 7:
                        reg[3] = (byte)(data & 0x01);
                        break;
                    default:
                        break;
                }

                SetPROM_32K_Bank(reg[1]);
                SetVROM_8K_Bank(reg[2] * 8 + 0, reg[2] * 8 + 1, reg[2] * 8 + 2, reg[2] * 8 + 3,
                         reg[2] * 8 + 4, reg[2] * 8 + 5, reg[2] * 8 + 6, reg[2] * 8 + 7);

                if (reg[3] != 0)
                {
                    SetVRAM_Mirror(VRAM_VMIRROR);
                }
                else
                {
                    SetVRAM_Mirror(VRAM_HMIRROR);
                }
            }
        }

        public override bool IsStateSave()
        {
            return true;
        }



        //void Mapper243::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            //p[0] = reg[0];
            //p[1] = reg[1];
            //p[2] = reg[2];
            //p[3] = reg[3];
        }

        //void Mapper243::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            //reg[0] = p[0];
            //reg[1] = p[1];
            //reg[2] = p[2];
            //reg[3] = p[3];
        }

    }
}
