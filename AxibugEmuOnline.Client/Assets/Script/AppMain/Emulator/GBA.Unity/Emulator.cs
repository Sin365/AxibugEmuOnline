using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using ICSharpCode.SharpZipLib.Zip;
using OptimeGBA;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class Emulator : EmuCore<GBAKeyCode>
    {
        public static Emulator instance;
        const int FrameCycles = 70224 * 4;
        const int ScanlineCycles = 1232;
        const float FrameRate = 59.7275f;
        static bool SyncToAudio = true;

        //public Renderer screenRenderer;
        public VideoProvider videoProvider;
        public AudioProvider audioProvider;
        public InputProvider inputProvider;
        public static System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        public bool ShowBackBuf = false;
        //public bool RunEmulator;
        public bool EnableAudio;
        public bool BootBIOS = false;

        public Gba gba;
        public static bool bLogicUpdatePause { get; private set; }
        //Thread EmulationThread;
        //AutoResetEvent ThreadSync = new AutoResetEvent(false);


        public bool RomLoaded { get; private set; } = false;
        #region 子类实现

        public override RomPlatformType Platform => RomPlatformType.GameBoyAdvance;

        public override uint PushFrame => AxiEmuRunFrame;//???

        public override uint PhysicsFrame => AxiVirtualFrame;

        public override Texture OutputPixel => videoProvider.wrapTex;

        public override RawImage DrawCanvas => videoProvider.m_drawCanvas;

        public override Vector3 DrawCanvas_SrcRot => videoProvider.srcCanvasLocalEulerAngles;

        public override Vector3 DrawLocalScale => new Vector3(1, -1, 1);

        protected override GBAKeyCode GetLocalInput()
        {
            return (GBAKeyCode)inputProvider.DoLocalPressedKeys();
        }

        protected override GBAKeyCode ConvertInputDataFromNet(ReplayStep step)
        {
            throw new NotImplementedException();
        }

        protected override ulong InputDataToNet(GBAKeyCode inputData)
        {
            return (ushort)inputData;
        }

        protected override bool OnPushEmulatorFrame(GBAKeyCode InputData)
        {
            if (!bLogicUpdatePause) return false;
            inputProvider.SetCurrKeyArr(InputData);

            DoUpdate();
            return true;
        }

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
            bLogicUpdatePause = false;
        }
        public override void Resume()
        {
            bLogicUpdatePause = true;
        }
        public override MsgBool StartGame(RomFile romFile)
        {
            string path = romFile.LocalProxyPath;
            byte[] romdata = GetBytesZippedFile(path);
            try
            {
                LoadRom(romdata, romFile.LocalProxyFileName);
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
        }

        public override void DoReset()
        {
            ResetGba();
        }

        public override IControllerSetuper GetControllerSetuper()
        {
            return inputProvider.ControllerMapper;
        }

        protected override void AfterPushFrame()
        {

        }

        public override void GetAudioParams(out int frequency, out int channels)
        {
            frequency = audioProvider.SampleRate;
            channels = audioProvider.Channels;
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
        }

        // Update is called once per frame
        void DoUpdate()
        {
            if (RomLoaded)
            {
                videoProvider.OnRenderFrame();
            }
            Frame_UpdateByCpuTime();
        }

        public void LoadRom(byte[] rom, string name)
        {
            string savPath = App.PersistentDataPath(this.Platform) + "/" + name.Substring(0, name.Length - 3) + "sav";
            byte[] sav = new byte[0];
            if (AxiIO.File.Exists(savPath))
            {
                Debug.Log($"{savPath} exists, loading");
                try
                {
                    sav = AxiIO.File.ReadAllBytes(savPath);
                }
                catch
                {
                    Debug.Log("Failed to load .sav file!");
                }
            }
            else
            {
                Debug.Log(".sav not available");
            }

            LoadRomAndSave(rom, sav, savPath);
            Debug.Log("Load Rom Success");
            audioProvider.Initialize();
            RomLoaded = true;
            //RunEmulator = true;
        }

        public void LoadRomAndSave(byte[] rom, byte[] sav, string savPath)
        {
            byte[] bios = Resources.Load<TextAsset>("GBA.Unity/gba_bios.bin").bytes;
            //byte[] bios = BetterStreamingAssets.ReadAllBytes("gba_bios.bin");
            Debug.Log(bios.Length);
            gba = new Gba(new ProviderGba(bios, rom, savPath, audioProvider.AudioReady) { BootBios = BootBIOS });
            gba.Mem.SaveProvider.LoadSave(sav);

        }

        public void ResetGba()
        {
            byte[] save = gba.Mem.SaveProvider.GetSave();
            ProviderGba p = gba.Provider;
            gba = new Gba(p);
            gba.Mem.SaveProvider.LoadSave(save);
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
            while (CyclesLeft > 0 && !gba.Cpu.Errored)
            {
                CyclesLeft -= (int)gba.Step();
            }
        }

        public void RunScanline()
        {
            CyclesLeft += ScanlineCycles;
            while (CyclesLeft > 0 && !gba.Cpu.Errored)
            {
                CyclesLeft -= (int)gba.Step();
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

        void RunSingleFrame()
        {
            int cyclesLeft = 70224 * 4;
            while (cyclesLeft > 0 && !gba.Cpu.Errored)
            {
                cyclesLeft -= (int)gba.Step();
            }

            while (!SyncToAudio && !gba.Cpu.Errored /*&& RunEmulator*/)
            {
                gba.Step();
            }

            if (gba.Mem.SaveProvider.Dirty)
            {
                DumpSavReady();
                //清理脏标记，否则一直保存
                gba.Mem.SaveProvider.Dirty = false;
            }
            Update_CheckSave();
        }



        public void DumpSavReady()
        {
            bNeedWriteSav = true;
            writeSavTargetPath = gba.Provider.SavPath;
            writeSavData = gba.Mem.SaveProvider.GetSave();
            setWriteReadyTime = Time.time;
        }

        #region
        bool bNeedWriteSav = false;
        float setWriteReadyTime = 0;
        string writeSavTargetPath = string.Empty;
        byte[] writeSavData = null;

        void Update_CheckSave(bool withouttime = false)
        {
            if (!bNeedWriteSav)
                return;
            if (!withouttime && Time.time - setWriteReadyTime < 0.5f)
                return;
            try
            {
                AxiIO.File.WriteAllBytes(writeSavTargetPath, writeSavData, true);
                OverlayManager.PopTip("GBA存档写入");
            }
            catch
            {
                Debug.Log("Failed to write .sav file!");
            }
            bNeedWriteSav = false;
        }
        #endregion
        private static byte[] GetBytesZippedFile(string filename)
        {
            byte[] bytes = AxiIO.File.ReadAllBytes(filename);
            if (bytes == null)
            {
                throw new Exception("[GetBytesZippedFile]中断 data == null");
            }
            UnityEngine.Debug.Log("[GetBytesZippedFile] zip大小：" + bytes.Length);
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
    }

}