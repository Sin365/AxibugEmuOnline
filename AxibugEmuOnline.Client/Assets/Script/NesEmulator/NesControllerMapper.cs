using System;
using System.Text;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class NesControllerMapper
    {
        public MapperSetter Player1 = new MapperSetter(1);
        public MapperSetter Player2 = new MapperSetter(2);
        public MapperSetter Player3 = new MapperSetter(3);
        public MapperSetter Player4 = new MapperSetter(4);

        public ControllerState CreateState()
        {
            var state1 = Player1.GetButtons();
            var state2 = Player2.GetButtons();
            var state3 = Player3.GetButtons();
            var state4 = Player4.GetButtons();

            var result = new ControllerState(state1, state2, state3, state4);
            return result;
        }

        public class Mapper
        {
            MapperSetter m_setter;
            EnumButtonType m_buttonType;
            IKeyListener m_keyListener;
            int m_controllerIndex => m_setter.ControllerIndex;

            public Mapper(MapperSetter setter, EnumButtonType buttonType)
            {
                m_setter = setter;
                m_buttonType = buttonType;

                loadConfig();
            }

            private void loadConfig()
            {
                m_keyListener = MapperSetter.GetKey_Legacy(m_controllerIndex, m_buttonType);

            }

            public EnumButtonType SampleKey()
            {
                return m_keyListener.IsPressing() ? m_buttonType : 0;
            }
        }

        public class MapperSetter
        {
            /// <summary> 控制器序号(手柄1,2,3,4) </summary>
            public int ControllerIndex { get; }
            public Mapper UP { get; private set; }
            public Mapper DOWN { get; private set; }
            public Mapper LEFT { get; private set; }
            public Mapper RIGHT { get; private set; }
            public Mapper A { get; private set; }
            public Mapper B { get; private set; }
            public Mapper SELECT { get; private set; }
            public Mapper START { get; private set; }
            public Mapper MIC { get; private set; }

            public MapperSetter(int controllerIndex)
            {
                ControllerIndex = controllerIndex;
                UP = new Mapper(this, EnumButtonType.UP);
                DOWN = new Mapper(this, EnumButtonType.DOWN);
                LEFT = new Mapper(this, EnumButtonType.LEFT);
                RIGHT = new Mapper(this, EnumButtonType.RIGHT);
                A = new Mapper(this, EnumButtonType.A);
                B = new Mapper(this, EnumButtonType.B);
                SELECT = new Mapper(this, EnumButtonType.SELECT);
                START = new Mapper(this, EnumButtonType.START);
                MIC = new Mapper(this, EnumButtonType.MIC);
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

            public static IKeyListener GetKey_Legacy(int controllerInput, EnumButtonType nesConBtnType)
            {
                string configKey = $"NES_{controllerInput}_{nesConBtnType}";

                if (PlayerPrefs.HasKey(configKey))
                {
                    return new KeyListener_Legacy(PlayerPrefs.GetString(configKey));
                }
                else
                {
                    var defaultKeyCode = GetDefaultKey();
                    PlayerPrefs.SetString(configKey, defaultKeyCode.ToString());
                    return defaultKeyCode;
                }

                KeyListener_Legacy GetDefaultKey()
                {
                    switch (controllerInput)
                    {
                        case 1:
                            if (nesConBtnType == EnumButtonType.LEFT) return new KeyListener_Legacy(KeyCode.A, KeyCode.Joystick1Button12);
                            if (nesConBtnType == EnumButtonType.RIGHT) return new KeyListener_Legacy(KeyCode.D, KeyCode.Joystick1Button13);
                            if (nesConBtnType == EnumButtonType.UP) return new KeyListener_Legacy(KeyCode.W, KeyCode.Joystick1Button10);
                            if (nesConBtnType == EnumButtonType.DOWN) return new KeyListener_Legacy(KeyCode.S, KeyCode.Joystick1Button11);
                            if (nesConBtnType == EnumButtonType.START) return new KeyListener_Legacy(KeyCode.B, KeyCode.Joystick1Button7);
                            if (nesConBtnType == EnumButtonType.SELECT) return new KeyListener_Legacy(KeyCode.V, KeyCode.Joystick1Button6);
                            if (nesConBtnType == EnumButtonType.A) return new KeyListener_Legacy(KeyCode.K, KeyCode.Joystick1Button1);
                            if (nesConBtnType == EnumButtonType.B) return new KeyListener_Legacy(KeyCode.J, KeyCode.Joystick1Button2);
                            if (nesConBtnType == EnumButtonType.MIC) return new KeyListener_Legacy(KeyCode.M, KeyCode.Joystick1Button12);
                            break;
                        case 2:
                            if (nesConBtnType == EnumButtonType.LEFT) return new KeyListener_Legacy(KeyCode.Delete, KeyCode.Joystick2Button12);
                            if (nesConBtnType == EnumButtonType.RIGHT) return new KeyListener_Legacy(KeyCode.PageDown, KeyCode.Joystick2Button13);
                            if (nesConBtnType == EnumButtonType.UP) return new KeyListener_Legacy(KeyCode.Home, KeyCode.Joystick2Button10);
                            if (nesConBtnType == EnumButtonType.DOWN) return new KeyListener_Legacy(KeyCode.End, KeyCode.Joystick2Button11);
                            if (nesConBtnType == EnumButtonType.START) return new KeyListener_Legacy(KeyCode.PageUp, KeyCode.Joystick2Button7);
                            if (nesConBtnType == EnumButtonType.SELECT) return new KeyListener_Legacy(KeyCode.Insert, KeyCode.Joystick2Button6);
                            if (nesConBtnType == EnumButtonType.A) return new KeyListener_Legacy(KeyCode.Keypad5, KeyCode.Joystick2Button1);
                            if (nesConBtnType == EnumButtonType.B) return new KeyListener_Legacy(KeyCode.Keypad4, KeyCode.Joystick2Button2);
                            if (nesConBtnType == EnumButtonType.MIC) return new KeyListener_Legacy(KeyCode.KeypadPeriod, KeyCode.Joystick2Button12);
                            break;
                    }

                    return default;
                }

            }
        }

        public interface IKeyListener
        {
            bool IsPressing();
        }

        public struct KeyListener_Legacy : IKeyListener
        {
            private KeyCode[] m_keys;

            public KeyListener_Legacy(params KeyCode[] keys)
            {
                m_keys = keys;
            }

            /// <summary>
            /// 从配置表字符串构建
            /// </summary>
            /// <param name="confStr">以:分割的键值字符串</param>
            public KeyListener_Legacy(string confStr)
            {
                m_keys = new KeyCode[2];

                var temp = confStr.Split(':');
                m_keys = new KeyCode[temp.Length];
                for (int i = 0; i < temp.Length; i++)
                {
                    if (int.TryParse(temp[i], out int result))
                        m_keys[i] = (KeyCode)result;
                }
            }

            public bool IsPressing()
            {
                if (m_keys == null || m_keys.Length == 0) return false;

                foreach (var key in m_keys)
                {
                    if (Input.GetKey(key)) return true;
                }

                return false;
            }

            public override string ToString()
            {
                if (m_keys == null || m_keys.Length == 0) return string.Empty;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < m_keys.Length; i++)
                {
                    var keyVal = (int)m_keys[i];
                    sb.Append(keyVal);
                    if (i != m_keys.Length - 1) sb.Append(':');
                }

                return sb.ToString();
            }
        }
    }


}
