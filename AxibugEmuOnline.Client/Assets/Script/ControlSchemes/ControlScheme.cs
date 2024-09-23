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

                m_current.SetUIKeys(CommandDispatcher.Instance.GetKeyMapper());
            }
        }

        public string Name { get; private set; }

        public virtual void SetUIKeys(Dictionary<KeyCode, EnumCommand> uiKeyMapper)
        {
            uiKeyMapper.Clear();

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
        }
    }
}
