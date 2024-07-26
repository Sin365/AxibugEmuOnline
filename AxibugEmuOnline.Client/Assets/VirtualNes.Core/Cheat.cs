namespace VirtualNes.Core
{
    public class CHEATCODE
    {
        public byte enable;
        public byte type;
        public byte length;
        public ushort address;
        public uint data;

        public string comment;
    }

    class GENIECODE
    {
        public ushort address;
        public byte data;
        public byte cmp;
    };
}
