using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class GamingChanger : CommandChanger
    {

        Dictionary<KeyCode, EnumCommand> m_uiKeyMapper = new Dictionary<KeyCode, EnumCommand>();
        public GamingChanger()
        {
            m_uiKeyMapper[KeyCode.Escape] = EnumCommand.OptionMenu;

            if (Application.platform == RuntimePlatform.PSP2)
            {
                m_uiKeyMapper[Common.PSVitaKey.L] = EnumCommand.OptionMenu;
                m_uiKeyMapper[Common.PSVitaKey.R] = EnumCommand.OptionMenu;
            }

            //PC XBOX
            m_uiKeyMapper[Common.PC_XBOXKEY.Y] = EnumCommand.OptionMenu;
        }

        public override object GetConfig() => m_uiKeyMapper;
    }
}
