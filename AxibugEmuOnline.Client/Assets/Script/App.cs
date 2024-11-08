using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static AxibugEmuOnline.Client.HttpAPI;

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

        #region Mono
        public static TickLoop tickLoop;
        private static CoroutineRunner coRunner;
        #endregion

#if UNITY_PSP2
        public static string PersistentDataPath => "ux0:data/AxibugEmu";
#else
        public static string PersistentDataPath => Application.persistentDataPath;
#endif

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
            tickLoop = go.AddComponent<TickLoop>();
            coRunner = go.AddComponent<CoroutineRunner>();

            if (UnityEngine.Application.platform == RuntimePlatform.PSP2)
            {
                //PSV 等平台需要手动创建目录
                PersistentDataPathDir();
            }

            var importNode = GameObject.Find("IMPORTENT");
            if (importNode != null) GameObject.DontDestroyOnLoad(importNode);

            StartCoroutine(AppTickFlow());
            RePullNetInfo();
        }

        private static void PersistentDataPathDir()
        {
            if (!Directory.Exists(PersistentDataPath))
            {
                Directory.CreateDirectory(PersistentDataPath);
            }
        }

        private static IEnumerator AppTickFlow()
        {
            while (true)
            {
                Tick();
                yield return null;
            }
        }

        public static void RePullNetInfo()
        {
            StartCoroutine(StartNetInit());
        }

        static IEnumerator StartNetInit()
        {
            if (App.network.isConnected)
                yield break;

            int platform = 0;
            bool bTest = true;
            if (bTest)
            {
                yield return null;
                Connect("192.168.0.47", 10492);
                yield break;
            }

            UnityWebRequest request = UnityWebRequest.Get($"{App.httpAPI.WebSiteApi}/CheckStandInfo?platform={platform}&version={Application.version}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                yield break;

            App.log.Debug($"ApiResp => {request.downloadHandler.text}");
            Resp_CheckStandInfo resp = JsonUtility.FromJson<Resp_CheckStandInfo>(request.downloadHandler.text);
            //需要更新
            if (resp.needUpdateClient == 1)
            {
                //TODO
            }

            yield return null;
            //Connect("127.0.0.1", 10492);
            Connect(resp.serverIp, resp.serverPort);
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

        public static void Connect(string IP, int port)
        {
            Task task = new Task(() =>
            {
                network.Init(IP, port);
            });
            task.Start();
        }

        public static void Close()
        {
            App.log.Info("停止");
        }
        static void OnNoSugarNetLog(int LogLevel, string msg)
        {
            Debug.Log("[AxibugEmuOnline]:" + msg);
        }

    }
}