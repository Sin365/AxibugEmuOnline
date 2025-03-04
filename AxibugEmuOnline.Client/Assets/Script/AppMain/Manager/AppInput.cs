using Assets.Script.AppMain.AxiInput;
using AxibugEmuOnline.Client.Common;
using System.Collections.Generic;
using UnityEngine;
using static AxibugEmuOnline.Client.Manager.MAMEKSingleKeysSeting;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppInput
    {
        public MAMEKMultiKeysSetting mame;
        public AppInput()
        {
            mame = new MAMEKMultiKeysSetting();
            LoadDefaultSetting();
        }

        public void LoadDefaultSetting()
        {
            mame.LoadDefaultSetting();
        }
    }

    public interface MultiKeysSetting
    {
        bool HadAnyKeyDown(int index);
        void ClearAll();
        void LoadDefaultSetting();
    }

    public interface SingleKeysSetting
    {
        void ClearAll();
        void SetKey(ulong Key, AxiInput input);
        void ColletAllKey();
        bool HadAnyKeyDown();
    }

    public class MAMEKMultiKeysSetting : MultiKeysSetting
    {
        public MAMEKSingleKeysSeting[] controllers;

        public MAMEKMultiKeysSetting()
        {
            controllers = new MAMEKSingleKeysSeting[4];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new MAMEKSingleKeysSeting();
        }

        public bool HadAnyKeyDown(int index)
        {
            if (index >= controllers.Length)
                return false;
            return controllers[index].HadAnyKeyDown();
        }
        public void ClearAll()
        {
            controllers[0].ClearAll();
            controllers[1].ClearAll();
            controllers[2].ClearAll();
            controllers[3].ClearAll();
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
            controllers[0].SetKey((ulong)MAMEKSingleKey.GAMESTART, AxiInputEx.ByKeyCode(KeyCode.Alpha1));
            controllers[0].SetKey((ulong)MAMEKSingleKey.INSERT_COIN, AxiInputEx.ByKeyCode(KeyCode.Alpha5));

            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByKeyCode(KeyCode.W));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByKeyCode(KeyCode.S));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByKeyCode(KeyCode.A));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByKeyCode(KeyCode.D));


            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByKeyCode(KeyCode.G));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByKeyCode(KeyCode.V));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByKeyCode(KeyCode.C));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByKeyCode(KeyCode.B));

            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_A, AxiInputEx.ByKeyCode(KeyCode.J));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_B, AxiInputEx.ByKeyCode(KeyCode.K));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_C, AxiInputEx.ByKeyCode(KeyCode.L));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_D, AxiInputEx.ByKeyCode(KeyCode.U));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_E, AxiInputEx.ByKeyCode(KeyCode.I));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_F, AxiInputEx.ByKeyCode(KeyCode.O));

            //Axis
            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));

            //P1 UGUI
            controllers[0].SetKey((ulong)MAMEKSingleKey.GAMESTART, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_1));
            controllers[0].SetKey((ulong)MAMEKSingleKey.INSERT_COIN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.POTION_2));
            controllers[0].SetKey((ulong)MAMEKSingleKey.UP, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)MAMEKSingleKey.DOWN, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)MAMEKSingleKey.LEFT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)MAMEKSingleKey.RIGHT, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_A, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_1));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_B, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_2));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_C, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_3));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_D, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_4));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_E, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_5));
            controllers[0].SetKey((ulong)MAMEKSingleKey.BTN_F, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_6));

            controllers[0].ColletAllKey();
            #endregion
        }
    }

    public class MAMEKSingleKeysSeting : SingleKeysSetting
    {
        public enum MAMEKSingleKey
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

        Dictionary<MAMEKSingleKey, List<AxiInput>> DictSkey2AxiInput = new Dictionary<MAMEKSingleKey, List<AxiInput>>();
        AxiInput[] AxiInputArr = null;

        public void SetKey(ulong Key, AxiInput input)
        {
            List<AxiInput> list;
            if (!DictSkey2AxiInput.TryGetValue((MAMEKSingleKey)Key, out list))
                list = DictSkey2AxiInput[(MAMEKSingleKey)Key] = ObjectPoolAuto.AcquireList<AxiInput>();
            list.Add(input);
        }

        public bool GetKey(MAMEKSingleKey Key)
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

            for (int i = 0; AxiInputArr.Length > 0; i++)
            {
                if (AxiInputArr[i].IsKey())
                    return true;
            }
            return false;
        }

    }
}