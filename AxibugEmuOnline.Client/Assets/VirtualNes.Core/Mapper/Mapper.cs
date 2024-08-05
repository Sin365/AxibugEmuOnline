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
                return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
            }

            return (byte)(addr >> 8);
        }
        public virtual void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
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
                case 4: return new Mapper004(parent);
                case 16: return new Mapper016(parent);
                case 17: return new Mapper017(parent);
                case 18: return new Mapper018(parent);
                case 19: return new Mapper019(parent);
                case 21: return new Mapper021(parent);
                case 22: return new Mapper022(parent);
                case 23: return new Mapper023(parent);
                case 24: return new Mapper024(parent);
                case 25: return new Mapper025(parent);
                case 26: return new Mapper026(parent);
                case 27: return new Mapper027(parent);
                case 32: return new Mapper032(parent);
                case 33: return new Mapper033(parent);
                case 34: return new Mapper034(parent);
                case 35: return new Mapper035(parent);
                case 40: return new Mapper040(parent);
                case 41: return new Mapper041(parent);
                case 42: return new Mapper042(parent);
                case 43: return new Mapper043(parent);
                case 44: return new Mapper044(parent);
                case 45: return new Mapper045(parent);
                case 46: return new Mapper046(parent);
                case 47: return new Mapper047(parent);
                case 48: return new Mapper048(parent);
                case 50: return new Mapper050(parent);
                case 51: return new Mapper051(parent);
                case 57: return new Mapper057(parent);
                case 58: return new Mapper058(parent);
                case 60: return new Mapper060(parent);
                case 61: return new Mapper061(parent);
                case 62: return new Mapper062(parent);
                case 64: return new Mapper064(parent);
                case 65: return new Mapper065(parent);
                case 66: return new Mapper066(parent);
                case 67: return new Mapper067(parent);
                case 68: return new Mapper068(parent);
                case 69: return new Mapper069(parent);
                case 70: return new Mapper070(parent);
                case 71: return new Mapper071(parent);
                case 72: return new Mapper072(parent);
                case 73: return new Mapper073(parent);
                case 74: return new Mapper074(parent);
                case 75: return new Mapper075(parent);
                case 76: return new Mapper076(parent);
                case 77: return new Mapper077(parent);
                case 78: return new Mapper078(parent);
                case 79: return new Mapper079(parent);
                case 80: return new Mapper080(parent);
                case 82: return new Mapper082(parent);
                case 83: return new Mapper083(parent);
                case 85: return new Mapper085(parent);
                case 86: return new Mapper086(parent);
                case 87: return new Mapper087(parent);
                case 88: return new Mapper088(parent);
                case 89: return new Mapper089(parent);
                case 90: return new Mapper090(parent);
                case 91: return new Mapper091(parent);
                case 92: return new Mapper092(parent);
                case 93: return new Mapper093(parent);
                case 94: return new Mapper094(parent);
                case 95: return new Mapper095(parent);
                case 96: return new Mapper096(parent);
                case 97: return new Mapper097(parent);
                case 99: return new Mapper099(parent);
                case 100: return new Mapper100(parent);
                case 101: return new Mapper101(parent);
                case 105: return new Mapper105(parent);
                case 108: return new Mapper108(parent);
                case 109: return new Mapper109(parent);
                case 110: return new Mapper110(parent);
                case 111: return new Mapper111(parent);
                case 112: return new Mapper112(parent);
                case 113: return new Mapper113(parent);
                case 114: return new Mapper114(parent);
                case 115: return new Mapper115(parent);
                case 116: return new Mapper116(parent);
                case 117: return new Mapper117(parent);
                case 118: return new Mapper118(parent);
                case 119: return new Mapper119(parent);
                case 122: return new Mapper122(parent);
                case 133: return new Mapper133(parent);
                case 134: return new Mapper134(parent);
                case 135: return new Mapper135(parent);
                case 140: return new Mapper140(parent);
                case 142: return new Mapper142(parent);
                case 151: return new Mapper151(parent);
                case 160: return new Mapper160(parent);
                case 162: return new Mapper162(parent);
                case 163: return new Mapper163(parent);
                case 164: return new Mapper164(parent);
                case 165: return new Mapper165(parent);
                case 167: return new Mapper167(parent);
                case 175: return new Mapper175(parent);
                case 176: return new Mapper176(parent);
                case 178: return new Mapper178(parent);
                case 180: return new Mapper180(parent);
                case 181: return new Mapper181(parent);
                case 182: return new Mapper182(parent);
                case 183: return new Mapper183(parent);
                case 185: return new Mapper185(parent);
                case 187: return new Mapper187(parent);
                case 188: return new Mapper188(parent);
                case 189: return new Mapper189(parent);
                case 190: return new Mapper190(parent);
                case 191: return new Mapper191(parent);
                case 192: return new Mapper192(parent);
                case 193: return new Mapper193(parent);
                case 194: return new Mapper194(parent);
                case 195: return new Mapper195(parent);
                case 198: return new Mapper198(parent);
                case 199: return new Mapper199(parent);
                case 200: return new Mapper200(parent);
                case 201: return new Mapper201(parent);
                case 202: return new Mapper202(parent);
                case 216: return new Mapper216(parent);
                case 222: return new Mapper222(parent);
                case 225: return new Mapper225(parent);
                case 226: return new Mapper226(parent);
                case 227: return new Mapper227(parent);
                case 228: return new Mapper228(parent);
                case 229: return new Mapper229(parent);
                case 230: return new Mapper230(parent);
                case 231: return new Mapper231(parent);
                case 232: return new Mapper232(parent);
                case 233: return new Mapper233(parent);
                case 234: return new Mapper234(parent);
                case 235: return new Mapper235(parent);
                case 236: return new Mapper236(parent);
                case 240: return new Mapper240(parent);
                case 241: return new Mapper241(parent);
                case 242: return new Mapper242(parent);
                case 243: return new Mapper243(parent);
                case 244: return new Mapper244(parent);
                case 245: return new Mapper245(parent);
                case 246: return new Mapper246(parent);
                case 248: return new Mapper248(parent);
                case 249: return new Mapper249(parent);
                case 251: return new Mapper251(parent);
                case 252: return new Mapper252(parent);
                case 254: return new Mapper254(parent);
                case 255: return new Mapper255(parent);

                default:
                    throw new NotImplementedException($"Mapper#{no:000} is not Impl");
            }
        }
    }
}
