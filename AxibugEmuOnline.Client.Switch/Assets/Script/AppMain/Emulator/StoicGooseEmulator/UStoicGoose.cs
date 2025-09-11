using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using StoicGoose.Common.Utilities;
using StoicGoose.Core.Machines;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CartridgeMetadata = StoicGoose.Core.Cartridges.Metadata;

public class UStoicGoose : EmuCore<ulong>
{
    public static UStoicGoose instance;
    public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

    /* Constants */
    readonly static int maxScreenSizeFactor = 5;
    readonly static int maxRecentFiles = 15;
    readonly static int statusIconSize = 12;

    readonly static List<(string description, string extension, Func<string, System.IO.Stream> streamReadFunc)> supportedFileInformation = new()
        {
            ("WonderSwan ROMs", ".ws", GetStreamFromFile),
            ("WonderSwan Color ROMs", ".wsc", GetStreamFromFile),
            ("Zip Archives", ".zip", GetStreamFromFirstZippedFile)
        };

    /* Various handlers */
    DatabaseHandler databaseHandler = default;
    SGVideoPlayer graphicsHandler = default;
    SGSoundPlayer soundHandler = default;
    SGKeyboard inputHandler = default;
    SGLogger loggerHandler = default;
    public EmulatorHandler emulatorHandler = default;
    private RomPlatformType mPlatform = RomPlatformType.WonderSwan;

    /* Misc. windows */
    //SoundRecorderForm soundRecorderForm = default;
    //CheatsForm cheatsForm = default;

    /* Misc. runtime variables */
    Type machineType = default;
    bool isVerticalOrientation = false;
    string internalEepromPath = string.Empty;

    public string CurrRomName { get; private set; }

    #region 实现IEmuCore
    public override RomPlatformType Platform => mPlatform;

    public override uint Frame => (uint)emulatorHandler.AxiEmuRunFrame;

    public override Texture OutputPixel => graphicsHandler.rawBufferWarper;

    public override RawImage DrawCanvas => graphicsHandler.DrawCanvas;
    public override object GetState()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetStateBytes()
    {
        throw new NotImplementedException();
    }

    public override void LoadState(object state)
    {
        throw new NotImplementedException();
    }

