namespace VirtualNes.Core
{
    public abstract class APU_INTERFACE : IStateBufferObject
    {
        public const float APU_CLOCK = 1789772.5f;

        public virtual void Dispose() { }

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
        public virtual void SaveState(StateBuffer buffer) { }
        public virtual void LoadState(StateReader buffer) { }

        public static int INT2FIX(int x)
        {
            return x << 16;
        }

        public static int FIX2INT(int x)
        {
            return x >> 16;
        }


        public virtual uint GetSize()
        {
            return 0;
        }
    }
}
