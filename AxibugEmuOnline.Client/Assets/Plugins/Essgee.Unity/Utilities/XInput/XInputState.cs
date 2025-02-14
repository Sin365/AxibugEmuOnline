using System.Runtime.InteropServices;

namespace Essgee.Utilities.XInput
{
    /* https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.reference.xinput_state%28v=vs.85%29.aspx */
    [StructLayout(LayoutKind.Explicit)]
    public struct XInputState
    {
        [FieldOffset(0)]
        public uint dwPacketNumber;
        [FieldOffset(4)]
        public XInputGamepad Gamepad;
    }
}
