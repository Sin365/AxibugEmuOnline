using System;
using System.Collections.Generic;
using UnityEngine;

namespace MAME.Core
{
    public static class MotionKey
    {

        public const ulong None = (0L);
        public const ulong P1_INSERT_COIN = (1L);
        public const ulong P1_GAMESTART = (1L << 1);
        public const ulong P1_UP = (1L << 2);
        public const ulong P1_DOWN = (1L << 3);
        public const ulong P1_LEFT = (1L << 4);
        public const ulong P1_RIGHT = (1L << 5);
        public const ulong P1_BTN_1 = (1L << 6);
        public const ulong P1_BTN_2 = (1L << 7);
        public const ulong P1_BTN_3 = (1L << 8);
        public const ulong P1_BTN_4 = (1L << 9);
        public const ulong P1_BTN_5 = (1L << 10);
        public const ulong P1_BTN_6 = (1L << 11);
        public const ulong P1_BTN_E = (1L << 12);
        public const ulong P1_BTN_F = (1L << 13);


        public const ulong P2_INSERT_COIN = (1L << (16));
        public const ulong P2_GAMESTART = (1L << (16 + 1));
        public const ulong P2_UP = (1L << (16 + 2));
        public const ulong P2_DOWN = (1L << (16 + 3));
        public const ulong P2_LEFT = (1L << (16 + 4));
        public const ulong P2_RIGHT = (1L << (16 + 5));
        public const ulong P2_BTN_1 = (1L << (16 + 6));
        public const ulong P2_BTN_2 = (1L << (16 + 7));
        public const ulong P2_BTN_3 = (1L << (16 + 8));
        public const ulong P2_BTN_4 = (1L << (16 + 9));
        public const ulong P2_BTN_5 = (1L << (16 + 10));
        public const ulong P2_BTN_6 = (1L << (16 + 11));
        public const ulong P2_BTN_E = (1L << (16 + 12));
        public const ulong P2_BTN_F = (1L << (16 + 13));


        public const ulong P3_INSERT_COIN = (1L << (32));
        public const ulong P3_GAMESTART = (1L << (32 + 1));
        public const ulong P3_UP = (1L << (32 + 2));
        public const ulong P3_DOWN = (1L << (32 + 3));
        public const ulong P3_LEFT = (1L << (32 + 4));
        public const ulong P3_RIGHT = (1L << (32 + 5));
        public const ulong P3_BTN_1 = (1L << (32 + 6));
        public const ulong P3_BTN_2 = (1L << (32 + 7));
        public const ulong P3_BTN_3 = (1L << (32 + 8));
        public const ulong P3_BTN_4 = (1L << (32 + 9));
        public const ulong P3_BTN_5 = (1L << (32 + 10));
        public const ulong P3_BTN_6 = (1L << (32 + 11));
        public const ulong P3_BTN_E = (1L << (32 + 12));
        public const ulong P3_BTN_F = (1L << (32 + 13));


        public const ulong P4_INSERT_COIN = (1L << (48));
        public const ulong P4_GAMESTART = (1L << (48 + 1));
        public const ulong P4_UP = (1L << (48 + 2));
        public const ulong P4_DOWN = (1L << (48 + 3));
        public const ulong P4_LEFT = (1L << (48 + 4));
        public const ulong P4_RIGHT = (1L << (48 + 5));
        public const ulong P4_BTN_1 = (1L << (48 + 6));
        public const ulong P4_BTN_2 = (1L << (48 + 7));
        public const ulong P4_BTN_3 = (1L << (48 + 8));
        public const ulong P4_BTN_4 = (1L << (48 + 9));
        public const ulong P4_BTN_5 = (1L << (48 + 10));
        public const ulong P4_BTN_6 = (1L << (48 + 11));
        public const ulong P4_BTN_E = (1L << (48 + 12));
        public const ulong P4_BTN_F = (1L << (48 + 13));

        //预留按键
        //(1L << 14)
        //(1L << 15)
        //(1L << (16 + 14))
        //(1L << (16 + 15))
        //(1L << (32 + 14))
        //(1L << (32 + 15))
        //(1L << (48 + 14))
        //这个应该是不能用了-->(1L << (48 + 15))

