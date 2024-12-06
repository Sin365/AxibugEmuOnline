using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using HaoYueNet.ClientNetworkNet.Standard2;
using System;
using System.Net.Sockets;
using System.Threading;

namespace AxibugEmuOnline.Client.Network
{
    /// <summary>
    /// 继承网络库，以支持网络功能
    /// </summary>
    public class NetworkHelper : NetworkHelperCore
    {
        public NetworkHelper()
        {
            //指定接收服务器数据事件
            OnReceiveData += GetDataCallBack;
            //断开连接
            OnClose += OnConnectClose;
            OnConnected += NetworkConnected;
            //网络库调试信息输出事件，用于打印网络内容
            OnLogOut += NetworkDeBugLog;
        }

        public delegate void OnReConnectedHandler();
        /// <summary>
        /// 重连成功事件
        /// </summary>
        public event OnReConnectedHandler OnReConnected;
        public bool isConnected => CheckIsConnectd();
        /// <summary>
        /// 是否自动重连
        /// </summary>
        public bool bAutoReConnect = true;
        /// <summary>
        /// 重连尝试时间
        /// </summary>
        const int ReConnectTryTime = 1000;

        public void NetworkConnected(bool IsConnect)
        {
            NetworkDeBugLog($"NetworkConnected:{IsConnect}");
            if (IsConnect)
            {
                //从未登录过
                if (!App.user.IsLoggedIn)
                {
                    //首次登录
                    App.login.Login();
                }
            }
            else
            {
                //连接失败
                NetworkDeBugLog("连接失败！");


                //自动重连开关
                if (bAutoReConnect)
                    ReConnect();
            }
        }

        public void NetworkDeBugLog(string str)
        {
            //用于Unity内的输出
            //Debug.Log("NetCoreDebug >> "+str);
            App.log.Info("NetCoreDebug >> " + str);
        }

        /// <summary>
        /// 接受包回调
        /// </summary>
        /// <param name="CMDID">协议ID</param>
        /// <param name="ERRCODE">错误编号</param>
        /// <param name="data">业务数据</param>
        public void GetDataCallBack(int CMDID, int ERRCODE, byte[] data)
        {
            //NetworkDeBugLog("收到消息 CMDID =>" + CMDID + " ERRCODE =>" + ERRCODE + " 数据长度=>" + data.Length);
            try
            {
                //抛出网络数据
                //网络线程直接抛
                if (CMDID == (int)CommandID.CmdPing || CMDID == (int)CommandID.CmdPong)
                    NetMsg.Instance.PostNetMsgEvent(CMDID, ERRCODE, data);
                else//加入队列，主线程来取
                    NetMsg.Instance.EnqueueNesMsg(CMDID, ERRCODE, data);
            }
            catch (Exception ex)
            {
                NetworkDeBugLog("逻辑处理错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void OnConnectClose()
        {
            NetworkDeBugLog("OnConnectClose");

            App.user.LoginOutData();

            //自动重连开关
            if (bAutoReConnect)
                ReConnect();
        }

        Thread ReConnectTask = null;
        /// <summary>
        /// 自动重连
        /// </summary>
        void ReConnect()
        {
            if (ReConnectTask != null)
                return;
            ReConnectTask = new Thread(() =>
            {
                bool bflagDone = false;
                do
                {
                    //等待时间
                    Thread.Sleep(ReConnectTryTime);
                    App.log.Info($"尝试自动重连{LastConnectIP}:{LastConnectPort}……");
                    //第一步
                    if (Init(LastConnectIP, LastConnectPort))
                    {
                        App.log.Info($"自动重连成功!");
                        bflagDone = true;
                        App.log.Info($"触发重连后的自动逻辑!");
                        OnReConnected?.Invoke();
                    }
                } while (!bflagDone && App.network.bAutoReConnect);

                if (!App.network.bAutoReConnect)
                {
                    if (App.network.isConnected)
                    {
                        App.network.CloseConntect();
                    }
                }
                ReConnectTask = null;
            });
            ReConnectTask.Start();
        }

        bool CheckIsConnectd()
        {
            Socket socket = GetClientSocket();
            if (socket == null)
                return false;
            return socket.Connected;
        }
    }
}
