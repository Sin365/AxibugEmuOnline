﻿using AxibugEmuOnline.Client.Common;
using AxiInputSP;
using AxiInputSP.Setting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppInput
    {
        public XMBMultiKeysSetting xmb;
        public GamingMultiKeysSetting gaming;
        public UMAMEMultiKeysSetting mame;
        public NESMultiKeysSetting nes;
        public MasterSystemMultiKeysSetting sms;
        public ColecoVisionMultiKeysSetting cv;
        public GameBoyColorMultiKeysSetting gbc;
        public GameBoyMultiKeysSetting gb;
        public GameGearMultiKeysSetting gg;
        public SC3000MultiKeysSetting sc3000;
        public SG1000MultiKeysSetting sg1000;
        public AppInput()
        {
            xmb = new XMBMultiKeysSetting();
            gaming = new GamingMultiKeysSetting();
            mame = new UMAMEMultiKeysSetting();
            nes = new NESMultiKeysSetting();
            sms = new MasterSystemMultiKeysSetting();
            cv = new ColecoVisionMultiKeysSetting();
            gbc = new GameBoyColorMultiKeysSetting();
            gb = new GameBoyMultiKeysSetting();
            gg = new GameGearMultiKeysSetting();
            sc3000 = new SC3000MultiKeysSetting();
            sg1000 = new SG1000MultiKeysSetting();
            LoadDefaultSetting();
        }

        public void LoadDefaultSetting()
        {
            xmb.LoadDefaultSetting();
            gaming.LoadDefaultSetting();
            mame.LoadDefaultSetting();
            nes.LoadDefaultSetting();
            sms.LoadDefaultSetting();
            cv.LoadDefaultSetting();
            gbc.LoadDefaultSetting();
            gb.LoadDefaultSetting();
            gg.LoadDefaultSetting();
            sc3000.LoadDefaultSetting();
            sg1000.LoadDefaultSetting();
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
        bool GetKey(ulong Key);
        bool GetKeyDown(ulong Key);
        bool GetKeyUp(ulong Key);
        void ColletAllKey();
        bool HadAnyKeyDown();
    }

    public abstract class MultiKeysSettingBase : MultiKeysSetting
    {
        public SingleKeySettingBase[] controllers;

        public bool HadAnyKeyDown(int index)
        {
            if (index >= controllers.Length)
                return false;
            return controllers[index].HadAnyKeyDown();
        }
        public void ClearAll()
        {
            for (int i = 0; i < controllers.Length; i++)
                controllers[i].ClearAll();
        }

        public abstract void LoadDefaultSetting();
    }


    public abstract class SingleKeySettingBase : SingleKeysSetting
    {
        protected Dictionary<ulong, List<AxiInput>> mDictSkey2AxiInput = new Dictionary<ulong, List<AxiInput>>();
        protected AxiInput[] AxiInputArr = null;

        public void SetKey(ulong Key, AxiInput input)
        {
            List<AxiInput> list;
            if (!mDictSkey2AxiInput.TryGetValue(Key, out list))
                list = mDictSkey2AxiInput[Key] = ObjectPoolAuto.AcquireList<AxiInput>();
            list.Add(input);
        }

        public bool GetKey(ulong Key)
        {
            List<AxiInput> list;
            if (!mDictSkey2AxiInput.TryGetValue(Key, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKey())
                    return true;
            }
            return false;
        }
        public bool GetKeyUp(ulong Key)
        {
            List<AxiInput> list;
            if (!mDictSkey2AxiInput.TryGetValue(Key, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKeyUp())
                    return true;
            }
            return false;
        }

        public bool GetKeyDown(ulong Key)
        {
            List<AxiInput> list;
            if (!mDictSkey2AxiInput.TryGetValue(Key, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKeyDown())
                    return true;
            }
            return false;
        }

        public void ClearAll()
        {
            foreach (List<AxiInput> singlelist in mDictSkey2AxiInput.Values)
                ObjectPoolAuto.Release(singlelist);
            mDictSkey2AxiInput.Clear();
            AxiInputArr = null;
        }

        public void ColletAllKey()
        {
            List<AxiInput> list = ObjectPoolAuto.AcquireList<AxiInput>();
            foreach (List<AxiInput> singlelist in mDictSkey2AxiInput.Values)
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
                if (AxiInputArr[i].GetKey())
                    return true;
            }
            return false;
        }

        public T[] GetAllCmd<T>()
        {
            return mDictSkey2AxiInput.Keys.Select(k => (T)Enum.ToObject(typeof(T), k)).ToArray();
        }
    }

}