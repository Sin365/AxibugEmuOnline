using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static AxibugEmuOnline.Client.HttpAPI;
using static AxibugEmuOnline.Client.Manager.LogManager;

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
        public static AppRoom roomMgr;
        public static AppSettings settings;
        public static AppShare share;
        static bool bTest;
        static string mTestSrvIP;
        #region Mono
        public static TickLoop tickLoop;
        private static CoroutineRunner coRunner;

#if UNITY_PSP2
        public static SonyVitaCommonDialog sonyVitaCommonDialog;
#endif

        #endregion

#if UNITY_PSP2 && !UNITY_EDITOR //PSV真机
        public static string PersistentDataPath => "ux0:data/AxibugEmu";
#else
        public static string PersistentDataPath => Application.persistentDataPath;
#endif
        public static void Init( bool isTest = false, string testSrvIP = "")
        {
            log = new LogManager(OnLogOut);

            //其他平台必要的初始化
            if (UnityEngine.Application.platform == RuntimePlatform.PSP2)
            {
                PSP2Init();
            }

            settings = new AppSettings();
            network = new NetworkHelper();
            login = new AppLogin();
            chat = new AppChat();
            user = new UserDataManager();
            emu = new AppEmu();
            //netgame = new AppNetGame();
            httpAPI = new HttpAPI();
            nesRomLib = new RomLib(RomPlatformType.Nes);
            CacheMgr = new CacheManager();
            roomMgr = new AppRoom();
            share = new AppShare();
            bTest = isTest;
            mTestSrvIP = testSrvIP;
            var go = new GameObject("[AppAxibugEmuOnline]");
            GameObject.DontDestroyOnLoad(go);
            tickLoop = go.AddComponent<TickLoop>();
            coRunner = go.AddComponent<CoroutineRunner>();


            var importNode = GameObject.Find("IMPORTENT");
            if (importNode != null) GameObject.DontDestroyOnLoad(importNode);

            StartCoroutine(AppTickFlow());
            RePullNetInfo();
        }


        private static void PSP2Init()
        {
            //PSVita最好手动创建目录
            if (!Directory.Exists(PersistentDataPath))
                Directory.CreateDirectory(PersistentDataPath);

#if UNITY_PSP2
            //创建PSV弹窗UI
            sonyVitaCommonDialog = new GameObject().AddComponent<SonyVitaCommonDialog>();
            //释放解码 FMV的26M内存，一般游戏用不上（PSP才用那破玩意儿）
            UnityEngine.PSVita.PSVitaVideoPlayer.TransferMemToMonoHeap();
#endif

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
            if (bTest)
            {
                yield return null;
                Connect(mTestSrvIP, 10492);
                yield break;
            }

            bool bHttpCheckDone = false;
            Resp_CheckStandInfo resp = null;
            while (true)
            {
                AxiHttpProxy.SendWebRequestProxy request = AxiHttpProxy.Get($"{App.httpAPI.WebSiteApi}/CheckStandInfo?platform={platform}&version={Application.version}");
                yield return request.SendWebRequest;
                if (!request.downloadHandler.isDone)
                {
                    bHttpCheckDone = false;
                }
                else if (request.downloadHandler.bHadErr)
                {
                    bHttpCheckDone = false;
                    App.log.Error(request.downloadHandler.ErrInfo);
                }
                else
                {
                    try
                    {
                        resp = JsonUtility.FromJson<Resp_CheckStandInfo>(request.downloadHandler.text);
                        bHttpCheckDone = true;
                    }
                    catch (Exception ex)
                    {
                        bHttpCheckDone = false;
                        App.log.Error(ex.ToString());
                    }
                }

                //请求成功
                if (bHttpCheckDone)
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(1);
                    App.log.Debug("请求失败，重试请求API...");
                }
            }

            /*UnityWebRequest request = UnityWebRequest.Get($"{App.httpAPI.WebSiteApi}/CheckStandInfo?platform={platform}&version={Application.version}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                yield break;

            App.log.Debug($"ApiResp => {request.downloadHandler.text}");
            Resp_CheckStandInfo resp = JsonUtility.FromJson<Resp_CheckStandInfo>(request.downloadHandler.text);*/

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
        static void OnLogOut(int LogLevel, string msg)
        {
            E_LogType logType = (E_LogType)LogLevel;
            switch (logType)
            {
                case E_LogType.Debug:
                case E_LogType.Info:
                    Debug.Log("[AxiNet]:" + msg);
                    break;
                case E_LogType.Warning:
                    Debug.LogWarning("[AxiNet]:" + msg);
                    break;
                case E_LogType.Error:
                    Debug.LogError("[AxiNet]:" + msg);
                    break;
            }
        }

    }
}