namespace VirtualNes.Core
{
    public struct BLOCKHDR : IStateBufferObject
    {
        public readonly bool Valid => !string.IsNullOrEmpty(ID);
        /// <summary> 总是8个字节 </summary>
        public string ID;
        public ushort Reserved;
        public ushort BlockVersion;
        public uint BlockSize;



        public readonly uint GetSize()
        {
            return (uint)(8 + sizeof(ushort) + sizeof(ushort) + sizeof(uint));
        }

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

        public void LoadState(StateReader buffer)
        {
            ID = buffer.Read_string(8);
            Reserved = buffer.Read_ushort();
            BlockVersion = buffer.Read_ushort();
            BlockSize = buffer.Read_uint();
        }
    }
}
