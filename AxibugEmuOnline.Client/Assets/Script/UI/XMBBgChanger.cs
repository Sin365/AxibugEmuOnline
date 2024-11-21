using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class XMBBgChanger : MonoBehaviour
    {
        public Image imgUI;

        private void OnEnable()
        {
            App.settings.BgColor.OnColorChanged += BgColor_OnColorChanged;

            imgUI.GetMaterial().SetColor("_Color1", App.settings.BgColor.CurrentColor.color1);
            imgUI.GetMaterial().SetColor("_Color2", App.settings.BgColor.CurrentColor.color2);
        }

        private void OnDisable()
        {
            App.settings.BgColor.OnColorChanged -= BgColor_OnColorChanged;
        }

        private void BgColor_OnColorChanged(XMBColor color)
        {
            imgUI.GetMaterial().SetColor("_Color1", color.color1);
            imgUI.GetMaterial().SetColor("_Color2", color.color2);
        }
    }
}
