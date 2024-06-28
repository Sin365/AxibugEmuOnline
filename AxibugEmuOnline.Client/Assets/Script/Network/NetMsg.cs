using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client.Network
{

    public class NetMsg
    {
        private static NetMsg instance = new NetMsg();
        public static NetMsg Instance { get { return instance; } }

        private Dictionary<int, List<Delegate>> netEventDic = new Dictionary<int, List<Delegate>>(128);

        private NetMsg() { }


        #region RegisterMsgEvent

        public void RegNetMsgEvent(int cmd, Action<byte[]> callback)
        {
            InterRegNetMsgEvent(cmd, callback);
        }

        private void InterRegNetMsgEvent(int cmd, Delegate callback)
        {
            if (netEventDic.ContainsKey(cmd))
            {
                if (netEventDic[cmd].IndexOf(callback) < 0)
                {
                    netEventDic[cmd].Add(callback);
                }
            }
            else
            {
                netEventDic.Add(cmd, new List<Delegate>() { callback });
            }
        }
        #endregion

        #region UnregisterCMD

        public void UnregisterCMD(int evt, Action<byte[]> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterCMD(evt, tempDelegate);
        }

        private void InterUnregisterCMD(int cmd, Delegate callback)
        {
            if (netEventDic.ContainsKey(cmd))
            {
                netEventDic[cmd].Remove(callback);
                if (netEventDic[cmd].Count == 0) netEventDic.Remove(cmd);
            }
        }
        #endregion

        #region PostEvent
        public void PostNetMsgEvent(int cmd, byte[] arg)
        {
            List<Delegate> eventList = GetNetEventDicList(cmd);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<byte[]>)callback)(arg);
                    }
                    catch (Exception e)
                    {
                        AppAxibugEmuOnline.log.Error(e.Message);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取所有事件
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private List<Delegate> GetNetEventDicList(int cmd)
        {
            if (netEventDic.ContainsKey(cmd))
            {
                List<Delegate> tempList = netEventDic[cmd];
                if (null != tempList)
                {
                    return tempList;
                }
            }
            return null;
        }
    }
}
