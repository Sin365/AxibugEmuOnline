using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using System;
using System.Collections;
using UnityEngine;

namespace AxibugEmuOnline.Client.ClientCore
{
    public static class AppAxibugEmuOnline
    {
        public static string TokenStr;
        public static long RID = -1;
        public static string IP;
        public static int Port;
        public static LogManager log;
        public static NetworkHelper networkHelper;
        public static AppLogin login;
        public static AppChat chat;
        public static UserDataManager user;
        public static AppNetGame netgame;
        public static AppEmu emu;
        public static RomLib nesRomLib;
        public static HttpAPI httpAPI;
        public static CacheManager CacheMgr;

        private static CoroutineRunner coRunner;

        public static string PersistentDataPath => Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            log = new LogManager();
            LogManager.OnLog += OnNoSugarNetLog;
            networkHelper = new NetworkHelper();
            login = new AppLogin();
            chat = new AppChat();
            user = new UserDataManager();
            emu = new AppEmu();
            netgame = new AppNetGame();
            httpAPI = new HttpAPI();
            nesRomLib = new RomLib(EnumPlatform.NES);
            CacheMgr = new CacheManager();

            var go = new GameObject("[AppAxibugEmuOnline]");
            GameObject.DontDestroyOnLoad(go);
            coRunner = go.AddComponent<CoroutineRunner>();

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
            return networkHelper.Init(IP, port);
        }

        public static void Close()
        {
            AppAxibugEmuOnline.log.Info("停止");
        }
        static void OnNoSugarNetLog(int LogLevel, string msg)
        {
            Debug.Log("[AxibugEmuOnline]:" + msg);
        }
    }
}