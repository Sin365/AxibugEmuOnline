using AxibugProtobuf;
using System.Collections.Generic;
using IngameDebugConsole;
using AxibugEmuOnline.Client.ClientCore;

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
            set
            {
                AxiPlayerPrefs.SetInt(key_GlobalMode, value ? 1 : 0);
                OnDebugHubSettingChanged?.Invoke();
            }
        }

        public void RefreshForSetting()
        {
            Initer.debugger_instance.gameObject.SetActive(IsDebugHubOn);

#if UNITY_SWITCH
            if (App.emu != null)
            {
                switch (App.emu.LoadStep)
                {
                    case Manager.AppEmu.E_RUN_ROM_STEP.READY_JOIN_ROOM:
                    case Manager.AppEmu.E_RUN_ROM_STEP.RECV_JOIN_ROOM:
                    case Manager.AppEmu.E_RUN_ROM_STEP.READY_START_GAME:
                    case Manager.AppEmu.E_RUN_ROM_STEP.LOADING:
                        if (!Initer.debugger_instance.gameObject.activeSelf)
                            Initer.debugger_instance.gameObject.SetActive(true);
                        DebugLogManager.Instance.ShowLogWindow();
                        break;
                    case Manager.AppEmu.E_RUN_ROM_STEP.NONE:
                    case Manager.AppEmu.E_RUN_ROM_STEP.FINISH:
                        if(!IsDebugHubOn)
                            DebugLogManager.Instance.HideLogWindow();
                        break;
                }
            }
#endif
        }
    }
}
