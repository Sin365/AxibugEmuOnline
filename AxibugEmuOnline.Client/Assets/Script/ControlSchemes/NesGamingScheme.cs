using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class NesGamingScheme : ControlScheme
    {
        public override void SetUIKeys(Dictionary<KeyCode, EnumCommand> uiKeyMapper)
        {
            base.SetUIKeys(uiKeyMapper);
        }
    }

    public static partial class ControlSchemeSetts
    {
        public static NesGamingScheme NES { get; private set; } = new NesGamingScheme();
    }
}
