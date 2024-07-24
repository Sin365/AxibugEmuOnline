using UnityEngine;
using VirtualNes.Core;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class NesEmulator : MonoBehaviour
    {
        private void Start()
        {
            StartGame("Kirby.nes");
        }

        public void StartGame(string romName)
        {
            Supporter.Setup(new CoreSupporter());
            Debuger.Setup(new CoreDebuger());
            NES nes = new NES(romName);
        }
    }
}
