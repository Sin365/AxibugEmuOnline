using System.Runtime.InteropServices;

namespace AxibugEmuOnline.Server.Data
{

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ServerInputSnapShot
    {
        [FieldOffset(0)]
        public UInt64 all;

        [FieldOffset(0)]
        public byte p1_byte;
        [FieldOffset(1)]
        public byte p2_byte;
        [FieldOffset(2)]
        public byte p3_byte;
        [FieldOffset(3)]
        public byte p4_byte;

        [FieldOffset(0)]
        public ushort p1_ushort;
        [FieldOffset(2)]
        public ushort p2_ushort;
        [FieldOffset(4)]
        public ushort p3_ushort;
        [FieldOffset(6)]
        public ushort p4_ushort;
    }
}
