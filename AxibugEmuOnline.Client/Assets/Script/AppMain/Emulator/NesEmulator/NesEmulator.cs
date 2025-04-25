using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
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
            App.tick.SetFrameRate(60);
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;
        }

        public RomPlatformType Platform => RomPlatformType.Nes;
        private CoreSupporter m_coreSupporter;
        /// <summary>
        /// 指定ROM开始游戏
        /// </summary>
        public MsgBool StartGame(RomFile rom)
        {
            StopGame();

            m_coreSupporter = new CoreSupporter(ControllerMapper);
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
        //推进帧
        public bool PushEmulatorFrame()
        {
            if (NesCore == null || IsPause) return false;

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


        public unsafe void AfterPushFrame()
        {
            var screenBuffer = NesCore.ppu.GetScreenPtr();
            VideoProvider.SetDrawData(screenBuffer);
        }

        public IControllerSetuper GetControllerSetuper()
        {
            return ControllerMapper;
        }

        public void Dispose()
        {
            StopGame();
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

        public Texture OutputPixel => VideoProvider.OutputPixel;
        public RawImage DrawCanvas => VideoProvider.Drawer;
        public void GetAudioParams(out int frequency, out int channels)
        {
            AudioProvider.GetAudioParams(out frequency, out channels);
        }

    }
}