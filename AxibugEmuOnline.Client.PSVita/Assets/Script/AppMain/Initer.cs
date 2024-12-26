using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        public CanvasGroup m_filterPreview;
        public CanvasGroup m_xmbBg;
        public static string dev_UUID;
        public RenderTexture renderTest;
        public static Initer instance;

#if UNITY_EDITOR
        public bool bTest = false;
        public string mTestSrvIP = "192.168.0.47";
#endif

        private void Awake()
        {
            instance = this;
#if UNITY_EDITOR
            App.Init(this, bTest, mTestSrvIP);
#else
            App.Init(this);
#endif
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
        }
    }
}
