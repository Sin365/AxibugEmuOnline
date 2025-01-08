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
        public bool bTestSkipWebApiToConServer = false;
        public string mTestSrvIP = "192.168.0.47";
        public bool bUseLocalWebApi = false;
        public string mLocalWebApi = "http://localhost:5051";
        public bool bEditorUUID = false;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            App.Init(bTestSkipWebApiToConServer, mTestSrvIP, bUseLocalWebApi,mLocalWebApi);
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
            if (bEditorUUID)
            {
                dev_UUID += "_Editor";
            }
#else
            App.Init(this);
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
#endif

            m_refs = Instantiate(IMPORTENT, transform).GetComponent<GlobalRef>();
        }

        private void Start()
        {
            App.settings.Filter.ShutDownFilterPreview();
            App.settings.Filter.ShutDownFilter();
        }
    }
}
