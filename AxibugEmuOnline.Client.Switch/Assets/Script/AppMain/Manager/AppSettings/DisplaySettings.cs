using System;
using UnityEngine;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary> 颜色设置 </summary>
    public class DisplaySettings
    {
        public enum E_ScreenResolutionType
        {
            MODE_Full_AutoForScreen,
            MODE_Wnd_1920_1080,
            MODE_Wnd_1366_768,
            MODE_Wnd_1280_720
        }

        public delegate void OnDisplaySettingChangedHandle();
        public event OnDisplaySettingChangedHandle OnDisplaySettingChanged;

        string key_Resolution = nameof(DebugHubManager) + ".ResolutionType";

        /// <summary>
        /// 分辨率模式
        /// </summary>
        public E_ScreenResolutionType ResolutionType
        {
            get => (E_ScreenResolutionType)AxiPlayerPrefs.GetInt(key_Resolution, (int)E_ScreenResolutionType.MODE_Full_AutoForScreen);
            set
            {
                AxiPlayerPrefs.SetInt(key_Resolution, (int)value);
                OnDisplaySettingChanged?.Invoke();
            }
        }


        public bool PlatfromCanUse()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer
            &&
            Application.platform != RuntimePlatform.WindowsEditor
            &&
            Application.platform != RuntimePlatform.OSXPlayer
            &&
            Application.platform != RuntimePlatform.OSXEditor
            &&
            Application.platform != RuntimePlatform.LinuxPlayer
            &&
            Application.platform != RuntimePlatform.LinuxEditor
            )
                return false;
            return true;
        }
        public void ApplyResolution()
        {
            //仅几个桌面平台生效
            
            if(!PlatfromCanUse())
                return;

            Resolution res = GetResolution(ResolutionType);
            if (ResolutionType == E_ScreenResolutionType.MODE_Full_AutoForScreen)
            {
                Screen.SetResolution(
                    res.width,
                    res.height,
                    FullScreenMode.FullScreenWindow
                );
            }
            else
            {
                Screen.SetResolution(
                    res.width,
                    res.height,
                    FullScreenMode.Windowed
                );
            }
        }
        public static Resolution GetResolution(E_ScreenResolutionType type)
        {
            switch (type)
            {
                case E_ScreenResolutionType.MODE_Full_AutoForScreen:
                    return new Resolution
                    {
                        width = Screen.currentResolution.width,
                        height = Screen.currentResolution.height
                    };

                case E_ScreenResolutionType.MODE_Wnd_1920_1080:
                    return new Resolution { width = 1920, height = 1080 };

                case E_ScreenResolutionType.MODE_Wnd_1366_768:
                    return new Resolution { width = 1366, height = 768 };

                case E_ScreenResolutionType.MODE_Wnd_1280_720:
                    return new Resolution { width = 1280, height = 720 };

                default:
                    return Screen.currentResolution;
            }
        }

        internal static string GetResolutionEnumName(E_ScreenResolutionType resolutionType)
        {
            switch (resolutionType)
            {
                case E_ScreenResolutionType.MODE_Full_AutoForScreen:
                    return "全屏";
                case E_ScreenResolutionType.MODE_Wnd_1920_1080:
                    return "窗口1920*1080";
                case E_ScreenResolutionType.MODE_Wnd_1366_768:
                    return "窗口1366*768";
                case E_ScreenResolutionType.MODE_Wnd_1280_720:
                    return "窗口1280*720";
                default:
                    return "缺省";
            }
        }
    }
}
