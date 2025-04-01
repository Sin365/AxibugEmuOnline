using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Settings;
using AxiReplay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UEGKeyboard : MonoBehaviour
{
    public EssgeeControllerMapper ControllerMapper { get; private set; }
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
    public ulong CurrRemoteInpuAllData = 0;
    public ulong CurrLocalInpuAllData { get; private set; }
    void Awake()
    {
    }

    public void Init(Essgee.Emulation.Machines.IMachine Machine)
    {
        ControllerMapper = new EssgeeControllerMapper();
        Init(Machine, false);
    }


    public EssgeeMotionKey[] GetPressedKeys()
    {
        return mCurrKey;
    }

    public void SetRePlay(bool IsReplay)
    {
        //bReplayMode = IsReplay;
    }

    void Init(Essgee.Emulation.Machines.IMachine Machine, bool IsReplay)
    {
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

    public bool SampleInput()
    {
        //Net模式
        if (InGameUI.Instance.IsNetPlay)
        {
            bool bHadNetData = false;
            int targetFrame; ReplayStep replayData; int frameDiff; bool inputDiff;
            if (App.roomMgr.netReplay.TryGetNextFrame((int)UEssgee.instance.Frame, out replayData, out frameDiff, out inputDiff))
            {
                if (inputDiff)
                {
                    App.log.Debug($"{DateTime.Now.ToString("hh:mm:ss.fff")} TryGetNextFrame remoteFrame->{App.roomMgr.netReplay.mRemoteFrameIdx} diff->{frameDiff} " +
                        $"frame=>{replayData.FrameStartID} InPut=>{replayData.InPut}");
                }
                CurrRemoteInpuAllData = replayData.InPut;
                SetCurrKeyArr(CurrRemoteInpuAllData);
                bHadNetData = true;
            }
            else//无输入
            {
                CurrRemoteInpuAllData = 0;
            }

            //发送本地操作
            App.roomMgr.SendRoomSingelPlayerInput(UEssgee.instance.Frame,
             DoLocalPressedKeys());

            return bHadNetData;
        }
        //单机模式
        else
        {
            ulong inputData = DoLocalPressedKeys();
            SetCurrKeyArr(inputData);
            return true;
        }
    }

    void SetCurrKeyArr(ulong inputData)
    {
        temp.Clear();
        for (int i = 0; i < CheckList.Length; i++)
        {
            ulong key = CheckList[i];
            if ((inputData & key) > 0)
            {
                EssgeeMotionKey mk = dictKey2Motion[key];
                temp.Add(mk);
            }
        }
        mCurrKey = temp.ToArray();
    }

    ulong DoLocalPressedKeys()
    {
        //tempInputAllData = 0;
        //for (int i = 0; i < CheckList.Length; i++)
        //{
        //    ulong key = CheckList[i];
        //    if (Input.GetKey(dictMotion2RealKey[key]))
        //    {
        //        EssgeeMotionKey mk = dictKey2Motion[key];
        //        tempInputAllData |= (ulong)mk;
        //    }
        //}
        //return tempInputAllData;

        ulong tempLocalInputAllData = 0;
        tempLocalInputAllData |= ControllerMapper.Controller0.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller1.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller2.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller3.GetSingleAllInput();

#if UNITY_EDITOR
        if (CurrLocalInpuAllData != tempLocalInputAllData)
        {
            string ShowKeyNames = string.Empty;
        }
#endif

        CurrLocalInpuAllData = tempLocalInputAllData;

        CheckPlayerSlotChanged();

        return CurrLocalInpuAllData;
    }


    void CheckPlayerSlotChanged()
    {
        if (!ControllerMapper.Controller0.ConnectSlot.HasValue && ControllerMapper.Controller0.AnyButtonDown())
            Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 0);

        if (!ControllerMapper.Controller1.ConnectSlot.HasValue && ControllerMapper.Controller1.AnyButtonDown())
            Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 1);

        if (!ControllerMapper.Controller2.ConnectSlot.HasValue && ControllerMapper.Controller2.AnyButtonDown())
            Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 2);

        if (!ControllerMapper.Controller3.ConnectSlot.HasValue && ControllerMapper.Controller3.AnyButtonDown())
            Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 3);
    }
}

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
    public const ulong FinalKey = 281474976710656 << 10;
}


public class EssgeeControllerMapper : IControllerSetuper
{
    public EssgssSingleController Controller0 = new EssgssSingleController(0);
    public EssgssSingleController Controller1 = new EssgssSingleController(1);
    public EssgssSingleController Controller2 = new EssgssSingleController(2);
    public EssgssSingleController Controller3 = new EssgssSingleController(3);

    ulong mCurrAllInput;

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
        EssgssSingleController targetController;
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
        if (targetController.ConnectSlot.HasValue) return;

        targetController.ConnectSlot = slotIndex;
        Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
    }

}
public class EssgssSingleController : IController
{
    //public KeyCode UP, DOWN, LEFT, RIGHT, BTN_1, BTN_2, BTN_3, BTN_4, OPTION_1, OPTION_2;

