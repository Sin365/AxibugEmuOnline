using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.Network
{

    public class NetMsg
    {
        private static NetMsg instance = new NetMsg();
        public static NetMsg Instance { get { return instance; } }

        private Dictionary<int, List<Delegate>> netEventDic = new Dictionary<int, List<Delegate>>(128);

        private Queue<ValueTuple<int, int, byte[]>> queueNetMsg = new Queue<ValueTuple<int, int, byte[]>>();
        public static object lockQueueNetMsg = new object();

        private Queue<Action> queueEventFromNet = new Queue<Action>();
        public static object lockQueueEventFromNet = new object();


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


        public void NextNetEvent()
        {
            DequeueNesMsg();
            DequeueEventFromNet();
        }

        #region PostEventFromNet

        public void EnqueueEventFromNet(Action act)
        {
            lock (lockQueueEventFromNet)
            {
                queueEventFromNet.Enqueue(act);
            }
        }

        public void DequeueEventFromNet()
        {
            lock (lockQueueEventFromNet)
            {
                while (queueEventFromNet.Count > 0)
                {
                    var msgData = queueEventFromNet.Dequeue();
                    PostNetEventFromNet(msgData);
                }
            }
        }

        void PostNetEventFromNet(Action act)
        {
            try
            { 
                act?.Invoke();
            }
            catch(Exception ex)
            {
                App.log.Error(ex.ToString());
            }
        }
        #endregion

        #region PostNetMsg

        public void EnqueueNetMsg(int cmd, int ERRCODE, byte[] arg)
        {
            lock (lockQueueNetMsg)
            {
                queueNetMsg.Enqueue(new ValueTuple<int, int, byte[]>(cmd, ERRCODE, arg));
            }
        }

        public void DequeueNesMsg()
        {
            lock (lockQueueNetMsg)
            {
                while (queueNetMsg.Count > 0)
                {
                    var msgData = queueNetMsg.Dequeue();
                    PostNetMsgEvent(msgData.Item1, msgData.Item2, msgData.Item3);
                }
            }
        }

        public void PostNetMsgEvent(int cmd, int ERRCODE, byte[] arg)
        {
            ErrorCode err = ((ErrorCode)ERRCODE);
            if (err != ErrorCode.ErrorOk)
            {
                string errMsg = string.Empty;
                switch (err)
                {
                    case ErrorCode.ErrorRoomNotFound:
                        errMsg = "房间不存在";
                        break;
                    case ErrorCode.ErrorRoomSlotAlreadlyHadPlayer:
                        errMsg = "加入目标位置已经有人";
                        break;
                    case ErrorCode.ErrorRoomCantDoCurrState:
                        errMsg = "当前房间状态不允许本操作";
                        break;
                    case ErrorCode.ErrorRomDontHadSavedata:
                        errMsg = "即时存档不存在";
                        break;
                    case ErrorCode.ErrorRomFailSavedata:
                        errMsg = "处理即时存档失败";
                        break;
                    case ErrorCode.ErrorRomAlreadyHadCoverimg:
                        errMsg = "该游戏已经有封面图";
                        break;
                    case ErrorCode.ErrorRomAlreadyHadStar:
                        errMsg = "已经收藏";
                        break;
                    case ErrorCode.ErrorRomDontHadStar:
                        errMsg = "并没有收藏";
                        break;
                    case ErrorCode.ErrorRomFailCoverimg:
                        errMsg = "处理游戏截图失败";
                        break;
                    case ErrorCode.ErrorDefaul:
                    case ErrorCode.ErrorOk:
                    default:
                        break;
                }
                OverlayManager.PopTip("错误:"+ errMsg);
                App.log.Error("错误:" + errMsg);
            }

            //如果报错，则不往前继续推进
            if (err > ErrorCode.ErrorOk)
                return;

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
                        App.log.Error(e.ToString());
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
