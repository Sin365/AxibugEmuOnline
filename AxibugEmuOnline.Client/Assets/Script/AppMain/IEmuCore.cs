using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public interface IEmuCore
    {
        GameObject gameObject { get; }

        object GetState();
        byte[] GetStateBytes();
        void LoadState(object state);
        void LoadStateFromBytes(byte[] data);
        void Pause();
        void Resume();
        void SetupScheme();
        void StartGame(RomFile romFile);
        void DoReset();
        IControllerSetuper GetControllerSetuper();

        EnumPlatform Platform { get; }
        uint Frame { get; }
        Vector2Int GetRawResolution();
    }

    public static class IEnumCoreTool
    {
        public static bool IsNull(this IEmuCore core)
        {
            if (core == null) return true;
            return core.Equals(null);
        }
    }
}
