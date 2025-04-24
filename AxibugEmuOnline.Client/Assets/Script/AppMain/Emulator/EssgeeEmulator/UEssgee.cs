using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using Essgee;
using Essgee.Emulation;
using Essgee.Emulation.Configuration;
using Essgee.EventArguments;
using Essgee.Exceptions;
using Essgee.Extensions;
using Essgee.Metadata;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UEssgee : MonoBehaviour, IEmuCore
{
    public static UEssgee instance;
    public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
    public static bool bInGame => instance?.emulatorHandler?.IsRunning == true ? true : false;

    public RomPlatformType Platform => mPlatform;

    public uint Frame => (uint)emulatorHandler.AxiEmuRunFrame;

    public Texture OutputPixel => graphicsHandler.rawBufferWarper;

    public RawImage DrawCanvas => graphicsHandler.DrawCanvas;

    public static bool bLogicUpdatePause { get; private set; }
    #region

    public UEGVideoPlayer graphicsHandler;
    UEGSoundPlayer soundHandler;
    GameMetadataHandler gameMetadataHandler;
    GameMetadata lastGameMetadata;
    Essgee.Emulation.EmulatorHandler emulatorHandler;
    UEGResources uegResources;
    UEGIO uegIO;
    UEGLog uegLog;
    UEGSaveByteConvert uegSaveByteConvert;
    private Canvas mCanvas;
    bool lastUserPauseState;
    double currentPixelAspectRatio;
    private UEGKeyboard mUniKeyboard;
    private RomPlatformType mPlatform = RomPlatformType.MasterSystem;

    #endregion

    void Awake()
    {
        App.tick.SetFrameRate(60);
        instance = this;
        uegResources = new UEGResources();
        uegIO = new UEGIO();
        uegLog = new UEGLog();
        uegSaveByteConvert = new UEGSaveByteConvert();
        mCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        mCanvas.worldCamera = Camera.main;
        //InitAll(uegResources, App.PersistentDataPath(mPlatform));
    }

    void OnDisable()
    {
        SaveConfiguration();
        Dispose(false);
    }


    #region EmuCore接入实现

    public object GetState()
    {
        return emulatorHandler.GetStateData();
    }

    public byte[] GetStateBytes()
    {
        return emulatorHandler.GetStateData();
    }

    public void LoadState(object state)
    {
        emulatorHandler.SetStateData((byte[])state);
    }

    public void LoadStateFromBytes(byte[] data)
    {
        emulatorHandler.SetStateData(data);
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

        InitAll(uegResources, uegIO, App.PersistentDataPath(mPlatform));

        bLogicUpdatePause = true;

        //保存当前正在进行的游戏存档
        if (emulatorHandler != null && !emulatorHandler.IsRunning)
        {
            emulatorHandler.SaveCartridge();
        }

        if (LoadAndRunCartridge(romFile.LocalFilePath))
            return true;
        else
            return "Rom加载失败";
    }

    public void Dispose()
    {
        if (!emulatorHandler.IsRunning)
        { 
            emulatorHandler.SaveCartridge();
        }
        ShutdownEmulation();
    }

    public void DoReset()
    {
        emulatorHandler.SaveCartridge();
        emulatorHandler.Reset();
    }

    public IControllerSetuper GetControllerSetuper()
    {
        return mUniKeyboard.ControllerMapper;
    }

    public bool PushEmulatorFrame()
    {
        if (!emulatorHandler.IsRunning) return false;
        if (!bLogicUpdatePause) return false;

        //采集本帧Input
        bool bhadNext = mUniKeyboard.SampleInput();
        //如果未收到Input数据,核心帧不推进
        if (!bhadNext) return false;

        emulatorHandler.Update_Frame();
        return true;
    }

    public void AfterPushFrame()
    {
    }

    public void GetAudioParams(out int frequency, out int channels)
    {
        frequency = soundHandler.sampleRate;
        channels = soundHandler.channle;
    }
    #endregion

    void InitAll(IGameMetaReources metaresources, IEssgeeIOSupport uegIO, string CustonDataDir)
    {
        //初始化配置
        InitAppEnvironment(CustonDataDir, uegIO);
        InitEmu();
        //细节初始化
        InitializeHandlers(metaresources);
    }

    private void InitAppEnvironment(string CustonDataDir, IEssgeeIOSupport uegIO)
    {
        EmulatorHandler.io = uegIO;
        EssgeeLogger.Init(uegLog);

        //EmuStandInfo.datDirectoryPath = Path.Combine(BaseDataDir, "EssgeeAssets", "No-Intro");
        //EmuStandInfo.metadataDatabaseFilePath = Path.Combine(BaseDataDir, "EssgeeAssets", "MetadataDatabase.json");

        EmuStandInfo.jsonConfigFileName = "Config.json";
        EmuStandInfo.saveDataDirectoryName = "Saves";
        EmuStandInfo.screenshotDirectoryName = "Screenshots";
        EmuStandInfo.saveStateDirectoryName = "Savestates";
        EmuStandInfo.extraDataDirectoryName = "Extras";
        EmuStandInfo.ProductName = "AxibugEmu";
        EmuStandInfo.ProductVersion = "";

        EmuStandInfo.programDataDirectory = System.IO.Path.Combine(CustonDataDir, EmuStandInfo.ProductName);
        EmuStandInfo.programConfigPath = System.IO.Path.Combine(EmuStandInfo.programDataDirectory, EmuStandInfo.jsonConfigFileName);

        EmuStandInfo.ShaderPath = System.IO.Path.Combine(CustonDataDir, "Assets", "Shaders");
        EmuStandInfo.SaveDataPath = System.IO.Path.Combine(EmuStandInfo.programDataDirectory, EmuStandInfo.saveDataDirectoryName);
        EmuStandInfo.ScreenshotPath = System.IO.Path.Combine(EmuStandInfo.programDataDirectory, EmuStandInfo.screenshotDirectoryName);
        EmuStandInfo.SaveStatePath = System.IO.Path.Combine(EmuStandInfo.programDataDirectory, EmuStandInfo.saveStateDirectoryName);
        EmuStandInfo.ExtraDataPath = System.IO.Path.Combine(EmuStandInfo.programDataDirectory, EmuStandInfo.extraDataDirectoryName);

        LoadConfiguration();


        if (!AxiIO.Directory.Exists(EmuStandInfo.SaveDataPath))
            AxiIO.Directory.CreateDirectory(EmuStandInfo.SaveDataPath);

        if (!AxiIO.Directory.Exists(EmuStandInfo.ScreenshotPath))
            AxiIO.Directory.CreateDirectory(EmuStandInfo.ScreenshotPath);

        if (!AxiIO.Directory.Exists(EmuStandInfo.SaveStatePath))
            AxiIO.Directory.CreateDirectory(EmuStandInfo.SaveStatePath);

        if (!AxiIO.Directory.Exists(EmuStandInfo.ExtraDataPath))
            AxiIO.Directory.CreateDirectory(EmuStandInfo.ExtraDataPath);

        if (AppEnvironment.EnableLogger)
        {
            //TODO 关闭Debug
            //Logger.Flush();
            //Logger.Close();
        }
    }

    void InitEmu()
    {
        //keysDown = new List<MotionKey>();
    }

    #region 细节初始化

    private void InitializeHandlers(IGameMetaReources metaresources)
    {
        InitializeOSDHandler();
        InitializeGraphicsHandler();
        InitializeSoundHandler();
        InitializeMetadataHandler(metaresources);

        mUniKeyboard = this.gameObject.AddComponent<UEGKeyboard>();
    }

    private void InitializeOSDHandler()
    {

        //var osdFontText = Assembly.GetExecutingAssembly().ReadEmbeddedImageFile($"{Application.ProductName}.Assets.OsdFont.png");
        //onScreenDisplayHandler = new OnScreenDisplayHandler(osdFontText);

        //onScreenDisplayHandler?.EnqueueMessageDebug($"Hello from {GetProductNameAndVersionString(true)}, this is a debug build!\nOSD handler initialized; font bitmap is {osdFontText.Width}x{osdFontText.Height}.");

        //if (onScreenDisplayHandler == null) throw new HandlerException("Failed to initialize OSD handler");
    }

    private void InitializeGraphicsHandler()
    {
        graphicsHandler = this.gameObject.GetComponent<UEGVideoPlayer>();
        //graphicsHandler = new GraphicsHandler(onScreenDisplayHandler);
        //graphicsHandler?.LoadShaderBundle(Program.Configuration.LastShader);
    }

    private void InitializeSoundHandler()
    {
        soundHandler = this.gameObject.GetComponent<UEGSoundPlayer>();
        //soundHandler = new SoundHandler(onScreenDisplayHandler, Program.Configuration.SampleRate, 2, ExceptionHandler);
        //soundHandler.SetVolume(Program.Configuration.Volume);
        //soundHandler.SetMute(Program.Configuration.Mute);
        //soundHandler.SetLowPassFilter(Program.Configuration.LowPassFilter);
        //soundHandler.Startup();
    }

    private void InitializeMetadataHandler(IGameMetaReources metaresources)
    {
        //gameMetadataHandler = new GameMetadataHandler(onScreenDisplayHandler);
        gameMetadataHandler = new GameMetadataHandler(metaresources);
    }
    #endregion
    void Dispose(bool disposing)
    {
        //TODO 释放时
        //if (disposing)
        //{
        //    if (components != null) components.Dispose();

        //    if (onScreenDisplayHandler != null) onScreenDisplayHandler.Dispose();
        //    if (graphicsHandler != null) graphicsHandler.Dispose();
        //    if (soundHandler != null) soundHandler.Dispose();
        //}

        //base.Dispose(disposing);
    }
    #region 配置
    private static void LoadConfiguration()
    {
        //TODO 暂时跳过这里的配置加载
        //Directory.CreateDirectory(EmuStandInfo.programDataDirectory);
        //if (!File.Exists(EmuStandInfo.programConfigPath) || (EmuStandInfo.Configuration = EmuStandInfo.programConfigPath.DeserializeFromFile<Configuration>()) == null)
        //{
        //    EmuStandInfo.Configuration = new Configuration();
        //    EmuStandInfo.Configuration.SerializeToFile(EmuStandInfo.programConfigPath);
        //}

        EmuStandInfo.Configuration = new Essgee.Configuration();

        List<Type> machineType = new List<Type>();
        machineType.Add(typeof(GameBoy));
        machineType.Add(typeof(GameBoyColor));
        machineType.Add(typeof(ColecoVision));
        machineType.Add(typeof(GameGear));
        machineType.Add(typeof(MasterSystem));
        machineType.Add(typeof(SC3000));
        machineType.Add(typeof(SG1000));

        //foreach (var machineConfigType in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IConfiguration).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
        foreach (var machineConfigType in machineType)
        {
            if (!EmuStandInfo.Configuration.Machines.ContainsKey(machineConfigType.Name))
                EmuStandInfo.Configuration.Machines.Add(machineConfigType.Name, (IConfiguration)Activator.CreateInstance(machineConfigType));
        }

        //foreach (var debuggerFormType in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IDebuggerForm).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
        //{
        //    if (!StandInfo.Configuration.DebugWindows.ContainsKey(debuggerFormType.Name))
        //        StandInfo.Configuration.DebugWindows.Add(debuggerFormType.Name, Point.Empty);
        //}
    }

    private void ApplyConfigOverrides(Type machineType)
    {
        var forcePowerOnWithoutCart = false;
        var hasTVStandardOverride = false;
        var hasRegionOverride = false;
        var hasDisallowMemoryControlOverride = false;

        var overrideConfig = EmuStandInfo.Configuration.Machines[machineType.Name].CloneObject();

        if (lastGameMetadata == null)
        {
            var property = overrideConfig.GetType().GetProperty("UseBootstrap");
            if (property != null && (bool)property.GetValue(overrideConfig) != true)
            {
                property.SetValue(overrideConfig, true);
                forcePowerOnWithoutCart = true;
            }
        }

        if (lastGameMetadata != null && lastGameMetadata.PreferredTVStandard != TVStandard.Auto)
        {
            var property = overrideConfig.GetType().GetProperty("TVStandard");
            if (property != null)
            {
                property.SetValue(overrideConfig, lastGameMetadata.PreferredTVStandard);
                hasTVStandardOverride = true;
            }
        }

        if (lastGameMetadata != null && lastGameMetadata.PreferredRegion != Essgee.Emulation.Region.Auto)
        {
            var property = overrideConfig.GetType().GetProperty("Region");
            if (property != null)
            {
                property.SetValue(overrideConfig, lastGameMetadata.PreferredRegion);
                hasRegionOverride = true;
            }
        }

        if (lastGameMetadata != null && lastGameMetadata.AllowMemoryControl != true)
        {
            var propertyMem = overrideConfig.GetType().GetProperty("AllowMemoryControl");
            if (propertyMem != null)
            {
                propertyMem.SetValue(overrideConfig, lastGameMetadata.AllowMemoryControl);
                hasDisallowMemoryControlOverride = true;

                var propertyBoot = overrideConfig.GetType().GetProperty("UseBootstrap");
                if (propertyBoot != null)
                {
                    propertyBoot.SetValue(overrideConfig, false);
                }
            }
        }

        if (forcePowerOnWithoutCart)
            EssgeeLogger.EnqueueMessageWarning("Bootstrap ROM is disabled in settings; enabling it for this startup.");

        if (hasTVStandardOverride)
            EssgeeLogger.EnqueueMessageWarning($"Overriding TV standard setting; running game as {lastGameMetadata?.PreferredTVStandard}.");

        if (hasRegionOverride)
            EssgeeLogger.EnqueueMessageWarning($"Overriding region setting; running game as {lastGameMetadata?.PreferredRegion}.");

        if (hasDisallowMemoryControlOverride)
            EssgeeLogger.EnqueueMessageWarning("Game-specific hack: Preventing software from reconfiguring memory control.\nBootstrap ROM has been disabled for this startup due to memory control hack.");

        if (forcePowerOnWithoutCart || hasTVStandardOverride || hasRegionOverride || hasDisallowMemoryControlOverride)
            emulatorHandler.SetConfiguration(overrideConfig);
    }
    public static void SaveConfiguration()
    {
        //不用保存这个配置
        //EmuStandInfo.Configuration.SerializeToFile(EmuStandInfo.programConfigPath);
    }
    #endregion

    #region 模拟器基本设置

    public void SetEmuFpsLimit(bool bOpen)
    {
        emulatorHandler?.SetFpsLimiting(bOpen);
    }
    public void SetSoundMute(bool bOpen)
    {
        //soundHandler?.SetMute(Program.Configuration.Mute);
    }
    public void SetSoundLowPassFilter(bool bOpen)
    {
        //soundHandler?.SetLowPassFilter(Program.Configuration.LowPassFilter);;
    }
    public void SetTemporaryPause(bool newTemporaryPauseState)
    {
        if (emulatorHandler == null || !emulatorHandler.IsRunning || !EmuStandInfo.Configuration.AutoPause) return;

        if (newTemporaryPauseState)
            emulatorHandler.Pause(true);
        else if (!lastUserPauseState)
            emulatorHandler.Pause(false);
    }
    #endregion

    #region 模拟器生命周期


    private void PowerOnWithoutCartridge(Type machineType)
    {
        //TODO IsRecording?? 可能需要实现
        //if (soundHandler.IsRecording)
        //    soundHandler.CancelRecording();

        InitializeEmulation(machineType);

        lastGameMetadata = null;

        ApplyConfigOverrides(machineType);


        emulatorHandler.Startup();

        EssgeeLogger.EnqueueMessageSuccess("Power on without cartridge.");
    }


    private bool LoadAndRunCartridge(string fileName)
    {
        try
        {

            var (machineType, romData) = CartridgeLoader.Load(fileName, "ROM image");

            //TODO IsRecording?? 可能需要实现
            //if (soundHandler.IsRecording)
            //    soundHandler.CancelRecording();


            InitializeEmulation(machineType);

            lastGameMetadata = gameMetadataHandler.GetGameMetadata(emulatorHandler.Information.DatFileName, fileName, Crc32.Calculate(romData), romData.Length);

            ApplyConfigOverrides(machineType);

            emulatorHandler.LoadCartridge(romData, lastGameMetadata);

            //AddToRecentFiles(fileName);
            //CreateRecentFilesMenu();
            //CreateLoadSaveStateMenus();
            //CreateToggleGraphicsLayersMenu();
            //CreateToggleSoundChannelsMenu();

            //takeScreenshotToolStripMenuItem.Enabled = pauseToolStripMenuItem.Enabled = resetToolStripMenuItem.Enabled = stopToolStripMenuItem.Enabled = true;
            //loadStateToolStripMenuItem.Enabled = saveStateToolStripMenuItem.Enabled = true;
            //startRecordingToolStripMenuItem.Enabled = true;
            //toggleLayersToolStripMenuItem.Enabled = enableChannelsToolStripMenuItem.Enabled = true;


            //初始化不同平台的按钮
            mUniKeyboard.Init(emulatorHandler.emulator);

            emulatorHandler.Startup();


            //初始化音频
            soundHandler.Initialize();

            //SizeAndPositionWindow();
            //SetWindowTitleAndStatus();

            EssgeeLogger.EnqueueMessage($"Loaded '{lastGameMetadata?.KnownName ?? "unrecognized game"}'.");

            return true;
        }
        catch (Exception ex) when (!AppEnvironment.DebugMode)
        {
            ExceptionHandler(ex);
            return false;
        }
    }
    private void InitializeEmulation(Type machineType)
    {
        if (emulatorHandler != null)
            ShutdownEmulation();

        emulatorHandler = new Essgee.Emulation.EmulatorHandler(machineType, ExceptionHandler, uegSaveByteConvert);
        emulatorHandler.Initialize();

        emulatorHandler.SendLogMessage += EmulatorHandler_SendLogMessage;
        emulatorHandler.EmulationReset += EmulatorHandler_EmulationReset;
        emulatorHandler.RenderScreen += EmulatorHandler_RenderScreen;
        emulatorHandler.SizeScreen += EmulatorHandler_SizeScreen;
        emulatorHandler.ChangeViewport += EmulatorHandler_ChangeViewport;
        emulatorHandler.PollInput += EmulatorHandler_PollInput;
        emulatorHandler.EnqueueSamples += EnqueueSoundSamples;
        emulatorHandler.SaveExtraData += EmulatorHandler_SaveExtraData;
        emulatorHandler.EnableRumble += EmulatorHandler_EnableRumble;
        emulatorHandler.PauseChanged += EmulatorHandler_PauseChanged;

        //emulatorHandler.EnqueueSamples += soundDebuggerForm.EnqueueSamples;

        emulatorHandler.SetFpsLimiting(EmuStandInfo.Configuration.LimitFps);

        emulatorHandler.SetConfiguration(EmuStandInfo.Configuration.Machines[machineType.Name]);

        currentPixelAspectRatio = emulatorHandler.Information.PixelAspectRatio;

        //pauseToolStripMenuItem.DataBindings.Clear();
        //pauseToolStripMenuItem.CheckedChanged += (s, e) =>
        //{
        //    var pauseState = (s as ToolStripMenuItem).Checked;

        //    emulatorHandler.Pause(pauseState);
        //    lastUserPauseState = pauseState;
        //};

        EssgeeLogger.EnqueueMessageSuccess($"{emulatorHandler.Information.Manufacturer} {emulatorHandler.Information.Model} emulation initialized.");
    }


    private void ExceptionHandler(Exception ex)
    {
        //this.CheckInvokeMethod(() =>
        //{
        if (!AppEnvironment.TemporaryDisableCustomExceptionForm)
        {
            //TODO debug窗口？
            //(_, ExceptionResult result, string prefix, string postfix) = ExceptionForm.GetExceptionInfo(ex);

            //if (result == ExceptionResult.Continue)
            //{
            //    //MessageBox.Show($"{prefix}{ex.InnerException?.Message ?? ex.Message}\n\n{postfix}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    EssgeeLogger.Err($"{prefix}{ex.InnerException?.Message ?? ex.Message}\n\n{postfix}.");
            //}
            //else
            //{
            //    //var exceptionForm = new ExceptionForm(ex) { Owner = this };
            //    //exceptionForm.ShowDialog();

            //    switch (result)
            //    {
            //        case ExceptionResult.StopEmulation:
            //            SignalStopEmulation();
            //            break;

            //        case ExceptionResult.ExitApplication:
            //            Environment.Exit(-1);
            //            break;
            //    }
            //}
        }
        else
        {
            var exceptionInfoBuilder = new StringBuilder();
            exceptionInfoBuilder.AppendLine($"Thread: {ex.Data["Thread"] ?? "<unnamed>"}");
            exceptionInfoBuilder.AppendLine($"Function: {ex.TargetSite.ReflectedType.FullName}.{ex.TargetSite.Name}");
            exceptionInfoBuilder.AppendLine($"Exception: {ex.GetType().Name}");
            exceptionInfoBuilder.Append($"Message: {ex.Message}");

            var isUnhandled = Convert.ToBoolean(ex.Data["IsUnhandled"]);

            if (!isUnhandled && ex is CartridgeLoaderException)
            {
                //MessageBox.Show($"{ex.InnerException?.Message ?? ex.Message}\n\nFailed to load cartridge.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EssgeeLogger.Err($"{ex.InnerException?.Message ?? ex.Message}\n\nFailed to load cartridge.");
            }
            else if (!isUnhandled && ex is EmulationException)
            {
                //MessageBox.Show($"An emulation exception has occured!\n\n{exceptionInfoBuilder.ToString()}\n\nEmulation cannot continue and will be terminated.", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EssgeeLogger.Err($"An emulation exception has occured!\n\n{exceptionInfoBuilder.ToString()}\n\nEmulation cannot continue and will be terminated.");
                SignalStopEmulation();
            }
            else
            {
                var errorBuilder = new StringBuilder();
                errorBuilder.AppendLine("An unhandled exception has occured!");
                errorBuilder.AppendLine();
                errorBuilder.AppendLine(exceptionInfoBuilder.ToString());
                errorBuilder.AppendLine();
                errorBuilder.AppendLine("Exception occured:");
                errorBuilder.AppendLine($"{ex.StackTrace}");
                errorBuilder.AppendLine();
                errorBuilder.AppendLine("Execution cannot continue and the application will be terminated.");

                //EssgeeLogger.Err(errorBuilder.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EssgeeLogger.Err(errorBuilder.ToString());

                Environment.Exit(-1);
            }
        }
        //});
    }



    private void SignalStopEmulation()
    {
        ShutdownEmulation();

        lastGameMetadata = null;

        //takeScreenshotToolStripMenuItem.Enabled = pauseToolStripMenuItem.Enabled = resetToolStripMenuItem.Enabled = stopToolStripMenuItem.Enabled = false;
        //loadStateToolStripMenuItem.Enabled = saveStateToolStripMenuItem.Enabled = false;
        //startRecordingToolStripMenuItem.Enabled = false;
        //toggleLayersToolStripMenuItem.Enabled = enableChannelsToolStripMenuItem.Enabled = false;

        //SetWindowTitleAndStatus();
    }

    private void ShutdownEmulation()
    {
        if (emulatorHandler == null) return;

        emulatorHandler.SaveCartridge();

        emulatorHandler.SendLogMessage -= EmulatorHandler_SendLogMessage;
        emulatorHandler.EmulationReset -= EmulatorHandler_EmulationReset;
        emulatorHandler.RenderScreen -= EmulatorHandler_RenderScreen;
        emulatorHandler.SizeScreen -= EmulatorHandler_SizeScreen;
        emulatorHandler.ChangeViewport -= EmulatorHandler_ChangeViewport;
        emulatorHandler.PollInput -= EmulatorHandler_PollInput;
        emulatorHandler.EnqueueSamples -= EnqueueSoundSamples;
        emulatorHandler.SaveExtraData -= EmulatorHandler_SaveExtraData;
        emulatorHandler.EnableRumble -= EmulatorHandler_EnableRumble;
        emulatorHandler.PauseChanged -= EmulatorHandler_PauseChanged;

        //emulatorHandler.EnqueueSamples -= soundDebuggerForm.EnqueueSamples;

        emulatorHandler.Shutdown();
        while (emulatorHandler.IsRunning) { }

        emulatorHandler = null;
        GC.Collect();

        EssgeeLogger.WriteLine("Emulation stopped.");
    }
    #endregion

    #region 模拟器内部事件

    private void EmulatorHandler_SendLogMessage(object sender, SendLogMessageEventArgs e)
    {
        //this.CheckInvokeMethod(delegate () { onScreenDisplayHandler.EnqueueMessageCore($"{emulatorHandler.Information.Model}: {e.Message}"); });
        //TODO log
        EssgeeLogger.EnqueueMessageSuccess($"{emulatorHandler.Information.Model}: {e.Message}");
    }

    private void EmulatorHandler_EmulationReset(object sender, EventArgs e)
    {
        //this.CheckInvokeMethod(delegate () { onScreenDisplayHandler.EnqueueMessage("Emulation reset."); });
        EssgeeLogger.EnqueueMessageSuccess("Emulation reset.");
    }

    private void EmulatorHandler_RenderScreen(object sender, RenderScreenEventArgs e)
    {
        //this.CheckInvokeMethod(delegate ()
        //{

        //if (e.Width != lastFramebufferSize.width || e.Height != lastFramebufferSize.height)
        //{
        //    lastFramebufferSize = (e.Width, e.Height);
        //    graphicsHandler?.SetTextureSize(e.Width, e.Height);
        //}
        //lastFramebufferData = e.FrameData;
        //graphicsHandler?.SetTextureData(e.FrameData);

        //graphicsHandler.SubmitVideo(e.Width, e.Height, e.FrameData, 0);
        graphicsHandler.SubmitVideo(e.Width, e.Height, e.FrameDataPtr, 0);

        // TODO: create emulation "EndOfFrame" event for this?
        //ControllerManager.Update();
        //});
    }

    private void EmulatorHandler_SizeScreen(object sender, SizeScreenEventArgs e)
    {
        //TODO 待实现 屏幕大小

        //this.CheckInvokeMethod(delegate ()
        //{
        //    lastFramebufferSize = (e.Width, e.Height);
        //    graphicsHandler?.SetTextureSize(e.Width, e.Height);
        //});
    }

    private void EmulatorHandler_ChangeViewport(object sender, ChangeViewportEventArgs e)
    {
        //TODO 待实现

        //this.CheckInvokeMethod(delegate ()
        //{
        //    graphicsHandler?.SetScreenViewport(currentViewport = e.Viewport);
        //    SizeAndPositionWindow();
        //});
    }

    private void EmulatorHandler_PollInput(object sender, PollInputEventArgs e)
    {
        //TODO Input实现

        //e.Keyboard = mUniKeyboard.mKeyCodeCore.GetPressedKeys();
        e.Keyboard.AddRange(mUniKeyboard.GetPressedKeys());
        e.MouseButtons = default;
        e.MousePosition = default;

        // TODO: rare, random, weird argument exceptions on e.Keyboard assignment; does this lock help??
        //lock (uiLock)
        //{
        //    e.Keyboard = new List<MotionKey>(keysDown);
        //    e.MouseButtons = mouseButtonsDown;

        //    var vx = (currentViewport.x - 50);
        //    var dvx = renderControl.ClientSize.Width / (currentViewport.width - (double)vx);
        //    var dvy = renderControl.ClientSize.Height / (currentViewport.height - (double)currentViewport.y);
        //    e.MousePosition = ((int)(mousePosition.x / dvx) - vx, (int)(mousePosition.y / dvy) - currentViewport.y);

        //    if (EmuStandInfo.Configuration.EnableXInput)
        //        e.ControllerState = ControllerManager.GetController(0).GetControllerState();
        //}
    }

    private void EmulatorHandler_SaveExtraData(object sender, SaveExtraDataEventArgs e)
    {
        /* Extract options etc. */
        var includeDateTime = e.Options.HasFlag(ExtraDataOptions.IncludeDateTime);
        var allowOverwrite = e.Options.HasFlag(ExtraDataOptions.AllowOverwrite);

        var extension = string.Empty;
        switch (e.DataType)
        {
            case ExtraDataTypes.Image: extension = "png"; break;
            case ExtraDataTypes.Raw: extension = "bin"; break;
            default: throw new EmulationException($"Unknown extra data type {e.DataType}");
        }

        /* Generate filename/path */
        var filePrefix = $"{System.IO.Path.GetFileNameWithoutExtension(lastGameMetadata.FileName)} ({e.Description}{(includeDateTime ? $" {DateTime.Now:yyyy-MM-dd HH-mm-ss})" : ")")}";
        var filePath = System.IO.Path.Combine(EmuStandInfo.ExtraDataPath, $"{filePrefix}.{extension}");
        if (!allowOverwrite)
        {
            var existingFiles = AxiIO.Directory.EnumerateFiles(EmuStandInfo.ExtraDataPath, $"{filePrefix}*{extension}");
            if (existingFiles.Contains(filePath))
                for (int i = 2; existingFiles.Contains(filePath = System.IO.Path.Combine(EmuStandInfo.ExtraDataPath, $"{filePrefix} ({i}).{extension}")); i++) { }
        }

        /* Handle data */
        //if (e.Data is Bitmap image)
        if (e.DataType == ExtraDataTypes.Image)
        {
            /* Images, ex. GB Printer printouts */
            //image.Save(filePath);

            //TODO 图像存储
        }
        else if (e.Data is byte[] raw)
        {
            /* Raw bytes */
            //using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    file.Write(raw, 0, raw.Length);
            //}

            AxiIO.File.WriteAllBytes(filePath, raw);
        }
    }

    private void EmulatorHandler_EnableRumble(object sender, EventArgs e)
    {
        //if (EmuStandInfo.Configuration.EnableXInput && EmuStandInfo.Configuration.EnableRumble)
        //    ControllerManager.GetController(0).Vibrate(0.0f, 0.5f, TimeSpan.FromSeconds(0.1f));
    }

    private void EmulatorHandler_PauseChanged(object sender, EventArgs e)
    {
        //SetWindowTitleAndStatus();

        if (emulatorHandler.IsPaused)
        {
            //TODO 音频暂停？
            //soundHandler?.ClearSampleBuffer();
        }
    }

    public unsafe void EnqueueSoundSamples(object sender, EnqueueSamplesEventArgs e)
    {
        //if (sampleQueue.Count > MaxQueueLength)
        //{
        //    var samplesToDrop = (sampleQueue.Count - MaxQueueLength);
        //    onScreenDisplayHandler.EnqueueMessageDebug($"({GetType().Name}/{DateTime.Now.Second:D2}s) Sample queue overflow; dropping {samplesToDrop} of {sampleQueue.Count} samples.");
        //    for (int i = 0; i < samplesToDrop; i++)
        //        if (sampleQueue.Count != 0)
        //            sampleQueue.Dequeue();
        //}

        //sampleQueue.Enqueue(e.MixedSamples.ToArray());

        //if (IsRecording)
        //{
        //    dataChunk.AddSampleData(e.MixedSamples);
        //    waveHeader.FileLength += (uint)e.MixedSamples.Length;
        //}

        //TODO 音频处理
        //soundHandler.SubmitSamples(e.MixedSamples, e.ChannelSamples, e.MixedSamples.Length);
        soundHandler.SubmitSamples(e.MixedSamples, e.ChannelSamples, e.MixedSamplesLength);
    }


    #endregion
}

