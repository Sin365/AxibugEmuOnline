namespace VirtualNes.Core
{
    public struct EXCTRSTAT : IStateBufferObject
    {
        public uint data;

        public readonly uint GetSize()
        {
            return sizeof(uint);
        }

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(data);
        }

        public void LoadState(StateReader buffer)
        {
            data = buffer.Read_uint();
        }
    }
}
