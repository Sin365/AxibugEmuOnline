using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

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
        public List<uint> dskdata;

        public BLOCKHDR exctrBLOCK;
        public EXCTRSTAT exctr;

        public readonly byte[] ToBytes()
        {
            StateBuffer buffer = new StateBuffer();

            HEADER.SaveState(buffer);

            buffer.Write(WRAM != null ? WRAM.Length : 0);
            buffer.Write(CPU_MEM_BANK != null ? CPU_MEM_BANK.Count : 0);
            buffer.Write(VRAM != null ? VRAM.Length : 0);
            buffer.Write(CRAM != null ? CRAM.Count : 0);
            buffer.Write(dskdata != null ? dskdata.Count : 0);

            if (regBLOCK.Valid)
            {
                regBLOCK.SaveState(buffer);
                reg.SaveState(buffer);
            }

            if (ramBLOCK.Valid)
            {
                ramBLOCK.SaveState(buffer);
                ram.SaveState(buffer);
                if (WRAM != null) buffer.Write(WRAM);
            }

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
                foreach (var data in dskdata)
                {
                    buffer.Write(data);
                }
            }

            if (exctrBLOCK.Valid)
            {
                exctrBLOCK.SaveState(buffer);
                exctr.SaveState(buffer);
            }

            return buffer.Data.ToArray();
        }
        public void FromByte(byte[] data)
        {
            StateReader buffer = new StateReader(data);

            HEADER.LoadState(buffer);

            var WRAM_Length = buffer.Read_int();
            var CPU_MEM_BANK_Length = buffer.Read_int();
            var VRAM_Length = buffer.Read_int();
            var CRAM_Length = buffer.Read_int();
            var dskdata_Length = buffer.Read_int();

            while (buffer.Remain > 0)
            {
                BLOCKHDR block = new BLOCKHDR();
                block.LoadState(buffer);

                switch (block.ID)
                {
                    case "REG DATA":
                        regBLOCK = block;
                        reg.LoadState(buffer);
                        break;
                    case "RAM DATA":
                        ramBLOCK = block;
                        ram.LoadState(buffer);
                        if (WRAM_Length > 0)
                            WRAM = buffer.Read_bytes(WRAM_Length);
                        break;
                    case "MMU DATA":
                        mmuBLOCK = block;
                        mmu.LoadState(buffer);
                        if (CPU_MEM_BANK_Length > 0)
                            CPU_MEM_BANK = new List<byte>(buffer.Read_bytes(CPU_MEM_BANK_Length));
                        if (VRAM_Length > 0)
                            VRAM = buffer.Read_bytes(VRAM_Length);
                        if (CRAM_Length > 0)
                            CRAM = new List<byte>(buffer.Read_bytes(CRAM_Length));
                        break;
                    case "MMC DATA":
                        mmcBLOCK = block;
                        mmc.LoadState(buffer);
                        break;
                    case "CTR DATA":
                        ctrBLOCK = block;
                        ctr.LoadState(buffer);
                        break;
                    case "SND DATA":
                        sndBLOCK = block;
                        snd.LoadState(buffer);
                        break;
                    case "DISKDATA":
                        dskBLOCK = block;
                        dsk.LoadState(buffer);
                        if (dskdata_Length > 0)
                            dskdata = new List<uint>(buffer.Read_uints(dskdata_Length));
                        break;
                    case "EXCTRDAT":
                        exctrBLOCK = block;
                        exctr.LoadState(buffer);
                        break;
                }
            }
        }
    }
}
