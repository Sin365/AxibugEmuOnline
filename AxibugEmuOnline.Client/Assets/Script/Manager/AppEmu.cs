using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        public void BeginGame(RomFile romFile)
        {
            if (InGameUI.Instance.Enable) return;

            switch (romFile.Platform)
            {
                case EnumPlatform.NES:
                    App.SceneLoader.BeginLoad("Scene/Emu_NES", () =>
                    {
                        var nesEmu = GameObject.FindObjectOfType<NesEmulator>();
                        nesEmu.StartGame(romFile);

                        LaunchUI.Instance.HideMainMenu();
                        InGameUI.Instance.Show(romFile, nesEmu);
                    });
                    break;
            }
        }

        public void StopGame()
        {
            if (!InGameUI.Instance.enabled) return;

            App.SceneLoader.BeginLoad("Scene/AxibugEmuOnline.Client", () =>
            {
                InGameUI.Instance.Hide();
                LaunchUI.Instance.ShowMainMenu();
            });
        }
    }
}
