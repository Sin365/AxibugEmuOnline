//////////////////////////////////////////////////////////////////////////
// Mapper110                                                           //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper110 : Mapper
    {
        BYTE reg0, reg1;
        public Mapper110(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);
            SetVROM_8K_Bank(0);

            reg0 = 0;
            reg1 = 0;
        }
        //void Mapper110::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x4100:
                    reg1 = (byte)(data & 0x07);
                    break;
                case 0x4101:
                    switch (reg1)
                    {
                        case 5:
                            SetPROM_32K_Bank(data);
                            break;
                        case 0:
                            reg0 = (byte)(data & 0x01);
                            SetVROM_8K_Bank(reg0);
                            break;
                        case 2:
                            reg0 = data;
                            SetVROM_8K_Bank(reg0);
                            break;
                        case 4:
                            reg0 = (byte)(reg0 | (data << 1));
                            SetVROM_8K_Bank(reg0);
                            break;
                        case 6:
                            reg0 = (byte)(reg0 | (data << 2));
                            SetVROM_8K_Bank(reg0);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        //void Mapper110::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg0;
            p[1] = reg1;
        }

        //void Mapper110::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg0 = p[0];
            reg1 = p[1];
        }

        public override bool IsStateSave()
        {
            return true;
        }

    }
}
