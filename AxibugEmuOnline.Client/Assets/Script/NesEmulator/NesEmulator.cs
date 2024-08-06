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
        private NES m_nesIns;

        public VideoProvider VideoProvider;
        public AudioProvider AudioProvider;

        private void Start()
        {
            Application.targetFrameRate = 60;
            StartGame("ff1.nes");
        }

        public void StartGame(string romName)
        {
            StopGame();

            Supporter.Setup(new CoreSupporter());
            Debuger.Setup(new CoreDebuger());

            try
            {
                m_nesIns = new NES(romName);
            }
            catch (Exception ex)
            {
                m_nesIns = null;
                Debug.LogError(ex);
            }
        }

        public void StopGame()
        {
            m_nesIns?.Dispose();
            m_nesIns = null;
        }

        private void Update()
        {
            if (m_nesIns != null)
            {
                m_nesIns.EmulateFrame(true);

                var screenBuffer = m_nesIns.ppu.GetScreenPtr();
                var lineColorMode = m_nesIns.ppu.GetLineColorMode();
                VideoProvider.SetDrawData(screenBuffer, lineColorMode, 256, 240);

                AudioProvider.ProcessSound(m_nesIns);
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
