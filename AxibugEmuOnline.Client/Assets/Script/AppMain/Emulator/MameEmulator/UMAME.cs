using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using MAME.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UMAME : MonoBehaviour, IEmuCore
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
    public RomPlatformType Platform { get { return mPlatform; } }
    RomPlatformType mPlatform = RomPlatformType.Cps1;
    public uint Frame => (uint)emu.currEmuFrame;
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
        mChangeRomName = string.Empty;
        mTimeSpan = new UniTimeSpan();
        emu.Init(RomPath, mUniLog, mUniResources, mUniVideoPlayer, mUniSoundPlayer, mUniKeyboard, mUniMouse, mTimeSpan);
    }
    void OnEnable()
    {
    }
    void OnDisable()
    {
        StopGame();
    }
    #region 实现接口
    public object GetState()
    {
        return SaveState();
    }
    public byte[] GetStateBytes()
    {
        return SaveState();
    }
    public void LoadState(object state)
    {
        LoadState((byte[])state);
    }
    public void LoadStateFromBytes(byte[] data)
    {
        LoadState(data);
    }
    public void Pause()
    {
        bLogicUpdatePause = false;
    }
    public void Resume()
    {
        bLogicUpdatePause = true;
    }
    public MsgBool StartGame(RomFile romFile)
    {
        mPlatform = romFile.Platform;
        mTimeSpan.InitStandTime();
        if (LoadGame(romFile.FileName, false))
            return true;
        else
            return "Rom加载失败";
    }
    public void Dispose()
    {
        StopGame();
    }
    public void DoReset()
    {
        StopGame();
        LoadGame(mChangeRomName, false);
    }
    public IControllerSetuper GetControllerSetuper()
    {
        return mUniKeyboard.ControllerMapper;
    }


    public void GetAudioParams(out int frequency, out int channels)
    {
        mUniSoundPlayer.GetAudioParams(out frequency, out channels);
    }

    #endregion
    bool LoadGame(string loadRom, bool bReplay = false)
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
            if (bReplay)
            {
                string Path = SavePath + Machine.sName + ".rp";
                mReplayReader = new ReplayReader(Path);
                mUniKeyboard.SetRePlay(true);
            }

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

    public bool PushEmulatorFrame()
    {
        if (!bInGame) return false;
        if (!bLogicUpdatePause) return false;

        //采集本帧Input
        bool bhadNext = mUniKeyboard.SampleInput();
        //如果未收到Input数据,核心帧不推进
        if (!bhadNext) return false;
        //放行下一帧
        //emu.UnlockNextFreme();
        //推帧
        emu.UpdateFrame();
        return true;
    }
    public void AfterPushFrame()
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
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);
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
        MemoryStream fs = new MemoryStream(data);
        BinaryReader br = new BinaryReader(fs);
        emu.LoadState(br);
        br.Close();
        fs.Close();
    }


    public Texture OutputPixel => mUniVideoPlayer.rawBufferWarper;

    public RawImage DrawCanvas => mUniVideoPlayer.DrawCanvas;


}