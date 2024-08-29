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
            throw new System.NotImplementedException();
        }
    }
}
