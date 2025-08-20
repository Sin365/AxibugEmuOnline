﻿using AxibugEmuOnline.Client.ClientCore;
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

        public DebuggerByGUI debugger;


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
            GameObject.DontDestroyOnLoad(debugger);
            bool UseJoyStack = false;

            if (Application.platform == RuntimePlatform.Android && Application.platform != RuntimePlatform.WindowsEditor)
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
    }
}
