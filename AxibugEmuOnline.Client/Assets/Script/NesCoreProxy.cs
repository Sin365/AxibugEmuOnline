using AxibugEmuOnline.Client.Input;
using AxibugEmuOnline.Client.Manager;
using MyNes.Core;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class NesCoreProxy : MonoBehaviour
    {
        public UguiVideoProvider VideoCom;
        public AudioProvider AudioCom;
        public InputManager InputManager;

        private AppEmu m_appEnum = new AppEmu();

        private void Start()
        {
            m_appEnum.Init(VideoCom, AudioCom, InputManager);

            m_appEnum.LoadGame("kirby.nes");
        }

        private void OnDestroy()
        {
            m_appEnum.Dispose();
        }
    }
}
