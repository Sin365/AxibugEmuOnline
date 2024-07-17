using AxibugEmuOnline.Client.Manager;
using MyNes.Core;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class NesCoreProxy : MonoBehaviour
    {
        public static NesCoreProxy Instance { get; private set; }

        public UguiVideoProvider VideoCom;
        public AudioProvider AudioCom;

        private AppEmu m_appEnum = new AppEmu();

        private void Start()
        {
            Instance = this;
            m_appEnum.Init(VideoCom, AudioCom);
        }

        private void OnDestroy()
        {
            Instance = null;
            m_appEnum.Dispose();
        }
    }
}
