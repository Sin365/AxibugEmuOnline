using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.AppMain.AxiInput.Settings
{
    public enum UMAMEKSingleKey
    {
        INSERT_COIN,
        GAMESTART,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        BTN_A,
        BTN_B,
        BTN_C,
        BTN_D,
        BTN_E,
        BTN_F
    }
    public class UMAMEMultiKeysSetting : MultiKeysSetting
    {
        public UMAMEKSingleKeysSeting[] controllers;

        public UMAMEMultiKeysSetting()
        {
            controllers = new UMAMEKSingleKeysSeting[4];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new UMAMEKSingleKeysSeting();
        }

        public bool HadAnyKeyDown(int index)
        {
            if (index >= controllers.Length)
                return false;
            return controllers[index].HadAnyKeyDown();
        }
        public void ClearAll()
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].ClearAll();
            }
        }

        public void LoadDefaultSetting()
        {
            ClearAll();
#if UNITY_PSP2 && !UNITY_EDITOR
            //PSV 摇杆
            controllers[0].SetKey((ulong)MAMEKSingleKey.GAMESTART, AxiInputEx.ByKeyCode(PSVitaKey.Start));
            controllers[0].SetKey((ulong)MAMEKSingleKey.INSERT_COIN, AxiInputEx.ByKeyCode(PSVitaKey.Select));
            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByKeyCode(PSVitaKey.Up));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByKeyCode(PSVitaKey.Down));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByKeyCode(PSVitaKey.Left));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByKeyCode(PSVitaKey.Right));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_A, AxiInputEx.ByKeyCode(PSVitaKey.Block));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_B, AxiInputEx.ByKeyCode(PSVitaKey.Cross));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_C, AxiInputEx.ByKeyCode(PSVitaKey.Circle));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_D, AxiInputEx.ByKeyCode(PSVitaKey.Triangle));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_E, AxiInputEx.ByKeyCode(PSVitaKey.L));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_F, AxiInputEx.ByKeyCode(PSVitaKey.R));
            //PSV 摇杆
            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));
            controllers[0].ColletAllKey();
            return;
#endif
            #region P1
            //P1 键盘
            controllers[0].SetKey((ulong)UMAMEKSingleKey.GAMESTART, AxiInputEx.ByKeyCode(KeyCode.Alpha1));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.INSERT_COIN, AxiInputEx.ByKeyCode(KeyCode.Alpha5));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.UP, AxiInputEx.ByKeyCode(KeyCode.W));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.DOWN, AxiInputEx.ByKeyCode(KeyCode.S));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.LEFT, AxiInputEx.ByKeyCode(KeyCode.A));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.RIGHT, AxiInputEx.ByKeyCode(KeyCode.D));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_A, AxiInputEx.ByKeyCode(KeyCode.J));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_B, AxiInputEx.ByKeyCode(KeyCode.K));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_C, AxiInputEx.ByKeyCode(KeyCode.L));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_D, AxiInputEx.ByKeyCode(KeyCode.U));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_E, AxiInputEx.ByKeyCode(KeyCode.I));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_F, AxiInputEx.ByKeyCode(KeyCode.O));

            //Axis
            controllers[0].SetKey((ulong)UMAMEKSingleKey.UP, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.DOWN, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.LEFT, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.RIGHT, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));

            //P1 UGUI
            controllers[0].SetKey((ulong)UMAMEKSingleKey.GAMESTART, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_1));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.INSERT_COIN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_2));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.UP, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.DOWN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.LEFT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.RIGHT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_A, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_1));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_B, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_2));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_C, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_3));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_D, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_4));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_E, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_5));
            controllers[0].SetKey((ulong)UMAMEKSingleKey.BTN_F, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_6));

            controllers[0].ColletAllKey();
            #endregion
        }
    }
    public class UMAMEKSingleKeysSeting : SingleKeysSetting
    {
        Dictionary<UMAMEKSingleKey, List<AxiInput>> DictSkey2AxiInput = new Dictionary<UMAMEKSingleKey, List<AxiInput>>();
        AxiInput[] AxiInputArr = null;

        public void SetKey(ulong Key, AxiInput input)
        {
            List<AxiInput> list;
            if (!DictSkey2AxiInput.TryGetValue((UMAMEKSingleKey)Key, out list))
                list = DictSkey2AxiInput[(UMAMEKSingleKey)Key] = ObjectPoolAuto.AcquireList<AxiInput>();
            list.Add(input);
        }

        public bool GetKey(UMAMEKSingleKey Key)
        {
            List<AxiInput> list;
            if (!DictSkey2AxiInput.TryGetValue(Key, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsKey())
                    return true;
            }
            return false;
        }

        public bool GetKey(ulong Key)
        {
            return GetKey((UMAMEKSingleKey)Key);
        }

        public void ClearAll()
        {
            foreach (List<AxiInput> singlelist in DictSkey2AxiInput.Values)
                ObjectPoolAuto.Release(singlelist);
            DictSkey2AxiInput.Clear();
            AxiInputArr = null;
        }

        public void ColletAllKey()
        {
            List<AxiInput> list = ObjectPoolAuto.AcquireList<AxiInput>();
            foreach (List<AxiInput> singlelist in DictSkey2AxiInput.Values)
                list.AddRange(singlelist);
            AxiInputArr = list.ToArray();
            ObjectPoolAuto.Release(list);
        }

        public bool HadAnyKeyDown()
        {
            if (AxiInputArr == null)
                return false;

            for (int i = 0; i < AxiInputArr.Length; i++)
            {
                if (AxiInputArr[i].IsKey())
                    return true;
            }
            return false;
        }

    }
}
