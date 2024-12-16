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
                m_keyListener = MapperSetter.GetKey(m_controllerIndex, m_buttonType);

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

            public static IKeyListener GetKey(int controllerInput, EnumButtonType nesConBtnType)
            {
                string configKey = $"NES_{controllerInput}_{nesConBtnType}";
                if (PlayerPrefs.HasKey(configKey))
                {
                    return new KeyListener(PlayerPrefs.GetString(configKey));
                }
                else
                {
                    var defaultKeyCode = GetDefaultKey();
                    PlayerPrefs.SetString(configKey, defaultKeyCode.ToString());
                    return defaultKeyCode;
                }

                KeyListener GetDefaultKey()
                {
                    switch (controllerInput)
                    {
                        case 1:
                            if (nesConBtnType == EnumButtonType.LEFT) return new KeyListener(KeyCode.A);
                            if (nesConBtnType == EnumButtonType.RIGHT) return new KeyListener(KeyCode.D);
                            if (nesConBtnType == EnumButtonType.UP) return new KeyListener(KeyCode.W);
                            if (nesConBtnType == EnumButtonType.DOWN) return new KeyListener(KeyCode.S);
                            if (nesConBtnType == EnumButtonType.START) return new KeyListener(KeyCode.B);
                            if (nesConBtnType == EnumButtonType.SELECT) return new KeyListener(KeyCode.V);
                            if (nesConBtnType == EnumButtonType.A) return new KeyListener(KeyCode.K);
                            if (nesConBtnType == EnumButtonType.B) return new KeyListener(KeyCode.J);
                            if (nesConBtnType == EnumButtonType.MIC) return new KeyListener(KeyCode.M);
                            break;
                        case 2:
                            if (nesConBtnType == EnumButtonType.LEFT) return new KeyListener(KeyCode.Delete);
                            if (nesConBtnType == EnumButtonType.RIGHT) return new KeyListener(KeyCode.PageDown);
                            if (nesConBtnType == EnumButtonType.UP) return new KeyListener(KeyCode.Home);
                            if (nesConBtnType == EnumButtonType.DOWN) return new KeyListener(KeyCode.End);
                            if (nesConBtnType == EnumButtonType.START) return new KeyListener(KeyCode.PageUp);
                            if (nesConBtnType == EnumButtonType.SELECT) return new KeyListener(KeyCode.Insert);
                            if (nesConBtnType == EnumButtonType.A) return new KeyListener(KeyCode.Keypad5);
                            if (nesConBtnType == EnumButtonType.B) return new KeyListener(KeyCode.Keypad4);
                            if (nesConBtnType == EnumButtonType.MIC) return new KeyListener(KeyCode.KeypadPeriod);
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

        public struct KeyListener : IKeyListener
        {
            private KeyCode m_key;

            public KeyListener(KeyCode key)
            {
                m_key = key;
            }

            /// <summary>
            /// 从配置表字符串构建
            /// </summary>
            public KeyListener(string confStr)
            {
                m_key = KeyCode.None;

                if (int.TryParse(confStr, out int result))
                    m_key = (KeyCode)result;
            }

            public bool IsPressing()
            {
                if (Input.GetKey(m_key)) return true;

                return false;
            }

            public override string ToString()
            {
                return ((int)(m_key)).ToString();
            }
        }
    }


}
