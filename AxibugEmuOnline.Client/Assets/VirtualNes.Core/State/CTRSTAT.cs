namespace VirtualNes.Core
{
    public struct CTRSTAT : IStateBufferObject
    {
        public uint pad1bit;
        public uint pad2bit;
        public uint pad3bit;
        public uint pad4bit;
        public byte strobe;

        public  uint GetSize()
        {
            return sizeof(uint) * 4 + sizeof(byte);
        }

        public  void SaveState(StateBuffer buffer)
        {
            buffer.Write(pad1bit);
            buffer.Write(pad2bit);
            buffer.Write(pad3bit);
            buffer.Write(pad4bit);
            buffer.Write(strobe);
        }

        public void LoadState(StateReader buffer)
        {
            pad1bit = buffer.Read_uint();
            pad2bit = buffer.Read_uint();
            pad3bit = buffer.Read_uint();
            pad4bit = buffer.Read_uint();
            strobe = buffer.Read_byte();
        }
    }
}
