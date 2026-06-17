using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;

namespace AxibugEmuOnline.Client
{
    public class UI_UserSetting_ModiftRoleName_Item : MenuItem, IVirtualItem
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
            SetBaseInfo("用户名",
                (
                App.user.IsLoggedIn ? App.user.userdata.NickName : "未登录"
                )
                , null);
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
            if (!App.user.IsLoggedIn)
            {
                OverlayManager.PopTip("您尚未登录");
                return false;
            }
            OverlayManager.Input(((msg) =>
            {
                if (string.IsNullOrEmpty(msg.Trim()))
                {
                    OverlayManager.PopTip("未输入修改内容");
                    return;
                }

                msg = msg.Trim();
                if (string.Equals(msg, App.user.userdata.NickName))
                {
                    OverlayManager.PopTip("输入的昵称没有变化");
                    return;
                }

                if (msg.Length < 2)
                {
                    OverlayManager.PopTip("昵称不可少于2个字符");
                    return;
                }

                if (msg.Length > 10)
                {
                    OverlayManager.PopTip("不可超过10个字符");
                    return;
                }
                App.user.Send_ModifyNickName(msg);
            }),
                "[修改昵称]输入新昵称"
                , string.Empty);
            return false;
        }
    }
}
