using AxibugEmuOnline.Client.ClientCore;
using System.Collections.Generic;
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
        List<Shader> SHADER;

        [SerializeField]
        GameObject IMPORTENT;

        //public DebuggerByGUI debugger;
        //public static DebuggerByGUI debugger_instance;

        public Transform debugger;
        public static Transform debugger_instance;

        public static VerScriptable versionInfo;


#if UNITY_EDITOR
        public bool bTestSkipWebApiToConServer = false;
        public string mTestSrvIP = "192.168.0.47";
        public bool bUseLocalWebApi = false;
        public string mLocalWebApi = "http://localhost:5051";
        public bool bEditorUUID = false;
        public bool bEditorOpenGUIJoyStick = false;
#endif

        private void Awake()
        {
            //PC关闭事件监听
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
            Application.wantsToQuit += ChangeByPassClose;
#endif
            versionInfo = Resources.Load<VerScriptable>("Version/VersionInfo");
            GameObject.DontDestroyOnLoad(debugger);
            debugger_instance = debugger;
            bool UseJoyStack = false;

            if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) && Application.platform != RuntimePlatform.WindowsEditor)
            {
                UseJoyStack = true;
            }

#if UNITY_EDITOR
            if (bEditorOpenGUIJoyStick)
                UseJoyStack = true;
            App.Init(bTestSkipWebApiToConServer, UseJoyStack, mTestSrvIP, bUseLocalWebApi, mLocalWebApi);
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
            if (bEditorUUID)
            {
                dev_UUID += "_Editor";
            }
#else
            App.Init(false,UseJoyStack);
            dev_UUID = SystemInfo.deviceUniqueIdentifier;
#endif

            m_refs = Instantiate(IMPORTENT, transform).GetComponent<GlobalRef>();
        }

        private void Start()
        {
            App.settings.Filter.ShutDownFilterPreview();
            App.settings.Filter.ShutDownFilter();
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN //Unity6 关闭后进程残留的问题,PC版调用Win32API 杀死自己
        static bool ChangeByPassClose()
        {
            try { SelfKill.KillSelf(); } catch { };
            try { System.Diagnostics.Process.GetCurrentProcess().Kill(); } catch { };
            return true;
        }

        public static class SelfKill
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
            private static extern System.IntPtr GetCurrentProcess();

            [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
            private static extern void TerminateProcess(System.IntPtr hProcess, uint uExitCode);

            public static void KillSelf()
            {
                System.IntPtr hProc = GetCurrentProcess();
                TerminateProcess(hProc, 0);
            }
        }
#endif
    }
}