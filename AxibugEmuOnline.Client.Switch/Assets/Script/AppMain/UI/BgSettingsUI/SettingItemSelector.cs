using AxibugEmuOnline.Client.Settings;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class SettingItemSelector : ItemSelector
    {
        protected override RectTransform OnGetTemplate(object data)
        {
            if (data is BgColorSettings) return ItemList[0];
            else return null;
        }
    }
}
