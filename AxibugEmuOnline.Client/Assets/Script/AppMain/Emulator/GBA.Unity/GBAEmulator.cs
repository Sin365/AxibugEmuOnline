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
    public class GBAEmulator : EmuCore<GBAKeyCode>, AxiGbaIO
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

        public RawImage[] OtherGbaSreen;
        public static bool bLogicUpdatePause { get; private set; }
        //Thread EmulationThread;
        //AutoResetEvent ThreadSync = new AutoResetEvent(false);


        public bool RomLoaded { get; private set; } = false;
        #region 子类实现

        public override RomPlatformType Platform => RomPlatformType.GameBoyAdvance;

        public override uint PushFrame => AxiEmuRunFrame;

        public override uint PhysicsFrame => AxiVirtualFrame;

        public override Texture OutputPixel => main_videoProvider.wrapTex;

        public override RawImage DrawCanvas => main_videoProvider.m_drawCanvas;

        public override Vector3 DrawCanvas_SrcRot => main_videoProvider.srcCanvasLocalEulerAngles;

        public override Vector3 DrawLocalScale => new Vector3(1, -1, 1);

        protected override GBAKeyCode GetLocalInput()
        {
            return (GBAKeyCode)main_inputProvider.DoLocalPressedKeys();
        }

        protected override GBAKeyCode ConvertInputDataFromNet(ReplayStep step)
        {
            return (GBAKeyCode)step.InPut;
        }

        protected override ulong InputDataToNet(GBAKeyCode inputData)
        {
            return (ushort)inputData;
        }

        protected override bool OnPushEmulatorFrame(GBAKeyCode InputData)
        {
            if (!bLogicUpdatePause) return false;
            //TODO联机时区分
            main_inputProvider.SetCurrKeyArr(InputData);

            for (int i = 0; i < other_process.Count; i++)
                other_process[i].SetCurrKeyArr(other_process[i].DoLocalInputToTest());

            DoUpdate();
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
            GBAGameStartInfo gbastartInfo = (GBAGameStartInfo)startInfo;
            RomFile romFile = startInfo.romFile;
            other_process.Clear();
            other_process.Add(new GBAEmuLiteProcess());
            string path = romFile.LocalProxyPath;
            byte[] romdata = GetBytesZippedFile(path);
            string savpath = GetGameSavPath(romFile.LocalProxyFileName);
            byte[] savdata = null;
            if (AxiIO.File.Exists(savpath))
                savdata = AxiIO.File.ReadAllBytes(savpath);
            try
            {
                //LoadRom(romdata, romFile.LocalProxyFileName);
                //for (int i = 0; i < other_process.Count; i++)
                //{
                //    other_process[i].VideoInit(OtherGbaSreen[i]);
                //    other_process[i].LoadRom(romdata, cache_savdata);
                //}
                //gbaLink = new GbaLink(main_gba, other_process[0].gba);


                //单人启动
                //SetGBALink(gbastartInfo, 1, 0, new List<byte[]> { savdata });

                //测试多人
                SetGBALink(gbastartInfo, 2, 0, new List<byte[]> { savdata, savdata });

                bLogicUpdatePause = true;
                return true;
            }
            catch (Exception e)
            {
                bLogicUpdatePause = false;
                return "失败";
            }
        }

        void SetGBALink(GameStartInfo startInfo, int playerCount, int mineIdx, List<byte[]> otherSavDataList)
        {
            GbaInstSlot = new Gba[playerCount];
            gbaLink = null;
            other_process.Clear();

            for (int i = 0; i < OtherGbaSreen.Length; i++)
            {
                OtherGbaSreen[i]?.gameObject.SetActive(false);
            }

            RomFile romFile = startInfo.romFile;
            string path = romFile.LocalProxyPath;
            byte[] romdata = GetBytesZippedFile(path);
            if (playerCount == 1)//单人
            {
                LoadRom(romdata, otherSavDataList[0], romFile.LocalProxyFileName);
                GbaInstSlot[0] = main_gba;
                main_gba.SetLiteMode(false);
            }
            else//多人，对号入座
            {
                for (int i = 0; i < playerCount; i++)
                {
                    if (mineIdx == i)
                    {
                        LoadRom(romdata, otherSavDataList[0], romFile.LocalProxyFileName);
                        GbaInstSlot[i] = main_gba;
                        main_gba.SetLiteMode(false);
                    }
                    else
                    {
                        GBAEmuLiteProcess otherGba = new GBAEmuLiteProcess();
                        other_process.Add(otherGba);
                        otherGba.VideoInit(OtherGbaSreen[other_process.Count - 1]);
                        OtherGbaSreen[other_process.Count - 1]?.gameObject.SetActive(true);
                        otherGba.LoadRom(romdata, otherSavDataList[0]);
                        otherGba.gba.SetLiteMode(false);
                        GbaInstSlot[i] = otherGba.gba;
                    }
                }
                gbaLink = new GbaLink(
                    GbaInstSlot[0],
                    GbaInstSlot[1],
                    GbaInstSlot.Length >= 3 ? GbaInstSlot[2] : null,
                    GbaInstSlot.Length == 4 ? GbaInstSlot[3] : null);
            }
            bLogicUpdatePause = true;
        }

        public override void Dispose()
        {
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

        // Update is called once per frame
        void DoUpdate()
        {
            if (RomLoaded)
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
                App.log.Info("SetLiteMode => "+ main_gba.EnablePpuRender);
                main_gba.SetLiteMode(main_gba.EnablePpuRender);
            }
#endif
            Frame_UpdateByCpuTime();
        }

        string GetGameSavPath(string name)
        {
            string savPath = App.PersistentDataPath(this.Platform) + "/" + name.Substring(0, name.Length - 3) + "sav";
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
            byte[] save = main_gba.Mem.SaveProvider.GetSave();
            ProviderGba p = main_gba.Provider;
            main_gba = new Gba(p);
            main_gba.Mem.SaveProvider.LoadSave(save);
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
        //        if(cyclesLeft_main > 0 && !main_gba.Cpu.Errored)
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

        const int CyclesPerFrame = 70224 * 4;
        const int Slice = 64; // 可试 32~256

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

        //void LoadSaveFileToCache(string savpath)
        //{
        //    cache_bNeedWriteSav = false;
        //    cache_savdata = null;
        //    cache_targetpath = savpath;
        //    if (!AxiIO.File.Exists(savpath))
        //        return;
        //    cache_bNeedWriteSav = true;
        //    cache_savdata = AxiIO.File.ReadAllBytes(savpath);
        //}

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