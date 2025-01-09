using AxibugEmuOnline.Client.ClientCore;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class StarRomListMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            App.starRomLib.FetchRomCount((roms) =>
            {
                List<RomFile> data = new List<RomFile>(roms);
                callback.Invoke(data, 0);
            });
        }
    }
}
