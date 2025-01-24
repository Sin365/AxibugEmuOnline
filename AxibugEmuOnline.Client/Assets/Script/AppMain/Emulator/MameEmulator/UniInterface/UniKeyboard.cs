using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxiReplay;
using MAME.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniKeyboard : MonoBehaviour, IKeyboard
{
    public MameControllerMapper ControllerMapper { get; private set; }
    bool bReplayMode;
    PlayMode mPlayMode;
    ReplayMode mReplayMode;

    void Awake()
    {
        ControllerMapper = new MameControllerMapper();
        Init(false);
    }

    public ulong GetPressedKeys()
    {
        ulong InputData;
        if (!bReplayMode)//游玩模式（单机或联机）
            return mPlayMode.GetPressedKeys();
        else//Replay模式
            return mReplayMode.GetPressedKeys();
    }

    public bool SampleInput()
    {
        if (bReplayMode) return true;
        return mPlayMode.SampleInput();
    }

    #region

    public void SetRePlay(bool IsReplay)
    {
        bReplayMode = IsReplay;
    }

    public void Init(bool IsReplay)
    {
        bReplayMode = IsReplay;
        mPlayMode = new PlayMode(this);
        mReplayMode = new ReplayMode();
    }

    public static IEnumerable<string> GetInputpDataToMotionKey(ulong inputdata)
    {
        if (inputdata == 0)
            yield break;
        for (int i = 0; i < MotionKey.AllNeedCheckList.Length; i++)
        {
            if ((inputdata & MotionKey.AllNeedCheckList[i]) > 0)
                yield return MotionKey.GetKeyName(MotionKey.AllNeedCheckList[i]);
        }
    }
    public class PlayMode
    {
        UniKeyboard mUniKeyboard;
        public ulong CurrLocalInpuAllData = 0;
        public ulong CurrRemoteInpuAllData = 0;

        public PlayMode(UniKeyboard uniKeyboard)
        {
            this.mUniKeyboard = uniKeyboard;
        }

        public ulong GetPressedKeys()
        {
            if (InGameUI.Instance.IsNetPlay)
                return CurrRemoteInpuAllData;
            else
                return CurrLocalInpuAllData;
        }

        public bool SampleInput()
        {
            //Net模式
            if (InGameUI.Instance.IsNetPlay)
            {
                bool bHadNetData = false;
                int targetFrame; ReplayStep replayData; int frameDiff; bool inputDiff;
                if (App.roomMgr.netReplay.TryGetNextFrame((int)UMAME.instance.Frame, out replayData, out frameDiff, out inputDiff))
                {
                    if (inputDiff)
                    {
                        App.log.Debug($"{DateTime.Now.ToString("hh:mm:ss.fff")} TryGetNextFrame remoteFrame->{App.roomMgr.netReplay.mRemoteFrameIdx} diff->{frameDiff} " +
                            $"frame=>{replayData.FrameStartID} InPut=>{replayData.InPut}");
                    }
                    CurrRemoteInpuAllData = replayData.InPut;

                    bHadNetData = true;
                }
                else//无输入
                {
                    CurrRemoteInpuAllData = 0;
                }
                //发送本地操作
                App.roomMgr.SendRoomSingelPlayerInput(UMAME.instance.Frame, DoLocalPressedKeys());
                return bHadNetData;
            }
            //单人模式
            else
            {
                DoLocalPressedKeys();
                return true;
            }
        }

        ulong DoLocalPressedKeys()
        {
            ulong tempLocalInputAllData = 0;
            tempLocalInputAllData |= mUniKeyboard.ControllerMapper.Controller0.GetSingleAllInput();
            tempLocalInputAllData |= mUniKeyboard.ControllerMapper.Controller1.GetSingleAllInput();
            tempLocalInputAllData |= mUniKeyboard.ControllerMapper.Controller2.GetSingleAllInput();
            tempLocalInputAllData |= mUniKeyboard.ControllerMapper.Controller3.GetSingleAllInput();

#if UNITY_EDITOR
            if (CurrLocalInpuAllData != tempLocalInputAllData)
            {
                string ShowKeyNames = string.Empty;
                foreach (string keyname in GetInputpDataToMotionKey(CurrLocalInpuAllData))
                {
                    ShowKeyNames += keyname + "   |";
                }
                Debug.Log("GetPressedKeys=>" + ShowKeyNames);
            }
#endif

            CurrLocalInpuAllData = tempLocalInputAllData;
            //写入replay
            UMAME.instance.mReplayWriter.NextFramebyFrameIdx((int)UMAME.instance.mUniVideoPlayer.mFrame, CurrLocalInpuAllData);

            return CurrLocalInpuAllData;
        }

    }
    public class ReplayMode
    {
        ulong currInputData;

        public ReplayMode()
        {
        }

        public ulong GetPressedKeys()
        {
            int targetFrame = (int)UMAME.instance.mUniVideoPlayer.mFrame;
            AxiReplay.ReplayStep stepData;
            //有变化
            if (UMAME.instance.mReplayReader.NextFramebyFrameIdx(targetFrame, out stepData))
            {
#if UNITY_EDITOR
                string ShowKeyNames = string.Empty;
                foreach (string keyname in GetInputpDataToMotionKey(currInputData))
                {
                    ShowKeyNames += keyname + "   |";
                }
                Debug.Log("GetPressedKeys=>" + ShowKeyNames);
#endif
                currInputData = stepData.InPut;
            }
            return currInputData;
        }
    }
    #endregion
}

