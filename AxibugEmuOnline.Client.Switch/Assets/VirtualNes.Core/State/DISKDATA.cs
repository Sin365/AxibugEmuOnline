namespace VirtualNes.Core
{
    public struct DISKDATA : IStateBufferObject
    {
        public int DifferentSize;

        public uint GetSize()
        {
            return sizeof(int);
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(DifferentSize);
        }

        public void LoadState(StateReader buffer)
        {
            DifferentSize = buffer.Read_int();
        }
    }
}
