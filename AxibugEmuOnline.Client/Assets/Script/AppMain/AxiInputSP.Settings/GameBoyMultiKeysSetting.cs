using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Settings;
using UnityEngine;

namespace AxiInputSP.Setting
{
    public class GameBoyMultiKeysSetting : MultiKeysSettingBase
    {

        public GameBoyMultiKeysSetting()
        {
            controllers = new GameBoySingleKeysSeting[4];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new GameBoySingleKeysSeting();
        }

        public override void LoadDefaultSetting()
        {
            ClearAll();

#if UNITY_PSP2 && !UNITY_EDITOR
            //PSV 摇杆
            controllers[0].SetKey((ulong)EssgeeSingleKey.POTION_1, AxiInputEx.ByKeyCode(PSVitaKey.Start));
            controllers[0].SetKey((ulong)EssgeeSingleKey.POTION_2, AxiInputEx.ByKeyCode(PSVitaKey.Select));
            controllers[0].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByKeyCode(PSVitaKey.Up));
            controllers[0].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByKeyCode(PSVitaKey.Down));
            controllers[0].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByKeyCode(PSVitaKey.Left));
            controllers[0].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByKeyCode(PSVitaKey.Right));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_1, AxiInputEx.ByKeyCode(PSVitaKey.Cross));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_2, AxiInputEx.ByKeyCode(PSVitaKey.Circle));
            //PSV 摇杆
            controllers[0].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));
            controllers[0].ColletAllKey();
            return;
#endif
            #region P1
            //P1 键盘
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_1, AxiInputEx.ByKeyCode(KeyCode.Return));
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_2, AxiInputEx.ByKeyCode(KeyCode.RightShift));
            controllers[0].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByKeyCode(KeyCode.W));
            controllers[0].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByKeyCode(KeyCode.S));
            controllers[0].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByKeyCode(KeyCode.A));
            controllers[0].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByKeyCode(KeyCode.D));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_1, AxiInputEx.ByKeyCode(KeyCode.J));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_2, AxiInputEx.ByKeyCode(KeyCode.K));

            //P1 UGUI
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_1, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_1));
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_2, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_2));
            controllers[0].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_1, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_A));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_2, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_B));

            controllers[0].ColletAllKey();
            #endregion
        }
    }

    public class GameBoySingleKeysSeting : SingleKeySettingBase
    {
    }
}

