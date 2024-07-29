using System;

namespace VirtualNes.Core
{
    public abstract class Mapper
    {
        protected NES nes;
        public Mapper(NES parent)
        {
            nes = parent;
        }

        public virtual void Dispose() { }

        public abstract void Reset();

        // $8000-$FFFF Memory write
        public virtual void Write(ushort addr, byte data) { }
        // $8000-$FFFF Memory read(Dummy)
        public virtual void Read(ushort addr, byte data) { }

        // $4100-$7FFF Lower Memory read/write
        public virtual byte ReadLow(ushort addr)
        {
            // $6000-$7FFF WRAM
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                return MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF];
            }

            return (byte)(addr >> 8);
        }
        public virtual void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF] = data;
            }
        }

        // $4018-$40FF Extention register read/write
        public virtual byte ExRead(ushort addr) { return 0x00; }
        public virtual void ExWrite(ushort addr, byte data) { }

        public virtual byte ExCmdRead(EXCMDRD cmd) { return 0x00; }
        public virtual void ExCmdWrite(EXCMDWR cmd, byte data) { }

        // H sync/V sync/Clock sync
        public virtual void HSync(int scanline) { }
        public virtual void VSync() { }
        public virtual void Clock(int cycles) { }
        // PPU address bus latch
        public virtual void PPU_Latch(ushort addr) { }
        // PPU Character latch
        public virtual void PPU_ChrLatch(ushort addr) { }
        // PPU Extension character/palette
        public virtual void PPU_ExtLatchX(int x) { }
        public virtual void PPU_ExtLatch(ushort addr, ref byte chr_l, ref byte chr_h, ref byte attr) { }
        // For State save
        public virtual bool IsStateSave() { return false; }
        public virtual void SaveState(byte[] p) { }
        public virtual void LoadState(byte[] p) { }

        // Extension commands
        // For ExCmdRead command
        public enum EXCMDRD
        {
            EXCMDRD_NONE = 0,
            EXCMDRD_DISKACCESS,
        }
        // For ExCmdWrite command
        public enum EXCMDWR
        {
            EXCMDWR_NONE = 0,
            EXCMDWR_DISKINSERT,
            EXCMDWR_DISKEJECT,
        }

        public static Mapper CreateMapper(NES parent, int no)
        {
            //todo : 实现加载mapper
            switch (no)
            {
                default:
                    throw new NotImplementedException($"Mapper#{no} is not Impl");
            }
        }
    }
}
