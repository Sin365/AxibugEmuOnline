using MAME.Core;
using System;
using System.Collections.Generic;

namespace MAME.Core
{
    public class Keyboard
    {
        public static bool bF10;

        static IKeyboard mKeyboard;

        //const int CheckMaxEnumIdx = 33;

        class KeyState
        {
            public bool IsPressed;
            public bool IsTriggered;
            public bool WasPressed;
        };
        private static Dictionary<ulong, KeyState> m_KeyStates = new Dictionary<ulong, KeyState>();

        static ulong[] mKeysValue;
        public static void InitializeInput(IKeyboard ikb)
        {
            mKeyboard = ikb;
            List<ulong> temp = new List<ulong>();

            #region 初始化m_KeyStates
            m_KeyStates[MotionKey.None] = new KeyState();
            m_KeyStates[MotionKey.P1_INSERT_COIN] = new KeyState();
            m_KeyStates[MotionKey.P1_GAMESTART] = new KeyState();
            m_KeyStates[MotionKey.P1_UP] = new KeyState();
            m_KeyStates[MotionKey.P1_DOWN] = new KeyState();
            m_KeyStates[MotionKey.P1_LEFT] = new KeyState();
            m_KeyStates[MotionKey.P1_RIGHT] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_1] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_2] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_3] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_4] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_5] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_6] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_E] = new KeyState();
            m_KeyStates[MotionKey.P1_BTN_F] = new KeyState();
            m_KeyStates[MotionKey.P2_INSERT_COIN] = new KeyState();
            m_KeyStates[MotionKey.P2_GAMESTART] = new KeyState();
            m_KeyStates[MotionKey.P2_UP] = new KeyState();
            m_KeyStates[MotionKey.P2_DOWN] = new KeyState();
            m_KeyStates[MotionKey.P2_LEFT] = new KeyState();
            m_KeyStates[MotionKey.P2_RIGHT] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_1] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_2] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_3] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_4] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_5] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_6] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_E] = new KeyState();
            m_KeyStates[MotionKey.P2_BTN_F] = new KeyState();
            m_KeyStates[MotionKey.P3_INSERT_COIN] = new KeyState();
            m_KeyStates[MotionKey.P3_GAMESTART] = new KeyState();
            m_KeyStates[MotionKey.P3_UP] = new KeyState();
            m_KeyStates[MotionKey.P3_DOWN] = new KeyState();
            m_KeyStates[MotionKey.P3_LEFT] = new KeyState();
            m_KeyStates[MotionKey.P3_RIGHT] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_1] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_2] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_3] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_4] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_5] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_6] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_E] = new KeyState();
            m_KeyStates[MotionKey.P3_BTN_F] = new KeyState();
            m_KeyStates[MotionKey.P4_INSERT_COIN] = new KeyState();
            m_KeyStates[MotionKey.P4_GAMESTART] = new KeyState();
            m_KeyStates[MotionKey.P4_UP] = new KeyState();
            m_KeyStates[MotionKey.P4_DOWN] = new KeyState();
            m_KeyStates[MotionKey.P4_LEFT] = new KeyState();
            m_KeyStates[MotionKey.P4_RIGHT] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_1] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_2] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_3] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_4] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_5] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_6] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_E] = new KeyState();
            m_KeyStates[MotionKey.P4_BTN_F] = new KeyState();
            m_KeyStates[MotionKey.EMU_PAUSED] = new KeyState();
            m_KeyStates[MotionKey.Escape] = new KeyState();
            m_KeyStates[MotionKey.LeftShift] = new KeyState();
            m_KeyStates[MotionKey.RightShift] = new KeyState();
            m_KeyStates[MotionKey.FinalKey] = new KeyState();
            m_KeyStates[MotionKey.F10] = new KeyState();
            m_KeyStates[MotionKey.F9] = new KeyState();
            m_KeyStates[MotionKey.F8] = new KeyState();
            m_KeyStates[MotionKey.F7] = new KeyState();
            m_KeyStates[MotionKey.F6] = new KeyState();
            m_KeyStates[MotionKey.F5] = new KeyState();
            m_KeyStates[MotionKey.F4] = new KeyState();
            m_KeyStates[MotionKey.F3] = new KeyState();
            m_KeyStates[MotionKey.F2] = new KeyState();
            m_KeyStates[MotionKey.F1] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_Q] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_N] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_R] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_T] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_M] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_V] = new KeyState();
            m_KeyStates[MotionKey.UNKNOW_B] = new KeyState();



            temp.Add(MotionKey.None);
            temp.Add(MotionKey.P1_INSERT_COIN);
            temp.Add(MotionKey.P1_GAMESTART);
            temp.Add(MotionKey.P1_UP);
            temp.Add(MotionKey.P1_DOWN);
            temp.Add(MotionKey.P1_LEFT);
            temp.Add(MotionKey.P1_RIGHT);
            temp.Add(MotionKey.P1_BTN_1);
            temp.Add(MotionKey.P1_BTN_2);
            temp.Add(MotionKey.P1_BTN_3);
            temp.Add(MotionKey.P1_BTN_4);
            temp.Add(MotionKey.P1_BTN_5);
            temp.Add(MotionKey.P1_BTN_6);
            temp.Add(MotionKey.P1_BTN_E);
            temp.Add(MotionKey.P1_BTN_F);
            temp.Add(MotionKey.P2_INSERT_COIN);
            temp.Add(MotionKey.P2_GAMESTART);
            temp.Add(MotionKey.P2_UP);
            temp.Add(MotionKey.P2_DOWN);
            temp.Add(MotionKey.P2_LEFT);
            temp.Add(MotionKey.P2_RIGHT);
            temp.Add(MotionKey.P2_BTN_1);
            temp.Add(MotionKey.P2_BTN_2);
            temp.Add(MotionKey.P2_BTN_3);
            temp.Add(MotionKey.P2_BTN_4);
            temp.Add(MotionKey.P2_BTN_5);
            temp.Add(MotionKey.P2_BTN_6);
            temp.Add(MotionKey.P2_BTN_E);
            temp.Add(MotionKey.P2_BTN_F);
            temp.Add(MotionKey.P3_INSERT_COIN);
            temp.Add(MotionKey.P3_GAMESTART);
            temp.Add(MotionKey.P3_UP);
            temp.Add(MotionKey.P3_DOWN);
            temp.Add(MotionKey.P3_LEFT);
            temp.Add(MotionKey.P3_RIGHT);
            temp.Add(MotionKey.P3_BTN_1);
            temp.Add(MotionKey.P3_BTN_2);
            temp.Add(MotionKey.P3_BTN_3);
            temp.Add(MotionKey.P3_BTN_4);
            temp.Add(MotionKey.P3_BTN_5);
            temp.Add(MotionKey.P3_BTN_6);
            temp.Add(MotionKey.P3_BTN_E);
            temp.Add(MotionKey.P3_BTN_F);
            temp.Add(MotionKey.P4_INSERT_COIN);
            temp.Add(MotionKey.P4_GAMESTART);
            temp.Add(MotionKey.P4_UP);
            temp.Add(MotionKey.P4_DOWN);
            temp.Add(MotionKey.P4_LEFT);
            temp.Add(MotionKey.P4_RIGHT);
            temp.Add(MotionKey.P4_BTN_1);
            temp.Add(MotionKey.P4_BTN_2);
            temp.Add(MotionKey.P4_BTN_3);
            temp.Add(MotionKey.P4_BTN_4);
            temp.Add(MotionKey.P4_BTN_5);
            temp.Add(MotionKey.P4_BTN_6);
            temp.Add(MotionKey.P4_BTN_E);
            temp.Add(MotionKey.P4_BTN_F);
            temp.Add(MotionKey.EMU_PAUSED);
            temp.Add(MotionKey.Escape);
            temp.Add(MotionKey.LeftShift);
            temp.Add(MotionKey.RightShift);
            temp.Add(MotionKey.FinalKey);
            temp.Add(MotionKey.F10);
            temp.Add(MotionKey.F9);
            temp.Add(MotionKey.F8);
            temp.Add(MotionKey.F7);
            temp.Add(MotionKey.F6);
            temp.Add(MotionKey.F5);
            temp.Add(MotionKey.F4);
            temp.Add(MotionKey.F3);
            temp.Add(MotionKey.F2);
            temp.Add(MotionKey.F1);
            temp.Add(MotionKey.UNKNOW_Q);
            temp.Add(MotionKey.UNKNOW_N);
            temp.Add(MotionKey.UNKNOW_R);
            temp.Add(MotionKey.UNKNOW_T);
            temp.Add(MotionKey.UNKNOW_M);
            temp.Add(MotionKey.UNKNOW_V);
            temp.Add(MotionKey.UNKNOW_B);

            mKeysValue = temp.ToArray();
            #endregion
        }

        public static bool IsPressed(ulong key)
        {
            return m_KeyStates[key].IsPressed;
        }
        public static bool IsTriggered(ulong key)
        {
            return m_KeyStates[key].IsTriggered;
        }

        public static void Update()
        {
            ulong currKeys = mKeyboard.GetPressedKeys();
            for (byte i = 0; i < mKeysValue.Length; i++)
            {
                //m_KeyStates[mKeyName[i]].IsPressed = false;
                m_KeyStates[mKeysValue[i]].IsPressed = (currKeys & (ulong)mKeysValue[i]) > 0;
            }

            //等待放行帧
            Machine.mainMotion.WaitNextFrame();

            //foreach (MotionKey key in mKeyboard.GetPressedKeys())
            //{
            //    m_KeyStates[key].IsPressed = true;
            //}



            for (int i = 0; i < mKeysValue.Length; i++)
            {
                ulong key = mKeysValue[i];
                if (m_KeyStates[key].IsPressed)
                {
                    if (m_KeyStates[key].WasPressed)
                    {
                        m_KeyStates[key].IsTriggered = false;
                    }
                    else
                    {
                        m_KeyStates[key].WasPressed = true;
                        m_KeyStates[key].IsTriggered = true;
                    }
                }
                else
                {
                    m_KeyStates[key].WasPressed = false;
                    m_KeyStates[key].IsTriggered = false;
                }
            }
            //byte finalIndex = CheckMaxEnumIdx;
            //for (byte i = 0; i < finalIndex; i++)
            //{
            //    m_KeyStates[i].IsPressed = false;
            //}
            //foreach (MotionKey key in mKeyboard.GetPressedKeys())
            //{
            //    m_KeyStates[(int)key].IsPressed = true;
            //}
            //for (int i = 0; i < finalIndex; i++)
            //{
            //    if (m_KeyStates[i].IsPressed)
            //    {
            //        if (m_KeyStates[i].WasPressed)
            //        {
            //            m_KeyStates[i].IsTriggered = false;
            //        }
            //        else
            //        {
            //            m_KeyStates[i].WasPressed = true;
            //            m_KeyStates[i].IsTriggered = true;
            //        }
            //    }
            //    else
            //    {
            //        m_KeyStates[i].WasPressed = false;
            //        m_KeyStates[i].IsTriggered = false;
            //    }
            //}

        }
    }
}