    public override void LoadStateFromBytes(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override void Pause()
    {
        PauseEmulation();
    }

    public override void Resume()
    {
        UnpauseEmulation();
    }

    public override MsgBool StartGame(RomFile romFile)
    {
        mPlatform = romFile.Platform;

        Init();

        //保存当前正在进行的游戏存档
        if (emulatorHandler != null && !emulatorHandler.IsRunning)
        {
            SaveAllData();
        }

        if (LoadAndRunCartridge(romFile.LocalFilePath))
            return true;
        else
            return "Rom加载失败";
    }

    public override void Dispose()
    {
        //保存当前正在进行的游戏存档
        if (emulatorHandler != null && !emulatorHandler.IsRunning)
        {
            SaveAllData();
        }
        EmuClose();
    }

    public override void DoReset()
    {
        ResetEmulation();
    }

    public override IControllerSetuper GetControllerSetuper()
    {
        return inputHandler.ControllerMapper;
    }
    protected override bool OnPushEmulatorFrame(ulong InputData)
    {
        if (!emulatorHandler.IsRunning) return false;
        //if (!bLogicUpdatePause) return false;

        inputHandler.SetInputData(InputData);

        emulatorHandler.Frame_Update();
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
        return inputHandler.Update_InputData();
    }

    protected override void AfterPushFrame()
    {
    }

    public override void GetAudioParams(out int frequency, out int channels)
    {
        channels = soundHandler.channle;
        frequency = soundHandler.sampleRate;
    }
    #endregion

    //Cheat[] cheats = default;

    #region Unity 生命周期

    void Awake()
    {
        //关闭垂直同步
        QualitySettings.vSyncCount = 0;
        //设为60帧
        Application.targetFrameRate = 60;

        instance = this;
        loggerHandler = new SGLogger();
        graphicsHandler = this.gameObject.GetComponent<SGVideoPlayer>();
        soundHandler = this.gameObject.GetComponent<SGSoundPlayer>();
        inputHandler = this.gameObject.GetComponent<SGKeyboard>();
        Log.Initialize(loggerHandler);
        Program.InitPath(Application.persistentDataPath);
        //Init();
        //LoadAndRunCartridge("G:/BaiduNetdiskDownload/Rockman & Forte - Mirai Kara no Chousen Sha (J) [M][!].ws");
    }

    //依靠外部核心驱动
    //private void Update()
    //{
    //    if (!emulatorHandler.IsRunning)
    //        return;
    //    inputHandler.Update_InputData();
    //    emulatorHandler.Frame_Update();
    //}

    void OnDestroy()
    {
        EmuClose();
    }
    #endregion
    private void Init()
    {
        Log.WriteEvent(LogSeverity.Information, this, "Initializing emulator and UI...");

        machineType = Program.Configuration.General.PreferOriginalWS ? typeof(WonderSwan) : typeof(WonderSwanColor);

        InitializeEmulatorHandler();
        VerifyConfiguration();
        InitializeOtherHandlers();
        //InitializeWindows();

        //SizeAndPositionWindow();
        SetWindowTitleAndStatus();
        Log.WriteEvent(LogSeverity.Information, this, "Initialization done!");
    }


    private void EmuClose()
    {
        SaveAllData();
        emulatorHandler.Shutdown();

        Program.SaveConfiguration();
    }


    private void InitializeEmulatorHandler()
    {
        emulatorHandler = new EmulatorHandler(machineType);
        emulatorHandler.SetFpsLimiter(Program.Configuration.General.LimitFps);
    }

    private void VerifyConfiguration()
    {
        foreach (var button in emulatorHandler.Machine.GameControls.Replace(" ", "").Split(','))
        {
            if (!Program.Configuration.Input.GameControls.ContainsKey(button))
                Program.Configuration.Input.GameControls[button] = new();
        }

        foreach (var button in emulatorHandler.Machine.HardwareControls.Replace(" ", "").Split(','))
        {
            if (!Program.Configuration.Input.SystemControls.ContainsKey(button))
                Program.Configuration.Input.SystemControls[button] = new();
        }

        if (Program.Configuration.Video.ScreenSize < 2 || Program.Configuration.Video.ScreenSize > maxScreenSizeFactor)
            Program.Configuration.Video.ResetToDefault(nameof(Program.Configuration.Video.ScreenSize));

        //if (string.IsNullOrEmpty(Program.Configuration.Video.Shader) || (graphicsHandler != null && !graphicsHandler.AvailableShaders.Contains(Program.Configuration.Video.Shader)))
        //    Program.Configuration.Video.Shader = GraphicsHandler.DefaultShaderName;
    }

    private void InitializeOtherHandlers()
    {
        databaseHandler = new DatabaseHandler();

        //statusIconsLocation = machineType == typeof(WonderSwan) ? new(0, DisplayControllerCommon.ScreenHeight) : new(DisplayControllerCommon.ScreenWidth, 0);

        //TODO graphicsHandler基本参数，可能需要补上
        //graphicsHandler = new GraphicsHandler(machineType, new(emulatorHandler.Machine.ScreenWidth, emulatorHandler.Machine.ScreenHeight), statusIconsLocation, statusIconSize, machineType != typeof(WonderSwan), Program.Configuration.Video.Shader)
        //{
        //    IsVerticalOrientation = isVerticalOrientation
        //};

        //TODO 声音基本参数，可能需要补上
        //soundHandler = new SoundHandler(44100, 2);
        //soundHandler.SetVolume(1.0f);
        //soundHandler.SetMute(Program.Configuration.Sound.Mute);
        //soundHandler.SetLowPassFilter(Program.Configuration.Sound.LowPassFilter);

        //TODO Input基本参数，可能需要补上
        //inputHandler = new InputHandler(renderControl);
        //inputHandler.SetKeyMapping(Program.Configuration.Input.GameControls, Program.Configuration.Input.SystemControls);
        //inputHandler.SetVerticalOrientation(isVerticalOrientation);
        //inputHandler.SetEnableRemapping(Program.Configuration.Input.AutoRemap);
        //inputHandler.SetVerticalRemapping(emulatorHandler.Machine.VerticalControlRemap
        //    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        //    .Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        //    .ToDictionary(x => x[0], x => x[1]));


        emulatorHandler.Machine.DisplayController.SendFramebuffer = graphicsHandler.UpdateScreen;
        emulatorHandler.Machine.SoundController.SendSamples = (s) =>
        {
            soundHandler.EnqueueSamples(s);
            //soundRecorderForm.EnqueueSamples(s);
        };

        emulatorHandler.Machine.ReceiveInput += () =>
        {
            //var buttonsPressed = new List<string>();
            //var buttonsHeld = new List<string>();

            //inputHandler.PollInput(ref buttonsPressed, ref buttonsHeld);
            long buttonsHeld = 0;
            inputHandler.PollInput(ref buttonsHeld);
            return buttonsHeld;
            //if (buttonsPressed.Contains("Volume"))
            //    emulatorHandler.Machine.SoundController.ChangeMasterVolume();

            //return (buttonsPressed, buttonsHeld);
        };

        //renderControl.Resize += (s, e) => { if (s is Control control) graphicsHandler.Resize(control.ClientRectangle); };
        //renderControl.Paint += (s, e) =>
        //{
        //    graphicsHandler.SetClearColor(Color.Black);

        //    graphicsHandler.ClearFrame();

        //    if (emulatorHandler.Machine is MachineCommon machine)
        //    {
        //        var activeIcons = new List<string>() { "Power" };

        //        if (machine.BuiltInSelfTestOk) activeIcons.Add("Initialized");

        //        if (machine.DisplayController.IconSleep) activeIcons.Add("Sleep");
        //        if (machine.DisplayController.IconVertical) activeIcons.Add("Vertical");
        //        if (machine.DisplayController.IconHorizontal) activeIcons.Add("Horizontal");
        //        if (machine.DisplayController.IconAux1) activeIcons.Add("Aux1");
        //        if (machine.DisplayController.IconAux2) activeIcons.Add("Aux2");
        //        if (machine.DisplayController.IconAux3) activeIcons.Add("Aux3");

        //        if (machine.SoundController.HeadphonesConnected) activeIcons.Add("Headphones");
        //        if (machine.SoundController.MasterVolume == 0) activeIcons.Add("Volume0");
        //        if (machine.SoundController.MasterVolume == 1) activeIcons.Add("Volume1");
        //        if (machine.SoundController.MasterVolume == 2) activeIcons.Add("Volume2");
        //        if (machine.SoundController.MasterVolume == 3 && machine is WonderSwanColor) activeIcons.Add("Volume3");

        //        graphicsHandler.UpdateStatusIcons(activeIcons);
        //    }
        //    graphicsHandler.DrawFrame();
        //};

        internalEepromPath = System.IO.Path.Combine(Program.InternalDataPath, $"{machineType.Name}.eep");
    }

    //private void InitializeWindows()
    //{
    //    soundRecorderForm = new(soundHandler.SampleRate, soundHandler.NumChannels);
    //    cheatsForm = new()
    //    {
    //        Callback = (c) =>
    //        {
    //            if (emulatorHandler.IsRunning)
    //                cheats = (Cheat[])c.Clone();
    //        }
    //    };
    //}

    private void SizeAndPositionWindow()
    {
        graphicsHandler.SetSize(emulatorHandler.Machine.ScreenWidth, emulatorHandler.Machine.ScreenHeight);
        //if (WindowState == For emulatorHandler.Machine.ScreenHeight;mWindowState.Maximized)
        //    WindowState = FormWindowState.Normal;

        //MinimumSize = SizeFromClientSize(CalculateRequiredClientSize(2));
        //Size = SizeFromClientSize(CalculateRequiredClientSize(Program.Configuration.Video.ScreenSize));
        //var screen = Screen.FromControl(this);
        //var workingArea = screen.WorkingArea;
        //Location = new Point()
        //{
        //    X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - Width) / 2),
        //    Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - Height) / 2)
        //};
    }


    //TODO 设置屏幕宽高 看是否需要
    //private Size CalculateRequiredClientSize(int screenSize)
    //{
    //    if (emulatorHandler == null || graphicsHandler == null)
    //        return ClientSize;

    //    var statusIconsOnRight = statusIconsLocation.X > statusIconsLocation.Y;

    //    int screenWidth, screenHeight;

    //    if (!isVerticalOrientation)
    //    {
    //        screenWidth = emulatorHandler.Machine.ScreenWidth;
    //        screenHeight = emulatorHandler.Machine.ScreenHeight;
    //        if (statusIconsOnRight) screenWidth += statusIconSize;
    //        if (!statusIconsOnRight) screenHeight += statusIconSize;
    //    }
    //    else
    //    {
    //        screenWidth = emulatorHandler.Machine.ScreenHeight;
    //        screenHeight = emulatorHandler.Machine.ScreenWidth;
    //        if (!statusIconsOnRight) screenWidth += statusIconSize;
    //        if (statusIconsOnRight) screenHeight += statusIconSize;
    //    }

    //    return new(screenWidth * screenSize, (screenHeight * screenSize) + menuStrip.Height + statusStrip.Height);
    //}

    private void SetWindowTitleAndStatus()
    {
        //TODO 修改为状态字符串，显示在某个地方

        //var titleStringBuilder = new StringBuilder();

        //titleStringBuilder.Append($"{Application.ProductName} {Program.GetVersionString(false)}");

        //if (emulatorHandler.Machine.Cartridge.IsLoaded)
        //{
        //    titleStringBuilder.Append($" - [{Path.GetFileName(Program.Configuration.General.RecentFiles.First())}]");

        //    var statusStringBuilder = new StringBuilder();
        //    statusStringBuilder.Append($"Emulating {emulatorHandler.Machine.Manufacturer} {emulatorHandler.Machine.Model}, ");
        //    statusStringBuilder.Append($"playing {databaseHandler.GetGameTitle(emulatorHandler.Machine.Cartridge.Crc32, emulatorHandler.Machine.Cartridge.SizeInBytes)} ({emulatorHandler.Machine.Cartridge.Metadata.GameIdString})");

        //    tsslStatus.Text = statusStringBuilder.ToString();
        //    tsslEmulationStatus.Text = emulatorHandler.IsRunning ? (emulatorHandler.IsPaused ? "Paused" : "Running") : "Stopped";
        //}
        //else
        //{
        //    tsslStatus.Text = "Ready";
        //    tsslEmulationStatus.Text = "Stopped";
        //}

        //Text = titleStringBuilder.ToString();
    }

    private void LoadBootstrap(string filename)
    {
        if (GlobalVariables.EnableSkipBootstrapIfFound) return;

        if (!emulatorHandler.IsRunning)
        {
            if (AxiIO.File.Exists(filename))
            {
                //using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //var data = new byte[stream.Length];
                //stream.Read(data, 0, data.Length);

                var data = AxiIO.File.ReadAllBytes(filename);

                emulatorHandler.Machine.LoadBootstrap(data);
            }
            emulatorHandler.Machine.UseBootstrap = Program.Configuration.General.UseBootstrap;
        }
    }

    private void LoadInternalEeprom()
    {
        if (!emulatorHandler.IsRunning && AxiIO.File.Exists(internalEepromPath))
        {
            //using var stream = new FileStream(internalEepromPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //var data = new byte[stream.Length];
            //stream.Read(data, 0, data.Length);

            var data = AxiIO.File.ReadAllBytes(internalEepromPath);

            emulatorHandler.Machine.LoadInternalEeprom(data);
        }
    }

    private static System.IO.Stream GetStreamFromFile(string filename)
    {
        //return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] data = AxiIO.File.ReadAllBytes(filename);
        return new System.IO.MemoryStream(data);
    }

    private static System.IO.Stream GetStreamFromFirstZippedFile(string filename)
    {
        //return new ZipArchive(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).Entries.FirstOrDefault()?.Open();

        byte[] data = AxiIO.File.ReadAllBytes(filename);
        return new ZipArchive(new System.IO.MemoryStream(data)).Entries.FirstOrDefault()?.Open();
    }

    private bool LoadAndRunCartridge(string filename)
    {
        try
        {
            if (emulatorHandler.IsRunning)
            {
                SaveAllData();
                emulatorHandler.Shutdown();
            }

            using var inputStream = supportedFileInformation.FirstOrDefault(x => x.extension == System.IO.Path.GetExtension(filename)).streamReadFunc(filename) ?? GetStreamFromFile(filename);
            using var stream = new System.IO.MemoryStream();
            inputStream.CopyTo(stream);
            stream.Position = 0;

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            emulatorHandler.Machine.LoadRom(data);

            graphicsHandler.IsVerticalOrientation = isVerticalOrientation = emulatorHandler.Machine.Cartridge.Metadata.Orientation == CartridgeMetadata.Orientations.Vertical;
            inputHandler.SetVerticalOrientation(isVerticalOrientation);

            CurrRomName = System.IO.Path.GetFileName(filename);

            LoadRam();

            LoadBootstrap(emulatorHandler.Machine is WonderSwan ? Program.Configuration.General.BootstrapFile : Program.Configuration.General.BootstrapFileWSC);
            LoadInternalEeprom();

            //初始化音频
            soundHandler.Initialize();

            emulatorHandler.Startup();

            SizeAndPositionWindow();
            SetWindowTitleAndStatus();

            Program.SaveConfiguration();
            return true;
        }
        catch (Exception ex) when (!AppEnvironment.DebugMode)
        {
            App.log.Error("ex=>"+ex.ToString());
            return false;
        }
    }

    private void LoadRam()
    {
        //var path = Path.Combine(Program.SaveDataPath, $"{Path.GetFileNameWithoutExtension(Program.Configuration.General.RecentFiles.First())}.sav");
        var path = System.IO.Path.Combine(Program.SaveDataPath, $"{CurrRomName}.sav");
        //if (!File.Exists(path)) return;
        if (!AxiIO.File.Exists(path)) return;

        //using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //var data = new byte[stream.Length];
        //stream.Read(data, 0, data.Length);

        var data = AxiIO.File.ReadAllBytes(path);
        if (data.Length != 0)
            emulatorHandler.Machine.LoadSaveData(data);
    }

    private void SaveAllData()
    {
        SaveInternalEeprom();
        SaveRam();
    }

    private void SaveInternalEeprom()
    {
        var data = emulatorHandler.Machine.GetInternalEeprom();
        if (data.Length == 0) return;

        //using var stream = new FileStream(internalEepromPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        //stream.Write(data, 0, data.Length);

        AxiIO.File.WriteAllBytes(internalEepromPath, data);
    }

    private void SaveRam()
    {
        var data = emulatorHandler.Machine.GetSaveData();
        if (data.Length == 0) return;

        //var path = Path.Combine(Program.SaveDataPath, $"{Path.GetFileNameWithoutExtension(Program.Configuration.General.RecentFiles.First())}.sav");
        var path = System.IO.Path.Combine(Program.SaveDataPath, $"{CurrRomName}.sav");

        //using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        //stream.Write(data, 0, data.Length);

        AxiIO.File.WriteAllBytes(path, data);
    }


    private void PauseEmulation()
    {
        if (!emulatorHandler.IsRunning) return;

        emulatorHandler.Pause();
        soundHandler.Pause();

        SetWindowTitleAndStatus();
    }

    private void UnpauseEmulation()
    {
        if (!emulatorHandler.IsRunning) return;

        emulatorHandler.Unpause();
        soundHandler.Unpause();

        SetWindowTitleAndStatus();
    }

    private void ResetEmulation()
    {
        SaveAllData();
        emulatorHandler.Reset();

        Program.SaveConfiguration();
    }

}

static class Program
{
    static string jsonConfigFileName;//= "Config.json";
    static string logFileName;//= "Log.txt";
    static string internalDataDirectoryName;//= "Internal";
    static string saveDataDirectoryName;//= "Saves";
    static string cheatDataDirectoryName;//= "Cheats";
    static string debuggingDataDirectoryName;//= "Debugging";
    static string assetsDirectoryName;//= "Assets";
    static string shaderDirectoryName;//= "Shaders";
    static string noIntroDatDirectoryName;//= "No-Intro";
    static string mutexName;//= $"Unity_{GetVersionDetails()}";
    static string programDataDirectory;//= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.ProductName);
    static string programConfigPath;//= Path.Combine(programDataDirectory, jsonConfigFileName);

