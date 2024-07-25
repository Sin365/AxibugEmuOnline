using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes
{
    public static class MMU
    {
        // CPU 儊儌儕僶儞僋
        public static byte[][] CPU_MEM_BANK = new byte[8][];            // 8K扨埵

        // PPU 儊儌儕僶儞僋
        public static byte[][] PPU_MEM_BANK = new byte[12][];           // 1K扨埵
        public static byte[] PPU_MEM_TYPE = new byte[12];
        public static int[] PPU_MEM_PAGE = new int[12];                 // 僗僥乕僩僙乕僽梡
        public static byte[] CRAM_USED = new byte[16];		            // 僗僥乕僩僙乕僽梡

        // NES儊儌儕
        public static byte[] RAM = new byte[8 * 1024];		            // NES撪憻RAM
        public static byte[] WARM = new byte[128 * 1024];               // 儚乕僋/僶僢僋傾僢僾RAM
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

        // 儈儔乕僞僀僾                        
        public const byte VRAM_HMIRROR = 0x00;     // Horizontal
        public const byte VRAM_VMIRROR = 0x01;     // Virtical
        public const byte VRAM_MIRROR4 = 0x02;     // All screen
        public const byte VRAM_MIRROR4L = 0x03;     // PA10 L屌掕 $2000-$23FF偺儈儔乕
        public const byte VRAM_MIRROR4H = 0x04;     // PA10 H屌掕 $2400-$27FF偺儈儔乕

    }
}
