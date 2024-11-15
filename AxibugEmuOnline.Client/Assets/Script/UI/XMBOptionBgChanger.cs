using AxibugEmuOnline.Client.ClientCore;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class XMBOptionBgChanger : MonoBehaviour
    {
        public UIGradient gradient;

        private void OnEnable()
        {
            App.settings.BgColor.OnColorChanged += BgColor_OnColorChanged;

            var color = App.settings.BgColor.CurrentColor;
            gradient.color1 = color.color1;
            gradient.color2 = color.color2;
        }

        private void OnDisable()
        {
            App.settings.BgColor.OnColorChanged -= BgColor_OnColorChanged;
        }

        private void BgColor_OnColorChanged(XMBColor color)
        {
            gradient.color1 = color.color1;
            gradient.color2 = color.color2;
        }
    }
}
