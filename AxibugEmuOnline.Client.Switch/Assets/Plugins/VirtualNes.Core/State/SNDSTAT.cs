namespace VirtualNes.Core
{
    public struct SNDSTAT : IStateBufferObject
    {
        public byte[] snddata;

        public static SNDSTAT GetDefault()
        {
            return new SNDSTAT() { snddata = new byte[0x800] };
        }

        public readonly uint GetSize()
        {
            return (uint)snddata.Length;
        }

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(snddata);
        }

        public void LoadState(StateReader buffer)
        {
            snddata = buffer.Read_bytes(0x800);
        }
    }
}
