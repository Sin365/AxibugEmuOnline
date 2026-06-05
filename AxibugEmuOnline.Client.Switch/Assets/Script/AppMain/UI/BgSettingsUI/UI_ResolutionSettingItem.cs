using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Settings;
using AxibugEmuOnline.Client.UI;
using System;
using UnityEngine;
using static AxibugEmuOnline.Client.Settings.DisplaySettings;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 画面比例模式选项UI
    /// </summary>
    public class UI_ResolutionSettingItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }

        public void SetData(object data)
        {
            App.settings.displaySettings.OnDisplaySettingChanged += Setting_OnDisplaySettingChanged;
            UpdateView();
        }

        private void Setting_OnDisplaySettingChanged()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            string menuName;
            if (App.settings.displaySettings.PlatfromCanUse())
                menuName = "分辨率设置";
            else
                menuName = "分辨率设置(当前平台不适用)";
            SetBaseInfo(menuName, DisplaySettings.GetResolutionEnumName(App.settings.displaySettings.ResolutionType), null);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);
        }

        public void Release()
        {
            App.settings.displaySettings.OnDisplaySettingChanged -= Setting_OnDisplaySettingChanged;
        }

        public override bool OnEnterItem()
        {
            if (!App.settings.displaySettings.PlatfromCanUse())
            {
                OverlayManager.PopTip("当前平台不适用");
            }
            var options = ObjectPoolAuto.AcquireList<ResolutionSettingOption>();
            options.Add(new ResolutionSettingOption(E_ScreenResolutionType.MODE_Full_AutoForScreen));
            options.Add(new ResolutionSettingOption(E_ScreenResolutionType.MODE_Wnd_1920_1080));
            options.Add(new ResolutionSettingOption(E_ScreenResolutionType.MODE_Wnd_1366_768));
            options.Add(new ResolutionSettingOption(E_ScreenResolutionType.MODE_Wnd_1280_720));
            OverlayManager.PopSideBar(options, Mathf.Clamp((int)App.settings.displaySettings.ResolutionType, 0, options.Count - 1));
            return false;
        }

        public class ResolutionSettingOption : ExecuteMenu
        {
            public E_ScreenResolutionType ResolutionType;
            public override string Name
            {
                get
                {
                    return DisplaySettings.GetResolutionEnumName(ResolutionType);
                }
            }

            public ResolutionSettingOption(E_ScreenResolutionType val)
            {
                ResolutionType = val;
            }

            public override void OnShow(OptionUI_MenuItem ui)
            {
                ui.IconUI.gameObject.SetActiveEx(false);
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                App.settings.displaySettings.ResolutionType = ResolutionType;
                App.settings.displaySettings.ApplyResolution();
            }
        }
    }
}
