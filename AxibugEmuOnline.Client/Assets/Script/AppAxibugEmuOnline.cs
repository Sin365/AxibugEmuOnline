using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using System.Collections;
using UnityEngine;

namespace AxibugEmuOnline.Client.ClientCore
{
    public class AppAxibugEmuOnline
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
        public static RomLib romLib;
        public static HttpAPI httpAPI;

        private static CoroutineRunner coRunner;

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
            romLib = new RomLib();
            httpAPI = new HttpAPI();

            var go = new GameObject("[AppAxibugEmuOnline]");
            GameObject.DontDestroyOnLoad(go);
            coRunner = go.AddComponent<CoroutineRunner>();
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