        public const ulong EMU_PAUSED = (1L << 14);
        public const ulong Escape = (1L << 15);
        public const ulong LeftShift = (1L << (16 + 14));
        public const ulong RightShift = (1L << (16 + 15));
        public const ulong FinalKey = (1L << (32 + 14));
        public const ulong F10 = (1L << (32 + 14));
        public const ulong F9 = (1L << (32 + 14));
        public const ulong F8 = (1L << (32 + 14));
        public const ulong F7 = (1L << (32 + 14));
        public const ulong F6 = (1L << (32 + 14));
        public const ulong F5 = (1L << (32 + 14));
        public const ulong F4 = (1L << (32 + 14));
        public const ulong F3 = (1L << (32 + 14));
        public const ulong F2 = (1L << (32 + 14));
        public const ulong F1 = (1L << (32 + 14));
        public const ulong UNKNOW_Q = (1L << (32 + 14));
        public const ulong UNKNOW_N = (1L << (32 + 14));
        public const ulong UNKNOW_R = (1L << (32 + 14));
        public const ulong UNKNOW_T = (1L << (32 + 14));
        public const ulong UNKNOW_M = (1L << (32 + 14));
        public const ulong UNKNOW_V = (1L << (32 + 14));
        public const ulong UNKNOW_B = (1L << (32 + 14));

        public readonly static ulong[] AllNeedCheckList = new ulong[]
            {
            None,
            P1_INSERT_COIN,
            P1_GAMESTART,
            P1_UP,
            P1_DOWN,
            P1_LEFT,
            P1_RIGHT,
            P1_BTN_1,
            P1_BTN_2,
            P1_BTN_3,
            P1_BTN_4,
            P1_BTN_5,
            P1_BTN_6,
            P1_BTN_E,
            P1_BTN_F,
            P2_INSERT_COIN,
            P2_GAMESTART,
            P2_UP,
            P2_DOWN,
            P2_LEFT,
            P2_RIGHT,
            P2_BTN_1,
            P2_BTN_2,
            P2_BTN_3,
            P2_BTN_4,
            P2_BTN_5,
            P2_BTN_6,
            P2_BTN_E,
            P2_BTN_F,
            P3_INSERT_COIN,
            P3_GAMESTART,
            P3_UP,
            P3_DOWN,
            P3_LEFT,
            P3_RIGHT,
            P3_BTN_1,
            P3_BTN_2,
            P3_BTN_3,
            P3_BTN_4,
            P3_BTN_5,
            P3_BTN_6,
            P3_BTN_E,
            P3_BTN_F,
            P4_INSERT_COIN,
            P4_GAMESTART,
            P4_UP,
            P4_DOWN,
            P4_LEFT,
            P4_RIGHT,
            P4_BTN_1,
            P4_BTN_2,
            P4_BTN_3,
            P4_BTN_4,
            P4_BTN_5,
            P4_BTN_6,
            P4_BTN_E,
            P4_BTN_F,
            EMU_PAUSED,
            Escape,
            LeftShift,
            RightShift,
            FinalKey,
            F10,
            F9,
            F8,
            F7,
            F6,
            F5,
            F4,
            F3,
            F2,
            F1,
            UNKNOW_Q,
            UNKNOW_N,
            UNKNOW_R,
            UNKNOW_T,
            UNKNOW_M,
            UNKNOW_V,
            UNKNOW_B,
            };

