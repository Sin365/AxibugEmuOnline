using AxibugEmuOnline.Client.Manager;
using AxibugEmuOnline.Client.Network;
using System;
using UnityEngine;
using static AxibugEmuOnline.Client.Manager.LogManager;

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
        public static AppGame game;

        public static void Init()
        {
            log = new LogManager();
            LogManager.OnLog += OnNoSugarNetLog;
            networkHelper = new NetworkHelper();
            login = new AppLogin();
            chat = new AppChat();
            user = new UserDataManager();
            game = new AppGame();
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
            Debug.Log("[AxibugEmuOnline]:"+msg);
        }
    }
}