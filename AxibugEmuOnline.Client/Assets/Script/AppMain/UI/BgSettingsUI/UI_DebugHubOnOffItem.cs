using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.UI;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 画面比例模式选项UI
    /// </summary>
    public class UI_DebugHubOnOffItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public bool IsDebugHubOn { get; private set; }

        public void SetData(object data)
        {
            IsDebugHubOn = (bool)data;
            UpdateView();
        }

        private void UpdateView()
        {
            switch (IsDebugHubOn)
            {
                case true:
                    SetBaseInfo("打开", "打开调试面板", null);
                    break;
                default:
                    SetBaseInfo("隐藏", "隐藏调试面板", null);
                    break;
            }
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);

            if (m_select)
            {
                App.settings.debugHub.IsDebugHubOn = IsDebugHubOn;
                App.settings.debugHub.RefreshForSetting();
            }
        }

        public void Release() { }

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