public class MameControllerMapper : IControllerSetuper
{
    public MameSingleConoller Controller0 { get; } = new MameSingleConoller(0);
    public MameSingleConoller Controller1 { get; } = new MameSingleConoller(1);
    public MameSingleConoller Controller2 { get; } = new MameSingleConoller(2);
    public MameSingleConoller Controller3 { get; } = new MameSingleConoller(3);

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
        MameSingleConoller targetController;
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

/// <summary>
/// MAME控制器
/// </summary>
public class MameSingleConoller : IController
{
    public KeyCode INSERT_COIN, GAMESTART,
    UP, DOWN, LEFT, RIGHT,
    BTN_A, BTN_B, BTN_C, BTN_D, BTN_E, BTN_F;

    public ulong tg_INSERT_COIN, tg_GAMESTART,
    tg_UP, tg_DOWN, tg_LEFT, tg_RIGHT,
    tg_BTN_A, tg_BTN_B, tg_BTN_C, tg_BTN_D, tg_BTN_E, tg_BTN_F;

    ulong mTempSingleAllInput;

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
        set { mControllerIndex = value; this.LoadControlKeyForConfig(); }
    }
    public MameSingleConoller(int controllerIndex)
    {
        ControllerIndex = controllerIndex;
    }

    public bool AnyButtonDown()
    {
        //if (Input.GetKeyDown(INSERT_COIN)) return true;
        //if (Input.GetKeyDown(GAMESTART)) return true;
        //if (Input.GetKeyDown(UP)) return true;
        //if (Input.GetKeyDown(DOWN)) return true;
        //if (Input.GetKeyDown(LEFT)) return true;
        //if (Input.GetKeyDown(RIGHT)) return true;
        //if (Input.GetKeyDown(BTN_A)) return true;
        //if (Input.GetKeyDown(BTN_B)) return true;
        //if (Input.GetKeyDown(BTN_C)) return true;
        //if (Input.GetKeyDown(BTN_D)) return true;
        //if (Input.GetKeyDown(BTN_E)) return true;
        //if (Input.GetKeyDown(BTN_F)) return true;
        return mTempSingleAllInput > 0;
    }
    public ulong GetSingleAllInput()
    {
        if (!ConnectSlot.HasValue)
            return 0;
        mTempSingleAllInput = 0;
        if (Input.GetKey(INSERT_COIN)) mTempSingleAllInput |= (ulong)tg_INSERT_COIN;
        if (Input.GetKey(GAMESTART)) mTempSingleAllInput |= (ulong)tg_GAMESTART;
        if (Input.GetKey(UP)) mTempSingleAllInput |= (ulong)tg_UP;
        if (Input.GetKey(DOWN)) mTempSingleAllInput |= (ulong)tg_DOWN;
        if (Input.GetKey(LEFT)) mTempSingleAllInput |= (ulong)tg_LEFT;
        if (Input.GetKey(RIGHT)) mTempSingleAllInput |= (ulong)tg_RIGHT;
        if (Input.GetKey(BTN_A)) mTempSingleAllInput |= (ulong)tg_BTN_A;
        if (Input.GetKey(BTN_B)) mTempSingleAllInput |= (ulong)tg_BTN_B;
        if (Input.GetKey(BTN_C)) mTempSingleAllInput |= (ulong)tg_BTN_C;
        if (Input.GetKey(BTN_D)) mTempSingleAllInput |= (ulong)tg_BTN_D;
        if (Input.GetKey(BTN_E)) mTempSingleAllInput |= (ulong)tg_BTN_E;
        if (Input.GetKey(BTN_F)) mTempSingleAllInput |= (ulong)tg_BTN_F;
        return mTempSingleAllInput;
    }

}
public static class MameSingleControllSetter
{
    public static void LoadControlKeyForConfig(this MameSingleConoller singlecontrol)
    {
        //TODO 等待支持配置，或统一
        switch (singlecontrol.ControllerIndex)
        {
            case 0:
                singlecontrol.INSERT_COIN = KeyCode.Alpha5;
                singlecontrol.GAMESTART = KeyCode.Alpha1;
                singlecontrol.UP = KeyCode.W;
                singlecontrol.DOWN = KeyCode.S;
                singlecontrol.LEFT = KeyCode.A;
                singlecontrol.RIGHT = KeyCode.D;
                singlecontrol.BTN_A = KeyCode.J;
                singlecontrol.BTN_B = KeyCode.K;
                singlecontrol.BTN_C = KeyCode.L;
                singlecontrol.BTN_D = KeyCode.U;
                singlecontrol.BTN_E = KeyCode.I;
                singlecontrol.BTN_F = KeyCode.O;
                break;
            case 1:
                singlecontrol.INSERT_COIN = KeyCode.KeypadMultiply;
                singlecontrol.GAMESTART = KeyCode.KeypadDivide;
                singlecontrol.UP = KeyCode.UpArrow;
                singlecontrol.DOWN = KeyCode.DownArrow;
                singlecontrol.LEFT = KeyCode.LeftArrow;
                singlecontrol.RIGHT = KeyCode.RightArrow;
                singlecontrol.BTN_A = KeyCode.Keypad1;
                singlecontrol.BTN_B = KeyCode.Keypad2;
                singlecontrol.BTN_C = KeyCode.Keypad3;
                singlecontrol.BTN_D = KeyCode.Keypad4;
                singlecontrol.BTN_E = KeyCode.Keypad5;
                singlecontrol.BTN_F = KeyCode.Keypad6;
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    public static void ResetTargetMotionKey(this MameSingleConoller singlecontrol)
    {
        if (!singlecontrol.ConnectSlot.HasValue)
        {
            singlecontrol.tg_INSERT_COIN
            = singlecontrol.tg_GAMESTART
            = singlecontrol.tg_UP
            = singlecontrol.tg_DOWN
            = singlecontrol.tg_LEFT
            = singlecontrol.tg_RIGHT
            = singlecontrol.tg_BTN_A
            = singlecontrol.tg_BTN_B
            = singlecontrol.tg_BTN_C
            = singlecontrol.tg_BTN_D
            = singlecontrol.tg_BTN_E
            = singlecontrol.tg_BTN_F
            = MotionKey.FinalKey;
            return;
        }
        switch (singlecontrol.ConnectSlot.Value)
        {
            case 0:
                singlecontrol.tg_INSERT_COIN = MotionKey.P1_INSERT_COIN;
                singlecontrol.tg_GAMESTART = MotionKey.P1_GAMESTART;
                singlecontrol.tg_UP = MotionKey.P1_UP;
                singlecontrol.tg_DOWN = MotionKey.P1_DOWN;
                singlecontrol.tg_LEFT = MotionKey.P1_LEFT;
                singlecontrol.tg_RIGHT = MotionKey.P1_RIGHT;
                singlecontrol.tg_BTN_A = MotionKey.P1_BTN_1;
                singlecontrol.tg_BTN_B = MotionKey.P1_BTN_2;
                singlecontrol.tg_BTN_C = MotionKey.P1_BTN_3;
                singlecontrol.tg_BTN_D = MotionKey.P1_BTN_4;
                singlecontrol.tg_BTN_E = MotionKey.P1_BTN_5;
                singlecontrol.tg_BTN_F = MotionKey.P1_BTN_6;
                break;
            case 1:
                singlecontrol.tg_INSERT_COIN = MotionKey.P2_INSERT_COIN;
                singlecontrol.tg_GAMESTART = MotionKey.P2_GAMESTART;
                singlecontrol.tg_UP = MotionKey.P2_UP;
                singlecontrol.tg_DOWN = MotionKey.P2_DOWN;
                singlecontrol.tg_LEFT = MotionKey.P2_LEFT;
                singlecontrol.tg_RIGHT = MotionKey.P2_RIGHT;
                singlecontrol.tg_BTN_A = MotionKey.P2_BTN_1;
                singlecontrol.tg_BTN_B = MotionKey.P2_BTN_2;
                singlecontrol.tg_BTN_C = MotionKey.P2_BTN_3;
                singlecontrol.tg_BTN_D = MotionKey.P2_BTN_4;
                singlecontrol.tg_BTN_E = MotionKey.P2_BTN_5;
                singlecontrol.tg_BTN_F = MotionKey.P2_BTN_6;
                break;
            //后续修改后 支持P3 P4
            case 2:
                singlecontrol.tg_INSERT_COIN = MotionKey.FinalKey;
                singlecontrol.tg_GAMESTART = MotionKey.FinalKey;
                singlecontrol.tg_UP = MotionKey.FinalKey;
                singlecontrol.tg_DOWN = MotionKey.FinalKey;
                singlecontrol.tg_LEFT = MotionKey.FinalKey;
                singlecontrol.tg_RIGHT = MotionKey.FinalKey;
                singlecontrol.tg_BTN_A = MotionKey.FinalKey;
                singlecontrol.tg_BTN_B = MotionKey.FinalKey;
                singlecontrol.tg_BTN_C = MotionKey.FinalKey;
                singlecontrol.tg_BTN_D = MotionKey.FinalKey;
                singlecontrol.tg_BTN_E = MotionKey.FinalKey;
                singlecontrol.tg_BTN_F = MotionKey.FinalKey;
                break;
            case 3:
                singlecontrol.tg_INSERT_COIN = MotionKey.FinalKey;
                singlecontrol.tg_GAMESTART = MotionKey.FinalKey;
                singlecontrol.tg_UP = MotionKey.FinalKey;
                singlecontrol.tg_DOWN = MotionKey.FinalKey;
                singlecontrol.tg_LEFT = MotionKey.FinalKey;
                singlecontrol.tg_RIGHT = MotionKey.FinalKey;
                singlecontrol.tg_BTN_A = MotionKey.FinalKey;
                singlecontrol.tg_BTN_B = MotionKey.FinalKey;
                singlecontrol.tg_BTN_C = MotionKey.FinalKey;
                singlecontrol.tg_BTN_D = MotionKey.FinalKey;
                singlecontrol.tg_BTN_E = MotionKey.FinalKey;
                singlecontrol.tg_BTN_F = MotionKey.FinalKey;
                break;
        }
    }
}

