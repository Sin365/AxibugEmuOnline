using Codice.CM.Client.Differences;
using VirtualNes.Core;

namespace VirtualNes
{
    public static class MMU
    {
        // CPU 儊儌儕僶儞僋
        public static ArrayRef<byte>[] CPU_MEM_BANK = new ArrayRef<byte>[8];            // 8K扨埵
        public static byte[] CPU_MEM_TYPE = new byte[8];
        public static int[] CPU_MEM_PAGE = new int[8];	                    // 僗僥乕僩僙乕僽梡
        // PPU 儊儌儕僶儞僋
        public static ArrayRef<byte>[] PPU_MEM_BANK = new ArrayRef<byte>[12];           // 1K扨埵
        public static byte[] PPU_MEM_TYPE = new byte[12];
        public static int[] PPU_MEM_PAGE = new int[12];                 // 僗僥乕僩僙乕僽梡
        public static byte[] CRAM_USED = new byte[16];		            // 僗僥乕僩僙乕僽梡

        // NES儊儌儕
        public static byte[] RAM = new byte[8 * 1024];		            // NES撪憻RAM
        public static byte[] WRAM = new byte[128 * 1024];               // 儚乕僋/僶僢僋傾僢僾RAM
        public static byte[] DRAM = new byte[40 * 1024];                // 僨傿僗僋僔僗僥儉RAM
        public static byte[] XRAM = new byte[8 * 1024];                 // 僟儈乕僶儞僋
        public static byte[] ERAM = new byte[32 * 1024];                // 奼挘婡婍梡RAM

        public static byte[] CRAM = new byte[32 * 1024];                // 僉儍儔僋僞僷僞乕儞RAM
        public static byte[] VRAM = new byte[4 * 1024];                 // 僱乕儉僥乕僽儖/傾僩儕價儏乕僩RAM

        public static byte[] SPRAM = new byte[0x100];                   // 僗僾儔僀僩RAM
        public static byte[] BGPAL = new byte[0x10];                    // BG僷儗僢僩
        public static byte[] SPPAL = new byte[0x10];		            // SP僷儗僢僩
        // 儗僕僗僞
        public static byte[] CPUREG = new byte[0x18];                   // Nes $4000-$4017
        public static byte[] PPUREG = new byte[0x04];		            // Nes $2000-$2003

        // PPU撪晹儗僕僗僞
        public static byte PPU56Toggle;                                 // $2005-$2006 Toggle
        public static byte PPU7_Temp;                                   // $2007 read buffer
        public static ushort loopy_t;                                   // same as $2005/$2006
        public static ushort loopy_v;                                   // same as $2005/$2006
        public static ushort loopy_x;                                   // tile x offset

        // ROM僨乕僞億僀儞僞
        public static byte[] PROM;        // PROM ptr
        public static byte[] VROM;      // VROM ptr

        // For dis...
        public static byte PROM_ACCESS;

        // ROM 僶儞僋僒僀僘
        public static int PROM_8K_SIZE, PROM_16K_SIZE, PROM_32K_SIZE;
        public static int VROM_1K_SIZE, VROM_2K_SIZE, VROM_4K_SIZE, VROM_8K_SIZE;

        // 儊儌儕僞僀僾
        // For PROM (CPU)
        public const byte BANKTYPE_ROM = 0x00;
        public const byte BANKTYPE_RAM = 0xFF;
        public const byte BANKTYPE_DRAM = 0x01;
        public const byte BANKTYPE_MAPPER = 0x80;
        // For VROM/VRAM=/CRAM (PPU)           
        public const byte BANKTYPE_VROM = 0x00;
        public const byte BANKTYPE_CRAM = 0x01;
        public const byte BANKTYPE_VRAM = 0x80;

        // =ミラータイプ;                       
        public const byte VRAM_HMIRROR = 0x00;     // Horizontal
        public const byte VRAM_VMIRROR = 0x01;     // Virtical
        public const byte VRAM_MIRROR4 = 0x02;     // All screen
        public const byte VRAM_MIRROR4L = 0x03;    // PA10 L固定 $2000-$23FFのミラー
        public const byte VRAM_MIRROR4H = 0x04;    // PA10 H固定 $2400-$27FFのミラー

