using AxiReplay;
using MAME.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UMAME : MonoBehaviour
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
    public Button btnStop;
    public Button btnStart;
    public Button btnRePlay;
    public Button btnRePayySave;
    public Button btnRomDir;
    public Button btnSaveState;
    public Button btnLoadState;
    public Dictionary<string, RomInfo> ALLGame;
    public List<RomInfo> HadGameList = new List<RomInfo>();
    string mChangeRomName = string.Empty;
    public UniTimeSpan mTimeSpan;
    public bool bQuickTestRom = false;
    public string mQuickTestRom = string.Empty;
    public ReplayWriter mReplayWriter;
    public ReplayReader mReplayReader;
    public long currEmuFrame => emu.currEmuFrame;

    Dropdown optionDropdown;

    public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
    public static bool bInGame { get; private set; }


#if UNITY_EDITOR_WIN
    public static string EmuDataPath => "G:/MAME.Core";
#elif UNITY_ANDROID
    public static string EmuDataPath => Application.persistentDataPath;
#elif UNITY_PSP2
	public static string EmuDataPath => "ux0:data/MAME.Unity";
#else
    public static string EmuDataPath => Application.persistentDataPath;
#endif

    public static string RomPath => EmuDataPath + "/roms/";
    public static string SavePath => EmuDataPath + "/sav/";

    private void Awake()
    {
        // 强制横屏
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        instance = this;
        mFPS = GameObject.Find("FPS").GetComponent<Text>();
        optionDropdown = GameObject.Find("optionDropdown").GetComponent<Dropdown>();
        emu = new MAMEEmu();
        mUniLog = new UniLog();
        mUniMouse = this.gameObject.AddComponent<UniMouse>();
        mUniVideoPlayer = this.gameObject.AddComponent<UniVideoPlayer>();
        mUniSoundPlayer = GameObject.Find("Audio").transform.GetComponent<UniSoundPlayer>();
        mUniKeyboard = this.gameObject.AddComponent<UniKeyboard>();
        mUniResources = new UniResources();
        mChangeRomName = UniMAMESetting.instance.LastGameRom;
        mTimeSpan = new UniTimeSpan();

        emu.Init(RomPath, mUniLog, mUniResources, mUniVideoPlayer, mUniSoundPlayer, mUniKeyboard, mUniMouse, mTimeSpan);
        ALLGame = emu.GetGameList();

        Debug.Log($"ALLGame:{ALLGame.Count}");

#if !UNITY_EDITOR
        bQuickTestRom = false;
#endif

        if (bQuickTestRom)
            mChangeRomName = mQuickTestRom;

        GetHadRomList();

        if (bQuickTestRom)
            LoadGame();
    }

    void OnEnable()
    {
        btnStop.onClick.AddListener(StopGame);
        btnStart.onClick.AddListener(() => { LoadGame(false); });
        btnRePlay.onClick.AddListener(() => { LoadGame(true); });
        btnRePayySave.onClick.AddListener(() => SaveReplay());
        btnSaveState.onClick.AddListener(SaveState);
        btnLoadState.onClick.AddListener(LoadState);
    }

    void OnDisable()
    {
        StopGame();
    }

    void LoadGame(bool bReplay = false)
    {
        //Application.targetFrameRate = 60;

        mReplayWriter = new ReplayWriter(mChangeRomName, "fuck", ReplayData.ReplayFormat.FM32IP64, Encoding.UTF8);
        mChangeRomName = HadGameList[optionDropdown.value].Name;
        UniMAMESetting.instance.LastGameRom = mChangeRomName;
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
            Debug.Log($"_width->{_width}, _height->{_height}, _framePtr->{_framePtr}");
            mUniVideoPlayer.Initialize(_width, _height, _framePtr);
            //初始化音频
            mUniSoundPlayer.Initialize();
            //开始游戏
            emu.StartGame();
            bInGame = true;
        }
        else
        {
            Debug.Log($"ROM加载失败");
        }
    }

    void Update()
    {
        mFPS.text = ($"fpsv {mUniVideoPlayer.videoFPS.ToString("F2")} fpsa {mUniSoundPlayer.audioFPS.ToString("F2")}");

        if (!bInGame)
            return;

        //采集本帧Input
        mUniKeyboard.UpdateInputKey();
        //放行下一帧
        //emu.UnlockNextFreme();
        //推帧
        emu.UpdateFrame();

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SaveReplay();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            string Path = SavePath  + Machine.sName + ".rp";
            string dbgPath = SavePath  + Machine.sName + ".rpread";
            mReplayReader = new ReplayReader(Path, true, dbgPath);
            mUniKeyboard.Init(true);
        }
    }

    void SaveReplay()
    {
        string Path = SavePath + Machine.sName + ".rp";
        string dbgPath = SavePath  + Machine.sName + ".rpwrite";
        mReplayWriter.SaveData(Path, true, dbgPath);
    }

    void StopGame()
    {
        if (bInGame)
        {
            emu.StopGame();
            mUniVideoPlayer.StopVideo();
            mUniSoundPlayer.StopPlay();
            bInGame = false;
        }
    }

    void GetHadRomList()
    {
        HadGameList.Clear();
        optionDropdown.options.Clear();

        Debug.Log($"GetHadRomList:{RomPath}");
        string[] directoryEntries = Directory.GetDirectories(RomPath);
        for (int i = 0; i < directoryEntries.Length; i++)
        {
            string path = directoryEntries[i];
            string dirName = Path.GetFileName(path);
            if (ALLGame.ContainsKey(dirName))
            {
                HadGameList.Add(ALLGame[dirName]);
                optionDropdown.options.Add(new Dropdown.OptionData(dirName));
            }
        }
        Debug.Log($"HadGameList:{HadGameList.Count}");

        RomInfo tempCurrRom = HadGameList.Where(w => w.Name == mChangeRomName).FirstOrDefault();
        if (tempCurrRom != null)
        {
            optionDropdown.value = HadGameList.IndexOf(tempCurrRom);
        }
        else
        {
            optionDropdown.value = 0;
        }
        optionDropdown.RefreshShownValue();
    }

    void OpenFolderRomPath()
    {
        System.Diagnostics.Process.Start("explorer.exe", "/select," + RomPath);
    }

    void SaveState()
    {
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        FileStream fs = new FileStream(SavePath + Machine.sName + ".sta", FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        emu.SaveState(bw);
        bw.Close();
        fs.Close();

        byte[] screenData = UMAME.instance.mUniVideoPlayer.GetScreenImg();

        FileStream fsImg = new FileStream(SavePath + Machine.sName + ".jpg", FileMode.Create);
        fsImg.Write(screenData, 0, screenData.Length);
        fsImg.Close();
    }
    void LoadState()
    {
        string Path = SavePath + Machine.sName + ".sta";
        if (!File.Exists(Path))
        {
            Debug.Log($"文件不存在{Path}");
            return;
        }

        FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        emu.LoadState(br);
        br.Close();
        fs.Close();
    }
}
