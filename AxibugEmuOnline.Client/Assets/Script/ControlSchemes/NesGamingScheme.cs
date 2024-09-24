using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class NesGamingScheme : ControlScheme
    {
        public override void SetUIKeys(Dictionary<KeyCode, EnumCommand> uiKeyMapper)
        {
            uiKeyMapper[KeyCode.Escape] = EnumCommand.OptionMenu;

            if (Application.platform == RuntimePlatform.PSP2)
            {
                uiKeyMapper[Common.PSVitaKey.L] = EnumCommand.OptionMenu;
                uiKeyMapper[Common.PSVitaKey.R] = EnumCommand.OptionMenu;
            }
        }
    }

    public static partial class ControlSchemeSetts
    {
        public static NesGamingScheme NES { get; private set; } = new NesGamingScheme();
    }
}
