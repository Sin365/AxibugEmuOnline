using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.UI;
using System;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 画面比例模式选项UI
    /// </summary>
    public class UI_DebugHubOnOffItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }

        public void SetData(object data)
        {
            App.settings.debugHub.OnDebugHubSettingChanged += Setting_OnDebugHubSettingChanged;
            UpdateView();
        }

        private void Setting_OnDebugHubSettingChanged()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            switch (App.settings.debugHub.IsDebugHubOn)
            {
                case true:
                    SetBaseInfo("调试面板", "显示", null);
                    break;
                default:
                    SetBaseInfo("调试面板", "隐藏", null);
                    break;
            }
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);
        }

        public void Release()
        {
            App.settings.debugHub.OnDebugHubSettingChanged -= Setting_OnDebugHubSettingChanged;
        }

        public override bool OnEnterItem()
        {
            var options = ObjectPoolAuto.AcquireList<DebugHubOnOffOption>();
            options.Add(new DebugHubOnOffOption(true));
            options.Add(new DebugHubOnOffOption(false));
            OverlayManager.PopSideBar(options, Mathf.Clamp(App.settings.debugHub.IsDebugHubOn ? 0 : 1, 0, options.Count - 1));
            return false;
        }

        public class DebugHubOnOffOption : ExecuteMenu
        {
            public bool valueflag;
            public override string Name => valueflag ? "开启" : "关闭";

            public DebugHubOnOffOption(bool val)
            {
                valueflag = val;
            }

            public override void OnShow(OptionUI_MenuItem ui)
            {
                ui.IconUI.gameObject.SetActiveEx(false);
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                App.settings.debugHub.IsDebugHubOn = valueflag;
                App.settings.debugHub.RefreshForSetting();
            }
        }
    }
}
