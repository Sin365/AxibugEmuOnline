using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class BackgroundListMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(Action<object> datas)
        {
            List<object> list = new List<object>()
            {
                App.settings.BgColor,
            };
            datas.Invoke(list);
        }
    }
}
