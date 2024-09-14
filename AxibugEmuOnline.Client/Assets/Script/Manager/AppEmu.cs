using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        private GameObject m_emuInstance;

        public void BeginGame(RomFile romFile)
        {
            if (m_emuInstance != null) return;

            switch (romFile.Platform)
            {
                case EnumPlatform.NES:
                    var nesEmu = GameObject.Instantiate(Resources.Load<GameObject>("NES/NesEmulator")).GetComponent<NesEmulator>();
                    m_emuInstance = nesEmu.gameObject;

                    nesEmu.StartGame(romFile);
                    LaunchUI.Instance.HideMainMenu();
                    InGameUI.Instance.Show(romFile, nesEmu);
                    break;
            }
        }

        public void StopGame()
        {
            if (m_emuInstance == null) return;
            GameObject.Destroy(m_emuInstance);
            m_emuInstance = null;

            InGameUI.Instance.Hide();
            LaunchUI.Instance.ShowMainMenu();
        }
    }
}
