using AxibugEmuOnline.Client.ClientCore;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class DebugHubOnOffMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            List<object> list = new List<object>()
            {
                App.settings.debugHub.IsDebugHubOn,
            };
            callback.Invoke(list, 0);
        }
    }
}
