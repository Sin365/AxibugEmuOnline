using AxibugEmuOnline.Client.Manager;
using AxiInputSP;
using UnityEngine;
using VirtualNes.Core;

namespace AxiInputSP.Setting
{
    public class NESMultiKeysSetting : MultiKeysSettingBase
    {
        public NESMultiKeysSetting()
        {
            controllers = new NESSingleKeysSeting[4];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new NESSingleKeysSeting();
        }

        public override void LoadDefaultSetting()
        {
            ClearAll();
#if UNITY_PSP2 && !UNITY_EDITOR
            //PSV 摇杆
            controllers[0].SetKey((ulong)EnumButtonType.START, AxiInputEx.ByKeyCode(PSVitaKey.Start));
            controllers[0].SetKey((ulong)EnumButtonType.SELECT, AxiInputEx.ByKeyCode(PSVitaKey.Select));
            controllers[0].SetKey((ulong)EnumButtonType.UP, AxiInputEx.ByKeyCode(PSVitaKey.Up));
            controllers[0].SetKey((ulong)EnumButtonType.DOWN, AxiInputEx.ByKeyCode(PSVitaKey.Down));
            controllers[0].SetKey((ulong)EnumButtonType.LEFT, AxiInputEx.ByKeyCode(PSVitaKey.Left));
            controllers[0].SetKey((ulong)EnumButtonType.RIGHT, AxiInputEx.ByKeyCode(PSVitaKey.Right));
            controllers[0].SetKey((ulong)EnumButtonType.A, AxiInputEx.ByKeyCode(PSVitaKey.Cross));
            controllers[0].SetKey((ulong)EnumButtonType.B, AxiInputEx.ByKeyCode(PSVitaKey.Circle));
            controllers[0].SetKey((ulong)EnumButtonType.MIC, AxiInputEx.ByKeyCode(PSVitaKey.Block));
            //PSV 摇杆
            controllers[0].SetKey((ulong)EnumButtonType.UP, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)EnumButtonType.DOWN, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)EnumButtonType.LEFT, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)EnumButtonType.RIGHT, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));
            controllers[0].ColletAllKey();
            return;
#endif
            #region P1
            //P1 键盘
            controllers[0].SetKey((ulong)EnumButtonType.START, AxiInputEx.ByKeyCode(KeyCode.Return));
            controllers[0].SetKey((ulong)EnumButtonType.SELECT, AxiInputEx.ByKeyCode(KeyCode.RightShift));
            controllers[0].SetKey((ulong)EnumButtonType.UP, AxiInputEx.ByKeyCode(KeyCode.W));
            controllers[0].SetKey((ulong)EnumButtonType.DOWN, AxiInputEx.ByKeyCode(KeyCode.S));
            controllers[0].SetKey((ulong)EnumButtonType.LEFT, AxiInputEx.ByKeyCode(KeyCode.A));
            controllers[0].SetKey((ulong)EnumButtonType.RIGHT, AxiInputEx.ByKeyCode(KeyCode.D));
            controllers[0].SetKey((ulong)EnumButtonType.A, AxiInputEx.ByKeyCode(KeyCode.K));
            controllers[0].SetKey((ulong)EnumButtonType.B, AxiInputEx.ByKeyCode(KeyCode.J));
            controllers[0].SetKey((ulong)EnumButtonType.MIC, AxiInputEx.ByKeyCode(KeyCode.U));

            //P1 UGUI
            controllers[0].SetKey((ulong)EnumButtonType.START, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_1));
            controllers[0].SetKey((ulong)EnumButtonType.SELECT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_2));
            controllers[0].SetKey((ulong)EnumButtonType.UP, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)EnumButtonType.DOWN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)EnumButtonType.LEFT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)EnumButtonType.RIGHT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)EnumButtonType.A, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_B));
            controllers[0].SetKey((ulong)EnumButtonType.B, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_A));
            controllers[0].SetKey((ulong)EnumButtonType.MIC, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_C));

            //P2 键盘
            controllers[1].SetKey((ulong)EnumButtonType.START, AxiInputEx.ByKeyCode(KeyCode.Keypad0));
            controllers[1].SetKey((ulong)EnumButtonType.SELECT, AxiInputEx.ByKeyCode(KeyCode.Delete));
            controllers[1].SetKey((ulong)EnumButtonType.UP, AxiInputEx.ByKeyCode(KeyCode.UpArrow));
            controllers[1].SetKey((ulong)EnumButtonType.DOWN, AxiInputEx.ByKeyCode(KeyCode.DownArrow));
            controllers[1].SetKey((ulong)EnumButtonType.LEFT, AxiInputEx.ByKeyCode(KeyCode.LeftArrow));
            controllers[1].SetKey((ulong)EnumButtonType.RIGHT, AxiInputEx.ByKeyCode(KeyCode.RightArrow));
            controllers[1].SetKey((ulong)EnumButtonType.A, AxiInputEx.ByKeyCode(KeyCode.Keypad2));
            controllers[1].SetKey((ulong)EnumButtonType.B, AxiInputEx.ByKeyCode(KeyCode.Keypad1));
            controllers[1].SetKey((ulong)EnumButtonType.MIC, AxiInputEx.ByKeyCode(KeyCode.Keypad4));

            controllers[0].ColletAllKey();
            #endregion
        }
    }
    public class NESSingleKeysSeting : SingleKeySettingBase
    {
    }
}
