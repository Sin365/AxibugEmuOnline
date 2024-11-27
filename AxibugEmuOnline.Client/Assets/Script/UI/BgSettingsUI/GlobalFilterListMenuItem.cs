using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client
{
    public class GlobalFilterListMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(Action<object> datas)
        {
            List<object> list = new List<object>();
            list.Add(new FilterManager.Filter());
            list.AddRange(App.filter.Filters.Select(f => (object)f));
            datas.Invoke(list);
        }
    }
}
