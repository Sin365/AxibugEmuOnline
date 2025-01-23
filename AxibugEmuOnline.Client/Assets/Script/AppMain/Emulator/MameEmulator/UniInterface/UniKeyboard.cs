using MAME.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniKeyboard : MonoBehaviour, IKeyboard
{
    #region UIButton
    public UILongClickButton btnP1;
    public UILongClickButton btnCoin1;
    public UILongClickButton btnA;
    public UILongClickButton btnB;
    public UILongClickButton btnC;
    public UILongClickButton btnD;
    //public UILongClickButton btnE;
    //public UILongClickButton btnF;
    public UILongClickButton btnAB;
    public UILongClickButton btnCD;
    public UILongClickButton btnABC;
    public Transform tfKeyPad;
    public FloatingJoystick mJoystick;
    public List<UILongClickButton> mUIBtns = new List<UILongClickButton>();
    #endregion

    public static Dictionary<KeyCode, MotionKey> dictKeyCfgs = new Dictionary<KeyCode, MotionKey>();
    public static KeyCode[] CheckList;
    bool bReplayMode;
    PlayMode mPlayMode;
    ReplayMode mReplayMode;
    ulong last_CurryInpuAllData_test = 0;

    void Awake()
    {
        mJoystick = GameObject.Find("tfJoystick").GetComponent<FloatingJoystick>();
        tfKeyPad = GameObject.Find("tfKeyPad").transform;
        btnP1 = GameObject.Find("btnP1").GetComponent<UILongClickButton>();
        btnCoin1 = GameObject.Find("btnCoin1").GetComponent<UILongClickButton>();
        btnA = GameObject.Find("btnA").GetComponent<UILongClickButton>();
        btnB = GameObject.Find("btnB").GetComponent<UILongClickButton>();
        btnC = GameObject.Find("btnC").GetComponent<UILongClickButton>();
        btnD = GameObject.Find("btnD").GetComponent<UILongClickButton>();
        //btnE = GameObject.Find("btnE")?.GetComponent<UILongClickButton>();
        //btnF = GameObject.Find("btnF")?.GetComponent<UILongClickButton>();
        btnAB = GameObject.Find("btnAB").GetComponent<UILongClickButton>();
        btnCD = GameObject.Find("btnCD").GetComponent<UILongClickButton>();
        btnABC = GameObject.Find("btnABC").GetComponent<UILongClickButton>();

        mUIBtns.Add(btnP1);
        mUIBtns.Add(btnCoin1);
        mUIBtns.Add(btnA);
        mUIBtns.Add(btnB);
        mUIBtns.Add(btnC);
        mUIBtns.Add(btnD);
        mUIBtns.Add(btnAB);
        mUIBtns.Add(btnCD);
        mUIBtns.Add(btnABC);

        //if (btnE != null)
        //{
        //    mUIBtns.Add(btnE);
        //    btnE.gameObject.SetActive(false);
        //}
        //else
        //{ 
        //    mUIBtns.Add(btnF);
        //    btnF.gameObject.SetActive(false);
        //}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        tfKeyPad.gameObject.SetActive(false);
#endif
        Init(false);
    }

    public MotionKey[] GetPressedKeys()
    {
        MotionKey[] currkey;
        ulong InputData;
        if (!bReplayMode)
            currkey = mPlayMode.GetPressedKeys(out InputData);
        else
            currkey = mReplayMode.GetPressedKeys(out InputData);
#if UNITY_EDITOR
        if (last_CurryInpuAllData_test != InputData)
        {
            string TempStr = "";
            foreach (var item in currkey)
            {
                TempStr += $"{item.ToString()}|";
            }
            Debug.Log($"{UMAME.instance.mUniVideoPlayer.mFrame} | {EmuTimer.get_current_time().attoseconds} |{EmuTimer.get_current_time().seconds} |   {InputData} |   {TempStr}");
            last_CurryInpuAllData_test = InputData;
        }
#endif
        return currkey;
    }

    public void UpdateInputKey()
    {
        UpdateLogic();
    }

    #region

    public void SetRePlay(bool IsReplay)
    {
        bReplayMode = IsReplay;
    }

    public void Init(bool IsReplay)
    {
        bReplayMode = IsReplay;
        dictKeyCfgs.Clear();
        //dictKeyCfgs.Add(KeyCode.P, MotionKey.EMU_PAUSED);
        dictKeyCfgs.Add(KeyCode.Alpha1, MotionKey.P1_GAMESTART);
        dictKeyCfgs.Add(KeyCode.Alpha5, MotionKey.P1_INSERT_COIN);
        dictKeyCfgs.Add(KeyCode.W, MotionKey.P1_UP);
        dictKeyCfgs.Add(KeyCode.S, MotionKey.P1_DOWN);
        dictKeyCfgs.Add(KeyCode.A, MotionKey.P1_LEFT);
        dictKeyCfgs.Add(KeyCode.D, MotionKey.P1_RIGHT);
        dictKeyCfgs.Add(KeyCode.J, MotionKey.P1_BTN_1);
        dictKeyCfgs.Add(KeyCode.K, MotionKey.P1_BTN_2);
        dictKeyCfgs.Add(KeyCode.L, MotionKey.P1_BTN_3);
        dictKeyCfgs.Add(KeyCode.U, MotionKey.P1_BTN_4);
        dictKeyCfgs.Add(KeyCode.KeypadDivide, MotionKey.P2_GAMESTART);
        dictKeyCfgs.Add(KeyCode.KeypadMultiply, MotionKey.P2_INSERT_COIN);
        dictKeyCfgs.Add(KeyCode.UpArrow, MotionKey.P2_UP);
        dictKeyCfgs.Add(KeyCode.DownArrow, MotionKey.P2_DOWN);
        dictKeyCfgs.Add(KeyCode.LeftArrow, MotionKey.P2_LEFT);
        dictKeyCfgs.Add(KeyCode.RightArrow, MotionKey.P2_RIGHT);
        dictKeyCfgs.Add(KeyCode.Keypad1, MotionKey.P2_BTN_1);
        dictKeyCfgs.Add(KeyCode.Keypad2, MotionKey.P2_BTN_2);
        dictKeyCfgs.Add(KeyCode.Keypad3, MotionKey.P2_BTN_3);
        dictKeyCfgs.Add(KeyCode.Keypad4, MotionKey.P2_BTN_4);
        CheckList = dictKeyCfgs.Keys.ToArray();

        btnP1.Key = new long[] { (long)MotionKey.P1_GAMESTART };
        btnCoin1.Key = new long[] { (long)MotionKey.P1_INSERT_COIN };
        btnA.Key = new long[] { (long)MotionKey.P1_BTN_1 };
        btnB.Key = new long[] { (long)MotionKey.P1_BTN_2 };
        btnC.Key = new long[] { (long)MotionKey.P1_BTN_3 };
        btnD.Key = new long[] { (long)MotionKey.P1_BTN_4 };
        //btnE.Key = new long[] { (long)MotionKey.P1_BTN_5 };
        //btnF.Key = new long[] { (long)MotionKey.P1_BTN_6 };
        btnAB.Key = new long[] { (long)MotionKey.P1_BTN_1, (long)MotionKey.P1_BTN_2 };
        btnCD.Key = new long[] { (long)MotionKey.P1_BTN_3, (long)MotionKey.P1_BTN_4 };
        btnABC.Key = new long[] { (long)MotionKey.P1_BTN_1, (long)MotionKey.P1_BTN_2, (long)MotionKey.P1_BTN_3 };

        mPlayMode = new PlayMode(this);
        mReplayMode = new ReplayMode();
    }

    public void UpdateLogic()
    {
        if (bReplayMode) return;
        mPlayMode.UpdateLogic();
    }
    public class PlayMode
    {
        Dictionary<KeyCode, MotionKey> dictKeyCfgs => UniKeyboard.dictKeyCfgs;
        UniKeyboard mUniKeyboard;
        KeyCode[] CheckList => UniKeyboard.CheckList;
        ulong tempInputAllData = 0;
        List<MotionKey> temp = new List<MotionKey>();
        public ulong CurryInpuAllData = 0;
        public MotionKey[] mCurrKey = new MotionKey[0];

        public PlayMode(UniKeyboard uniKeyboard)
        {
            this.mUniKeyboard = uniKeyboard;
        }

        public void UpdateLogic()
        {
            tempInputAllData = 0;
            temp.Clear();
            for (int i = 0; i < CheckList.Length; i++)
            {
                if (Input.GetKey(CheckList[i]))
                {
                    MotionKey mk = dictKeyCfgs[CheckList[i]];
                    temp.Add(mk);
                    tempInputAllData |= (ulong)mk;
                }
            }

            for (int i = 0; i < mUniKeyboard.mUIBtns.Count; i++)
            {
                if (mUniKeyboard.mUIBtns[i].bHotKey)
                {
                    for (int j = 0; j < mUniKeyboard.mUIBtns[i].Key.Length; j++)
                    {
                        MotionKey mk = (MotionKey)mUniKeyboard.mUIBtns[i].Key[j];
                        temp.Add(mk);
                        tempInputAllData |= (ulong)mk;
                    }
                }
            }

            Vector2Int inputV2 = mUniKeyboard.mJoystick.RawInputV2;
            //Debug.Log($"{inputV2.x},{inputV2.y}");
            if (inputV2.x > 0)
            {
                temp.Add(MotionKey.P1_RIGHT);
                tempInputAllData |= (ulong)MotionKey.P1_RIGHT;
            }
            else if (inputV2.x < 0)
            {
                temp.Add(MotionKey.P1_LEFT);
                tempInputAllData |= (ulong)MotionKey.P1_LEFT;
            }
            if (inputV2.y > 0)
            {
                temp.Add(MotionKey.P1_UP);
                tempInputAllData |= (ulong)MotionKey.P1_UP;
            }
            else if (inputV2.y < 0)
            {
                temp.Add(MotionKey.P1_DOWN);
                tempInputAllData |= (ulong)MotionKey.P1_DOWN;
            }
            CurryInpuAllData = tempInputAllData;
            mCurrKey = temp.ToArray();
        }

        public MotionKey[] GetPressedKeys(out ulong InputData)
        {
            //UMAME.instance.mReplayWriter.NextFramebyFrameIdx((int)UMAME.instance.mUniVideoPlayer.mFrame, CurryInpuAllData);
            UMAME.instance.mReplayWriter.NextFramebyFrameIdx((int)UMAME.instance.mUniVideoPlayer.mFrame, CurryInpuAllData);
            InputData = CurryInpuAllData;
            return mCurrKey;
        }
    }
    public class ReplayMode
    {
        public MotionKey[] mCurrKey = new MotionKey[0];
        MotionKey[] ReplayCheckKey;
        ulong currInputData;
        List<MotionKey> temp = new List<MotionKey>();

        public ReplayMode()
        {
            ReplayCheckKey = dictKeyCfgs.Values.ToArray();
        }

        public MotionKey[] GetPressedKeys(out ulong InputData)
        {
            //有变化
            //if (UMAME.instance.mReplayReader.NextFrame(out AxiReplay.ReplayStep stepData))
            int targetFrame = (int)UMAME.instance.mUniVideoPlayer.mFrame;
            //if (UMAME.instance.mReplayReader.NextFramebyFrameIdx(targetFrame, out AxiReplay.ReplayStep stepData))
            //{
            //    temp.Clear();
            //    //有数据
            //    for (int i = 0; i < ReplayCheckKey.Length; i++)
            //    {
            //        if ((stepData.InPut & (ulong)ReplayCheckKey[i]) > 0)
            //            temp.Add(ReplayCheckKey[i]);
            //    }
            //    mCurrKey = temp.ToArray();
            //}
            AxiReplay.ReplayStep stepData;

            if (UMAME.instance.mReplayReader.NextFramebyFrameIdx(targetFrame, out stepData))
            {
                temp.Clear();
                //List<MotionKey> temp = new List<MotionKey>();
                //temp.Clear();
                ////有数据
                //for (int i = 0; i < ReplayCheckKey.Length; i++)
                //{
                //    if ((stepData.InPut & (ulong)ReplayCheckKey[i]) > 0)
                //        temp.Add(ReplayCheckKey[i]);
                //}
                //mCurrKey = temp.ToArray();
                foreach (MotionKey key in GetStepDataToMotionKey(stepData))
                {
                    temp.Add(key);
                }
                mCurrKey = temp.ToArray();
                currInputData = stepData.InPut;
            }
            InputData = currInputData;
            return mCurrKey;
        }

        IEnumerable<MotionKey> GetStepDataToMotionKey(AxiReplay.ReplayStep stepData)
        {
            //有数据
            for (int i = 0; i < ReplayCheckKey.Length; i++)
            {
                if ((stepData.InPut & (ulong)ReplayCheckKey[i]) > 0)
                    yield return ReplayCheckKey[i];
            }
        }

    }
    #endregion
}