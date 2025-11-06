using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using Google.Protobuf;
using HaoYueNet.ClientNetwork;
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
            OnConnected += NetworkConnected;
            //指定接收服务器数据事件
            OnReceiveData += GetDataCallBack;
            //断开连接
            OnClose += OnConnectClose;
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

        void NetworkConnected(bool IsConnect)
        {
            NetMsg.Instance.EnqueueEventFromNet(() => NetworkConnected_Delegate(IsConnect));
        }

        void OnConnectClose()
        {
            NetMsg.Instance.EnqueueEventFromNet(() => OnConnectClose_Delegate());
        }

        void NetworkConnected_Delegate(bool IsConnect)
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

        void NetworkDeBugLog(string str)
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
        void GetDataCallBack(int CMDID, int ERRCODE, byte[] data)
        {
            try
            {
                //抛出网络数据
                //网络线程直接抛
                if (CMDID <= (int)CommandID.CmdPong)
                    NetMsg.Instance.PostNetMsgEvent(CMDID, ERRCODE, data);
                else//加入队列，主线程来取
                    NetMsg.Instance.EnqueueNetMsg(CMDID, ERRCODE, data);
            }
            catch (Exception ex)
            {
                NetworkDeBugLog("逻辑处理错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        void OnConnectClose_Delegate()
        {
            NetworkDeBugLog("OnConnectClose");
            Eventer.Instance.PostEvent(EEvent.OnLossLoginState);

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

        public void SendToServer(int CmdID, IMessage msg)
        {

            //SendToServer(CmdID, ProtoBufHelper.Serizlize(msg));
            //return;

            //            byte[] data2 = ProtoBufHelper.Serizlize(msg);
            //            base.SendToServerWithLength(CmdID, ref data2, data2.Length);
            //#if UNITY_EDITOR
            //            if (CmdID > (int)CommandID.CmdPong)
            //                NetworkDeBugLog("[NET]<color=cyan>" + CmdID + "|" + (CommandID)CmdID + "| alllen:" + data2.Length + " usedlength:" + data2 + "</color>");
            //#endif
            //            return;

            ProtoBufHelper.RentSerizlizeData(msg, out byte[] data, out int usedlength);
            base.SendToServerWithLength(CmdID, ref data, usedlength);
#if UNITY_EDITOR
            //if (CmdID > (int)CommandID.CmdPong)
                //NetworkDeBugLog("[NET]<color=cyan>" + CmdID + "|" + (CommandID)CmdID + "| alllen:"+ data.Length + " usedlength:" + usedlength + "</color>");
#endif
            ProtoBufHelper.ReturnSerizlizeData(data);
        }

        public new void SendToServer(int CmdID, byte[] data)
        {
#if UNITY_EDITOR
            //if(CmdID > (int)CommandID.CmdPong)
                //NetworkDeBugLog("[NET]<color=cyan>" + CmdID + "|" + (CommandID)CmdID + "| length:" + data.Length + "</color>");
#endif
            base.SendToServer(CmdID, data);
        }
    }
}
