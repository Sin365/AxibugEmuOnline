using AxibugEmuOnline.Client.Manager;
using UnityEngine;

namespace AxiInputSP.Setting
{
    public class ColecoVisionMultiKeysSetting : MultiKeysSettingBase
    {
        public ColecoVisionMultiKeysSetting()
        {
            controllers = new ColecoVisionSingleKeysSeting[4];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new ColecoVisionSingleKeysSeting();
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
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_3, AxiInputEx.ByKeyCode(PSVitaKey.Block));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_4, AxiInputEx.ByKeyCode(PSVitaKey.Triangle));
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
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_3, AxiInputEx.ByKeyCode(KeyCode.U));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_4, AxiInputEx.ByKeyCode(KeyCode.I));

            //P1 UGUI
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_1, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_1));
            controllers[0].SetKey((ulong)EssgeeSingleKey.OPTION_2, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_2));
            controllers[0].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_1, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_A));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_2, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_B));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_3, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_C));
            controllers[0].SetKey((ulong)EssgeeSingleKey.BTN_4, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_D));

            //P2 键盘
            controllers[1].SetKey((ulong)EssgeeSingleKey.OPTION_1, AxiInputEx.ByKeyCode(KeyCode.Keypad0));
            controllers[1].SetKey((ulong)EssgeeSingleKey.OPTION_2, AxiInputEx.ByKeyCode(KeyCode.Delete));
            controllers[1].SetKey((ulong)EssgeeSingleKey.UP, AxiInputEx.ByKeyCode(KeyCode.UpArrow));
            controllers[1].SetKey((ulong)EssgeeSingleKey.DOWN, AxiInputEx.ByKeyCode(KeyCode.DownArrow));
            controllers[1].SetKey((ulong)EssgeeSingleKey.LEFT, AxiInputEx.ByKeyCode(KeyCode.LeftArrow));
            controllers[1].SetKey((ulong)EssgeeSingleKey.RIGHT, AxiInputEx.ByKeyCode(KeyCode.RightArrow));
            controllers[1].SetKey((ulong)EssgeeSingleKey.BTN_1, AxiInputEx.ByKeyCode(KeyCode.Keypad1));
            controllers[1].SetKey((ulong)EssgeeSingleKey.BTN_2, AxiInputEx.ByKeyCode(KeyCode.Keypad2));
            controllers[1].SetKey((ulong)EssgeeSingleKey.BTN_3, AxiInputEx.ByKeyCode(KeyCode.Keypad3));
            controllers[1].SetKey((ulong)EssgeeSingleKey.BTN_4, AxiInputEx.ByKeyCode(KeyCode.Keypad4));

            controllers[0].ColletAllKey();
            #endregion
        }
    }

    public class ColecoVisionSingleKeysSeting : SingleKeySettingBase
    {
    }
}

