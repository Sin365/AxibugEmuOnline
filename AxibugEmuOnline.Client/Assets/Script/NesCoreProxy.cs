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

        public AudioSource AS;
        public RawImage DrawImage;
        public DefaultAudioOutput DO;
        public Text Fps;

        private AppEmu m_appEnum = new AppEmu();

        private void Start()
        {
            Instance = this;
            m_appEnum.Init();
        }

        private void Update()
        {
            m_appEnum.Update();
        }

        private void OnDestroy()
        {
            Instance = null;
            m_appEnum.Dispose();
        }
    }
}
