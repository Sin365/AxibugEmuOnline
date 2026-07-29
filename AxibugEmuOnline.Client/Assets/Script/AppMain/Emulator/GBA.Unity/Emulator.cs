using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using ICSharpCode.SharpZipLib.Zip;
using OptimeGBA;
using System;
using System.Threading;
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

        public override uint PushFrame => gba.Ppu.VCount;//???

        public override uint PhysicsFrame => PushFrame;

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

        //private int _samplesAvailable;
        //private PipeStream _pipeStream;
        //private byte[] _buffer;
        public float audioGain = 1.0f;

        //public Button btnStart;
        private void Awake()
        {
            instance = this;
            var mCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            mCanvas.worldCamera = Camera.main;
            // must set it to 60 or it won't sync with audio or run too fast.
            Application.targetFrameRate = (int)FrameRate;
            // Get Unity Buffer size
            //AudioSettings.GetDSPBufferSize(out int bufferLength, out _);
            //_samplesAvailable = bufferLength;
            // Must be set to 32768
            var audioConfig = AudioSettings.GetConfiguration();
            audioConfig.sampleRate = GbaAudio.SampleRate;
            AudioSettings.Reset(audioConfig);
            // Prepare our buffer
            //_pipeStream = new PipeStream();
            //_pipeStream.MaxBufferLength = _samplesAvailable * 2 * sizeof(float);
            //_buffer = new byte[_samplesAvailable * 2 * sizeof(float)];
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
        }

        // Update is called once per frame
        void DoUpdate()
        {
            if (RomLoaded)
            {
                videoProvider.OnRenderFrame();
            }
            OnUpdateFrame();
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


        public void OnUpdateFrame()
        {
            SyncToAudio = !(Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Space));

            //if (RunEmulator)
            //{
            //    ThreadSync.Set();
            //}

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
                DumpSav();
            }
        }

        public void DumpSav()
        {
            try
            {
                AxiIO.File.WriteAllBytes(gba.Provider.SavPath, gba.Mem.SaveProvider.GetSave());
            }
            catch
            {
                Debug.Log("Failed to write .sav file!");
            }
        }
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