    public static Configuration Configuration;// { get; private set; } = LoadConfiguration(programConfigPath);

    public static string DataPath;//{ get; } = string.Empty;
    public static string InternalDataPath;//{ get; } = string.Empty;
    public static string SaveDataPath;//{ get; } = string.Empty;
    public static string CheatsDataPath;//{ get; } = string.Empty;
    public static string DebuggingDataPath;//{ get; } = string.Empty;

    static string programApplicationDirectory;// = AppDomain.CurrentDomain.BaseDirectory;
    static string programAssetsDirectory;// = Path.Combine(programApplicationDirectory, assetsDirectoryName);

    //public static string ShaderPath { get; } = string.Empty;
    public static string NoIntroDatPath;// { get; } = string.Empty;

    //static MainForm mainForm = default;

    public static void InitPath(string CustonDataDir)
    {
        try
        {
            jsonConfigFileName = "Config.json";
            logFileName = "Log.txt";
            internalDataDirectoryName = "Internal";
            saveDataDirectoryName = "Saves";
            cheatDataDirectoryName = "Cheats";
            debuggingDataDirectoryName = "Debugging";
            assetsDirectoryName = "Assets";
            shaderDirectoryName = "Shaders";
            noIntroDatDirectoryName = "No-Intro";
            mutexName = $"Unity_{GetVersionDetails()}";
            programDataDirectory = System.IO.Path.Combine(CustonDataDir, "AxibugEmu");
            programConfigPath = System.IO.Path.Combine(programDataDirectory, jsonConfigFileName);
            Configuration = LoadConfiguration(programConfigPath);
            Log.WriteLine(System.IO.Path.Combine(programDataDirectory, logFileName));
            programApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            programAssetsDirectory = System.IO.Path.Combine(programApplicationDirectory, assetsDirectoryName);
            AxiIO.Directory.CreateDirectory(DataPath = programDataDirectory);
            AxiIO.Directory.CreateDirectory(InternalDataPath = System.IO.Path.Combine(programDataDirectory, internalDataDirectoryName));
            AxiIO.Directory.CreateDirectory(SaveDataPath = System.IO.Path.Combine(programDataDirectory, saveDataDirectoryName));
            AxiIO.Directory.CreateDirectory(CheatsDataPath = System.IO.Path.Combine(programDataDirectory, cheatDataDirectoryName));
            AxiIO.Directory.CreateDirectory(DebuggingDataPath = System.IO.Path.Combine(programDataDirectory, debuggingDataDirectoryName));

            //if (!Directory.Exists(ShaderPath = Path.Combine(programAssetsDirectory, shaderDirectoryName)))
            //    throw new DirectoryNotFoundException("Shader directory missing");

            if (!AxiIO.Directory.Exists(NoIntroDatPath = System.IO.Path.Combine(programAssetsDirectory, noIntroDatDirectoryName)))
                throw new Exception("No-Intro .dat directory missing");
        }
        catch (Exception e)
        {
        }
    }

