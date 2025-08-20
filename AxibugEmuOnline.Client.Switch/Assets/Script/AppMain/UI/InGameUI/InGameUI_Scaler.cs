using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using static AxibugEmuOnline.Client.Settings.ScreenScaler;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_Scaler : ExpandMenu
    {
        private List<InternalOptionMenu> m_subMenus = new List<InternalOptionMenu>();
        public override string Name => "屏幕比例";
        public InGameUI_Scaler(InGameUI inGameUI)
        {
            m_subMenus.Add(new Scale(inGameUI, null));
            foreach (EnumScalerMode scaleModeValue in Enum.GetValues(typeof(EnumScalerMode)))
            {
                m_subMenus.Add(new Scale(inGameUI, scaleModeValue));
            }
        }

        protected override List<InternalOptionMenu> GetOptionMenus()
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
                    if (m_gameUI.Core == null) return false;

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
            public override string Name => ModeToName(m_mode);
            public Scale(InGameUI inGameUI, EnumScalerMode? mode)
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