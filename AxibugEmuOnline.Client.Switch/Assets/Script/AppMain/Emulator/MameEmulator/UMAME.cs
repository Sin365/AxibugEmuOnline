using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using MAME.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UMAME : EmuCore<ulong>
{
    public static UMAME instance { get; private set; }
    public MAMEEmu emu { get; private set; }
    UniLog mUniLog;
    UniMouse mUniMouse;
    [HideInInspector]
    public UniVideoPlayer mUniVideoPlayer;
    UniSoundPlayer mUniSoundPlayer;
    UniKeyboard mUniKeyboard;
    UniResources mUniResources;
    UniIO mUniIO;

    public Text mFPS;
    private Canvas mCanvas;
    public List<RomInfo> HadGameList = new List<RomInfo>();
    string mChangeRomName = string.Empty;
    public UniTimeSpan mTimeSpan;
    public bool bQuickTestRom = false;
    public string mQuickTestRom = string.Empty;
    public ReplayWriter mReplayWriter;
    public ReplayReader mReplayReader;
    public long currEmuFrame => emu.currEmuFrame;
    public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
    public static bool bInGame { get; private set; }
    public static bool bLogicUpdatePause { get; private set; }
    public string EmuDataPath { get { return App.PersistentDataPath(Platform); } }
    public string RomPath => EmuDataPath + "/RemoteRoms/";
    public string SavePath => EmuDataPath + "/sav/";
    public override RomPlatformType Platform { get { return mPlatform; } }
    RomPlatformType mPlatform = RomPlatformType.Cps1;
    public override uint Frame => (uint)emu.currEmuFrame;
    void Awake()
    {
        instance = this;
        mFPS = GameObject.Find("FPS").GetComponent<Text>();
        mCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        mCanvas.worldCamera = Camera.main;
        emu = new MAMEEmu();
        mUniLog = new UniLog();
        mUniMouse = this.gameObject.AddComponent<UniMouse>();
        mUniVideoPlayer = this.gameObject.AddComponent<UniVideoPlayer>();
        mUniSoundPlayer = GameObject.Find("Audio").transform.GetComponent<UniSoundPlayer>();
        mUniKeyboard = this.gameObject.AddComponent<UniKeyboard>();
        mUniResources = new UniResources();
        mUniIO = new UniIO();
        mChangeRomName = string.Empty;
        mTimeSpan = new UniTimeSpan();
        emu.Init(RomPath, mUniLog, mUniResources, mUniVideoPlayer, mUniSoundPlayer, mUniKeyboard, mUniMouse, mTimeSpan, mUniIO);
    }
    void OnEnable()
    {
    }
    void OnDisable()
    {
        StopGame();
    }
    #region 实现接口
    public override object GetState()
    {
        return SaveState();
    }
    public override byte[] GetStateBytes()
    {
        return SaveState();
    }
    public override void LoadState(object state)
    {
        LoadState((byte[])state);
    }
    public override void LoadStateFromBytes(byte[] data)
    {
        LoadState(data);
    }
    public override void Pause()
    {
        bLogicUpdatePause = false;
    }
    public override void Resume()
    {
        bLogicUpdatePause = true;
    }
    public override MsgBool StartGame(RomFile romFile)
    {
        mPlatform = romFile.Platform;
        mTimeSpan.InitStandTime();
        if (LoadGame(romFile.FileName))
            return true;
        else
            return "Rom加载失败";
    }
    public override void Dispose()
    {
        StopGame();
    }
    public override void DoReset()
    {
        StopGame();
        LoadGame(mChangeRomName);
    }
    public override IControllerSetuper GetControllerSetuper()
    {
        return mUniKeyboard.ControllerMapper;
    }


    public override void GetAudioParams(out int frequency, out int channels)
    {
        mUniSoundPlayer.GetAudioParams(out frequency, out channels);
    }

    #endregion
    bool LoadGame(string loadRom)
    {
        emu.ResetRomRoot(RomPath);
        //Application.targetFrameRate = 60;
        mReplayWriter = new ReplayWriter(mChangeRomName, "fuck", ReplayData.ReplayFormat.FM32IP64, Encoding.UTF8);
        mChangeRomName = loadRom;
        StopGame();
        //读取ROM
        emu.LoadRom(mChangeRomName);
        //读取成功
        if (emu.bRom)
        {

            //读取ROM之后获得宽高初始化画面
            int _width; int _height; IntPtr _framePtr;
            emu.GetGameScreenSize(out _width, out _height, out _framePtr);
            App.log.Debug($"_width->{_width}, _height->{_height}, _framePtr->{_framePtr}");
            mUniVideoPlayer.Initialize(_width, _height, _framePtr);
            //初始化音频
            mUniSoundPlayer.Initialize();
            //开始游戏
            emu.StartGame();
            bInGame = true;
            bLogicUpdatePause = true;
            return true;
        }
        else
        {
            App.log.Debug($"ROM加载失败");
            return false;
        }
    }
    protected override bool OnPushEmulatorFrame(ulong InputData)
    {
        if (!bInGame) return false;
        if (!bLogicUpdatePause) return false;

        mUniKeyboard.SyncInput(InputData);
        emu.UpdateFrame();
        //写入replay
        UMAME.instance.mReplayWriter.NextFramebyFrameIdx((int)UMAME.instance.mUniVideoPlayer.mFrame, InputData);
        return true;
    }

    protected override ulong ConvertInputDataFromNet(ReplayStep step)
    {
        return step.InPut;
    }

    protected override ulong InputDataToNet(ulong inputData)
    {
        return inputData;
    }

    protected override ulong GetLocalInput()
    {
        return mUniKeyboard.DoLocalPressedKeys();
    }

    protected override void AfterPushFrame()
    {
        mFPS.text = ($"fpsv {mUniVideoPlayer.videoFPS.ToString("F2")} fpsa {mUniSoundPlayer.audioFPS.ToString("F2")} ,Idx:{App.roomMgr.netReplay?.mCurrClientFrameIdx},RIdx:{App.roomMgr.netReplay?.mRemoteFrameIdx},RForward:{App.roomMgr.netReplay?.mRemoteForwardCount} ,RD:{App.roomMgr.netReplay?.mRemoteForwardCount} ,D:{App.roomMgr.netReplay?.mDiffFrameCount} ,Q:{App.roomMgr.netReplay?.mNetReplayQueue.Count}");
    }
    public void SaveReplay()
    {
        string Path = SavePath + Machine.sName + ".rp";
        string dbgPath = SavePath + Machine.sName + ".rpwrite";
        mReplayWriter.SaveData(Path, true, dbgPath);
    }
    public void StopGame()
    {
        if (bInGame)
        {
            emu.StopGame();
            mUniVideoPlayer.StopVideo();
            mUniSoundPlayer.StopPlay();
            bInGame = false;
            bLogicUpdatePause = false;
        }
    }
    byte[] SaveState()
    {
        if (!AxiIO.Directory.Exists(SavePath))
            AxiIO.Directory.CreateDirectory(SavePath);

        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
        emu.SaveState(bw);
        byte[] data = ms.ToArray();
        bw.Close();
        ms.Close();

        return data;


        //byte[] screenData = UMAME.instance.mUniVideoPlayer.GetScreenImg();

        //FileStream fsImg = new FileStream(SavePath + Machine.sName + ".jpg", FileMode.Create);
        //fsImg.Write(screenData, 0, screenData.Length);
        //fsImg.Close();
    }
    void LoadState(byte[] data)
    {
        System.IO.MemoryStream fs = new System.IO.MemoryStream(data);
        System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
        emu.LoadState(br);
        br.Close();
        fs.Close();
    }


    public override Texture OutputPixel => mUniVideoPlayer.rawBufferWarper;

    public override RawImage DrawCanvas => mUniVideoPlayer.DrawCanvas;


}