        public static string GetKeyName(ulong key)
        {
            switch (key)
            {
                case MotionKey.None: return "None";
                case MotionKey.P1_INSERT_COIN: return "P1_INSERT_COIN";
                case MotionKey.P1_GAMESTART: return "P1_GAMESTART";
                case MotionKey.P1_UP: return "P1_UP";
                case MotionKey.P1_DOWN: return "P1_DOWN";
                case MotionKey.P1_LEFT: return "P1_LEFT";
                case MotionKey.P1_RIGHT: return "P1_RIGHT";
                case MotionKey.P1_BTN_1: return "P1_BTN_1";
                case MotionKey.P1_BTN_2: return "P1_BTN_2";
                case MotionKey.P1_BTN_3: return "P1_BTN_3";
                case MotionKey.P1_BTN_4: return "P1_BTN_4";
                case MotionKey.P1_BTN_5: return "P1_BTN_5";
                case MotionKey.P1_BTN_6: return "P1_BTN_6";
                case MotionKey.P1_BTN_E: return "P1_BTN_E";
                case MotionKey.P1_BTN_F: return "P1_BTN_F";
                case MotionKey.P2_INSERT_COIN: return "P2_INSERT_COIN";
                case MotionKey.P2_GAMESTART: return "P2_GAMESTART";
                case MotionKey.P2_UP: return "P2_UP";
                case MotionKey.P2_DOWN: return "P2_DOWN";
                case MotionKey.P2_LEFT: return "P2_LEFT";
                case MotionKey.P2_RIGHT: return "P2_RIGHT";
                case MotionKey.P2_BTN_1: return "P2_BTN_1";
                case MotionKey.P2_BTN_2: return "P2_BTN_2";
                case MotionKey.P2_BTN_3: return "P2_BTN_3";
                case MotionKey.P2_BTN_4: return "P2_BTN_4";
                case MotionKey.P2_BTN_5: return "P2_BTN_5";
                case MotionKey.P2_BTN_6: return "P2_BTN_6";
                case MotionKey.P2_BTN_E: return "P2_BTN_E";
                case MotionKey.P2_BTN_F: return "P2_BTN_F";
                case MotionKey.P3_INSERT_COIN: return "P3_INSERT_COIN";
                case MotionKey.P3_GAMESTART: return "P3_GAMESTART";
                case MotionKey.P3_UP: return "P3_UP";
                case MotionKey.P3_DOWN: return "P3_DOWN";
                case MotionKey.P3_LEFT: return "P3_LEFT";
                case MotionKey.P3_RIGHT: return "P3_RIGHT";
                case MotionKey.P3_BTN_1: return "P3_BTN_1";
                case MotionKey.P3_BTN_2: return "P3_BTN_2";
                case MotionKey.P3_BTN_3: return "P3_BTN_3";
                case MotionKey.P3_BTN_4: return "P3_BTN_4";
                case MotionKey.P3_BTN_5: return "P3_BTN_5";
                case MotionKey.P3_BTN_6: return "P3_BTN_6";
                case MotionKey.P3_BTN_E: return "P3_BTN_E";
                case MotionKey.P3_BTN_F: return "P3_BTN_F";
                case MotionKey.P4_INSERT_COIN: return "P4_INSERT_COIN";
                case MotionKey.P4_GAMESTART: return "P4_GAMESTART";
                case MotionKey.P4_UP: return "P4_UP";
                case MotionKey.P4_DOWN: return "P4_DOWN";
                case MotionKey.P4_LEFT: return "P4_LEFT";
                case MotionKey.P4_RIGHT: return "P4_RIGHT";
                case MotionKey.P4_BTN_1: return "P4_BTN_1";
                case MotionKey.P4_BTN_2: return "P4_BTN_2";
                case MotionKey.P4_BTN_3: return "P4_BTN_3";
                case MotionKey.P4_BTN_4: return "P4_BTN_4";
                case MotionKey.P4_BTN_5: return "P4_BTN_5";
                case MotionKey.P4_BTN_6: return "P4_BTN_6";
                case MotionKey.P4_BTN_E: return "P4_BTN_E";
                case MotionKey.P4_BTN_F: return "P4_BTN_F";
                case MotionKey.EMU_PAUSED: return "EMU_PAUSED";
                case MotionKey.Escape: return "Escape";
                case MotionKey.LeftShift: return "LeftShift";
                case MotionKey.RightShift: return "RightShift";
                case MotionKey.FinalKey: return "FinalKey";
                default: return "None";
                    //case MotionKey.F10:return "F10";
                    //case MotionKey.F9:return "F9";
                    //case MotionKey.F8:return "F8";
                    //case MotionKey.F7:return "F7";
                    //case MotionKey.F6:return "F6";
                    //case MotionKey.F5:return "F5";
                    //case MotionKey.F4:return "F4";
                    //case MotionKey.F3:return "F3";
                    //case MotionKey.F2:return "F2";
                    //case MotionKey.F1:return "F1";
                    //case MotionKey.UNKNOW_Q:return "UNKNOW_Q";
                    //case MotionKey.UNKNOW_N:return "UNKNOW_N";
                    //case MotionKey.UNKNOW_R:return "UNKNOW_R";
                    //case MotionKey.UNKNOW_T:return "UNKNOW_T";
                    //case MotionKey.UNKNOW_M:return "UNKNOW_M";
                    //case MotionKey.UNKNOW_V:return "UNKNOW_V";
                    //case MotionKey.UNKNOW_B: return "UNKNOW_B";
            }
        }