    public ulong tg_UP, tg_DOWN, tg_LEFT, tg_RIGHT, tg_BTN_1, tg_BTN_2, tg_BTN_3, tg_BTN_4, tg_OPTION_1, tg_OPTION_2;
    public ulong CurrLocalSingleAllInput { get; private set; }

    int mControllerIndex;
    uint? mConnectSlot;

    /// <summary>
    /// 指示该手柄连接的手柄插槽
    /// <para><c>这个值代表了该手柄在实际游戏中控制的Player</c></para>
    /// <value>[0,3] 例外:为空代表未连接</value>
    /// </summary>
    public uint? ConnectSlot
    {
        get { return mConnectSlot; }
        set { mConnectSlot = value; this.ResetTargetMotionKey(); }
    }

    /// <summary>
    /// 控制器编号
    /// <para><c>此编号并非对应游戏中的player1,player2,player3,player4,仅仅作为本地4个手柄的实例</c></para>
    /// <value>[0,3]</value>
    /// </summary>
    public int ControllerIndex
    {
        get { return mControllerIndex; }
        set { mControllerIndex = value; /*this.LoadControlKeyForConfig();*/ }
    }

    public EssgssSingleController(int controllerIndex)
    {
        ControllerIndex = controllerIndex;
    }

    public bool AnyButtonDown()
    {
        return GetKeyMapper().AnyKeyDown(mControllerIndex);
    }

    public EssgeeKeyBinding GetKeyMapper()
    {
        return App.settings.KeyMapper.GetBinder<EssgeeKeyBinding>(UEssgee.instance.Platform);
    }

