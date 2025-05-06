using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes.Core;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : EmuCore<ControllerState>
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
            App.tick.SetFrameRate(60);
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;
        }

        public override RomPlatformType Platform => RomPlatformType.Nes;
        private CoreSupporter m_coreSupporter;
        /// <summary>
        /// 指定ROM开始游戏
        /// </summary>
        public override MsgBool StartGame(RomFile rom)
        {
            StopGame();

            m_coreSupporter = new CoreSupporter();
            Supporter.Setup(m_coreSupporter);
            Debuger.Setup(new CoreDebuger());

            App.GetRomLib(RomPlatformType.Nes).AddRomFile(rom);

            try
            {
                NesCore = new NES(rom.FileName);
                return true;
            }
            catch (Exception ex)
            {
                NesCore = null;
                App.log.Error(ex.ToString());
                return ex.Message;
            }
        }

        public override void Pause()
        {
            IsPause = true;
        }

        public override void Resume()
        {
            IsPause = false;
        }


        public override void DoReset()
        {
            NesCore.Reset();
        }

        public override void LoadState(object state)
        {
            NesCore.LoadState((State)state);
        }

        public override object GetState()
        {
            return NesCore.GetState();
        }

        /// <summary>
        ///     获取即时存档
        /// </summary>
        /// <returns></returns>
        public override byte[] GetStateBytes()
        {
            return NesCore.GetState().ToBytes();
        }

        /// <summary>
        ///     加载即时存档
        /// </summary>
        /// <param
        ///     name="data">
        /// </param>
        public override void LoadStateFromBytes(byte[] data)
        {
            var st = new State();
            st.FromByte(data);
            NesCore.LoadState(st);
        }

        public override uint Frame => NesCore.FrameCount;

        /// <summary>
        ///     停止游戏
        /// </summary>
        public void StopGame()
        {
            NesCore?.Dispose();
            NesCore = null;
        }

        //推进帧
        protected override bool OnPushEmulatorFrame(ControllerState inputData)
        {
            if (NesCore == null || IsPause) return false;

            NesCore.pad.Sync(inputData);
            NesCore.EmulateFrame(true);

            return true;
        }

        protected override ControllerState ConvertInputDataFromNet(ReplayStep step)
        {
            return m_coreSupporter.FromNet(step);
        }
        protected override ulong InputDataToNet(ControllerState inputData)
        {
            return m_coreSupporter.ToNet(inputData);
        }

        protected override ControllerState GetLocalInput()
        {
            return ControllerMapper.CreateState();
        }


        protected override unsafe void AfterPushFrame()
        {
            var screenBuffer = NesCore.ppu.GetScreenPtr();
            VideoProvider.SetDrawData(screenBuffer);
        }

        public override IControllerSetuper GetControllerSetuper()
        {
            return ControllerMapper;
        }

        public override void Dispose()
        {
            StopGame();
        }

        public override Texture OutputPixel => VideoProvider.OutputPixel;
        public override RawImage DrawCanvas => VideoProvider.Drawer;
        public override void GetAudioParams(out int frequency, out int channels)
        {
            AudioProvider.GetAudioParams(out frequency, out channels);
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

            var xmlStr = System.IO.File.ReadAllText("nes20db.xml");
            var xml = XDocument.Parse(xmlStr);
            var games = xml.Element("nes20db")?.Elements("game");
            System.Diagnostics.Debug.Assert(games != null, nameof(games) + " != null");
            foreach (var game in games)
            {
                var crcStr = game.Element("rom")?.Attribute("crc32")?.Value;
                var crc = uint.Parse($"{crcStr}", NumberStyles.HexNumber);

                var mapper = int.Parse($"{game.Element("pcb")?.Attribute("mapper")?.Value}");

                if (mapper > 255) continue;
                db.AddInfo(new RomDB.RomInfo { CRC = crc, Mapper = mapper });
            }

            UnityEditor.EditorUtility.SetDirty(db);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}