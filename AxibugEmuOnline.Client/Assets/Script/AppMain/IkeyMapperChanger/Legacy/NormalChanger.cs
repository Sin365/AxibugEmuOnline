using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{

    public class NormalChanger : CommandChanger
    {
        Dictionary<KeyCode, EnumCommand> m_uiKeyMapper = new Dictionary<KeyCode, EnumCommand>();
        public NormalChanger()
        {
            m_uiKeyMapper[KeyCode.A] = EnumCommand.SelectItemLeft;
            m_uiKeyMapper[KeyCode.D] = EnumCommand.SelectItemRight;
            m_uiKeyMapper[KeyCode.W] = EnumCommand.SelectItemUp;
            m_uiKeyMapper[KeyCode.S] = EnumCommand.SelectItemDown;
            m_uiKeyMapper[KeyCode.K] = EnumCommand.Enter;
            m_uiKeyMapper[KeyCode.L] = EnumCommand.Back;
            m_uiKeyMapper[KeyCode.I] = EnumCommand.OptionMenu;

            m_uiKeyMapper[KeyCode.LeftArrow] = EnumCommand.SelectItemLeft;
            m_uiKeyMapper[KeyCode.RightArrow] = EnumCommand.SelectItemRight;
            m_uiKeyMapper[KeyCode.UpArrow] = EnumCommand.SelectItemUp;
            m_uiKeyMapper[KeyCode.DownArrow] = EnumCommand.SelectItemDown;
            m_uiKeyMapper[KeyCode.Return] = EnumCommand.Enter;
            m_uiKeyMapper[KeyCode.Escape] = EnumCommand.Back;
            m_uiKeyMapper[KeyCode.RightShift] = EnumCommand.OptionMenu;
            m_uiKeyMapper[KeyCode.LeftShift] = EnumCommand.OptionMenu;

            if (Application.platform == RuntimePlatform.PSP2)
            {
                m_uiKeyMapper[Common.PSVitaKey.Left] = EnumCommand.SelectItemLeft;
                m_uiKeyMapper[Common.PSVitaKey.Right] = EnumCommand.SelectItemRight;
                m_uiKeyMapper[Common.PSVitaKey.Up] = EnumCommand.SelectItemUp;
                m_uiKeyMapper[Common.PSVitaKey.Down] = EnumCommand.SelectItemDown;
                m_uiKeyMapper[Common.PSVitaKey.Circle] = EnumCommand.Enter;
                m_uiKeyMapper[Common.PSVitaKey.Cross] = EnumCommand.Back;
                m_uiKeyMapper[Common.PSVitaKey.Triangle] = EnumCommand.OptionMenu;
            }

            //PC XBOX

            //m_uiKeyMapper[Common.PC_XBOXKEY.Left] = EnumCommand.SelectItemLeft;
            //m_uiKeyMapper[Common.PSVitaKey.Right] = EnumCommand.SelectItemRight;
            //m_uiKeyMapper[Common.PSVitaKey.Up] = EnumCommand.SelectItemUp;
            //m_uiKeyMapper[Common.PSVitaKey.Down] = EnumCommand.SelectItemDown;
            m_uiKeyMapper[Common.PC_XBOXKEY.MenuBtn] = EnumCommand.Enter;
            m_uiKeyMapper[Common.PC_XBOXKEY.ViewBtn] = EnumCommand.Back;
            m_uiKeyMapper[Common.PC_XBOXKEY.Y] = EnumCommand.OptionMenu;
        }

        public override object GetConfig() => m_uiKeyMapper;
    }

}