    //[STAThread]
    //static void Main()
    //{
    //    using var mutex = new Mutex(true, mutexName, out bool newInstance);
    //    if (!newInstance)
    //    {
    //        MessageBox.Show($"Another instance of {Application.ProductName} is already running.\n\nThis instance will now shut down.",
    //            $"{Application.ProductName} Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        Environment.Exit(-1);
    //    }

    //    Application.SetHighDpiMode(HighDpiMode.SystemAware);
    //    Application.EnableVisualStyles();
    //    Application.SetCompatibleTextRenderingDefault(false);

    //    if (!Debugger.IsAttached)
    //    {
    //        Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
    //        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    //        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    //    }

    //    Application.Run(mainForm = new MainForm());
    //}

    //static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    //{
    //    if (e.Exception is GLFWException glEx)
    //    {
    //        var renderControl = mainForm.Controls["renderControl"] as OpenGL.RenderControl;
    //        MessageBox.Show($"{glEx.Message.EnsureEndsWithPeriod()}\n\n{Application.ProductName} requires GPU and drivers supporting OpenGL {renderControl.APIVersion.Major}.{renderControl.APIVersion.Minor}.", $"{Application.ProductName} Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //    else
    //    {
    //        MessageBox.Show(e.Exception.Message.EnsureEndsWithPeriod(), $"{Application.ProductName} Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }

