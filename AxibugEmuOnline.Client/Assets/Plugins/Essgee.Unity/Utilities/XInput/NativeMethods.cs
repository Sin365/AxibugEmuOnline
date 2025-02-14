using System.Runtime.InteropServices;

namespace Essgee.Utilities.XInput
{
    static class NativeMethods
    {
        const string dllName = "xinput9_1_0.dll";

        public const int FlagGamepad = 0x00000001;

        [DllImport(dllName, EntryPoint = "XInputGetState")]
        public static extern int GetState(int dwUserIndex, ref XInputState pState);
        [DllImport(dllName, EntryPoint = "XInputSetState")]
        public static extern int SetState(int dwUserIndex, ref XInputVibration pVibration);
        [DllImport(dllName, EntryPoint = "XInputGetCapabilities")]
        public static extern int GetCapabilities(int dwUserIndex, int dwFlags, ref XInputCapabilities pCapabilities);
    }

    public enum Errors
    {
        Success = 0x00000000,
        BadArguments = 0x000000A0,
        DeviceNotConnected = 0x0000048F
    }
}