        //EMU_PAUSED = (1 << 36),
        //F10 = (1 << 37),
        //F9 = (1 << 38),
        //F8 = (1 << 39),
        //F7 = (1 << 40),
        //F6 = (1 << 41),
        //F5 = (1 << 42),
        //F4 = (1 << 43),
        //F3 = (1 << 44),
        //F2 = (1 << 45),
        //F1 = (1 << 46),
        //UNKNOW_Q = (1 << 47),
        //UNKNOW_N = (1 << 48),
        //UNKNOW_R = (1 << 49),
        //UNKNOW_T = (1 << 50),
        //UNKNOW_M = (1 << 51),
        //UNKNOW_V = (1 << 52),
        //UNKNOW_B = (1 << 53),
        //None = 0,
        //P1_INSERT_COIN = 1,
        //P1_GAMESTART = 2 << 1,
        //P1_UP = 4,
        //P1_DOWN = 8,
        //P1_LEFT = 16,
        //P1_RIGHT = 32,
        //P1_BTN_1 = 64,
        //P1_BTN_2 = 128,
        //P1_BTN_3 = 256,
        //P1_BTN_4 = 512,
        //P1_BTN_5 = 1024,
        //P1_BTN_6 = 4096,
        //P1_UNKNOW_E = 8192,
        //P1_UNKNOW_F,
        //P2_INSERT_COIN,
        //P2_GAMESTART,
        //P2_UP,
        //P2_DOWN,
        //P2_LEFT,
        //P2_RIGHT,
        //P2_BTN_1,
        //P2_BTN_2,
        //P2_BTN_3,
        //P2_BTN_4,
        //P2_BTN_5,
        //P2_BTN_6,
        //P2_UNKNOW_E,
        //P2_UNKNOW_F,
        //UNKNOW_Q,
        //UNKNOW_N,
        //UNKNOW_R,
        //UNKNOW_T,
        //UNKNOW_M,
        //UNKNOW_V,
        //UNKNOW_B,
        //F10,
        //F9,
        //F8,
        //F7,
        //F6,
        //F5,
        //F4,
        //F3,
        //F2,
        //F1,
        //Escape,
        //LeftShift,
        //RightShift,
        ///// <summary>
        ///// 用于标记最后一个
        ///// </summary>
        //FinalKey,



        //EMU_PAUSED,

    }

    //[Flags]
    //public enum MotionKey : long
    //{
    //    None = (0),
    //    P1_INSERT_COIN = (1),
    //    P1_GAMESTART = (1 << 1),
    //    P1_UP = (1 << 2),
    //    P1_DOWN = (1 << 3),
    //    P1_LEFT = (1 << 4),
    //    P1_RIGHT = (1 << 5),
    //    P1_BTN_1 = (1 << 6),
    //    P1_BTN_2 = (1 << 7),
    //    P1_BTN_3 = (1 << 8),
    //    P1_BTN_4 = (1 << 9),
    //    P1_BTN_5 = (1 << 10),
    //    P1_BTN_6 = (1 << 11),
    //    P1_UNKNOW_E = (1 << 12),
    //    P1_UNKNOW_F = (1 << 13),
    //    P2_INSERT_COIN = (1 << 14),
    //    P2_GAMESTART = (1 << 15),
    //    P2_UP = (1 << 16),
    //    P2_DOWN = (1 << 17),
    //    P2_LEFT = (1 << 18),
    //    P2_RIGHT = (1 << 19),
    //    P2_BTN_1 = (1 << 20),
    //    P2_BTN_2 = (1 << 21),
    //    P2_BTN_3 = (1 << 22),
    //    P2_BTN_4 = (1 << 23),
    //    P2_BTN_5 = (1 << 24),
    //    P2_BTN_6 = (1 << 25),
    //    P2_UNKNOW_E = (1 << 26),
    //    P2_UNKNOW_F = (1 << 27),
    //    Escape = (1 << 28),
    //    LeftShift = (1 << 29),
    //    RightShift = (1 << 30),
    //    FinalKey = (1 << 31),