    //    Environment.Exit(-1);
    //}

    //static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    //{
    //    MessageBox.Show((e.ExceptionObject as Exception).Message, $"{Application.ProductName} Startup Error");
    //    Environment.Exit(-1);
    //}

    private static Configuration LoadConfiguration(string filename)
    {
        AxiIO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));

        Configuration configuration;
        //if (!File.Exists(filename) || (configuration = filename.DeserializeFromFile<Configuration>()) == null)
        //{
        //    configuration = new Configuration();
        //    configuration.SerializeToFile(filename);
        //}

        configuration = new Configuration();

        return configuration;
    }

    public static void ReplaceConfiguration(Configuration newConfig)
    {
        ConfigurationBase.CopyConfiguration(newConfig, Configuration);
        SaveConfiguration();
    }

    public static void SaveConfiguration()
    {
        //Configuration?.SerializeToFile(programConfigPath);
    }

    private static string GetVersionDetails()
    {
        //return $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}{(ThisAssembly.Git.IsDirty ? "-dirty" : string.Empty)}{(GlobalVariables.IsDebugBuild ? "+debug" : string.Empty)}";
        return $"{(GlobalVariables.IsDebugBuild ? "+debug" : string.Empty)}";
    }

    //public static string GetVersionString(bool detailed)
    //{
    //    var version = new Version(Application.ProductVersion);
    //    var stringBuilder = new StringBuilder();
    //    stringBuilder.Append($"v{version.Major:D3}{(version.Minor != 0 ? $".{version.Minor}" : string.Empty)}");
    //    if (detailed) stringBuilder.Append($" ({GetVersionDetails()})");
    //    return stringBuilder.ToString();
    //}
}

