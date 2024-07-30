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

        private void Start()
        {
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
                m_nesIns.Command(NESCOMMAND.NESCMD_HWRESET);
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
                VideoProvider.SetDrawData(screenBuffer, PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT);
            }
        }
    }
}
