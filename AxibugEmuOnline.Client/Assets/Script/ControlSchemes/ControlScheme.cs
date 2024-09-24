using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class ControlScheme
    {
        private static ControlScheme m_current;
        public static ControlScheme Current
        {
            get => m_current;
            set
            {
                m_current = value;

                Dictionary<KeyCode, EnumCommand> mapper = new Dictionary<KeyCode, EnumCommand>();
                m_current.SetUIKeys(mapper);
                CommandDispatcher.Instance.SetKeyMapper(mapper);
            }
        }

        public string Name { get; private set; }

        public virtual void SetUIKeys(Dictionary<KeyCode, EnumCommand> uiKeyMapper)
        {
            uiKeyMapper[KeyCode.A] = EnumCommand.SelectItemLeft;
            uiKeyMapper[KeyCode.D] = EnumCommand.SelectItemRight;
            uiKeyMapper[KeyCode.W] = EnumCommand.SelectItemUp;
            uiKeyMapper[KeyCode.S] = EnumCommand.SelectItemDown;
            uiKeyMapper[KeyCode.K] = EnumCommand.Enter;
            uiKeyMapper[KeyCode.L] = EnumCommand.Back;
            uiKeyMapper[KeyCode.I] = EnumCommand.OptionMenu;

            uiKeyMapper[KeyCode.LeftArrow] = EnumCommand.SelectItemLeft;
            uiKeyMapper[KeyCode.RightArrow] = EnumCommand.SelectItemRight;
            uiKeyMapper[KeyCode.UpArrow] = EnumCommand.SelectItemUp;
            uiKeyMapper[KeyCode.DownArrow] = EnumCommand.SelectItemDown;
            uiKeyMapper[KeyCode.Return] = EnumCommand.Enter;
            uiKeyMapper[KeyCode.Escape] = EnumCommand.Back;
            uiKeyMapper[KeyCode.RightShift] = EnumCommand.OptionMenu;
            uiKeyMapper[KeyCode.LeftShift] = EnumCommand.OptionMenu;

            if (Application.platform == RuntimePlatform.PSP2)
            {
                uiKeyMapper[Common.PSVitaKey.Left] = EnumCommand.SelectItemLeft;
                uiKeyMapper[Common.PSVitaKey.Right] = EnumCommand.SelectItemRight;
                uiKeyMapper[Common.PSVitaKey.Up] = EnumCommand.SelectItemUp;
                uiKeyMapper[Common.PSVitaKey.Down] = EnumCommand.SelectItemDown;
                uiKeyMapper[Common.PSVitaKey.Circle] = EnumCommand.Enter;
                uiKeyMapper[Common.PSVitaKey.Cross] = EnumCommand.Back;
                uiKeyMapper[Common.PSVitaKey.Triangle] = EnumCommand.OptionMenu;
            }
        }
    }
}
