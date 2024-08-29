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

        public readonly byte[] ToBytes()
        {
            StateBuffer buffer = new StateBuffer();

            HEADER.SaveState(buffer);

            regBLOCK.SaveState(buffer);
            reg.SaveState(buffer);

            ramBLOCK.SaveState(buffer);
            ram.SaveState(buffer);

            if(WRAM!=null) buffer.Write(WRAM);

            mmuBLOCK.SaveState(buffer);
            mmu.SaveState(buffer);
            buffer.Write(CPU_MEM_BANK.ToArray());
            buffer.Write(VRAM);
            buffer.Write(CRAM.ToArray());

            mmcBLOCK.SaveState(buffer);
            mmc.SaveState(buffer);

            ctrBLOCK.SaveState(buffer);
            ctr.SaveState(buffer);

            sndBLOCK.SaveState(buffer);
            snd.SaveState(buffer);

            dskBLOCK.SaveState(buffer);
            dsk.SaveState(buffer);
            buffer.Write(dskdata);

            exctrBLOCK.SaveState(buffer);
            exctr.SaveState(buffer);

            return buffer.Data.ToArray();
        }
    }
}
