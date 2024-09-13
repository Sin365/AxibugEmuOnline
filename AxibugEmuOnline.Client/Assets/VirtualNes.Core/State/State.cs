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

            if (regBLOCK.Valid)
            {
                regBLOCK.SaveState(buffer);
                reg.SaveState(buffer);
            }

            if (regBLOCK.Valid)
            {
                ramBLOCK.SaveState(buffer);
                ram.SaveState(buffer);
            }

            if (WRAM != null) buffer.Write(WRAM);

            if (mmuBLOCK.Valid)
            {
                mmuBLOCK.SaveState(buffer);
                mmu.SaveState(buffer);
                buffer.Write(CPU_MEM_BANK.ToArray());
                buffer.Write(VRAM);
                buffer.Write(CRAM.ToArray());
            }

            if (mmcBLOCK.Valid)
            {
                mmcBLOCK.SaveState(buffer);
                mmc.SaveState(buffer);
            }

            if (ctrBLOCK.Valid)
            {
                ctrBLOCK.SaveState(buffer);
                ctr.SaveState(buffer);
            }

            if (sndBLOCK.Valid)
            {
                sndBLOCK.SaveState(buffer);
                snd.SaveState(buffer);
            }

            if (dskBLOCK.Valid)
            {
                dskBLOCK.SaveState(buffer);
                dsk.SaveState(buffer);
                buffer.Write(dskdata);
            }

            if (exctrBLOCK.Valid)
            {
                exctrBLOCK.SaveState(buffer);
                exctr.SaveState(buffer);
            }

            return buffer.Data.ToArray();
        }
    }
}
