using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyCodeCore
{
    //public Dictionary<KeyCode, EssgeeMotionKey> dictKeyCfgs = new Dictionary<KeyCode, EssgeeMotionKey>();
    public Dictionary<ulong, EssgeeMotionKey> dictKey2Motion = new Dictionary<ulong, EssgeeMotionKey>();
    public Dictionary<ulong, KeyCode> dictMotion2RealKey = new Dictionary<ulong, KeyCode>()
    {
{ EssgeeUnityKey.P1_UP,KeyCode.W},
{ EssgeeUnityKey.P1_DOWN,KeyCode.S},
{ EssgeeUnityKey.P1_LEFT,KeyCode.A},
{ EssgeeUnityKey.P1_RIGHT,KeyCode.D},
{ EssgeeUnityKey.P1_BTN_1,KeyCode.J},
{ EssgeeUnityKey.P1_BTN_2,KeyCode.K},
{ EssgeeUnityKey.P1_BTN_3,KeyCode.U},
{ EssgeeUnityKey.P1_BTN_4,KeyCode.I},
{ EssgeeUnityKey.P1_POTION_1,KeyCode.Return},
{ EssgeeUnityKey.P1_POTION_2,KeyCode.RightShift},
{ EssgeeUnityKey.P2_UP,KeyCode.UpArrow},
{ EssgeeUnityKey.P2_DOWN,KeyCode.DownArrow},
{ EssgeeUnityKey.P2_LEFT,KeyCode.LeftArrow},
{ EssgeeUnityKey.P2_RIGHT,KeyCode.RightArrow},
{ EssgeeUnityKey.P2_BTN_1,KeyCode.Keypad1},
{ EssgeeUnityKey.P2_BTN_2,KeyCode.Keypad2},
{ EssgeeUnityKey.P2_BTN_3,KeyCode.Keypad4},
{ EssgeeUnityKey.P2_BTN_4,KeyCode.Keypad5},
{ EssgeeUnityKey.P2_POTION_1,KeyCode.Keypad0},
{ EssgeeUnityKey.P2_POTION_2,KeyCode.KeypadPeriod},
{ EssgeeUnityKey.P3_UP,KeyCode.F12},
{ EssgeeUnityKey.P3_DOWN,KeyCode.F12},
{ EssgeeUnityKey.P3_LEFT,KeyCode.F12},
{ EssgeeUnityKey.P3_RIGHT,KeyCode.F12},
{ EssgeeUnityKey.P3_BTN_1,KeyCode.F12},
{ EssgeeUnityKey.P3_BTN_2,KeyCode.F12},
{ EssgeeUnityKey.P3_BTN_3,KeyCode.F12},
{ EssgeeUnityKey.P3_BTN_4,KeyCode.F12},
{ EssgeeUnityKey.P3_POTION_1,KeyCode.F12},
{ EssgeeUnityKey.P3_POTION_2,KeyCode.F12},
{ EssgeeUnityKey.P4_UP,KeyCode.F12},
{ EssgeeUnityKey.P4_DOWN,KeyCode.F12},
{ EssgeeUnityKey.P4_LEFT,KeyCode.F12},
{ EssgeeUnityKey.P4_RIGHT,KeyCode.F12},
{ EssgeeUnityKey.P4_BTN_1,KeyCode.F12},
{ EssgeeUnityKey.P4_BTN_2,KeyCode.F12},
{ EssgeeUnityKey.P4_BTN_3,KeyCode.F12},
{ EssgeeUnityKey.P4_BTN_4,KeyCode.F12},
{ EssgeeUnityKey.P4_POTION_1,KeyCode.F12},
{ EssgeeUnityKey.P4_POTION_2,KeyCode.F12},
    };
    public ulong[] CheckList;
    public EssgeeMotionKey[] mCurrKey = new EssgeeMotionKey[0];
    List<EssgeeMotionKey> temp = new List<EssgeeMotionKey>();
    ulong tempInputAllData = 0;
    UEGKeyboard mUniKeyboard;
    ulong last_CurryInpuAllData_test = 0;
    public static class EssgeeUnityKey
    {
        public const ulong NONE = 0;
        public const ulong P1_UP = 1;
        public const ulong P1_DOWN = 1 << 1;
        public const ulong P1_LEFT = 1 << 2;
        public const ulong P1_RIGHT = 1 << 3;
        public const ulong P1_BTN_1 = 1 << 4;
        public const ulong P1_BTN_2 = 1 << 5;
        public const ulong P1_BTN_3 = 1 << 6;
        public const ulong P1_BTN_4 = 1 << 7;
        public const ulong P1_POTION_1 = 1 << 8;
        public const ulong P1_POTION_2 = 1 << 9;
        public const ulong P2_UP = 65536;
        public const ulong P2_DOWN = 65536 << 1;
        public const ulong P2_LEFT = 65536 << 2;
        public const ulong P2_RIGHT = 65536 << 3;
        public const ulong P2_BTN_1 = 65536 << 4;
        public const ulong P2_BTN_2 = 65536 << 5;
        public const ulong P2_BTN_3 = 65536 << 6;
        public const ulong P2_BTN_4 = 65536 << 7;
        public const ulong P2_POTION_1 = 65536 << 8;
        public const ulong P2_POTION_2 = 65536 << 9;
        public const ulong P3_UP = 4294967296;
        public const ulong P3_DOWN = 4294967296 << 1;
        public const ulong P3_LEFT = 4294967296 << 2;
        public const ulong P3_RIGHT = 4294967296 << 3;
        public const ulong P3_BTN_1 = 4294967296 << 4;
        public const ulong P3_BTN_2 = 4294967296 << 5;
        public const ulong P3_BTN_3 = 4294967296 << 6;
        public const ulong P3_BTN_4 = 654294967296536 << 7;
        public const ulong P3_POTION_1 = 4294967296 << 8;
        public const ulong P3_POTION_2 = 4294967296 << 9;
        public const ulong P4_UP = 281474976710656;
        public const ulong P4_DOWN = 281474976710656 << 1;
        public const ulong P4_LEFT = 281474976710656 << 2;
        public const ulong P4_RIGHT = 281474976710656 << 3;
        public const ulong P4_BTN_1 = 281474976710656 << 4;
        public const ulong P4_BTN_2 = 281474976710656 << 5;
        public const ulong P4_BTN_3 = 281474976710656 << 6;
        public const ulong P4_BTN_4 = 281474976710656 << 7;
        public const ulong P4_POTION_1 = 281474976710656 << 8;
        public const ulong P4_POTION_2 = 281474976710656 << 9;
    }

    public EssgeeMotionKey[] GetPressedKeys()
    {
        return mCurrKey;
    }

    public void SetRePlay(bool IsReplay)
    {
        //bReplayMode = IsReplay;
    }
    public void Init(Essgee.Emulation.Machines.IMachine Machine, UEGKeyboard uniKeyboard, bool IsReplay)
    {
        mUniKeyboard = uniKeyboard;
        //dictKeyCfgs.Clear();
        dictKey2Motion.Clear();
        if (Machine is Essgee.Emulation.Machines.MasterSystem)
        {
            var machine = (Essgee.Emulation.Machines.MasterSystem)Machine;
            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.Joypad1Up);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.Joypad1Down);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.Joypad1Left);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.Joypad1Right);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.Joypad1Button1);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.Joypad1Button2);

            //dictKeyCfgs.Add(KeyCode.UpArrow, machine.configuration.Joypad2Up);
            //dictKeyCfgs.Add(KeyCode.DownArrow, machine.configuration.Joypad2Down);
            //dictKeyCfgs.Add(KeyCode.LeftArrow, machine.configuration.Joypad2Left);
            //dictKeyCfgs.Add(KeyCode.RightAlt, machine.configuration.Joypad2Right);
            //dictKeyCfgs.Add(KeyCode.Alpha1, machine.configuration.Joypad2Button1);
            //dictKeyCfgs.Add(KeyCode.Alpha2, machine.configuration.Joypad2Button2);


            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.Joypad1Up);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.Joypad1Down);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.Joypad1Left);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.Joypad1Right);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.Joypad1Button1);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.Joypad1Button2);

            dictKey2Motion.Add(EssgeeUnityKey.P2_UP, machine.configuration.Joypad2Up);
            dictKey2Motion.Add(EssgeeUnityKey.P2_DOWN, machine.configuration.Joypad2Down);
            dictKey2Motion.Add(EssgeeUnityKey.P2_LEFT, machine.configuration.Joypad2Left);
            dictKey2Motion.Add(EssgeeUnityKey.P2_RIGHT, machine.configuration.Joypad2Right);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_1, machine.configuration.Joypad2Button1);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_2, machine.configuration.Joypad2Button2);
        }
        else if (Machine is Essgee.Emulation.Machines.GameBoy)
        {
            var machine = (Essgee.Emulation.Machines.GameBoy)Machine;

            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.ControlsUp);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.ControlsDown);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.ControlsLeft);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.ControlsRight);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.ControlsB);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.ControlsA);
            //dictKeyCfgs.Add(KeyCode.Return, machine.configuration.ControlsStart);
            //dictKeyCfgs.Add(KeyCode.RightShift, machine.configuration.ControlsSelect);

            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.ControlsUp);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.ControlsDown);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.ControlsLeft);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.ControlsRight);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.ControlsB);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.ControlsA);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_1, machine.configuration.ControlsStart);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_2, machine.configuration.ControlsSelect);
        }
        else if (Machine is Essgee.Emulation.Machines.GameBoyColor)
        {
            var machine = (Essgee.Emulation.Machines.GameBoyColor)Machine;

            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.ControlsUp);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.ControlsDown);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.ControlsLeft);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.ControlsRight);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.ControlsB);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.ControlsA);

            //dictKeyCfgs.Add(KeyCode.Return, machine.configuration.ControlsStart);
            //dictKeyCfgs.Add(KeyCode.RightShift, machine.configuration.ControlsSelect);
            //dictKeyCfgs.Add(KeyCode.Space, machine.configuration.ControlsSendIR);

            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.ControlsUp);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.ControlsDown);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.ControlsLeft);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.ControlsRight);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.ControlsA);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.ControlsB);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_3, machine.configuration.ControlsSendIR);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_1, machine.configuration.ControlsStart);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_2, machine.configuration.ControlsSelect);

        }
        else if (Machine is Essgee.Emulation.Machines.GameGear)
        {
            var machine = (Essgee.Emulation.Machines.GameGear)Machine;
            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.ControlsUp);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.ControlsDown);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.ControlsLeft);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.ControlsRight);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.ControlsButton2);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.ControlsButton1);
            //dictKeyCfgs.Add(KeyCode.Return, machine.configuration.ControlsStart);


            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.ControlsUp);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.ControlsDown);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.ControlsLeft);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.ControlsRight);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.ControlsButton2);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.ControlsButton1);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_1, machine.configuration.ControlsStart);
        }
        else if (Machine is Essgee.Emulation.Machines.SC3000)
        {
            var machine = (Essgee.Emulation.Machines.SC3000)Machine;

            /*
             * InputReset = MotionKey.F12;
			InputChangeMode = MotionKey.F1;
			InputPlayTape = MotionKey.F2;

			Joypad1Up = MotionKey.Up;
			Joypad1Down = MotionKey.Down;
			Joypad1Left = MotionKey.Left;
			Joypad1Right = MotionKey.Right;
			Joypad1Button1 = MotionKey.A;
			Joypad1Button2 = MotionKey.S;

			Joypad2Up = MotionKey.NumPad8;
			Joypad2Down = MotionKey.NumPad2;
			Joypad2Left = MotionKey.NumPad4;
			Joypad2Right = MotionKey.NumPad6;
			Joypad2Button1 = MotionKey.NumPad1;
			Joypad2Button2 = MotionKey.NumPad3;
             */

            //dictKeyCfgs.Add(KeyCode.F12, machine.configuration.InputReset);

            //dictKeyCfgs.Add(KeyCode.F1, machine.configuration.InputChangeMode);
            //dictKeyCfgs.Add(KeyCode.F2, machine.configuration.InputPlayTape);

            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.Joypad1Up);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.Joypad1Down);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.Joypad1Left);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.Joypad1Right);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.Joypad1Button2);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.Joypad1Button1);

            //dictKeyCfgs.Add(KeyCode.UpArrow, machine.configuration.Joypad2Up);
            //dictKeyCfgs.Add(KeyCode.DownArrow, machine.configuration.Joypad2Down);
            //dictKeyCfgs.Add(KeyCode.LeftArrow, machine.configuration.Joypad2Left);
            //dictKeyCfgs.Add(KeyCode.RightAlt, machine.configuration.Joypad2Right);
            //dictKeyCfgs.Add(KeyCode.Alpha1, machine.configuration.Joypad2Button1);
            //dictKeyCfgs.Add(KeyCode.Alpha2, machine.configuration.Joypad2Button2);


            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_1, machine.configuration.InputChangeMode);
            dictKey2Motion.Add(EssgeeUnityKey.P1_POTION_2, machine.configuration.InputPlayTape);

            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.Joypad1Up);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.Joypad1Down);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.Joypad1Left);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.Joypad1Right);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.Joypad1Button2);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.Joypad1Button1);

            dictKey2Motion.Add(EssgeeUnityKey.P2_UP, machine.configuration.Joypad1Up);
            dictKey2Motion.Add(EssgeeUnityKey.P2_DOWN, machine.configuration.Joypad1Down);
            dictKey2Motion.Add(EssgeeUnityKey.P2_LEFT, machine.configuration.Joypad1Left);
            dictKey2Motion.Add(EssgeeUnityKey.P2_RIGHT, machine.configuration.Joypad1Right);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_1, machine.configuration.Joypad1Button2);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_2, machine.configuration.Joypad1Button1);

        }
        else if (Machine is Essgee.Emulation.Machines.SG1000)
        {
            var machine = (Essgee.Emulation.Machines.SG1000)Machine;

            /*
             TVStandard = TVStandard.NTSC;

			InputPause = MotionKey.Space;

			Joypad1Up = MotionKey.Up;
			Joypad1Down = MotionKey.Down;
			Joypad1Left = MotionKey.Left;
			Joypad1Right = MotionKey.Right;
			Joypad1Button1 = MotionKey.A;
			Joypad1Button2 = MotionKey.S;

			Joypad2Up = MotionKey.NumPad8;
			Joypad2Down = MotionKey.NumPad2;
			Joypad2Left = MotionKey.NumPad4;
			Joypad2Right = MotionKey.NumPad6;
			Joypad2Button1 = MotionKey.NumPad1;
			Joypad2Button2 = MotionKey.NumPad3;
             */

            //dictKeyCfgs.Add(KeyCode.W, machine.configuration.Joypad1Up);
            //dictKeyCfgs.Add(KeyCode.S, machine.configuration.Joypad1Down);
            //dictKeyCfgs.Add(KeyCode.A, machine.configuration.Joypad1Left);
            //dictKeyCfgs.Add(KeyCode.D, machine.configuration.Joypad1Right);
            //dictKeyCfgs.Add(KeyCode.J, machine.configuration.Joypad1Button2);
            //dictKeyCfgs.Add(KeyCode.K, machine.configuration.Joypad1Button1);

            //dictKeyCfgs.Add(KeyCode.UpArrow, machine.configuration.Joypad2Up);
            //dictKeyCfgs.Add(KeyCode.DownArrow, machine.configuration.Joypad2Down);
            //dictKeyCfgs.Add(KeyCode.LeftArrow, machine.configuration.Joypad2Left);
            //dictKeyCfgs.Add(KeyCode.RightAlt, machine.configuration.Joypad2Right);
            //dictKeyCfgs.Add(KeyCode.Alpha1, machine.configuration.Joypad2Button2);
            //dictKeyCfgs.Add(KeyCode.Alpha2, machine.configuration.Joypad2Button1);



            dictKey2Motion.Add(EssgeeUnityKey.P1_UP, machine.configuration.Joypad1Up);
            dictKey2Motion.Add(EssgeeUnityKey.P1_DOWN, machine.configuration.Joypad1Down);
            dictKey2Motion.Add(EssgeeUnityKey.P1_LEFT, machine.configuration.Joypad1Left);
            dictKey2Motion.Add(EssgeeUnityKey.P1_RIGHT, machine.configuration.Joypad1Right);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_1, machine.configuration.Joypad1Button2);
            dictKey2Motion.Add(EssgeeUnityKey.P1_BTN_2, machine.configuration.Joypad1Button1);

            dictKey2Motion.Add(EssgeeUnityKey.P2_UP, machine.configuration.Joypad1Up);
            dictKey2Motion.Add(EssgeeUnityKey.P2_DOWN, machine.configuration.Joypad1Down);
            dictKey2Motion.Add(EssgeeUnityKey.P2_LEFT, machine.configuration.Joypad1Left);
            dictKey2Motion.Add(EssgeeUnityKey.P2_RIGHT, machine.configuration.Joypad1Right);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_1, machine.configuration.Joypad1Button2);
            dictKey2Motion.Add(EssgeeUnityKey.P2_BTN_2, machine.configuration.Joypad1Button1);
        }
        CheckList = dictKey2Motion.Keys.ToArray();

        //mUniKeyboard.btnP1.Key = new long[] { (long)MotionKey.P1_GAMESTART };
        //mUniKeyboard.btnCoin1.Key = new long[] { (long)MotionKey.P1_INSERT_COIN };
        //mUniKeyboard.btnA.Key = new long[] { (long)MotionKey.P1_BTN_1 };
        //mUniKeyboard.btnB.Key = new long[] { (long)MotionKey.P1_BTN_2 };
        //mUniKeyboard.btnC.Key = new long[] { (long)MotionKey.P1_BTN_3 };
        //mUniKeyboard.btnD.Key = new long[] { (long)MotionKey.P1_BTN_4 };
        ////mUniKeyboard.btnE.Key = new long[] { (long)MotionKey.P1_BTN_5 };
        ////mUniKeyboard.btnF.Key = new long[] { (long)MotionKey.P1_BTN_6 };
        //mUniKeyboard.btnAB.Key = new long[] { (long)MotionKey.P1_BTN_1, (long)MotionKey.P1_BTN_2 };
        //mUniKeyboard.btnCD.Key = new long[] { (long)MotionKey.P1_BTN_3, (long)MotionKey.P1_BTN_4 };
        //mUniKeyboard.btnABC.Key = new long[] { (long)MotionKey.P1_BTN_1, (long)MotionKey.P1_BTN_2, (long)MotionKey.P1_BTN_3 };
    }

    public void UpdateLogic()
    {
        tempInputAllData = 0;
        temp.Clear();
        for (int i = 0; i < CheckList.Length; i++)
        {
            ulong key = CheckList[i];
            if (Input.GetKey(dictMotion2RealKey[key]))
            {
                EssgeeMotionKey mk = dictKey2Motion[key];
                temp.Add(mk);
                tempInputAllData |= (ulong)mk;
            }
        }
        mCurrKey = temp.ToArray();


        //if (bReplayMode) return;
        //tempInputAllData = 0;
        //temp.Clear();
        //for (int i = 0; i < CheckList.Length; i++)
        //{
        //    if (Input.GetKey(CheckList[i]))
        //    {
        //        EssgeeMotionKey mk = dictKeyCfgs[CheckList[i]];
        //        temp.Add(mk);
        //        tempInputAllData |= (ulong)mk;
        //    }
        //}
        //mCurrKey = temp.ToArray();

        //for (int i = 0; i < mUniKeyboard.mUIBtns.Count; i++)
        //{
        //    if (mUniKeyboard.mUIBtns[i].bHotKey)
        //    {
        //        for (int j = 0; j < mUniKeyboard.mUIBtns[i].Key.Length; j++)
        //        {
        //            MotionKey mk = (MotionKey)mUniKeyboard.mUIBtns[i].Key[j];
        //            temp.Add(mk);
        //            tempInputAllData |= (ulong)mk;
        //        }
        //    }
        //}

        //Vector2Int inputV2 = mUniKeyboard.mJoystick.RawInputV2;
        ////Debug.Log($"{inputV2.x},{inputV2.y}");
        //if (inputV2.x > 0)
        //{
        //    temp.Add(MotionKey.P1_RIGHT);
        //    tempInputAllData |= (ulong)MotionKey.P1_RIGHT;
        //}
        //else if (inputV2.x < 0)
        //{
        //    temp.Add(MotionKey.P1_LEFT);
        //    tempInputAllData |= (ulong)MotionKey.P1_LEFT;
        //}
        //if (inputV2.y > 0)
        //{
        //    temp.Add(MotionKey.P1_UP);
        //    tempInputAllData |= (ulong)MotionKey.P1_UP;
        //}
        //else if (inputV2.y < 0)
        //{
        //    temp.Add(MotionKey.P1_DOWN);
        //    tempInputAllData |= (ulong)MotionKey.P1_DOWN;
        //}
        //CurryInpuAllData = tempInputAllData;
        //mCurrKey = temp.ToArray();

    }
}