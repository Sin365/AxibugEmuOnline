﻿using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary> 颜色设置 </summary>
    public class BgColorSettings
    {
        public delegate void OnColorChangedHandle(XMBColor color);
        public event OnColorChangedHandle OnColorChanged;

        /// <summary> 当前颜色 </summary>
        public XMBColor CurrentColor
        {
            get
            {
                var color1 = AxiPlayerPrefs.GetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.color1)}", null);
                var color2 = AxiPlayerPrefs.GetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.color2)}", null);
                var name = AxiPlayerPrefs.GetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.Name)}", null);
                if (string.IsNullOrWhiteSpace(color1) || string.IsNullOrWhiteSpace(color2) || string.IsNullOrWhiteSpace(name))
                    return DEFAULT;
                else
                    return new XMBColor(name, color1, color2);
            }
            set
            {
                AxiPlayerPrefs.SetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.color1)}", $"#{ColorUtility.ToHtmlStringRGB(value.color1)}");
                AxiPlayerPrefs.SetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.color2)}", $"#{ColorUtility.ToHtmlStringRGB(value.color2)}");
                AxiPlayerPrefs.SetString($"{nameof(BgColorSettings)}.{nameof(CurrentColor)}.{nameof(CurrentColor.Name)}", value.Name);

                OnColorChanged?.Invoke(value);
            }
        }

        private static readonly XMBColor DEFAULT = new XMBColor("蓝", "#020754", "#0ab1dc");
        public List<XMBColor> Presets { get; private set; } = new List<XMBColor>()
        {
            DEFAULT,
            new XMBColor("白","#8a9fb2","#4e9eb6"),
            new XMBColor("黄","#987500","#d1a813"),
            new XMBColor("绿","#3e962b","#7ac25e"),
            new XMBColor("粉","#e65a8b","#c7acc6"),
            new XMBColor("墨绿","#00421a","#1c951f"),
            new XMBColor("紫","#633f93","#8763cc"),
            new XMBColor("天蓝","#038baa","#0ca2c2"),
            new XMBColor("红","#9c120e","#d73611"),
        };
    }


    public struct XMBColor
    {
        public string Name;
        public Color color1;
        public Color color2;

        public XMBColor(string name, string colorCodeStr1, string colorCodeStr2)
        {
            Name = name;
            ColorUtility.TryParseHtmlString(colorCodeStr1, out color1);
            ColorUtility.TryParseHtmlString(colorCodeStr2, out color2);
        }

        public static XMBColor Lerp(XMBColor start, XMBColor endColor, float t)
        {
            var result = new XMBColor();
            result.Name = endColor.Name;
            result.color1 = Color.Lerp(start.color1, endColor.color1, t);
            result.color2 = Color.Lerp(start.color2, endColor.color2, t);

            return result;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
            hash = hash * 23 + color1.GetHashCode();
            hash = hash * 23 + color2.GetHashCode();
            return hash;
        }
    }
}
