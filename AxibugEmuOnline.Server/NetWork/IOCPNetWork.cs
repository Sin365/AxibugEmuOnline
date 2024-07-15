using HaoYueNet.ServerNetwork;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.NetWork
{
    public class IOCPNetWork : TcpSaeaServer
    {
        public IOCPNetWork(int numConnections, int receiveBufferSize)
            : base(numConnections, receiveBufferSize)
        {
            OnClientNumberChange += ClientNumberChange;
            OnReceive += ReceiveData;
            OnDisconnected += OnDisconnect;
            OnNetLog += OnShowNetLog;
        }

        private void ClientNumberChange(int num, AsyncUserToken token)
        {
            Console.WriteLine("Client数发生变化");
        }

        /// <summary>
        /// 接受包回调
        /// </summary>
        /// <param name="CMDID">协议ID</param>
        /// <param name="ERRCODE">错误编号</param>
        /// <param name="data">业务数据</param>
        private void ReceiveData(AsyncUserToken token, int CMDID, byte[] data)
        {
            DataCallBack(token.Socket, CMDID, data);
        }

        public void DataCallBack(Socket sk, int CMDID, byte[] data)
        {
            //AppSrv.g_Log.Debug("收到消息 CMDID =>" + CMDID + " 数据长度=>" + data.Length);
            try
            {
                //抛出网络数据
                NetMsg.Instance.PostNetMsgEvent(CMDID, sk, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("逻辑处理错误：" + ex.ToString());
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sk"></param>
        public void OnDisconnect(AsyncUserToken token)
        {
            Console.WriteLine("断开连接");
            AppSrv.g_ClientMgr.SetClientOfflineForSocket(token.Socket);
        }

        public void OnShowNetLog(string msg)
        {
            AppSrv.g_Log.Debug(msg);
        }

    }
}
