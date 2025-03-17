using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Settings;
using AxibugEmuOnline.Client.UI;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 画面比例模式选项UI
    /// </summary>
    public class UI_ScalerModeItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public ScreenScaler.EnumScalerMode Datacontext { get; private set; }

        public void SetData(object data)
        {
            Datacontext = (ScreenScaler.EnumScalerMode)data;

            UpdateView();
        }

        private void UpdateView()
        {
            switch (Datacontext)
            {
                case ScreenScaler.EnumScalerMode.FullScreen:
                    SetBaseInfo("全屏", "模拟器输出画面将拉伸到全屏", null);
                    break;
                case ScreenScaler.EnumScalerMode.Raw:
                    SetBaseInfo("原始尺寸", "将保持模拟器输出画面的原始分辨率", null);
                    break;
                case ScreenScaler.EnumScalerMode.Fix:
                    SetBaseInfo("适应", "在保持原始画面比例的情况下适配到全屏", null);
                    break;
            }
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);

            if (m_select)
            {
                App.settings.Filter.EnableFilterPreview();
                App.settings.ScreenScaler.GlobalMode = Datacontext;
            }
        }

        public void Release() { }

        public override bool OnEnterItem()
        {
            return false;
        }
    }
}
