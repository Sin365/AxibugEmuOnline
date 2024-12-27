using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class ScreenScalerListMenuItem : VirtualSubMenuItem
    {
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
            foreach (var enumValue in Enum.GetValues(typeof(ScreenScaler.EnumScalerMode))) list.Add(enumValue);
            datas.Invoke(list);
        }
    }
}
