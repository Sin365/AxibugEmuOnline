using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AxibugEmuOnline.Client.ScreenScaler;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_Scaler : ExpandMenu
    {
        private List<OptionMenu> m_subMenus = new List<OptionMenu>();

        public InGameUI_Scaler(InGameUI inGameUI) : base("屏幕比例", null)
        {
            m_subMenus.Add(new Scale(inGameUI, null));
            foreach (EnumScalerMode scaleModeValue in Enum.GetValues(typeof(EnumScalerMode)))
            {
                m_subMenus.Add(new Scale(inGameUI, scaleModeValue));
            }
        }

        protected override List<OptionMenu> GetOptionMenus()
        {
            return m_subMenus;
        }

        public class Scale : ExecuteMenu
        {
            private EnumScalerMode? m_mode;
            private InGameUI m_gameUI;

            public override bool IsApplied
            {
                get
                {
                    if (m_gameUI.Core.IsNull()) return false;

                    var isSetMode = App.settings.ScreenScaler.IsSetMode(m_gameUI.Core.Platform);

                    if (m_mode == null && !isSetMode)
                    {
                        return true;
                    }
                    else if (isSetMode && m_mode.HasValue)
                    {
                        var mode = App.settings.ScreenScaler.GetMode(m_gameUI.Core.Platform);
                        return mode == m_mode.Value;
                    }
                    else return false;
                }
            }

            public Scale(InGameUI inGameUI, EnumScalerMode? mode) : base(ModeToName(mode), null)
            {
                m_mode = mode;
                m_gameUI = inGameUI;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                App.settings.ScreenScaler.SetMode(m_gameUI.Core.Platform, m_mode);
            }

            static string ModeToName(EnumScalerMode? mode)
            {
                if (mode == null) return "使用全局设置";
                else
                {
                    switch (mode.Value)
                    {
                        case EnumScalerMode.FullScreen: return "全屏";
                        case EnumScalerMode.Raw: return "原始尺寸";
                        case EnumScalerMode.Fix: return "适应";
                        default: throw new Exception($"Not Support Mode : {mode.Value}");
                    }
                }
            }
        }
    }
}