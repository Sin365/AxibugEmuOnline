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
            if (Controller1.ConnectSlot.HasValue) m_states[Controller1.ConnectSlot.Value] = Controller1.GetButtons();
            if (Controller2.ConnectSlot.HasValue) m_states[Controller2.ConnectSlot.Value] = Controller2.GetButtons();
            if (Controller3.ConnectSlot.HasValue) m_states[Controller3.ConnectSlot.Value] = Controller3.GetButtons();

            var result = new ControllerState(m_states);
            return result;
        }

        /// <summary>
        /// Nes控制器
        /// </summary>
        public class Controller
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

            public static KeyListener GetKey(int controllerInput, EnumButtonType nesConBtnType)
            {
                string configKey = $"NES_{controllerInput}_{nesConBtnType}";
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
                return m_keyListener.IsPressing() ? m_buttonType : 0;
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
    }
}