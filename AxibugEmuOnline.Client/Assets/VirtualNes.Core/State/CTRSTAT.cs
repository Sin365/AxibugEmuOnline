using Codice.CM.Client.Differences;
using System;

namespace VirtualNes.Core
{
    public struct CTRSTAT : IStateBufferObject
    {
        public uint pad1bit;
        public uint pad2bit;
        public uint pad3bit;
        public uint pad4bit;
        public byte strobe;

        public readonly uint GetSize()
        {
            return sizeof(uint) * 4 + sizeof(byte);
        }

        public readonly void SaveState(StateBuffer buffer)
        {
            buffer.Write(pad1bit);
            buffer.Write(pad2bit);
            buffer.Write(pad3bit);
            buffer.Write(pad4bit);
            buffer.Write(strobe);
        }
    }
}
