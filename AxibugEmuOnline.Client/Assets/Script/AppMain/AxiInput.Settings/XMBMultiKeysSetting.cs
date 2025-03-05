using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Script.AppMain.AxiInput.Settings
{
    public class XMBMultiKeysSetting : MultiKeysSetting
    {
        public XMBSingleKeysSeting[] controllers;

        public XMBMultiKeysSetting()
        {
            controllers = new XMBSingleKeysSeting[1];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new XMBSingleKeysSeting();
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
            controllers[0].SetKey((ulong)EnumCommand.SelectItemUp, AxiInputEx.ByKeyCode(PSVitaKey.Up));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemDown, AxiInputEx.ByKeyCode(PSVitaKey.Down));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemLeft, AxiInputEx.ByKeyCode(PSVitaKey.Left));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemRight, AxiInputEx.ByKeyCode(PSVitaKey.Right));
            controllers[0].SetKey((ulong)EnumCommand.Enter, AxiInputEx.ByKeyCode(PSVitaKey.Circle));
            controllers[0].SetKey((ulong)EnumCommand.Back, AxiInputEx.ByKeyCode(PSVitaKey.Cross));
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PSVitaKey.Triangle));
            controllers[0].ColletAllKey();
            return;
#endif
            //键盘
            controllers[0].SetKey((ulong)EnumCommand.SelectItemUp, AxiInputEx.ByKeyCode(KeyCode.W));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemDown, AxiInputEx.ByKeyCode(KeyCode.S));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemLeft, AxiInputEx.ByKeyCode(KeyCode.A));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemRight, AxiInputEx.ByKeyCode(KeyCode.D));
            controllers[0].SetKey((ulong)EnumCommand.Enter, AxiInputEx.ByKeyCode(KeyCode.J));
            controllers[0].SetKey((ulong)EnumCommand.Enter, AxiInputEx.ByKeyCode(KeyCode.Return));
            controllers[0].SetKey((ulong)EnumCommand.Back, AxiInputEx.ByKeyCode(KeyCode.K));
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(KeyCode.I));

            //Axis
            controllers[0].SetKey((ulong)EnumCommand.SelectItemUp, AxiInputEx.ByAxis(AxiInputAxisType.UP));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemDown, AxiInputEx.ByAxis(AxiInputAxisType.DOWN));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemLeft, AxiInputEx.ByAxis(AxiInputAxisType.LEFT));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemRight, AxiInputEx.ByAxis(AxiInputAxisType.RIGHT));

            //P1 UGUI
            controllers[0].SetKey((ulong)EnumCommand.SelectItemUp, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.UP));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemDown, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.DOWN));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemLeft, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.LEFT));
            controllers[0].SetKey((ulong)EnumCommand.SelectItemRight, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.RIGHT));
            controllers[0].SetKey((ulong)EnumCommand.Enter, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_1));
            controllers[0].SetKey((ulong)EnumCommand.Back, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.BTN_2));
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByUGUIBtn(AxiInputUGuiBtnType.HOME));

            //PC XBOX

            //TODO 待补全
            controllers[0].SetKey((ulong)EnumCommand.Enter, AxiInputEx.ByKeyCode(PC_XBOXKEY.MenuBtn));
            controllers[0].SetKey((ulong)EnumCommand.Back, AxiInputEx.ByKeyCode(PC_XBOXKEY.ViewBtn));
            controllers[0].SetKey((ulong)EnumCommand.OptionMenu, AxiInputEx.ByKeyCode(PC_XBOXKEY.Y));

            controllers[0].ColletAllKey();
        }
    }

    public class XMBSingleKeysSeting : SingleKeysSetting
    {
        Dictionary<EnumCommand, List<AxiInput>> DictSkey2AxiInput = new Dictionary<EnumCommand, List<AxiInput>>();
        AxiInput[] AxiInputArr = null;

        public void SetKey(ulong Key, AxiInput input)
        {
            List<AxiInput> list;
            if (!DictSkey2AxiInput.TryGetValue((EnumCommand)Key, out list))
                list = DictSkey2AxiInput[(EnumCommand)Key] = ObjectPoolAuto.AcquireList<AxiInput>();
            list.Add(input);
        }

        public bool GetKey(EnumCommand Key)
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

            for (int i = 0; i < AxiInputArr.Length; i++)
            {
                if (AxiInputArr[i].IsKey())
                    return true;
            }
            return false;
        }

        public bool GetKey(ulong Key)
        {
            return GetKey((EnumCommand)Key);
        }

        internal EnumCommand[] GetAllCmd()
        {
            return DictSkey2AxiInput.Keys.ToArray();
        }

    }
}
