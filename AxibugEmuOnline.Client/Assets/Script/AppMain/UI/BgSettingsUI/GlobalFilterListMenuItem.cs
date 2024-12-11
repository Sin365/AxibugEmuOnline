using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client
{
    public class GlobalFilterListMenuItem : VirtualSubMenuItem
    {
        public override bool OnEnterItem()
        {
            App.filter.EnableFilterPreview();

            return base.OnEnterItem();
        }

        public override bool OnExitItem()
        {
            App.filter.ShutDownFilterPreview();
            App.filter.ShutDownFilter();

            return base.OnExitItem();
        }

        protected override void GetVirtualListDatas(Action<object> datas)
        {
            List<object> list = new List<object>();
            list.AddRange(App.filter.Filters.Select(f => (object)f));
            datas.Invoke(list);
        }
    }
}
