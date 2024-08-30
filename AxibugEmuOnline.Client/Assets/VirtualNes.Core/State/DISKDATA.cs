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
    }
}
