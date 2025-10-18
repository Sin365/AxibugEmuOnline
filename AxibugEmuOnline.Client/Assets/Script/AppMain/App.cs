using AxibugEmuOnline.Client.InputDevices;
using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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
        public static InputDevicesManager input;
        public static AppEmu emu;
        public static HttpAPI httpAPI;
        public static CacheManager CacheMgr;
        public static AppRoom roomMgr;
        public static AppSettings settings;
        public static AppShare share;
        public static SaveSlotManager SavMgr;
        public static FileDownloader FileDownloader;
        static bool bTest;
        static string mTestSrvIP;
        public static bool bUseGUIButton;


        /// <summary> 收藏 Rom库 </summary>
        public static RomLib starRomLib;
        private static Dictionary<RomPlatformType, RomLib> s_romLibs = new Dictionary<RomPlatformType, RomLib>();

        #region Mono
        public static TickLoop tick;
        public static AudioMgr audioMgr;
        private static CoroutineRunner coRunner;

#if UNITY_PSP2
        public static SonyVitaCommonDialog sonyVitaCommonDialog;
#endif

#if UNITY_SWITCH
        public static SwitchCommon switchCommon;
#endif

        #endregion


        static string s_persistentRoot =
#if UNITY_PSP2 && !UNITY_EDITOR //PSV真机
            "ux0:data/AxibugEmu";
#elif UNITY_SWITCH && !UNITY_EDITOR //Switch 真机
            $"save:/AxibugEmu";
#else
            Application.persistentDataPath;
#endif
        public static string PersistentDataRootPath()
        {
            return s_persistentRoot;
        }

        public static string PersistentDataPath(RomPlatformType emuPlatform)
        {
            return s_persistentRoot + "/" + emuPlatform.ToString();
        }

        public static string UserPersistenDataPath(RomPlatformType emuPlatform)
        {
            return string.Format("{0}/{1}", PersistentDataPath(emuPlatform), user.userdata.UID);
        }

        public static string PersistentDataRoot() => s_persistentRoot;

        public static RomLib GetRomLib(RomPlatformType platform)
        {
            return s_romLibs[platform];
        }

        public static void Init(bool isTest = false, bool isUseGUIButton = false, string testSrvIP = "", bool bUseLocalWebApi = false, string mLocalWebApi = "")
        {
            log = new LogManager(OnLogOut);
            //其他平台必要的初始化
#if UNITY_PSP2
            PSP2Init();
#endif

#if UNITY_SWITCH
            SwitchInit();
#endif

            input = new InputDevicesManager();
            FileDownloader = new FileDownloader();
            settings = new AppSettings();
            network = new NetworkHelper();
            login = new AppLogin();
            chat = new AppChat();
            user = new UserDataManager();
            emu = new AppEmu();
            httpAPI = new HttpAPI();
            if (bUseLocalWebApi)
                httpAPI.WebHost = mLocalWebApi;

            foreach (RomPlatformType plat in Enum.GetValues(typeof(RomPlatformType)))
            {
                if (plat == RomPlatformType.All || plat == RomPlatformType.Invalid) continue;

                s_romLibs[plat] = new RomLib(plat);
            }

            starRomLib = new RomLib();
            CacheMgr = new CacheManager();
            roomMgr = new AppRoom();
            share = new AppShare();
            SavMgr = new SaveSlotManager();


            bTest = isTest;
            bUseGUIButton = isUseGUIButton;
            mTestSrvIP = testSrvIP;
            var go = new GameObject("[AppAxibugEmuOnline]");
            GameObject.DontDestroyOnLoad(go);
            tick = go.AddComponent<TickLoop>();
            audioMgr = go.AddComponent<AudioMgr>();
            coRunner = go.AddComponent<CoroutineRunner>();


            var importNode = GameObject.Find("IMPORTENT");
            if (importNode != null) GameObject.DontDestroyOnLoad(importNode);

            StartCoroutine(AppTickFlow());
            RePullNetInfo();
        }


        private static void PSP2Init()
        {
            //PSVita最好手动创建目录
            if (!AxiIO.Directory.Exists("ux0:data/AxibugEmu"))
                AxiIO.Directory.CreateDirectory("ux0:data/AxibugEmu");
            //if (!Directory.Exists("ux0:data/AxibugEmu"))
            //    Directory.CreateDirectory("ux0:data/AxibugEmu");

#if UNITY_PSP2
            //释放解码 FMV的26M内存，一般游戏用不上（PSP才用那破玩意儿）
            UnityEngine.PSVita.PSVitaVideoPlayer.TransferMemToMonoHeap();
            //创建PSV弹窗UI
            sonyVitaCommonDialog = new GameObject().AddComponent<SonyVitaCommonDialog>();
#endif


        }
        private static void SwitchInit()
        {
#if UNITY_SWITCH
            AxiNS.instance.Init();
            if (!AxiIO.Directory.Exists(App.PersistentDataRootPath()))
                AxiIO.Directory.CreateDirectory(App.PersistentDataRootPath());
            switchCommon = new GameObject().AddComponent<SwitchCommon>();
            switchCommon.gameObject.name = "[SwitchCommon]";
            GameObject.DontDestroyOnLoad(switchCommon);
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
                    App.log.Error(request.downloadHandler.errInfo);
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
            foreach (var romLib in s_romLibs.Values) romLib.ExecuteFetchRomInfo();
            starRomLib.ExecuteFetchRomInfo();
            FileDownloader.Update();
            input.Update();
            SavMgr.Update();
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
                    Debug.Log(msg);
                    break;
                case E_LogType.Warning:
                    Debug.LogWarning(msg);
                    break;
                case E_LogType.Error:
                    Debug.LogError(msg);
                    break;
            }
        }

    }
}