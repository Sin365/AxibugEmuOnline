namespace VirtualNes.Core
{
    public struct FILEHDR2 : IStateBufferObject
    {
        public string ID;
        /// <summary> 2字节 </summary>
        public ushort BlockVersion;
        /// <summary> 4字节 </summary>
        public uint Ext0;
        /// <summary> 2字节 </summary>
        public ushort Ext1;
        /// <summary> 2字节 </summary>
        public ushort Ext2;



        public uint GetSize()
        {
            return (uint)(ID.Length + sizeof(ushort) + sizeof(uint) + sizeof(ushort) + sizeof(ushort));
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(ID);
            buffer.Write(BlockVersion);
            buffer.Write(Ext0);
            buffer.Write(Ext1);
            buffer.Write(Ext2);
        }

        public void LoadState(StateReader buffer)
        {
            ID = buffer.Read_string(12);
            BlockVersion = buffer.Read_ushort();
            Ext0 = buffer.Read_uint();
            Ext1 = buffer.Read_ushort();
            Ext2 = buffer.Read_ushort();
        }
    }
}
