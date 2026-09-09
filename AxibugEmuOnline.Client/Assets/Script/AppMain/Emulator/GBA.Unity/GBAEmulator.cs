using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using ICSharpCode.SharpZipLib.Zip;
using OptimeGBA;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class GBAEmulator : EmuCore<GBAInputState>, AxiGbaIO
    {
        public static GBAEmulator instance;
        const int FrameCycles = 70224 * 4;
        const int ScanlineCycles = 1232;
        const float FrameRate = 59.7275f;
        //static bool SyncToAudio = true;

        //public Renderer screenRenderer;
        public GBAVideoProvider main_videoProvider;
        public GBAAudioProvider main_audioProvider;
        public GBAInputProvider main_inputProvider;
        public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        public bool ShowBackBuf = false;
        //public bool RunEmulator;
        public bool EnableAudio;
        public bool BootBIOS = false;

        public Gba main_gba;
        public List<GBAEmuLiteProcess> other_process = new List<GBAEmuLiteProcess>();
        public GbaLink gbaLink;
        public Gba[] GbaInstSlot;
        bool[] GbaInstSlot_LiteMode;
        public int MainGbaIdx;

        public RawImage[] OtherGbaSreen;
#if UNITY_EDITOR
        private bool IsEditorTestOther = false;
        private int EditorTestOtherCount = 1;
#endif
        public static bool bLogicUpdatePause { get; private set; }
        //Thread EmulationThread;
        //AutoResetEvent ThreadSync = new AutoResetEvent(false);


        public bool RomLoaded { get; private set; } = false;

        GBAGameStartInfo _single_start_gbastartInfo;
        #region 子类实现

        public override RomPlatformType Platform => RomPlatformType.GameBoyAdvance;

        public override uint PushFrame => AxiEmuRunFrame;

        public override uint PhysicsFrame => AxiVirtualFrame;

        public override Texture OutputPixel => main_videoProvider.wrapTex;

        public override RawImage DrawCanvas => main_videoProvider.m_drawCanvas;

        public override Vector3 DrawCanvas_SrcRot => main_videoProvider.srcCanvasLocalEulerAngles;

        public override Vector3 DrawLocalScale => new Vector3(1, -1, 1);

        protected override GBAInputState GetLocalInput()
        {
            switch (MainGbaIdx)
            {
                case 0: return new GBAInputState((ushort)main_inputProvider.DoLocalPressedKeys(), 0, 0, 0);
                case 1: return new GBAInputState(0, (ushort)main_inputProvider.DoLocalPressedKeys(), 0, 0);
                case 2: return new GBAInputState(0, 0, (ushort)main_inputProvider.DoLocalPressedKeys(), 0);
                case 3: return new GBAInputState(0, 0, 0, (ushort)main_inputProvider.DoLocalPressedKeys());
                default: return new GBAInputState(0);
            }
        }

        protected override GBAInputState ConvertInputDataFromNet(ReplayStep step)
        {
            return new GBAInputState(step.InPut);
        }

        protected override ulong InputDataToNet(GBAInputState inputData)
        {
            return inputData.Value;
        }

        protected override bool OnPushEmulatorFrame(GBAInputState InputData, bool isSkipFrame)
        {
            if (!bLogicUpdatePause) return false;

            ////TODO联机时区分
            //main_inputProvider.SetCurrKeyArr(InputData);
            //for (int i = 0; i < other_process.Count; i++)
            //    other_process[i].SetCurrKeyArr(other_process[i].DoLocalInputToTest());


            for (int i = 0; i < GbaInstSlot.Length; i++)
            {
                switch (i)
                {
                    case 0: main_inputProvider.SetCurrKeyArr(GbaInstSlot[i], (GBAKeyCode)InputData.P1); break;
                    case 1: main_inputProvider.SetCurrKeyArr(GbaInstSlot[i], (GBAKeyCode)InputData.P2); break;
                    case 2: main_inputProvider.SetCurrKeyArr(GbaInstSlot[i], (GBAKeyCode)InputData.P3); break;
                    case 3: main_inputProvider.SetCurrKeyArr(GbaInstSlot[i], (GBAKeyCode)InputData.P4); break;
                }
            }

#if UNITY_EDITOR
            if (IsEditorTestOther)
            {
                foreach (var ogba in other_process)
                {
                    main_inputProvider.SetCurrKeyArr(ogba.gba, ogba.DoLocalInputToTest());
                }
            }
#endif

            DoUpdate(isSkipFrame);
            return true;
        }

        public override object GetState()
        {
            OverlayManager.PopTip("暂不支持，即时存档");
            throw new NotImplementedException();
        }

        public override byte[] GetStateBytes()
        {
            OverlayManager.PopTip("暂不支持，即时存档");
            throw new NotImplementedException();
        }

        public override void LoadState(object state)
        {
            OverlayManager.PopTip("暂不支持，即时存档");
        }

        public override void LoadStateFromBytes(byte[] data)
        {
            OverlayManager.PopTip("暂不支持，即时存档");
        }

        static byte[] emptyArr = new byte[0];
        public override byte[] GetGameSaveDataBytes()
        {
            byte[] result;
            if (cache_had_savedata)
            {
                result = cache_savdata;
            }
            else
            {
                if (!AxiIO.File.Exists(cache_targetpath))
                    result = null;
                else
                {
                    result = AxiIO.File.ReadAllBytes(cache_targetpath);
                }
            }
            if (result == null)
                return emptyArr;
            else
                return result;
        }

        public void LoadGameSavDataForMulti(GBAGameStartInfo startInfo, List<byte[]> savDataList)
        {
            RomFile romFile = startInfo.romFile;
            SetGBALink(romFile, startInfo.PlayerCount, startInfo.MineIdx, savDataList);
        }

        public override void Pause()
        {
            bLogicUpdatePause = false;
        }
        public override void Resume()
        {
            bLogicUpdatePause = true;
        }
        public override MsgBool StartGame(GameStartInfo startInfo)
        {
            _single_start_gbastartInfo = (GBAGameStartInfo)startInfo;
            RomFile romFile = startInfo.romFile;
            string path = romFile.LocalProxyPath;
            string savpath = GetGameSavPath(romFile.LocalProxyFileName);
            byte[] savdata = null;
            if (AxiIO.File.Exists(savpath))
                savdata = AxiIO.File.ReadAllBytes(savpath);
            try
            {
#if UNITY_EDITOR
                if (IsEditorTestOther)
                {

                    var testsavlist = new List<byte[]>();
                    for (int i = 0; i < EditorTestOtherCount + 1; i++)
                    {
                        testsavlist.Add(savdata);
                    }
                    //测试多人
                    SetGBALink(romFile, EditorTestOtherCount + 1, 0, testsavlist);
                    bLogicUpdatePause = true;
                    return true;
                }
#endif
                //单人启动
                SetGBALink(romFile, 1, 0, new List<byte[]> { savdata });
                bLogicUpdatePause = true;
                return true;
            }
            catch (Exception e)
            {
                bLogicUpdatePause = false;
                return "失败";
            }
        }

        public override void Dispose()
        {
            _single_start_gbastartInfo = null;
        }

        public override void DoReset()
        {
            ResetGba();
        }

        public override IControllerSetuper GetControllerSetuper()
        {
            return main_inputProvider.ControllerMapper;
        }

        protected override void AfterPushFrame()
        {

        }

        public override void GetAudioParams(out int frequency, out int channels)
        {
            frequency = main_audioProvider.SampleRate;
            channels = main_audioProvider.Channels;
        }
        #endregion

        private void Awake()
        {
            instance = this;
            var mCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            mCanvas.worldCamera = Camera.main;
            App.tick.SetFrameRate();
            var audioConfig = AudioSettings.GetConfiguration();
            audioConfig.sampleRate = GbaAudio.SampleRate;
            AudioSettings.Reset(audioConfig);
        }
        void Start()
        {

        }

        private void OnEnable()
        {
            //EmulationThread = new Thread(EmulationThreadHandler);
            //EmulationThread.Name = "Emulation Core";
            //EmulationThread.Start();

            //btnStart.onClick.AddListener(
            //    () =>
            //    {
            //        byte[] romdata = Resources.Load<TextAsset>("mario_world.gba").bytes;
            //        LoadRom(romdata, "mario_world.gba");
            //    }
            //    );
        }
        private void OnDisable()
        {
            //EmulationThread.Abort();
            Update_CheckSave(true);
            ReleaseCache();
        }
        void SetGBALink(RomFile romFile, int playerCount, int mineIdx, List<byte[]> savDataList)
        {
            GbaInstSlot = new Gba[playerCount];
            GbaInstSlot_LiteMode = new bool[playerCount];
            gbaLink = null;
            MainGbaIdx = mineIdx;
            other_process.Clear();

            for (int i = 0; i < OtherGbaSreen.Length; i++)
            {
                OtherGbaSreen[i]?.gameObject.SetActive(false);
            }

            string path = romFile.LocalProxyPath;
            byte[] romdata = GetBytesZippedFile(path);
            if (playerCount == 1)//单人
            {
                LoadRom(romdata, savDataList[0], romFile.LocalProxyFileName);
                GbaInstSlot[0] = main_gba;
                main_gba.SetLiteMode(false);
                GbaInstSlot_LiteMode[0] = false;
            }
            else//多人，对号入座
            {
                for (int i = 0; i < playerCount; i++)
                {
                    if (mineIdx == i)
                    {
                        LoadRom(romdata, savDataList[i], romFile.LocalProxyFileName);
                        GbaInstSlot[i] = main_gba;
                        main_gba.SetLiteMode(false);
                        GbaInstSlot_LiteMode[i] = false;
                        App.log.Info($"[GBA]多实例槽位[{i}],本地玩家");
                    }
                    else
                    {
                        GBAEmuLiteProcess otherGba = new GBAEmuLiteProcess();
                        other_process.Add(otherGba);
                        otherGba.VideoInit(OtherGbaSreen[other_process.Count - 1]);
                        otherGba.LoadRom(romdata, savDataList[i]);
                        otherGba.gba.SetLiteMode(true);
                        GbaInstSlot_LiteMode[i] = true;
                        OtherGbaSreen[other_process.Count - 1]?.gameObject.SetActive(otherGba.gba.EnablePpuRender);
                        GbaInstSlot[i] = otherGba.gba;
                        App.log.Info($"[GBA]多实例槽位[{i}],虚拟玩家");
                    }
                }
                gbaLink = new GbaLink(
                    GbaInstSlot[0],
                    GbaInstSlot[1],
                    GbaInstSlot.Length >= 3 ? GbaInstSlot[2] : null,
                    GbaInstSlot.Length == 4 ? GbaInstSlot[3] : null);
            }

            AxiEmuRunFrame = 0;
            AxiVirtualFrame = 0;
            accumulatedUs = 0;
            CurrVirtualFrameIsSkim = false;

            bLogicUpdatePause = true;
        }


        // Update is called once per frame
        void DoUpdate(bool isSkipFrame)
        {
            if (RomLoaded && !isSkipFrame)
            {
                main_videoProvider.OnRenderFrame();
                for (int i = 0; i < other_process.Count; i++)
                {
                    other_process[i].OnRenderFrame();
                }
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                App.log.Info("SetLiteMode => " + main_gba.EnablePpuRender);
                main_gba.SetLiteMode(main_gba.EnablePpuRender);
            }
#endif

            //根据跳帧，如果是跳帧的帧，则用轻量级运行
            for (int i = 0; i < GbaInstSlot_LiteMode.Length; i++)
            {
                if (isSkipFrame)
                    GbaInstSlot[i].SetLiteMode(true);
                else
                    GbaInstSlot[i].SetLiteMode(GbaInstSlot_LiteMode[i]);
            }


            Frame_UpdateByCpuTime();
            //UpdateFrameOneByOne();
        }

        string GetGameSavPath(string name)
        {
            string savPath = App.PersistentDataPath(this.Platform) + "/SavSata/" + name.Substring(0, name.Length - 3) + "sav";
            return savPath;
        }

        public void LoadRom(byte[] rom, byte[] savdata, string name)
        {
            string savPath = GetGameSavPath(name);
            LoadSaveFileToCache(savdata, savPath);
            byte[] sav;
            if (cache_had_savedata)
            {
                sav = cache_savdata;
                App.log.Info($"savdata exists, loaded");
            }
            else
            {
                sav = new byte[0];
                App.log.Info(".sav not available");
            }

            LoadRomAndSave(rom, sav, savPath);
            App.log.Info("Load Rom Success");
            main_audioProvider.Initialize();
            RomLoaded = true;
            //RunEmulator = true;
        }

        public void LoadRomAndSave(byte[] rom, byte[] sav, string savPath)
        {
            byte[] bios = Resources.Load<TextAsset>("GBA.Unity/gba_bios.bin").bytes;
            //byte[] bios = BetterStreamingAssets.ReadAllBytes("gba_bios.bin");
            App.log.Debug(bios.Length.ToString());
            main_gba = new Gba(new ProviderGba(bios, rom, savPath, main_audioProvider.AudioReady, this) { BootBios = BootBIOS });
            main_gba.Mem.SaveProvider.LoadSave(sav);

        }

        public void ResetGba()
        {
            DumpSavDataToReady();
            Update_CheckSave(true);
            StartGame(_single_start_gbastartInfo);
        }

        //public void EmulationThreadHandler()
        //{
        //    while (true)
        //    {
        //        ThreadSync.WaitOne();

        //        int cyclesLeft = 70224 * 4;
        //        while (cyclesLeft > 0 && !gba.Cpu.Errored)
        //        {
        //            cyclesLeft -= (int)gba.Step();
        //        }

        //        while (!SyncToAudio && !gba.Cpu.Errored && RunEmulator)
        //        {
        //            gba.Step();
        //        }
        //    }
        //}

        //public int GetOutputSampleRate()
        //{
        //    return AudioSettings.outputSampleRate;
        //}

        //public int GetSamplesAvailable()
        //{
        //    return _samplesAvailable;
        //}

        //private void OnAudioFilterRead(float[] data, int channels)
        //{
        //    if (!EnableAudio) return;

        //    int r = _pipeStream.Read(_buffer, 0, data.Length * sizeof(float));
        //    float[] pcm = CoreUtil.ByteToFloatArray(_buffer);
        //    Array.Copy(pcm, data, data.Length);
        //}



        //public void RunCycles(int cycles)
        //{
        //    while (cycles > 0 && !gba.Cpu.Errored && RunEmulator)
        //    {
        //        cycles -= (int)gba.Step();
        //    }
        //}

        int CyclesLeft;
        public void RunFrame()
        {
            CyclesLeft += FrameCycles;
            while (CyclesLeft > 0 && !main_gba.Cpu.Errored)
            {
                CyclesLeft -= (int)main_gba.Step();
            }
        }

        public void RunScanline()
        {
            CyclesLeft += ScanlineCycles;
            while (CyclesLeft > 0 && !main_gba.Cpu.Errored)
            {
                CyclesLeft -= (int)main_gba.Step();
            }
        }

        #region 插帧处理


        long accumulatedUs = 0;
        long unityFrameUs = 16_666; // 60Hz = 16.6667ms

        public uint AxiEmuRunFrame;
        public uint AxiVirtualFrame;
        /// <summary>
        /// 当前当前虚拟帧是否快速掠过
        /// </summary>
        public bool CurrVirtualFrameIsSkim = false;
        public static class GBAConstants
        {
            // 16.78MHz
            public const int MASTER_CLOCK = 16_780_000;
            // 16.743ms = 13259us
            public const long FRAME_TIME_US = 16_743;
        }
        public void Frame_UpdateByCpuTime()
        {
            accumulatedUs += unityFrameUs;

            int runStep = 0;

            while (accumulatedUs >= GBAConstants.FRAME_TIME_US)
            {
                accumulatedUs -= GBAConstants.FRAME_TIME_US;
                runStep++;
            }

            for (int i = 0; i < runStep; i++)
            {
                CurrVirtualFrameIsSkim = i != runStep - 1;
                RunSingleFrame();
                AxiVirtualFrame++;
            }
            AxiEmuRunFrame++;
        }
        #endregion


        public void UpdateFrameOneByOne()
        {
            //SyncToAudio = !(Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Space));
            RunSingleFrame();
            AxiVirtualFrame++;
            AxiEmuRunFrame++;
        }

        //void RunSingleFrame()
        //{
        //    int cyclesLeft_main = 70224 * 4;
        //    int cyclesLeft_gba_2 = other_process.Count >= 1 ? 70224 * 4 : 0;
        //    int cyclesLeft_gba_3 = other_process.Count >= 2 ? 70224 * 4 : 0;
        //    int cyclesLeft_gba_4 = other_process.Count >= 3 ? 70224 * 4 : 0;

        //    while (cyclesLeft_main > 0 ||
        //        cyclesLeft_gba_2 > 0 ||
        //        cyclesLeft_gba_3 > 0 ||
        //        cyclesLeft_gba_4 > 0)
        //    {
        //        if (cyclesLeft_main > 0 && !main_gba.Cpu.Errored)
        //            cyclesLeft_main -= (int)main_gba.Step();
        //        if (cyclesLeft_gba_2 > 0 && !other_process[0].gba.Cpu.Errored)
        //            cyclesLeft_gba_2 -= (int)other_process[0].gba.Step();
        //        if (cyclesLeft_gba_3 > 0 && !other_process[1].gba.Cpu.Errored)
        //            cyclesLeft_gba_3 -= (int)other_process[1].gba.Step();
        //        if (cyclesLeft_gba_4 > 0 && !other_process[2].gba.Cpu.Errored)
        //            cyclesLeft_gba_4 -= (int)other_process[2].gba.Step();
        //    }

        //    if (main_gba.Mem.SaveProvider.Dirty)
        //    {
        //        DumpSavDataToReady();
        //        //清理脏标记，否则一直保存
        //        main_gba.Mem.SaveProvider.Dirty = false;
        //    }

        //    for (int i = 0; i < other_process.Count; i++)
        //    {
        //        if (other_process[i].gba.Mem.SaveProvider.Dirty)
        //        {
        //            other_process[i].DumpSavDataToReady();
        //            //清理脏标记，否则一直保存
        //            other_process[i].gba.Mem.SaveProvider.Dirty = false;
        //        }
        //    }

        //    Update_CheckSave();
        //}

#if FALSE && UNITY_SWITCH //暂时Switch只有用这个模式，才能慢60帧，但是这样高要求的联机游戏会有问题（如，马里奥水管工附赠游戏没问题，但是马里奥赛车，星之卡比梦之泉不行）
        void RunSingleFrame()
        {
            Gba[] cores = GbaInstSlot;
            int cyclesLeft_1 = 70224 * 4;
            int cyclesLeft_2 = cores.Length > 1 ? 70224 * 4 : 0;
            int cyclesLeft_3 = cores.Length > 2 ? 70224 * 4 : 0;
            int cyclesLeft_4 = cores.Length > 3 ? 70224 * 4 : 0;

            while (cyclesLeft_1 > 0 ||
                cyclesLeft_2 > 0 ||
                cyclesLeft_3 > 0 ||
                cyclesLeft_4 > 0)
            {
                if (cyclesLeft_1 > 0 && !cores[0].Cpu.Errored)
                    cyclesLeft_1 -= (int)cores[0].StateStepUntil();
                if (cyclesLeft_2 > 0 && !cores[1].Cpu.Errored)
                    cyclesLeft_2 -= (int)cores[1].StateStepUntil();
                if (cyclesLeft_3 > 0 && !cores[2].Cpu.Errored)
                    cyclesLeft_3 -= (int)cores[2].StateStepUntil();
                if (cyclesLeft_4 > 0 && !cores[3].Cpu.Errored)
                    cyclesLeft_4 -= (int)cores[3].StateStepUntil();
            }

            if (main_gba.Mem.SaveProvider.Dirty)
            {
                DumpSavDataToReady();
                //清理脏标记，否则一直保存
                main_gba.Mem.SaveProvider.Dirty = false;
            }

            for (int i = 0; i < other_process.Count; i++)
            {
                if (other_process[i].gba.Mem.SaveProvider.Dirty)
                {
                    other_process[i].DumpSavDataToReady();
                    //清理脏标记，否则一直保存
                    other_process[i].gba.Mem.SaveProvider.Dirty = false;
                }
            }

            Update_CheckSave();
        }
#elif FALSE
        const int CyclesPerFrame = 70224 * 4;
        //const int Slice = 64; // 可试 32~256
        const int Slice = 4096 * 2; // 可试 32~256
        //4096 * 4 XX

        void RunSingleFrame()
        {
            // 收集本帧要跑的核心
            // index0 = main，其余 other_process
            //int coreCount = 1 + other_process.Count;
            //Gba[] cores = new Gba[coreCount];
            //cores[0] = main_gba;
            //for (int i = 0; i < other_process.Count; i++)
            //    cores[i + 1] = other_process[i].gba;

            // 使用现在的实例数组，作为要跑的核心
            Gba[] cores = GbaInstSlot;
            int coreCount = GbaInstSlot.Length;

            // 以当前最慢核为基准，对齐到同一绝对时间轴
            long baseTick = cores[0].Scheduler.CurrentTicks;
            for (int i = 1; i < coreCount; i++)
            {
                if (cores[i].Scheduler.CurrentTicks < baseTick)
                    baseTick = cores[i].Scheduler.CurrentTicks;
            }

            long frameEnd = baseTick + CyclesPerFrame;

            while (true)
            {
                // 是否都已到达 frameEnd
                bool allDone = true;
                for (int i = 0; i < coreCount; i++)
                {
                    if (!cores[i].Cpu.Errored && cores[i].Scheduler.CurrentTicks < frameEnd)
                    {
                        allDone = false;
                        break;
                    }
                }
                if (allDone) break;

                // 目标：全体推到 min(当前最小 + Slice, frameEnd)
                long minTick = long.MaxValue;
                for (int i = 0; i < coreCount; i++)
                {
                    if (cores[i].Cpu.Errored) continue;
                    long t = cores[i].Scheduler.CurrentTicks;
                    if (t < minTick) minTick = t;
                }

                long target = minTick + Slice;
                if (target > frameEnd) target = frameEnd;

                for (int i = 0; i < coreCount; i++)
                {
                    var gba = cores[i];
                    if (gba.Cpu.Errored) continue;
                    if (gba.Scheduler.CurrentTicks >= frameEnd) continue;

                    gba.TickLimit = target;
                    // 可能需多次 StateStepUntil 才能到 target（中间有 event）
                    int guard = 0;
                    while (gba.Scheduler.CurrentTicks < target && !gba.Cpu.Errored && guard++ < 100000)
                    {
                        gba.TickLimit = target;
                        uint advanced = gba.StateStepUntil();
                        if (advanced == 0 && gba.Scheduler.CurrentTicks < target)
                        {
                            // 卡在 NextEvent == Current 等边界时，强制走一步
                            gba.Step();
                        }
                    }
                }
            }

            // 恢复不限制
            for (int i = 0; i < coreCount; i++)
                cores[i].TickLimit = long.MaxValue;

            // 两边都应：Link!=null，AttachedCount==2
            // main: PlayerId=0 IsChild=false
            // other: PlayerId=1 IsChild=true
            //UnityEngine.Debug.Log(
            //  $"P0 id={main_gba.Serial.PlayerId} child={main_gba.Serial.IsChild} mode={main_gba.Serial.Mode} siocnt={main_gba.Serial.Siocnt:X4}");
            //UnityEngine.Debug.Log(
            //  $"P1 id={other_process[0].gba.Serial.PlayerId} child={other_process[0].gba.Serial.IsChild} mode={other_process[0].gba.Serial.Mode} siocnt={other_process[0].gba.Serial.Siocnt:X4}");

            // ----- 存档（保持你原逻辑）-----
            if (main_gba.Mem.SaveProvider.Dirty)
            {
                DumpSavDataToReady();
                main_gba.Mem.SaveProvider.Dirty = false;
            }
            for (int i = 0; i < other_process.Count; i++)
            {
                if (other_process[i].gba.Mem.SaveProvider.Dirty)
                {
                    other_process[i].DumpSavDataToReady();
                    other_process[i].gba.Mem.SaveProvider.Dirty = false;
                }
            }
            Update_CheckSave();
        }
#elif FALSE
        const int CyclesPerFrame = 70224 * 4;

        /// <summary>平时允许的最大领先（cycle）。越大越接近写法1性能。</summary>
        const int MaxSkewNormal = 65536;   // 可试 32768~131072；单机/非传输期

        /// <summary>SIO 需要收紧时的最大领先。你侧镜之迷宫/赛车安全上限约 8192。</summary>
        const int MaxSkewTight = 4096;     // 必要时刻才用；可改 4096 更稳、更慢

        /// <summary>内层防死循环</summary>
        const int StepGuard = 100000;

        static bool CoreNeedsTightSync(Gba gba)
        {
            var s = gba.Serial;
            if (s == null) return false;

            // SIOCNT bit7 = Start/Busy（传输中）
            if ((s.Siocnt & 0x80) != 0)
                return true;

            // 已进 Multi 且挂了 Link：比完全不管略紧一点（可选）
            // 若只想「真正 Busy 才收紧」，删掉下面两行即可，性能更好
            if (s.Link != null && s.Link.AttachedCount > 1 && s.Mode == SerialGba.SioMode.Multi)
                return true;


            return false;
        }

        static bool AnyCoreNeedsTightSync(Gba[] cores)
        {
            for (int i = 0; i < cores.Length; i++)
            {
                if (cores[i] != null && !cores[i].Cpu.Errored && CoreNeedsTightSync(cores[i]))
                    return true;
            }
            return false;
        }

        void RunSingleFrame()
        {
            Gba[] cores = GbaInstSlot;
            int coreCount = cores.Length;
            if (coreCount <= 0) return;

            // 本帧终点：以当前最慢核为时间轴原点
            long baseTick = cores[0].Scheduler.CurrentTicks;
            for (int i = 1; i < coreCount; i++)
            {
                long t = cores[i].Scheduler.CurrentTicks;
                if (t < baseTick) baseTick = t;
            }
            long frameEnd = baseTick + CyclesPerFrame;

            int guardOuter = 0;
            while (guardOuter++ < StepGuard)
            {
                // 是否都已到达 frameEnd
                bool allDone = true;
                long minTick = long.MaxValue;
                for (int i = 0; i < coreCount; i++)
                {
                    var g = cores[i];
                    if (g == null || g.Cpu.Errored) continue;
                    long t = g.Scheduler.CurrentTicks;
                    if (t < frameEnd) allDone = false;
                    if (t < minTick) minTick = t;
                }
                if (allDone) break;
                if (minTick == long.MaxValue) break;

                // ---- 动态收紧：仅必要时用 MaxSkewTight ----
                int skew = AnyCoreNeedsTightSync(cores) ? MaxSkewTight : MaxSkewNormal;
                long limitCeil = minTick + skew;
                if (limitCeil > frameEnd) limitCeil = frameEnd;

                // 每核推进：TickLimit = min(自身可跑到的上限, limitCeil)
                // StateStepUntil 在 TickLimit=较大时会一次跑到 NextEvent，接近写法1
                for (int i = 0; i < coreCount; i++)
                {
                    var gba = cores[i];
                    if (gba == null || gba.Cpu.Errored) continue;
                    if (gba.Scheduler.CurrentTicks >= frameEnd) continue;

                    long target = limitCeil;
                    // 已领先到 limit 的核本轮跳过，等别人跟上
                    if (gba.Scheduler.CurrentTicks >= target) continue;

                    int guard = 0;
                    while (gba.Scheduler.CurrentTicks < target && !gba.Cpu.Errored && guard++ < StepGuard)
                    {
                        gba.TickLimit = target;
                        uint advanced = gba.StateStepUntil();
                        if (advanced == 0 && gba.Scheduler.CurrentTicks < target)
                            gba.Step();
                    }
                }
            }

            for (int i = 0; i < coreCount; i++)
            {
                if (cores[i] != null)
                    cores[i].TickLimit = long.MaxValue;
            }

            // ----- 存档（保持原逻辑）-----
            if (main_gba.Mem.SaveProvider.Dirty)
            {
                DumpSavDataToReady();
                main_gba.Mem.SaveProvider.Dirty = false;
            }
            for (int i = 0; i < other_process.Count; i++)
            {
                if (other_process[i].gba.Mem.SaveProvider.Dirty)
                {
                    other_process[i].DumpSavDataToReady();
                    other_process[i].gba.Mem.SaveProvider.Dirty = false;
                }
            }
            Update_CheckSave();
        }
#else

        const int CyclesPerFrame = 70224 * 4;

        /// <summary>SIO 需要收紧时的最大领先。你侧镜之迷宫/赛车安全上限约 8192。</summary>
        //const int MaxSkewTight = 4096;     // 必要时刻才用；可改 4096 更稳、更慢

        /// <summary>内层防死循环</summary>
        const int StepGuard = 100000;

        const int MaxSkewSolo = 131072; // 无 Link
        const int MaxSkewLinked = 16384;  // 有 Link 但未在传输（可调 8192~32768）
        const int MaxSkewTight = 4096;   // 传输中 / 粘滞期（你已验证）
        const int TightHoldCycles = 24576;  // Busy 消失后仍保持 Tight 的时间（约数条扫描线~几帧可调）

        static int GetMaxSkew(Gba[] cores)
        {
            bool anyLink = false;
            bool anyBusy = false;
            bool inHold = false;

            if (cores.Length <= 1)
                return MaxSkewSolo;

            for (int i = 0; i < cores.Length; i++)
            {
                var g = cores[i];
                if (g == null || g.Cpu.Errored) continue;
                var s = g.Serial;
                if (s == null) continue;

                if (s.Link != null && s.Link.AttachedCount > 1)
                    anyLink = true;

                if ((s.Siocnt & 0x80) != 0)
                    anyBusy = true;

                // 粘滞：最近传过
                long dt = g.Scheduler.CurrentTicks - s.LastTransferActivityTick;
                if (dt >= 0 && dt < TightHoldCycles)
                    inHold = true;
            }

            if (anyBusy || inHold)
                return MaxSkewTight;    // 真正需要传输对齐

            if (anyLink)
                return MaxSkewLinked; // 有线但标题/菜单：轻度限制，省性能

            return MaxSkewSolo;
        }

        void RunSingleFrame()
        {
            Gba[] cores = GbaInstSlot;
            int coreCount = cores.Length;
            if (coreCount <= 0) return;

            // 本帧终点：以当前最慢核为时间轴原点
            long baseTick = cores[0].Scheduler.CurrentTicks;
            for (int i = 1; i < coreCount; i++)
            {
                long t = cores[i].Scheduler.CurrentTicks;
                if (t < baseTick) baseTick = t;
            }
            long frameEnd = baseTick + CyclesPerFrame;

            int guardOuter = 0;
            while (guardOuter++ < StepGuard)
            {
                // 是否都已到达 frameEnd
                bool allDone = true;
                long minTick = long.MaxValue;
                for (int i = 0; i < coreCount; i++)
                {
                    var g = cores[i];
                    if (g == null || g.Cpu.Errored) continue;
                    long t = g.Scheduler.CurrentTicks;
                    if (t < frameEnd) allDone = false;
                    if (t < minTick) minTick = t;
                }
                if (allDone) break;
                if (minTick == long.MaxValue) break;

                // ---- 动态收紧：仅必要时用 MaxSkewTight ----
                int skew = GetMaxSkew(cores);
                long limitCeil = minTick + skew;
                if (limitCeil > frameEnd) limitCeil = frameEnd;

                // 每核推进：TickLimit = min(自身可跑到的上限, limitCeil)
                // StateStepUntil 在 TickLimit=较大时会一次跑到 NextEvent，接近写法1
                for (int i = 0; i < coreCount; i++)
                {
                    var gba = cores[i];
                    if (gba == null || gba.Cpu.Errored) continue;
                    if (gba.Scheduler.CurrentTicks >= frameEnd) continue;

                    long target = limitCeil;
                    // 已领先到 limit 的核本轮跳过，等别人跟上
                    if (gba.Scheduler.CurrentTicks >= target) continue;

                    int guard = 0;
                    while (gba.Scheduler.CurrentTicks < target && !gba.Cpu.Errored && guard++ < StepGuard)
                    {
                        gba.TickLimit = target;
                        uint advanced = gba.StateStepUntil();
                        if (advanced == 0 && gba.Scheduler.CurrentTicks < target)
                            gba.Step();
                    }
                }
            }

            for (int i = 0; i < coreCount; i++)
            {
                if (cores[i] != null)
                    cores[i].TickLimit = long.MaxValue;
            }

            // ----- 存档（保持原逻辑）-----
            if (main_gba.Mem.SaveProvider.Dirty)
            {
                DumpSavDataToReady();
                main_gba.Mem.SaveProvider.Dirty = false;
            }
            for (int i = 0; i < other_process.Count; i++)
            {
                if (other_process[i].gba.Mem.SaveProvider.Dirty)
                {
                    other_process[i].DumpSavDataToReady();
                    other_process[i].gba.Mem.SaveProvider.Dirty = false;
                }
            }
            Update_CheckSave();
        }
#endif

        private static byte[] GetBytesZippedFile(string filename)
        {
            byte[] bytes = AxiIO.File.ReadAllBytes(filename);
            if (bytes == null)
            {
                throw new Exception("[GetBytesZippedFile]中断 data == null");
            }
            App.log.Debug("[GetBytesZippedFile] zip大小：" + bytes.Length);
            var zip = new ZipInputStream(new System.IO.MemoryStream(bytes));
            while (true)
            {
                var currentEntry = zip.GetNextEntry();
                if (currentEntry == null) break;

                //当前平台单文件rom扩展名判断
                string entryName = currentEntry.Name.ToLower();
                if (!entryName.EndsWith(".gba")) continue;
                var buffer = new byte[1024];
                System.IO.MemoryStream output = new System.IO.MemoryStream();
                while (true)
                {
                    var size = zip.Read(buffer, 0, buffer.Length);
                    if (size == 0) break;
                    else output.Write(buffer, 0, size);
                }
                output.Flush();
                byte[] data = output.ToArray();
                App.log.Info("[GetBytesZippedFile] 解压" + entryName + " 大小：" + data.Length);
                return data;
            }
            throw new Exception("[GetBytesZippedFile] 没有合法entry");
        }


        #region SaveDataCache

        bool cache_bNeedWriteSav = false;
        string cache_targetpath;
        byte[] cache_savdata;
        float cache_writereadytime = 0;
        bool cache_had_savedata => cache_savdata != null;

        void LoadSaveFileToCache(byte[] savdata, string savpath)
        {
            cache_bNeedWriteSav = false;
            cache_savdata = null;
            cache_targetpath = savpath;
            if (savdata == null || savdata.Length < 1)
                return;
            cache_bNeedWriteSav = true;
            cache_savdata = savdata;
        }
        void DumpSavDataToReady()
        {
            cache_bNeedWriteSav = true;
            cache_targetpath = main_gba.Provider.SavPath;
            cache_savdata = main_gba.Mem.SaveProvider.GetSave();
            cache_writereadytime = Time.time;
        }

        void ReleaseCache()
        {
            cache_bNeedWriteSav = false;
            cache_targetpath = string.Empty;
            cache_savdata = null;
            cache_writereadytime = 0;
        }


        void Update_CheckSave(bool mustsave = false)
        {
            if (!cache_bNeedWriteSav)
                return;
            if (!mustsave && Time.time - cache_writereadytime < 5f)
                return;
            try
            {
                string dirpath = System.IO.Path.GetDirectoryName(cache_targetpath);
                if(!AxiIO.Directory.Exists(dirpath))
                    AxiIO.Directory.CreateDirectory(dirpath);
                AxiIO.File.WriteAllBytes(cache_targetpath, cache_savdata, mustsave);
                OverlayManager.PopTip("GBA存档写入");
            }
            catch
            {
                App.log.Error("Failed to write .sav file!");
            }
            cache_bNeedWriteSav = false;
        }

        public bool SavFileExists()
        {
            return cache_had_savedata;
        }

        public long GetSavFileLength()
        {
            if (!cache_had_savedata)
                return -1;
            return cache_savdata.LongLength;
        }
        #endregion
    }
}