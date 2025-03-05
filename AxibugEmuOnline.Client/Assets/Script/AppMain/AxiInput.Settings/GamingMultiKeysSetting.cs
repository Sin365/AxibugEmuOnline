using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Script.AppMain.AxiInput.Settings
{
    public class GamingMultiKeysSetting : MultiKeysSetting
    {
        public GamingSingleKeysSeting[] controllers;

        public GamingMultiKeysSetting()
        {
            controllers = new GamingSingleKeysSeting[1];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new GamingSingleKeysSeting();
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

    public class GamingSingleKeysSeting : SingleKeysSetting
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
