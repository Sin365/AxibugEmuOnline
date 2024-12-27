using AxibugEmuOnline.Client.UI;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 画面比例模式选项UI
    /// </summary>
    public class UI_ScalerModeItem : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public ScreenScalerListMenuItem.EnumScalerMode Datacontext { get; private set; }

        public void SetData(object data)
        {
            Datacontext = (ScreenScalerListMenuItem.EnumScalerMode)data;

            UpdateView();
        }

        private void UpdateView()
        {
            
            switch (Datacontext)
            {
                case ScreenScalerListMenuItem.EnumScalerMode.FullScreen:
                    SetBaseInfo("全屏", "模拟器输出画面将拉伸到全屏", null);
                    break;
                case ScreenScalerListMenuItem.EnumScalerMode.Raw:
                    SetBaseInfo("原始尺寸", "将保持模拟器输出画面的原始分辨率", null);
                    break;
                case ScreenScalerListMenuItem.EnumScalerMode.Fix:
                    SetBaseInfo("适应", "在保持原始画面比例的情况下适配到全屏", null);
                    break;
            }
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);
        }

        public void Release() { }

        public override bool OnEnterItem()
        {
            return false;
        }
    }
}
