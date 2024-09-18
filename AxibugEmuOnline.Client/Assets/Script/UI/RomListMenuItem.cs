using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using Codice.Client.Common;
using System;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RomListMenuItem : VirtualSubMenuItem
    {
        [SerializeField]
        protected EnumPlatform Platform;

        private RomLib RomLib
        {
            get
            {
                switch (Platform)
                {
                    case EnumPlatform.NES:
                        return App.nesRomLib;
                    default:
                        throw new System.NotImplementedException($"未实现的平台 {Platform}");
                }
            }
        }

        protected override void GetVirtualListDatas(Action<object> datas)
        {
            RomLib.FetchRomCount((roms) => datas.Invoke(roms));
        }
    }
}
