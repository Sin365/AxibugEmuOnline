using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxiInputSP.UGUI
{
    public class AxiScreenGamepad : MonoBehaviour
    {
        public delegate void OnAxiScreenGamepadActiveHandle(AxiScreenGamepad sender);
        public delegate void OnAxiScreenGamepadDisactiveHandle(AxiScreenGamepad sender);

        public static event OnAxiScreenGamepadActiveHandle OnGamepadActive;
        public static event OnAxiScreenGamepadActiveHandle OnGamepadDisactive;

        AxiIptButton[] m_buttons;
        FloatingJoystick m_joystick;
        HashSet<AxiInputUGuiBtnType> m_pressBtns = new HashSet<AxiInputUGuiBtnType>();
        Vector2 m_joyStickRaw;

        public Transform tfXMB;
        public Transform tfNES;
        public Transform tfGLOBAL;
        public Transform tfMAME;
        public Transform tfMAME_NEOGEO;
        public Transform tfGAMEBOYCOLOR;
        public Transform tfMASTERSYSTEM;
        List<Transform> mPlatfromList = new List<Transform>();


        public bool GetKey(AxiInputUGuiBtnType btnType)
        {
            return m_pressBtns.Contains(btnType);
        }

        public Vector2 GetJoystickValue()
        {
            return m_joyStickRaw;
        }

        private void Update()
        {
            m_joyStickRaw = m_joystick.GetJoyRaw();
            m_pressBtns.Clear();
            foreach (var btn in m_buttons)
            {
                if (btn.GetKey())
                {
                    foreach (var btnType in btn.axiBtnTypeList)
                        m_pressBtns.Add(btnType);
                }
            }
        }

        private void Awake()
        {
            m_buttons = GetComponentsInChildren<AxiIptButton>(true);
            m_joystick = GetComponentInChildren<FloatingJoystick>(true);

            mPlatfromList.Add(tfXMB);
            mPlatfromList.Add(tfNES);
            mPlatfromList.Add(tfGLOBAL);
            mPlatfromList.Add(tfMAME);
            mPlatfromList.Add(tfMAME_NEOGEO);
            mPlatfromList.Add(tfGAMEBOYCOLOR);
            mPlatfromList.Add(tfMASTERSYSTEM);
        }

        private void OnEnable()
        {
            m_joyStickRaw = Vector2.zero;
            m_pressBtns.Clear();
            OnGamepadActive?.Invoke(this);
            ChangePlatfrom();
            Eventer.Instance.RegisterEvent(EEvent.OnScreenGamepadPlatformTypeChanged, OnRomPlatformTypeChanged);
        }


        private void OnDisable()
        {
            OnGamepadDisactive?.Invoke(this);
            Eventer.Instance.UnregisterEvent(EEvent.OnScreenGamepadPlatformTypeChanged, OnRomPlatformTypeChanged);
        }

        private void OnRomPlatformTypeChanged()
        {
            App.log.Debug(">>OnRomPlatformTypeChanged");
            ChangePlatfrom();
        }

        RomPlatformType? _last_platformType = RomPlatformType.Invalid;
        void ChangePlatfrom()
        {
            RomPlatformType? platformType;
            //XMB
            if (App.emu.Core == null || CommandDispatcher.Instance.Mode == CommandListener.ScheduleType.Normal)
                platformType = null;
            else
                platformType = App.emu.Core.Platform;

            if (_last_platformType == platformType)
                return;

            _last_platformType = platformType;

            //先全部关闭
            foreach (var trans in mPlatfromList)
                trans.gameObject.SetActive(false);

            //切换时，重置所有键值,避免如按钮隐藏时候 OnPointerUp 未触发等问题
            foreach (var btn in m_buttons)
                btn.ResetKeyState();

            //开启指定平台
            GetPlatfromPanel(platformType).gameObject.SetActive(true);
        }

        Transform GetPlatfromPanel(RomPlatformType? platformType)
        {
            if (!platformType.HasValue)
                return tfXMB;

            switch (platformType)
            {
                case RomPlatformType.Nes:
                    return tfNES;
                case RomPlatformType.Neogeo:
                    return tfMAME_NEOGEO;
                case RomPlatformType.Igs:
                case RomPlatformType.Cps1:
                case RomPlatformType.Cps2:
                    return tfMAME;
                case RomPlatformType.MasterSystem:
                    return tfMASTERSYSTEM;
                case RomPlatformType.GameBoy:
                case RomPlatformType.GameBoyColor:
                    return tfGAMEBOYCOLOR;
                case RomPlatformType.GameGear:
                case RomPlatformType.ColecoVision:
                case RomPlatformType.Sc3000:
                case RomPlatformType.Sg1000:
                case RomPlatformType.ArcadeOld:
                case RomPlatformType.WonderSwan:
                case RomPlatformType.WonderSwanColor:
                case RomPlatformType.Invalid:
                case RomPlatformType.All:
                default:
                    return tfGLOBAL;
            }
        }
    }
}
