using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Essgee.Emulation.CPU
{
    public partial class Z80A
    {
        [DebuggerDisplay("{Word}")]
        [StructLayout(LayoutKind.Explicit)]
        [Serializable]
        public struct Register
        {
            [NonSerialized]
            [FieldOffset(0)]
            public byte Low;
            [NonSerialized]
            [FieldOffset(1)]
            public byte High;

            [FieldOffset(0)]
            public ushort Word;


            [FieldOffset(0)]
            public ushort axi_AllData;
        }
    }
}
