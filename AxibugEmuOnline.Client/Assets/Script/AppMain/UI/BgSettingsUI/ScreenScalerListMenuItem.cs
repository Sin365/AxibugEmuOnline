using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class ScreenScalerListMenuItem : VirtualSubMenuItem
    {
        /// <summary> 缩放模式 </summary>
        public enum EnumScalerMode
        {
            /// <summary> 全屏 </summary>
            FullScreen,
            /// <summary> 适应 </summary>
            Fix,
            /// <summary> 原始 </summary>
            Raw
        };

        public override bool OnEnterItem()
        {
            App.settings.Filter.EnableFilterPreview();

            return base.OnEnterItem();
        }

        public override bool OnExitItem()
        {
            App.settings.Filter.ShutDownFilterPreview();

            return base.OnExitItem();
        }

        protected override void GetVirtualListDatas(Action<object> datas)
        {
            List<object> list = new List<object>();
            foreach (var enumValue in Enum.GetValues(typeof(EnumScalerMode))) list.Add(enumValue);
            datas.Invoke(list);
        }
    }
}
