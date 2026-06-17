namespace VirtualNes.Core
{
    public class EXPAD
    {
        protected NES nes;

        public EXPAD(NES parent)
        {
            nes = parent;
        }

        public virtual void Dispose() { }

        public virtual void Reset() { }
        public virtual void Strobe() { }
        public virtual byte Read4016() { return 0x00; }
        public virtual byte Read4017() { return 0x00; }
        public virtual void Write4016(byte data) { }
        public virtual void Write4017(byte data) { }
        public virtual void Sync() { }
        public virtual void SetSyncData(int type, int data) { }
        public virtual int GetSyncData(int type) { return 0x00; }
    }
}