    public ulong GetSingleAllInput()
    {
        if (!ConnectSlot.HasValue)
            return 0;
        CurrLocalSingleAllInput = 0;

        EssgeeKeyBinding essgeeKeys = GetKeyMapper();

        if (essgeeKeys.GetKey(EssgeeSingleKey.UP, mControllerIndex)) CurrLocalSingleAllInput |= tg_UP;
        if (essgeeKeys.GetKey(EssgeeSingleKey.DOWN, mControllerIndex)) CurrLocalSingleAllInput |= tg_DOWN;
        if (essgeeKeys.GetKey(EssgeeSingleKey.LEFT, mControllerIndex)) CurrLocalSingleAllInput |= tg_LEFT;
        if (essgeeKeys.GetKey(EssgeeSingleKey.RIGHT, mControllerIndex)) CurrLocalSingleAllInput |= tg_RIGHT;
        if (essgeeKeys.GetKey(EssgeeSingleKey.BTN_1, mControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_1;
        if (essgeeKeys.GetKey(EssgeeSingleKey.BTN_2, mControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_2;
        if (essgeeKeys.GetKey(EssgeeSingleKey.BTN_3, mControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_3;
        if (essgeeKeys.GetKey(EssgeeSingleKey.BTN_4, mControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_4;
        if (essgeeKeys.GetKey(EssgeeSingleKey.OPTION_1, mControllerIndex)) CurrLocalSingleAllInput |= tg_OPTION_1;
        if (essgeeKeys.GetKey(EssgeeSingleKey.OPTION_2, mControllerIndex)) CurrLocalSingleAllInput |= tg_OPTION_2;

        return CurrLocalSingleAllInput;
    }
}


public static class EssgssSingleControllerSetter
{
    //public static void LoadControlKeyForConfig(this EssgssSingleController singlecontrol)
    //{
    //    //TODO 等待支持配置，或统一
    //    switch (singlecontrol.ControllerIndex)
    //    {
    //        case 0:
    //            singlecontrol.UP = KeyCode.W;
    //            singlecontrol.DOWN = KeyCode.S;
    //            singlecontrol.LEFT = KeyCode.A;
    //            singlecontrol.RIGHT = KeyCode.D;
    //            singlecontrol.BTN_1 = KeyCode.J;
    //            singlecontrol.BTN_2 = KeyCode.K;
    //            singlecontrol.BTN_3 = KeyCode.L;
    //            singlecontrol.BTN_4 = KeyCode.U;
    //            singlecontrol.OPTION_1 = KeyCode.Return;
    //            singlecontrol.OPTION_2 = KeyCode.LeftShift;
    //            break;
    //        case 1:
    //            singlecontrol.UP = KeyCode.UpArrow;
    //            singlecontrol.DOWN = KeyCode.DownArrow;
    //            singlecontrol.LEFT = KeyCode.LeftArrow;
    //            singlecontrol.RIGHT = KeyCode.RightArrow;
    //            singlecontrol.BTN_1 = KeyCode.Keypad1;
    //            singlecontrol.BTN_2 = KeyCode.Keypad2;
    //            singlecontrol.BTN_3 = KeyCode.Keypad3;
    //            singlecontrol.BTN_4 = KeyCode.Keypad4;
    //            singlecontrol.OPTION_1 = KeyCode.Keypad0;
    //            singlecontrol.OPTION_2 = KeyCode.KeypadPeriod;
    //            break;
    //        case 2:
    //            break;
    //        case 3:
    //            break;
    //    }
    //}
    public static void ResetTargetMotionKey(this EssgssSingleController singlecontrol)
    {
        if (!singlecontrol.ConnectSlot.HasValue)
        {
            singlecontrol.tg_UP
            = singlecontrol.tg_DOWN
            = singlecontrol.tg_LEFT
            = singlecontrol.tg_RIGHT
            = singlecontrol.tg_BTN_1
            = singlecontrol.tg_BTN_2
            = singlecontrol.tg_BTN_3
            = singlecontrol.tg_BTN_4
            = singlecontrol.tg_OPTION_1
            = singlecontrol.tg_OPTION_2
            = EssgeeUnityKey.FinalKey;
            return;
        }
        switch (singlecontrol.ConnectSlot.Value)
        {
            case 0:
                singlecontrol.tg_UP = EssgeeUnityKey.P1_UP;
                singlecontrol.tg_DOWN = EssgeeUnityKey.P1_DOWN;
                singlecontrol.tg_LEFT = EssgeeUnityKey.P1_LEFT;
                singlecontrol.tg_RIGHT = EssgeeUnityKey.P1_RIGHT;
                singlecontrol.tg_BTN_1 = EssgeeUnityKey.P1_BTN_1;
                singlecontrol.tg_BTN_2 = EssgeeUnityKey.P1_BTN_2;
                singlecontrol.tg_BTN_3 = EssgeeUnityKey.P1_BTN_3;
                singlecontrol.tg_BTN_4 = EssgeeUnityKey.P1_BTN_4;
                singlecontrol.tg_OPTION_1 = EssgeeUnityKey.P1_POTION_1;
                singlecontrol.tg_OPTION_2 = EssgeeUnityKey.P1_POTION_2;
                break;
            case 1:
                singlecontrol.tg_UP = EssgeeUnityKey.P2_UP;
                singlecontrol.tg_DOWN = EssgeeUnityKey.P2_DOWN;
                singlecontrol.tg_LEFT = EssgeeUnityKey.P2_LEFT;
                singlecontrol.tg_RIGHT = EssgeeUnityKey.P2_RIGHT;
                singlecontrol.tg_BTN_1 = EssgeeUnityKey.P2_BTN_1;
                singlecontrol.tg_BTN_2 = EssgeeUnityKey.P2_BTN_2;
                singlecontrol.tg_BTN_3 = EssgeeUnityKey.P2_BTN_3;
                singlecontrol.tg_BTN_4 = EssgeeUnityKey.P2_BTN_4;
                singlecontrol.tg_OPTION_1 = EssgeeUnityKey.P2_POTION_1;
                singlecontrol.tg_OPTION_2 = EssgeeUnityKey.P2_POTION_2;
                break;
            case 2:
                singlecontrol.tg_UP = EssgeeUnityKey.P3_UP;
                singlecontrol.tg_DOWN = EssgeeUnityKey.P3_DOWN;
                singlecontrol.tg_LEFT = EssgeeUnityKey.P3_LEFT;
                singlecontrol.tg_RIGHT = EssgeeUnityKey.P3_RIGHT;
                singlecontrol.tg_BTN_1 = EssgeeUnityKey.P3_BTN_1;
                singlecontrol.tg_BTN_2 = EssgeeUnityKey.P3_BTN_2;
                singlecontrol.tg_BTN_3 = EssgeeUnityKey.P3_BTN_3;
                singlecontrol.tg_BTN_4 = EssgeeUnityKey.P3_BTN_4;
                singlecontrol.tg_OPTION_1 = EssgeeUnityKey.P3_POTION_1;
                singlecontrol.tg_OPTION_2 = EssgeeUnityKey.P3_POTION_2;
                break;
            case 3:
                singlecontrol.tg_UP = EssgeeUnityKey.P4_UP;
                singlecontrol.tg_DOWN = EssgeeUnityKey.P4_DOWN;
                singlecontrol.tg_LEFT = EssgeeUnityKey.P4_LEFT;
                singlecontrol.tg_RIGHT = EssgeeUnityKey.P4_RIGHT;
                singlecontrol.tg_BTN_1 = EssgeeUnityKey.P4_BTN_1;
                singlecontrol.tg_BTN_2 = EssgeeUnityKey.P4_BTN_2;
                singlecontrol.tg_BTN_3 = EssgeeUnityKey.P4_BTN_3;
                singlecontrol.tg_BTN_4 = EssgeeUnityKey.P4_BTN_4;
                singlecontrol.tg_OPTION_1 = EssgeeUnityKey.P4_POTION_1;
                singlecontrol.tg_OPTION_2 = EssgeeUnityKey.P4_POTION_2;
                break;
        }
    }
}