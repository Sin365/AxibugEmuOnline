using System;
using System.Runtime.InteropServices;

namespace Essgee.Utilities.XInput
{
    /* https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.reference.xinput_gamepad%28v=vs.85%29.aspx */
    [StructLayout(LayoutKind.Explicit)]
    public struct XInputGamepad
    {
        [FieldOffset(0)]
        ushort wButtons;
        [FieldOffset(2)]
        public byte bLeftTrigger;
        [FieldOffset(3)]
        public byte bRightTrigger;
        [FieldOffset(4)]
        public short sThumbLX;
        [FieldOffset(6)]
        public short sThumbLY;
        [FieldOffset(8)]
        public short sThumbRX;
        [FieldOffset(10)]
        public short sThumbRY;

        public const int LeftThumbDeadzone = 7849;
        public const int RightThumbDeadzone = 8689;
        public const int TriggerThreshold = 30;

        public Buttons Buttons { get { return (Buttons)wButtons; } }
    }

    [Flags]
    public enum Buttons
    {
        None = 0x0000,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }
}
