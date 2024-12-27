using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        static GlobalRef m_refs;
        public static CanvasGroup FilterPreview => m_refs.FilterPreview;
        public static CanvasGroup XMBBg => m_refs.XMBBg;

        public static string dev_UUID;

        [SerializeField]
        GameObject IMPORTENT;


#if UNITY_EDITOR
        public bool bTest = false;
        public string mTestSrvIP = "192.168.0.47";
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            App.Init(bTest, mTestSrvIP);
#else
            App.Init(this);
#endif
            dev_UUID = SystemInfo.deviceUniqueIdentifier;

            m_refs = Instantiate(IMPORTENT, transform).GetComponent<GlobalRef>();
        }

        private void Start()
        {
            App.settings.Filter.ShutDownFilterPreview();
            App.settings.Filter.ShutDownFilter();
        }
    }
}
