using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class NesControllerMapper : IControllerSetuper
    {
        public Controller Controller0 { get; } = new Controller(0);
        public Controller Controller1 { get; } = new Controller(1);
        public Controller Controller2 { get; } = new Controller(2);
        public Controller Controller3 { get; } = new Controller(3);

        private readonly EnumButtonType[] m_states = new EnumButtonType[4];

        public ControllerState CreateState()
        {
            m_states[0] = m_states[1] = m_states[2] = m_states[3] = 0;

            if (Controller0.ConnectSlot.HasValue) m_states[Controller0.ConnectSlot.Value] = Controller0.GetButtons();
            else if (Controller0.AnyButtonDown()) Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 0);

            if (Controller1.ConnectSlot.HasValue) m_states[Controller1.ConnectSlot.Value] = Controller1.GetButtons();
            else if (Controller1.AnyButtonDown()) Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 1);

            if (Controller2.ConnectSlot.HasValue) m_states[Controller2.ConnectSlot.Value] = Controller2.GetButtons();
            else if (Controller2.AnyButtonDown()) Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 2);

            if (Controller3.ConnectSlot.HasValue) m_states[Controller3.ConnectSlot.Value] = Controller3.GetButtons();
            else if (Controller3.AnyButtonDown()) Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 3);

            var result = new ControllerState(m_states);
            return result;
        }

        public void SetConnect(uint? con0ToSlot = null,
            uint? con1ToSlot = null,
            uint? con2ToSlot = null,
            uint? con3ToSlot = null)
        {
            Controller0.ConnectSlot = con0ToSlot;
            Controller1.ConnectSlot = con1ToSlot;
            Controller2.ConnectSlot = con2ToSlot;
            Controller3.ConnectSlot = con3ToSlot;
        }

        public int? GetSlotConnectingControllerIndex(int slotIndex)
        {
            if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return 0;
            else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return 1;
            else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return 2;
            else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return 3;
            else return null;
        }

        public IController GetSlotConnectingController(int slotIndex)
        {
            if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return Controller0;
            else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return Controller1;
            else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return Controller2;
            else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return Controller3;
            else return null;
        }

        //static HashSet<uint> s_temp = new HashSet<uint>(4);
        //低版本不能这样初始化
        static HashSet<uint> s_temp = new HashSet<uint>();
        public uint? GetFreeSlotIndex()
        {
            s_temp.Clear();
            s_temp.Add(0);
            s_temp.Add(1);
            s_temp.Add(2);
            s_temp.Add(3);

            if (Controller0.ConnectSlot.HasValue) s_temp.Remove(Controller0.ConnectSlot.Value);
            if (Controller1.ConnectSlot.HasValue) s_temp.Remove(Controller1.ConnectSlot.Value);
            if (Controller2.ConnectSlot.HasValue) s_temp.Remove(Controller2.ConnectSlot.Value);
            if (Controller3.ConnectSlot.HasValue) s_temp.Remove(Controller3.ConnectSlot.Value);

            if (s_temp.Count > 0) return s_temp.First();
            else return null;
        }

        public void LetControllerConnect(int conIndex, uint slotIndex)
        {
            Controller targetController;
            switch (conIndex)
            {
                case 0: targetController = Controller0; break;
                case 1: targetController = Controller1; break;
                case 2: targetController = Controller2; break;
                case 3: targetController = Controller3; break;
                default:
                    throw new System.Exception($"Not Allowed conIndex Range: {conIndex}");
            }

            if (targetController.ConnectSlot.HasValue) return;

            targetController.ConnectSlot = slotIndex;

            Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
        }

        /// <summary>
        /// Nes控制器
        /// </summary>
        public class Controller : IController
        {
            static Lazy<NesKeyBinding> s_keyBinder = new Lazy<NesKeyBinding>(() => App.settings.KeyMapper.GetBinder<NesKeyBinding>());
            /// <summary>
            /// 控制器编号
            /// <para><c>此编号并非对应游戏中的player1,player2,player3,player4,仅仅作为本地4个手柄的实例</c></para>
            /// <value>[0,3]</value>
            /// </summary>
            public int ControllerIndex { get; }

            /// <summary>
            /// 指示该手柄连接的手柄插槽
            /// <para><c>这个值代表了该手柄在实际游戏中控制的Player</c></para>
            /// <value>[0,3] 例外:为空代表未连接</value>
            /// </summary>
            public uint? ConnectSlot { get; set; }

            public Controller(int controllerIndex)
            {
                ControllerIndex = controllerIndex;
            }

            public EnumButtonType GetButtons()
            {
                EnumButtonType res = 0;
                if (s_keyBinder.Value.GetKey(EnumButtonType.UP, ControllerIndex)) res |= EnumButtonType.UP;
                if (s_keyBinder.Value.GetKey(EnumButtonType.DOWN, ControllerIndex)) res |= EnumButtonType.DOWN;
                if (s_keyBinder.Value.GetKey(EnumButtonType.LEFT, ControllerIndex)) res |= EnumButtonType.LEFT;
                if (s_keyBinder.Value.GetKey(EnumButtonType.RIGHT, ControllerIndex)) res |= EnumButtonType.RIGHT;
                if (s_keyBinder.Value.GetKey(EnumButtonType.A, ControllerIndex)) res |= EnumButtonType.A;
                if (s_keyBinder.Value.GetKey(EnumButtonType.B, ControllerIndex)) res |= EnumButtonType.B;
                if (s_keyBinder.Value.GetKey(EnumButtonType.SELECT, ControllerIndex)) res |= EnumButtonType.SELECT;
                if (s_keyBinder.Value.GetKey(EnumButtonType.START, ControllerIndex)) res |= EnumButtonType.START;
                if (s_keyBinder.Value.GetKey(EnumButtonType.MIC, ControllerIndex)) res |= EnumButtonType.MIC;

                return res;
            }

            public bool AnyButtonDown()
            {
                return s_keyBinder.Value.AnyKeyDown(ControllerIndex);
            }

        }
    }
}