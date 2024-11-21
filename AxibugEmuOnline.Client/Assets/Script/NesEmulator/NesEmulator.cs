using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using VirtualNes.Core;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : MonoBehaviour, IEmuCore
    {
        //模拟器核心实例化对象
        public NES NesCore { get; private set; }

        //视频驱动（这里是Unity接收模拟器画面数据的并渲染出来的实现）
        public VideoProvider VideoProvider;
        //音频驱动（这里是Unity接收模拟器音频数据的并播放出来的实现）
        public AudioProvider AudioProvider;
        //是否暂停
        public bool m_bPause;

        private void Start()
        {
            Application.targetFrameRate = 60;
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;
        }

        /// <summary>
        /// 指定ROM开始游戏
        /// </summary>
        /// <param name="rom"></param>
        public void StartGame(RomFile rom)
        {
            StopGame();

            Supporter.Setup(new CoreSupporter());
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

        /// <summary>
        /// 停止游戏
        /// </summary>
        public void StopGame()
        {
            NesCore?.Dispose();
            NesCore = null;
        }

        /// <summary>
        /// Unity的逐帧驱动
        /// </summary>
        private unsafe void Update()
        {
            if (m_bPause) return;

            if (NesCore != null)
            {
                PushEmulatorFrame();
                if (InGameUI.Instance.IsNetPlay)
                    FixEmulatorFrame();

                var screenBuffer = NesCore.ppu.GetScreenPtr();
                VideoProvider.SetDrawData(screenBuffer);
            }
        }

        //是否跳帧，单机无效
        private void FixEmulatorFrame()
        {
            var skipFrameCount = App.roomMgr.netReplay.GetSkipFrameCount();

            if (skipFrameCount > 0) App.log.Debug($"SKIP FRAME : {skipFrameCount}");
            for (int i = 0; i < skipFrameCount; i++)
            {
                if (!PushEmulatorFrame()) break;
            }
        }

        ControllerState lastState;
        //推进帧
        private bool PushEmulatorFrame()
        {
            Supporter.SampleInput(NesCore.FrameCount);
            var controlState = Supporter.GetControllerState();

            //如果未收到Input数据,核心帧不推进
            if (!controlState.valid) return false;

#if UNITY_EDITOR
            if (controlState != lastState)
            {
                App.log.Info($"[LOCALDEBUG]{NesCore.FrameCount}-->{controlState}");
            }
#endif

            NesCore.pad.Sync(controlState);
            NesCore.EmulateFrame(true);

            lastState = controlState;

            return true;
        }

        public void Pause()
        {
            m_bPause = true;
        }

        public void Resume()
        {
            m_bPause = false;
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
        /// 获取即时存档
        /// </summary>
        /// <returns></returns>
        public byte[] GetStateBytes()
        {
            return NesCore.GetState().ToBytes();
        }

        /// <summary>
        /// 加载即时存档
        /// </summary>
        /// <param name="data"></param>
        public void LoadStateFromBytes(byte[] data)
        {
            State st = new State();
            st.FromByte(data);
            NesCore.LoadState(st);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器用
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [ContextMenu("ImportNesDB")]
        public void ImportNesDB()
        {
            var db = Resources.Load<RomDB>("NES/ROMDB");
            db.Clear();

            var xmlStr = File.ReadAllText("nes20db.xml");
            var xml = XDocument.Parse(xmlStr);
            var games = xml.Element("nes20db").Elements("game");
            foreach (var game in games)
            {
                var crcStr = game.Element("rom").Attribute("crc32").Value;
                var crc = uint.Parse($"{crcStr}", System.Globalization.NumberStyles.HexNumber);

                var mapper = int.Parse($"{game.Element("pcb").Attribute("mapper").Value}");

                if (mapper > 255) continue;
                db.AddInfo(new RomDB.RomInfo { CRC = crc, Mapper = mapper });
            }

            UnityEditor.EditorUtility.SetDirty(db);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        public uint Frame => NesCore.FrameCount;
    }
}
