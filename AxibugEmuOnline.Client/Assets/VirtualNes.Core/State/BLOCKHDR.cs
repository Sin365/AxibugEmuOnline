namespace VirtualNes.Core
{
    public struct BLOCKHDR : IStateBufferObject
    {
        public string ID;
        public ushort Reserved;
        public ushort BlockVersion;
        public uint BlockSize;

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(ID);
            buffer.Write(Reserved);
            buffer.Write(BlockVersion);
            buffer.Write(BlockSize);
        }

        public readonly uint GetSize()
        {
            return (uint)(ID.Length + sizeof(ushort) + sizeof(ushort) + sizeof(uint));
        }
    }
}
