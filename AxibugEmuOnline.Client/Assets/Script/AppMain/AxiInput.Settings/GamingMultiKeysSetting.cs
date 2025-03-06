using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Manager;
using UnityEngine;

namespace Assets.Script.AppMain.AxiInput.Settings
{
    public class GamingMultiKeysSetting : MultiKeysSettingBase
    {
        public GamingMultiKeysSetting()
        {
            controllers = new GamingSingleKeysSeting[1];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new GamingSingleKeysSeting();
        }

        public override void LoadDefaultSetting()
        {
            ClearAll();

#if UNITY_PSP2 && !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.PSP2)
            {
                controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PSVitaKey.L));
                controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PSVitaKey.R));
                controllers[0].ColletAllKey();
            }
#endif
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(KeyCode.Escape));

            //TODO 待补全
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PC_XBOXKEY.L));
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PC_XBOXKEY.R));

            controllers[0].ColletAllKey();
        }
    }

    public class GamingSingleKeysSeting : SingleKeySettingBase
    {
    }
}
