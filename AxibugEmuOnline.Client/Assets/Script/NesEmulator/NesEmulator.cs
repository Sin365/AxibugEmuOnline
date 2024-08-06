using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Xml.Linq;
using UnityEngine;
using VirtualNes.Core;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : MonoBehaviour
    {
        public NES NesCore { get; private set; }

        public VideoProvider VideoProvider;
        public AudioProvider AudioProvider;

#if UNITY_EDITOR
        public string RomName;
#endif

        private void Start()
        {
            Application.targetFrameRate = 60;
            VideoProvider.NesEmu = this;
            AudioProvider.NesEmu = this;

#if UNITY_EDITOR
            StartGame(RomName);
#endif
        }

        public void StartGame(string romName)
        {
            StopGame();

            Supporter.Setup(new CoreSupporter());
            Debuger.Setup(new CoreDebuger());

            try
            {
                NesCore = new NES(romName);
            }
            catch (Exception ex)
            {
                NesCore = null;
                Debug.LogError(ex);
            }
        }

        public void StopGame()
        {
            NesCore?.Dispose();
            NesCore = null;
        }

        private void Update()
        {
            if (NesCore != null)
            {
                var controlState = Supporter.GetControllerState();
                NesCore.pad.Sync(controlState);
                NesCore.EmulateFrame(true);

                var screenBuffer = NesCore.ppu.GetScreenPtr();
                var lineColorMode = NesCore.ppu.GetLineColorMode();
                VideoProvider.SetDrawData(screenBuffer, lineColorMode, 256, 240);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("IMPORT")]
        public void TTTA()
        {
            var db = Resources.Load<RomDB>("NES/ROMDB");
            db.Clear();

            var dbFile = Resources.Load<TextAsset>("NES/nes20db");
            var xml = XDocument.Parse(dbFile.text);
            var games = xml.Element("nes20db").Elements("game");
            foreach (var game in games)
            {
                var crcStr = game.Element("rom").Attribute("crc32").Value;
                var crc = uint.Parse($"{crcStr}", System.Globalization.NumberStyles.HexNumber);

                var mapper = int.Parse($"{game.Element("pcb").Attribute("mapper").Value}");

                db.AddInfo(new RomDB.RomInfo { CRC = crc, Mapper = mapper });
            }

            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}
