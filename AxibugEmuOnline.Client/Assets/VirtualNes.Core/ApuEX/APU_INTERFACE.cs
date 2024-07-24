using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes.Core
{
    public abstract class APU_INTERFACE
    {
        public abstract void Reset(float fClock, int nRate);
        public abstract void Setup(float fClock, int nRate);
        public abstract void Write(ushort addr, byte data);
        public abstract int Process(int channel);
        public virtual byte Read(ushort addr)
        {
            return (byte)(addr >> 8);
        }
        public virtual void WriteSync(ushort addr, byte data) { }
        public virtual byte ReadSync(ushort addr) { return 0; }
        public virtual void VSync() { }
        public virtual bool Sync(int cycles) { return false; }
        public virtual int GetFreq(int channel) { return 0; }
        public virtual int GetStateSize() { return 0; }
        public virtual void SaveState(byte[] p) { }
        public virtual void LoadState(byte[] p) { }
    }
}
