using UnityEngine;
using VirtualNes.Core;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : MonoBehaviour
    {
        private NES m_nesIns;

        private void Start()
        {

            //StartGame("Kirby.nes");
        }

        public void StartGame(string romName)
        {
            StopGame();

            Supporter.Setup(new CoreSupporter());
            Debuger.Setup(new CoreDebuger());
            m_nesIns = new NES(romName);
            m_nesIns.Command(NESCOMMAND.NESCMD_HWRESET);
        }

        public void StopGame()
        {
            m_nesIns?.Dispose();
            m_nesIns = null;
        }

        private void Update()
        {
            m_nesIns?.EmulateFrame(true);
        }
    }
}
