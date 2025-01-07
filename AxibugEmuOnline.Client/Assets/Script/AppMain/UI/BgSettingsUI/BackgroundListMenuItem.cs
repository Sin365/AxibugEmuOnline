using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class BackgroundListMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            List<object> list = new List<object>()
            {
                App.settings.BgColor,
            };
            callback.Invoke(list, 0);
        }
    }
}
