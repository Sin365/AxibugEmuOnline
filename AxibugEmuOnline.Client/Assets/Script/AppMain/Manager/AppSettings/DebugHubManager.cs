using AxibugProtobuf;
using System.Collections.Generic;
using IngameDebugConsole;

namespace AxibugEmuOnline.Client.Settings
{
    public class DebugHubManager
    {
        public delegate void OnDebugHubSettingChangedHandle();
        public event OnDebugHubSettingChangedHandle OnDebugHubSettingChanged;
        string key_GlobalMode = nameof(DebugHubManager) + ".IsDebugHubOn";
        Dictionary<RomPlatformType, string> cache_PlatMode = new Dictionary<RomPlatformType, string>();
        public DebugHubManager()
        {
            RefreshForSetting();
        }
        /// <summary>
        /// 全局设置的缩放模式
        /// </summary>
        public bool IsDebugHubOn
        {
            get => AxiPlayerPrefs.GetInt(key_GlobalMode, 0) == 1;
            set { 
                AxiPlayerPrefs.SetInt(key_GlobalMode, value ? 1 : 0);
                OnDebugHubSettingChanged?.Invoke();
            }
        }

        public void RefreshForSetting()
        {
            Initer.debugger_instance.gameObject.SetActive(IsDebugHubOn);
#if UNITY_SWITCH
            if (UMAME.bMAMEReadyLoadState)
            {
                Initer.debugger_instance.gameObject.SetActive(true);
                DebugLogManager.Instance.ShowLogWindow();
            }
#endif
        }
    }
}
