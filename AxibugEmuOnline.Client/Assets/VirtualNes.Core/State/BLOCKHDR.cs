namespace VirtualNes.Core
{
    public struct BLOCKHDR : IStateBufferObject
    {
        public readonly bool Valid => !string.IsNullOrEmpty(ID);
        public string ID;
        public ushort Reserved;
        public ushort BlockVersion;
        public uint BlockSize;

        public readonly void SaveState(StateBuffer buffer)
        {
            if (Valid)
            {
                buffer.Write(ID);
                buffer.Write(Reserved);
                buffer.Write(BlockVersion);
                buffer.Write(BlockSize);
            }
        }

        public readonly uint GetSize()
        {
            return (uint)(ID.Length + sizeof(ushort) + sizeof(ushort) + sizeof(uint));
        }
    }
}
