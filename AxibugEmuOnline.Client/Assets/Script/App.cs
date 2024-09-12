using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using System;
using System.Collections;
using UnityEngine;

namespace AxibugEmuOnline.Client.ClientCore
{
    public static class App
    {
        public static string TokenStr;
        public static string IP;
        public static int Port;
        public static LogManager log;
        public static NetworkHelper network;
        public static AppLogin login;
        public static AppChat chat;
        public static UserDataManager user;
        //public static AppNetGame netgame;
        public static AppEmu emu;
        public static RomLib nesRomLib;
        public static HttpAPI httpAPI;
        public static CacheManager CacheMgr;
        public static AppSceneLoader SceneLoader;
        public static AppRoom roomMgr;

        private static CoroutineRunner coRunner;

        public static string PersistentDataPath => Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            log = new LogManager();
            LogManager.OnLog += OnNoSugarNetLog;
            network = new NetworkHelper();
            login = new AppLogin();
            chat = new AppChat();
            user = new UserDataManager();
            emu = new AppEmu();
            //netgame = new AppNetGame();
            httpAPI = new HttpAPI();
            nesRomLib = new RomLib(EnumPlatform.NES);
            CacheMgr = new CacheManager();
            SceneLoader = new AppSceneLoader();
            roomMgr = new AppRoom();

            var go = new GameObject("[AppAxibugEmuOnline]");
            GameObject.DontDestroyOnLoad(go);
            coRunner = go.AddComponent<CoroutineRunner>();

            var importNode = GameObject.Find("IMPORTENT");
            GameObject.DontDestroyOnLoad(importNode);

            StartCoroutine(AppTickFlow());
        }

        private static IEnumerator AppTickFlow()
        {
            while (true)
            {
                Tick();
                yield return null;
            }
        }

        private static void Tick()
        {
            nesRomLib.ExecuteFetchRomInfo();
        }

        public static Coroutine StartCoroutine(IEnumerator itor)
        {
            return coRunner.StartCoroutine(itor);
        }

        public static void StopCoroutine(Coroutine cor)
        {
            coRunner.StopCoroutine(cor);
        }

        public static bool Connect(string IP, int port)
        {
            return network.Init(IP, port);
        }

        public static void Close()
        {
            App.log.Info("停止");
        }
        static void OnNoSugarNetLog(int LogLevel, string msg)
        {
            Debug.Log("[AxibugEmuOnline]:" + msg);
        }

        private static RomFile m_currentGame;
        public static void BeginGame(RomFile romFile)
        {
            if (m_currentGame != null) return;

            m_currentGame = romFile;

            switch (romFile.Platform)
            {
                case EnumPlatform.NES:
                    SceneLoader.BeginLoad("Scene/Emu_NES", () =>
                    {
                        LaunchUI.Instance.HideMainMenu();
                        var nesEmu = GameObject.FindObjectOfType<NesEmulator>();
                        nesEmu.StartGame(romFile);
                    });
                    break;
            }
        }

        public static void StopGame()
        {
            if (m_currentGame == null) return;

            SceneLoader.BeginLoad("Scene/AxibugEmuOnline.Client", () =>
            {
                LaunchUI.Instance.ShowMainMenu();
            });
        }
    }
}