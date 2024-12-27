using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using AxibugEmuOnline.Client.ClientCore;
using UnityEditor;
using UnityEngine;
using VirtualNes.Core;
using VirtualNes.Core.Debug;
using Debug = System.Diagnostics.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : MonoBehaviour, IEmuCore
    {
        public VideoProvider VideoProvider;
        public AudioProvider AudioProvider;

        //模拟器核心实例化对象
        public NES NesCore { get; private set; }

        /// <summary> 是否暂停 </summary>
        public bool IsPause { get; private set; }
        public NesControllerMapper ControllerMapper { get; private set; }

        private void Awake()
        {
            ControllerMapper = new NesControllerMapper();
        }

        private void Start()
        {
            //关闭垂直同步
            QualitySettings.vSyncCount = 0;
            //设为60帧
            Application.targetFrameRate = 60;
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;
        }

        /// <summary>
        ///     Unity的逐帧驱动
        /// </summary>
        private unsafe void Update()
        {
            if (NesCore != null && !IsPause)
            {
                PushEmulatorFrame();
                if (InGameUI.Instance.IsNetPlay)
                    FixEmulatorFrame();

                var screenBuffer = NesCore.ppu.GetScreenPtr();
                VideoProvider.SetDrawData(screenBuffer);
            }

            VideoProvider.ApplyScreenScaler();
            VideoProvider.ApplyFilterEffect();
        }

        public EnumSupportEmuPlatform Platform => EnumSupportEmuPlatform.NES;
        private CoreSupporter m_coreSupporter;
        /// <summary>
        /// 指定ROM开始游戏
        /// </summary>
        public void StartGame(RomFile rom)
        {
            StopGame();

            m_coreSupporter = new CoreSupporter(ControllerMapper);
            Supporter.Setup(m_coreSupporter);
            Debuger.Setup(new CoreDebuger());

            App.nesRomLib.AddRomFile(rom);

            try
            {
                NesCore = new NES(rom.FileName);
            }
            catch (Exception ex)
            {
                NesCore = null;
                App.log.Error(ex.ToString());
            }
        }

        public void Pause()
        {
            IsPause = true;
        }

        public void Resume()
        {
            IsPause = false;
        }


        public void DoReset()
        {
            NesCore.Reset();
        }

        public void SetupScheme()
        {
            CommandDispatcher.Instance.Current = CommandDispatcher.Instance.Gaming;
        }

        public void LoadState(object state)
        {
            NesCore.LoadState((State)state);
        }

        public object GetState()
        {
            return NesCore.GetState();
        }

        /// <summary>
        ///     获取即时存档
        /// </summary>
        /// <returns></returns>
        public byte[] GetStateBytes()
        {
            return NesCore.GetState().ToBytes();
        }

        /// <summary>
        ///     加载即时存档
        /// </summary>
        /// <param
        ///     name="data">
        /// </param>
        public void LoadStateFromBytes(byte[] data)
        {
            var st = new State();
            st.FromByte(data);
            NesCore.LoadState(st);
        }

        public uint Frame => NesCore.FrameCount;

        /// <summary>
        ///     停止游戏
        /// </summary>
        public void StopGame()
        {
            NesCore?.Dispose();
            NesCore = null;
        }


#if UNITY_EDITOR
        private ControllerState m_lastState;
#endif
        //是否跳帧，单机无效
        private void FixEmulatorFrame()
        {
            var skipFrameCount = App.roomMgr.netReplay.GetSkipFrameCount();

            if (skipFrameCount > 0) App.log.Debug($"SKIP FRAME : {skipFrameCount}");
            for (var i = 0; i < skipFrameCount; i++)
                if (!PushEmulatorFrame())
                    break;
        }

        //推进帧
        private bool PushEmulatorFrame()
        {
            m_coreSupporter.SampleInput(NesCore.FrameCount);
            var controlState = m_coreSupporter.GetControllerState();

            //如果未收到Input数据,核心帧不推进
            if (!controlState.valid) return false;

#if UNITY_EDITOR
            if (controlState != m_lastState) App.log.Info($"[LOCALDEBUG]{NesCore.FrameCount}-->{controlState}");
            m_lastState = controlState;
#endif

            NesCore.pad.Sync(controlState);
            NesCore.EmulateFrame(true);


            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        ///     编辑器用
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [ContextMenu("ImportNesDB")]
        public void ImportNesDB()
        {
            var db = Resources.Load<RomDB>("NES/ROMDB");
            db.Clear();

            var xmlStr = File.ReadAllText("nes20db.xml");
            var xml = XDocument.Parse(xmlStr);
            var games = xml.Element("nes20db")?.Elements("game");
            Debug.Assert(games != null, nameof(games) + " != null");
            foreach (var game in games)
            {
                var crcStr = game.Element("rom")?.Attribute("crc32")?.Value;
                var crc = uint.Parse($"{crcStr}", NumberStyles.HexNumber);

                var mapper = int.Parse($"{game.Element("pcb")?.Attribute("mapper")?.Value}");

                if (mapper > 255) continue;
                db.AddInfo(new RomDB.RomInfo { CRC = crc, Mapper = mapper });
            }

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
        }
#endif
        public IControllerSetuper GetControllerSetuper()
        {
            return ControllerMapper;
        }
    }
}