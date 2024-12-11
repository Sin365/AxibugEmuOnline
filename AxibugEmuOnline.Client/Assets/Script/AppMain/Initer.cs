using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        public PostProcessVolume m_filterVolume;
        public CanvasGroup m_filterPreview;
        public CanvasGroup m_xmbBg;
        public static string dev_UUID;

#if UNITY_EDITOR
        public bool bTest = false;
        public string mTestSrvIP = "192.168.0.47";
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            App.Init(this, bTest, mTestSrvIP);
#else
            App.Init(this);
#endif
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
        }
    }
}
