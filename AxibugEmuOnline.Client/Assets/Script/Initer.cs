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

        private void Awake()
        {
            App.Init(this);
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
        }
    }
}
