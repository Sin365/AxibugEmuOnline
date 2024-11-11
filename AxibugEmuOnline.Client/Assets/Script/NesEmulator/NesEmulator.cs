using AxibugEmuOnline.Client.ClientCore;
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
        public NES NesCore { get; private set; }

        public VideoProvider VideoProvider;
        public AudioProvider AudioProvider;
        public bool m_bPause;

        private void Start()
        {
            Application.targetFrameRate = 60;
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;
        }

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

        public void StopGame()
        {
            NesCore?.Dispose();
            NesCore = null;
        }

        private void Update()
        {
            if (m_bPause) return;

            if (NesCore != null)
            {
                PushEmulatorFrame();
                if (InGameUI.Instance.IsNetPlay)
                    FixEmulatorFrame();

                var screenBuffer = NesCore.ppu.GetScreenPtr();
                var lineColorMode = NesCore.ppu.GetLineColorMode();
                VideoProvider.SetDrawData(screenBuffer, lineColorMode, 256, 240);
            }
        }

        private void FixEmulatorFrame()
        {
            int skipFrameCount = 0;
            var frameGap = App.roomMgr.netReplay.mDiffFrameCount;
            if (frameGap > 10000) return;

            if (frameGap > 3 && frameGap < 6) skipFrameCount = 1;
            else if (frameGap > 7 && frameGap < 12) skipFrameCount = 2;
            else if (frameGap > 13 && frameGap < 20) skipFrameCount = 3;
            else skipFrameCount = frameGap - 3;

            if (skipFrameCount > 0) App.log.Debug($"SKIP FRAME : {skipFrameCount}");
            for (int i = 0; i < skipFrameCount; i++)
            {
                if (!PushEmulatorFrame()) break;
            }
        }

        private bool PushEmulatorFrame()
        {
            Supporter.SampleInput();
            var controlState = Supporter.GetControllerState();

            //如果未收到Input数据,核心帧不推进
            if (!controlState.valid) return false;

            NesCore.pad.Sync(controlState);
            NesCore.EmulateFrame(true);

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

        public void SetupScheme()
        {
            ControlScheme.Current = ControlSchemeSetts.NES;
        }

        public void LoadState(object state)
        {
            NesCore.LoadState((State)state);
        }

        public object GetState()
        {
            return NesCore.GetState();
        }

        public byte[] GetStateBytes()
        {
            return NesCore.GetState().ToBytes();
        }

        public void LoadStateFromBytes(byte[] data)
        {
            State st = new State();
            st.FromByte(data);
            NesCore.LoadState(st);
        }
    }
}
