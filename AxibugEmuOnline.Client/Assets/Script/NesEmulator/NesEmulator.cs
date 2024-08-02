using System;
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
            StartGame("Kirby.nes");
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
    }
}