        // Frame-IRQ儗僕僗僞($4017)
        public static int FrameIRQ;

        internal static void SetPROM_Bank(byte page, byte[] ptr, byte type)
        {
            CPU_MEM_BANK[page] = new ArrayRef<byte>(ptr, 0, ptr.Length);
            CPU_MEM_TYPE[page] = type;
            CPU_MEM_PAGE[page] = 0;
        }

        internal static void SetPROM_Bank(byte page, ArrayRef<byte> ptr, byte type)
        {
            CPU_MEM_BANK[page] = ptr;
            CPU_MEM_TYPE[page] = type;
            CPU_MEM_PAGE[page] = 0;
        }

        internal static void SetPROM_4K_Bank(ushort addr, int bank)
        {
            throw new System.NotImplementedException();

            bank %= (PROM_8K_SIZE * 2);

            //TODO
            //memcpy(&CPU_MEM_BANK[addr >> 13][addr & 0x1FFF], PROM + 0x1000 * bank, 0x1000);
            ////	memcpy( &CPU_MEM_BANK[addr>>13][addr&0x1FFF], YSRAM+0x1000*bank, 0x1000);
            CPU_MEM_TYPE[addr >> 13] = BANKTYPE_ROM;
            CPU_MEM_PAGE[addr >> 13] = 0;
        }


        internal static void SetPROM_8K_Bank(byte page, int bank)
        {
            bank %= PROM_8K_SIZE;
            CPU_MEM_BANK[page] = new ArrayRef<byte>(MMU.PROM, 0x2000 * bank, MMU.PROM.Length - 0x2000 * bank);
            CPU_MEM_TYPE[page] = BANKTYPE_ROM;
            CPU_MEM_PAGE[page] = bank;
        }

        internal static void SetPROM_16K_Bank(byte page, int bank)
        {
            SetPROM_8K_Bank((byte)(page + 0), bank * 2 + 0);
            SetPROM_8K_Bank((byte)(page + 1), bank * 2 + 1);
        }

        internal static void SetPROM_32K_Bank(int bank)
        {
            SetPROM_8K_Bank(4, bank * 4 + 0);
            SetPROM_8K_Bank(5, bank * 4 + 1);
            SetPROM_8K_Bank(6, bank * 4 + 2);
            SetPROM_8K_Bank(7, bank * 4 + 3);
        }

        internal static void SetPROM_32K_Bank(int bank0, int bank1, int bank2, int bank3)
        {
            SetPROM_8K_Bank(4, bank0);
            SetPROM_8K_Bank(5, bank1);
            SetPROM_8K_Bank(6, bank2);
            SetPROM_8K_Bank(7, bank3);
        }

        // PPU VROM bank
        internal static void SetVROM_Bank(byte page, ArrayRef<byte> ptr, byte type)
        {
            PPU_MEM_BANK[page] = ptr;
            PPU_MEM_TYPE[page] = type;
            PPU_MEM_PAGE[page] = 0;
        }

        internal static void SetVROM_1K_Bank(byte page, int bank)
        {
            bank %= VROM_1K_SIZE;
            PPU_MEM_BANK[page] = new ArrayRef<byte>(VROM, 0x0400 * bank, VROM.Length - (0x0400 * bank));
            PPU_MEM_TYPE[page] = BANKTYPE_VROM;
            PPU_MEM_PAGE[page] = bank;
        }

        internal static void SetVROM_2K_Bank(byte page, int bank)
        {
            SetVROM_1K_Bank((byte)(page + 0), bank * 2 + 0);
            SetVROM_1K_Bank((byte)(page + 1), bank * 2 + 1);
        }

        internal static void SetVROM_4K_Bank(byte page, int bank)
        {
            SetVROM_1K_Bank((byte)(page + 0), bank * 4 + 0);
            SetVROM_1K_Bank((byte)(page + 1), bank * 4 + 1);
            SetVROM_1K_Bank((byte)(page + 2), bank * 4 + 2);
            SetVROM_1K_Bank((byte)(page + 3), bank * 4 + 3);
        }

