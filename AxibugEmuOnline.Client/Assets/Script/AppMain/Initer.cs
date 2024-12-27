using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class Initer : MonoBehaviour
    {
        public CanvasGroup FilterPreview => m_refs.FilterPreview;
        public CanvasGroup XMBBg => m_refs.XMBBg;

        public static string dev_UUID;

        [SerializeField]
        GameObject IMPORTENT;

        GlobalRef m_refs;

#if UNITY_EDITOR
        public bool bTest = false;
        public string mTestSrvIP = "192.168.0.47";
#endif

        private void Awake()
        {
            m_refs = Instantiate(IMPORTENT, transform).GetComponent<GlobalRef>();
            
#if UNITY_EDITOR
            App.Init(this, bTest, mTestSrvIP);
#else
            App.Init(this);
#endif
            dev_UUID = SystemInfo.deviceUniqueIdentifier;

        }
    }
}
