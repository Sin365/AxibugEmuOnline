using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
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
                    break;
            }
            //var targetController = conIndex switch
            //{
            //    0 => Controller0,
            //    1 => Controller1,
            //    2 => Controller2,
            //    3 => Controller3,
            //    _ => throw new System.Exception($"Not Allowed conIndex Range: {conIndex}")
            //};

            if (targetController.ConnectSlot.HasValue) return;

            targetController.ConnectSlot = slotIndex;

            Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
        }

        /// <summary>
        /// Nes控制器
        /// </summary>
        public class Controller : IController
        {
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

            public Button UP { get; }
            public Button DOWN { get; }
            public Button LEFT { get; }
            public Button RIGHT { get; }
            public Button A { get; }
            public Button B { get; }
            public Button SELECT { get; }
            public Button START { get; }
            public Button MIC { get; }

            public Controller(int controllerIndex)
            {
                ControllerIndex = controllerIndex;
                UP = new Button(this, EnumButtonType.UP);
                DOWN = new Button(this, EnumButtonType.DOWN);
                LEFT = new Button(this, EnumButtonType.LEFT);
                RIGHT = new Button(this, EnumButtonType.RIGHT);
                A = new Button(this, EnumButtonType.A);
                B = new Button(this, EnumButtonType.B);
                SELECT = new Button(this, EnumButtonType.SELECT);
                START = new Button(this, EnumButtonType.START);
                MIC = new Button(this, EnumButtonType.MIC);
            }

            public EnumButtonType GetButtons()
            {
                EnumButtonType res = 0;

                res |= UP.SampleKey();
                res |= DOWN.SampleKey();
                res |= LEFT.SampleKey();
                res |= RIGHT.SampleKey();
                res |= A.SampleKey();
                res |= B.SampleKey();
                res |= SELECT.SampleKey();
                res |= START.SampleKey();
                res |= MIC.SampleKey();

                return res;
            }

            public bool AnyButtonDown()
            {
                return
                    UP.IsDown ||
                    DOWN.IsDown ||
                    LEFT.IsDown ||
                    RIGHT.IsDown ||
                    A.IsDown ||
                    B.IsDown ||
                    SELECT.IsDown ||
                    START.IsDown ||
                    MIC.IsDown;
            }

            public static KeyListener GetKey(int controllerInput, EnumButtonType nesConBtnType)
            {
                string configKey = $"NES_{controllerInput}_{nesConBtnType}";

                //PSV平台固定键值
                if (UnityEngine.Application.platform == RuntimePlatform.PSP2)
                {
                    return KeyListener.GetPSVitaKey(controllerInput, nesConBtnType);
                }

                if (PlayerPrefs.HasKey(configKey))
                {
                    return new KeyListener(PlayerPrefs.GetString(configKey));
                }
                else
                {
                    var defaultKeyCode = KeyListener.GetDefaultKey(controllerInput, nesConBtnType);
                    PlayerPrefs.SetString(configKey, defaultKeyCode.ToString());
                    return defaultKeyCode;
                }
            }
        }

        /// <summary>
        /// NES控制器按键类
        /// </summary>
        public class Button
        {
            /// <summary> 所属控制器 </summary>
            readonly Controller m_hostController;

            /// <summary> 按键 </summary>
            readonly EnumButtonType m_buttonType;

            /// <summary> 按键监听器 </summary>
            KeyListener m_keyListener;

            /// <summary> 指示按钮是否正在按下状态 </summary>
            public bool IsPressing => m_keyListener.IsPressing();
            /// <summary> 指示按钮是否被按下 </summary>
            public bool IsDown => m_keyListener.IsDown();

            public Button(Controller controller, EnumButtonType buttonType)
            {
                m_hostController = controller;
                m_buttonType = buttonType;

                CreateListener();
            }

            /// <summary>
            /// 采集按钮按下状态
            /// </summary>
            /// <returns></returns>
            public EnumButtonType SampleKey()
            {
                return IsPressing ? m_buttonType : 0;
            }

            private void CreateListener()
            {
                m_keyListener = Controller.GetKey(m_hostController.ControllerIndex, m_buttonType);
            }
        }
        //low C# readonly
        //public readonly struct KeyListener

        public struct KeyListener
        {
            private readonly KeyCode m_key;

            public KeyListener(KeyCode key)
            {
                m_key = key;
            }

            /// <summary> 从配置字符串构建 </summary>
            public KeyListener(string confStr)
            {
                m_key = KeyCode.None;

                int result;
                if (int.TryParse(confStr, out result))
                    m_key = (KeyCode)result;
            }

            public bool IsPressing()
            {
                return Input.GetKey(m_key);
            }
            public bool IsDown()
            {
                return Input.GetKeyDown(m_key);
            }

            public override string ToString()
            {
                return ((int)(m_key)).ToString();
            }

            public static KeyListener GetDefaultKey(int controllerIndex, EnumButtonType nesConBtnType)
            {
                switch (controllerIndex)
                {
                    case 0:
                        switch (nesConBtnType)
                        {
                            case EnumButtonType.LEFT:
                                return new KeyListener(KeyCode.A);
                            case EnumButtonType.RIGHT:
                                return new KeyListener(KeyCode.D);
                            case EnumButtonType.UP:
                                return new KeyListener(KeyCode.W);
                            case EnumButtonType.DOWN:
                                return new KeyListener(KeyCode.S);
                            case EnumButtonType.START:
                                return new KeyListener(KeyCode.B);
                            case EnumButtonType.SELECT:
                                return new KeyListener(KeyCode.V);
                            case EnumButtonType.A:
                                return new KeyListener(KeyCode.K);
                            case EnumButtonType.B:
                                return new KeyListener(KeyCode.J);
                            case EnumButtonType.MIC:
                                return new KeyListener(KeyCode.M);
                        }

                        break;
                    case 1:
                        switch (nesConBtnType)
                        {
                            case EnumButtonType.LEFT:
                                return new KeyListener(KeyCode.Delete);
                            case EnumButtonType.RIGHT:
                                return new KeyListener(KeyCode.PageDown);
                            case EnumButtonType.UP:
                                return new KeyListener(KeyCode.Home);
                            case EnumButtonType.DOWN:
                                return new KeyListener(KeyCode.End);
                            case EnumButtonType.START:
                                return new KeyListener(KeyCode.PageUp);
                            case EnumButtonType.SELECT:
                                return new KeyListener(KeyCode.Insert);
                            case EnumButtonType.A:
                                return new KeyListener(KeyCode.Keypad5);
                            case EnumButtonType.B:
                                return new KeyListener(KeyCode.Keypad4);
                            case EnumButtonType.MIC:
                                return new KeyListener(KeyCode.KeypadPeriod);
                        }

                        break;
                }

                return default(KeyListener);
            }


            public static KeyListener GetPSVitaKey(int controllerIndex, EnumButtonType nesConBtnType)
            {
                switch (controllerIndex)
                {
                    case 0:
                        switch (nesConBtnType)
                        {
                            case EnumButtonType.LEFT:
                                return new KeyListener(PSVitaKey.Left);
                            case EnumButtonType.RIGHT:
                                return new KeyListener(PSVitaKey.Right);
                            case EnumButtonType.UP:
                                return new KeyListener(PSVitaKey.Up);
                            case EnumButtonType.DOWN:
                                return new KeyListener(PSVitaKey.Down);
                            case EnumButtonType.START:
                                return new KeyListener(PSVitaKey.Start);
                            case EnumButtonType.SELECT:
                                return new KeyListener(PSVitaKey.Select);
                            case EnumButtonType.A:
                                return new KeyListener(PSVitaKey.Circle);
                            case EnumButtonType.B:
                                return new KeyListener(PSVitaKey.Cross);
                            case EnumButtonType.MIC:
                                return new KeyListener(PSVitaKey.Block);
                        }
                        break;
                }
                return default(KeyListener);
            }
        }
    }
}