        internal static void SetVROM_8K_Bank(int bank)
        {
            for (byte i = 0; i < 8; i++)
            {
                SetVROM_1K_Bank(i, bank * 8 + i);
            }
        }

        internal static void SetVROM_8K_Bank(int bank0, int bank1, int bank2, int bank3,
             int bank4, int bank5, int bank6, int bank7)
        {
            SetVROM_1K_Bank(0, bank0);
            SetVROM_1K_Bank(1, bank1);
            SetVROM_1K_Bank(2, bank2);
            SetVROM_1K_Bank(3, bank3);
            SetVROM_1K_Bank(4, bank4);
            SetVROM_1K_Bank(5, bank5);
            SetVROM_1K_Bank(6, bank6);
            SetVROM_1K_Bank(7, bank7);
        }

        internal static void SetCRAM_1K_Bank(byte page, int bank)
        {
            bank &= 0x1F;
            PPU_MEM_BANK[page] = new ArrayRef<byte>(MMU.CRAM, 0x0400 * bank, MMU.CRAM.Length - 0x0400 * bank);
            PPU_MEM_TYPE[page] = BANKTYPE_CRAM;
            PPU_MEM_PAGE[page] = bank;

            CRAM_USED[bank >> 2] = 0xFF;	// CRAM巊梡僼儔僌
        }

        internal static void SetCRAM_2K_Bank(byte page, int bank)
        {
            SetCRAM_1K_Bank((byte)(page + 0), bank * 2 + 0);
            SetCRAM_1K_Bank((byte)(page + 1), bank * 2 + 1);
        }

        internal static void SetCRAM_4K_Bank(byte page, int bank)
        {
            SetCRAM_1K_Bank((byte)(page + 0), bank * 4 + 0);
            SetCRAM_1K_Bank((byte)(page + 1), bank * 4 + 1);
            SetCRAM_1K_Bank((byte)(page + 2), bank * 4 + 2);
            SetCRAM_1K_Bank((byte)(page + 3), bank * 4 + 3);
        }

        internal static void SetCRAM_8K_Bank(int bank)
        {
            for (byte i = 0; i < 8; i++)
            {
                SetCRAM_1K_Bank(i, bank * 8 + 1);
            }
        }

        internal static void SetVRAM_1K_Bank(byte page, int bank)
        {
            bank &= 3;
            PPU_MEM_BANK[page] = new ArrayRef<byte>(VRAM, 0x0400 * bank, VRAM.Length - 0x0400 * bank);
            PPU_MEM_TYPE[page] = BANKTYPE_VRAM;
            PPU_MEM_PAGE[page] = bank;
        }

        internal static void SetVRAM_Bank(int bank0, int bank1, int bank2, int bank3)
        {
            SetVRAM_1K_Bank(8, bank0);
            SetVRAM_1K_Bank(9, bank1);
            SetVRAM_1K_Bank(10, bank2);
            SetVRAM_1K_Bank(11, bank3);
        }

        internal static void SetVRAM_Mirror(int type)
        {
            switch (type)
            {
                case VRAM_HMIRROR:
                    SetVRAM_Bank(0, 0, 1, 1);
                    break;
                case VRAM_VMIRROR:
                    SetVRAM_Bank(0, 1, 0, 1);
                    break;
                case VRAM_MIRROR4L:
                    SetVRAM_Bank(0, 0, 0, 0);
                    break;
                case VRAM_MIRROR4H:
                    SetVRAM_Bank(1, 1, 1, 1);
                    break;
                case VRAM_MIRROR4:
                    SetVRAM_Bank(0, 1, 2, 3);
                    break;
            }
        }

        internal static void SetVRAM_Mirror(int bank0, int bank1, int bank2, int bank3)
        {
            SetVRAM_1K_Bank(8, bank0);
            SetVRAM_1K_Bank(9, bank1);
            SetVRAM_1K_Bank(10, bank2);
            SetVRAM_1K_Bank(11, bank3);
        }
    }
}
