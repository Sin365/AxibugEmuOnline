using System.Collections.Generic;

namespace VirtualNes.Core
{
    public struct State
    {
        public FILEHDR2 HEADER;

        public BLOCKHDR regBLOCK;
        public REGSTAT reg;

        public BLOCKHDR ramBLOCK;
        public RAMSTAT ram;
        /// <summary> Maybe null cause by rom IsSaveRAM() </summary>
        public byte[] WRAM;

        public BLOCKHDR mmuBLOCK;
        public MMUSTAT mmu;
        public List<byte> CPU_MEM_BANK;
        public byte[] VRAM;
        public List<byte> CRAM;

        public BLOCKHDR mmcBLOCK;
        public MMCSTAT mmc;

        public BLOCKHDR ctrBLOCK;
        public CTRSTAT ctr;

        public BLOCKHDR sndBLOCK;
        public SNDSTAT snd;

        public BLOCKHDR dskBLOCK;
        public DISKDATA dsk;
        public uint dskdata;

        public BLOCKHDR exctrBLOCK;
        public EXCTRSTAT exctr;
    }
}
