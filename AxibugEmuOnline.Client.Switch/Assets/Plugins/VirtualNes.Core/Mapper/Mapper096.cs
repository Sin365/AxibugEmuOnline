////////////////////////////////
// Mapper096  Bandai 74161                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper096 : Mapper
    {
        BYTE[] reg = new byte[2];
        public Mapper096(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            reg[0] = reg[1] = 0;

            SetPROM_32K_Bank(0, 1, 2, 3);
            SetBank();

            SetVRAM_Mirror(VRAM_MIRROR4L);
        }

        //void Mapper096::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_32K_Bank(data & 0x03);

            reg[0] = (byte)((data & 0x04) >> 2);
            SetBank();
        }

        public override void PPU_Latch(ushort addr)
        {
            if ((addr & 0xF000) == 0x2000)
            {
                reg[1] = (byte)((addr >> 8) & 0x03);
                SetBank();
            }
        }

        void SetBank()
        {
            SetCRAM_4K_Bank(0, reg[0] * 4 + reg[1]);
            SetCRAM_4K_Bank(4, reg[0] * 4 + 0x03);
        }

        //void Mapper096::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
        }

        //void Mapper096::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
        }


        public override bool IsStateSave()
        {
            return true;
        }

    }
}
