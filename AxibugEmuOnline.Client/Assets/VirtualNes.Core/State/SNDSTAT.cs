using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes.Core
{
    public struct SNDSTAT : IStateBufferObject
    {
        public byte[] snddata;

        public static SNDSTAT GetDefault()
        {
            return new SNDSTAT() { snddata = new byte[0x800] };
        }

        public uint GetSize()
        {
            return (uint)snddata.Length;
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(snddata);
        }
    }
}
