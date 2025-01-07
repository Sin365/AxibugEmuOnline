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
            App.settings.Filter.EnableFilterPreview();

            return base.OnEnterItem();
        }

        public override bool OnExitItem()
        {
            App.settings.Filter.ShutDownFilterPreview();
            App.settings.Filter.ShutDownFilter();

            return base.OnExitItem();
        }

        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            List<object> list = new List<object>();
            list.Add(null);
            list.AddRange(App.settings.Filter.Filters.Select(f => (object)f));
            callback.Invoke(list, 0);
        }
    }
}
