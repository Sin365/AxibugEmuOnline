using System;
using System.Runtime.InteropServices;

namespace Essgee.Utilities.XInput
{
    /* https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.reference.xinput_capabilities%28v=vs.85%29.aspx */
    [StructLayout(LayoutKind.Explicit)]
    public struct XInputCapabilities
    {
        [FieldOffset(0)]
        byte type;
        [FieldOffset(1)]
        byte subType;
        [FieldOffset(2)]
        ushort flags;
        [FieldOffset(4)]
        public XInputGamepad Gamepad;
        [FieldOffset(16)]
        public XInputVibration Vibration;

        public DeviceType Type { get { return (DeviceType)type; } }
        public DeviceSubType SubType { get { return (DeviceSubType)subType; } }
        public DeviceFlags Flags { get { return (DeviceFlags)flags; } }
    }

    public enum DeviceType
    {
        Gamepad = 0x01
    }

    public enum DeviceSubType
    {
        Gamepad = 0x01,
        Wheel = 0x02,
        ArcadeStick = 0x03,
        FlightStick = 0x04,
        DancePad = 0x05,
        Guitar = 0x06,
        DrumKit = 0x08
    }

    [Flags]
    public enum DeviceFlags
    {
        VoiceSupported = 0x0004
    }
}