    //    EMU_PAUSED = 1<< 31,
    //    F10 = 1<< 31,
    //    F9 = 1<< 31,
    //    F8 = 1<< 31,
    //    F7 = 1<< 31,
    //    F6 = 1<< 31,
    //    F5 = 1<< 31,
    //    F4 = 1<< 31,
    //    F3 = 1<< 31,
    //    F2 = 1<< 31,
    //    F1 = 1<< 31,
    //    UNKNOW_Q = 1<< 31,
    //    UNKNOW_N = 1<< 31,
    //    UNKNOW_R = 1<< 31,
    //    UNKNOW_T = 1<< 31,
    //    UNKNOW_M = 1<< 31,
    //    UNKNOW_V = 1<< 31,
    //    UNKNOW_B = 1<< 31

    //    //EMU_PAUSED = (1 << 36),
    //    //F10 = (1 << 37),
    //    //F9 = (1 << 38),
    //    //F8 = (1 << 39),
    //    //F7 = (1 << 40),
    //    //F6 = (1 << 41),
    //    //F5 = (1 << 42),
    //    //F4 = (1 << 43),
    //    //F3 = (1 << 44),
    //    //F2 = (1 << 45),
    //    //F1 = (1 << 46),
    //    //UNKNOW_Q = (1 << 47),
    //    //UNKNOW_N = (1 << 48),
    //    //UNKNOW_R = (1 << 49),
    //    //UNKNOW_T = (1 << 50),
    //    //UNKNOW_M = (1 << 51),
    //    //UNKNOW_V = (1 << 52),
    //    //UNKNOW_B = (1 << 53),
    //    //None = 0,
    //    //P1_INSERT_COIN = 1,
    //    //P1_GAMESTART = 2 << 1,
    //    //P1_UP = 4,
    //    //P1_DOWN = 8,
    //    //P1_LEFT = 16,
    //    //P1_RIGHT = 32,
    //    //P1_BTN_1 = 64,
    //    //P1_BTN_2 = 128,
    //    //P1_BTN_3 = 256,
    //    //P1_BTN_4 = 512,
    //    //P1_BTN_5 = 1024,
    //    //P1_BTN_6 = 4096,
    //    //P1_UNKNOW_E = 8192,
    //    //P1_UNKNOW_F,
    //    //P2_INSERT_COIN,
    //    //P2_GAMESTART,
    //    //P2_UP,
    //    //P2_DOWN,
    //    //P2_LEFT,
    //    //P2_RIGHT,
    //    //P2_BTN_1,
    //    //P2_BTN_2,
    //    //P2_BTN_3,
    //    //P2_BTN_4,
    //    //P2_BTN_5,
    //    //P2_BTN_6,
    //    //P2_UNKNOW_E,
    //    //P2_UNKNOW_F,
    //    //UNKNOW_Q,
    //    //UNKNOW_N,
    //    //UNKNOW_R,
    //    //UNKNOW_T,
    //    //UNKNOW_M,
    //    //UNKNOW_V,
    //    //UNKNOW_B,
    //    //F10,
    //    //F9,
    //    //F8,
    //    //F7,
    //    //F6,
    //    //F5,
    //    //F4,
    //    //F3,
    //    //F2,
    //    //F1,
    //    //Escape,
    //    //LeftShift,
    //    //RightShift,
    //    ///// <summary>
    //    ///// 用于标记最后一个
    //    ///// </summary>
    //    //FinalKey,



    //    //EMU_PAUSED,
    //}
}
