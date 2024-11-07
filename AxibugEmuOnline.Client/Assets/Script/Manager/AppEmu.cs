using AxibugEmuOnline.Client.ClientCore;
using MyNes.Core;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        private GameObject m_emuInstance;
        private IEmuCore m_emuCore;

        public void BeginGame(RomFile romFile)
        {
            if (m_emuInstance != null) return;

            switch (romFile.Platform)
            {
                case EnumPlatform.NES:
                    m_emuCore = GameObject.Instantiate(Resources.Load<GameObject>("NES/NesEmulator")).GetComponent<IEmuCore>();
                    break;
            }

            m_emuInstance = m_emuCore.gameObject;

            m_emuCore.StartGame(romFile);
            LaunchUI.Instance.HideMainMenu();
            InGameUI.Instance.Show(romFile, m_emuCore);

            m_emuCore.SetupScheme();
        }

        public void StopGame()
        {
            if (m_emuInstance == null) return;
            GameObject.Destroy(m_emuInstance);
            m_emuInstance = null;

            InGameUI.Instance.Hide();
            LaunchUI.Instance.ShowMainMenu();

            ControlScheme.Current = ControlSchemeSetts.Normal;
        }
    }
}
