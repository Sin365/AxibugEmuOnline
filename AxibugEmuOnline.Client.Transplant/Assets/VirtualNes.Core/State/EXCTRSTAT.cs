namespace VirtualNes.Core
{
    public struct EXCTRSTAT : IStateBufferObject
    {
        public uint data;

        public uint GetSize()
        {
            return sizeof(uint);
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(data);
        }

        public void LoadState(StateReader buffer)
        {
            data = buffer.Read_uint();
        }
    }
}
