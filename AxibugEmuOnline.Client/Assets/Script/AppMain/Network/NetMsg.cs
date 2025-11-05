using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using AxibugEmuOnline.Client.Common;

namespace AxibugEmuOnline.Client.Network
{
    public class NetMsg
    {
        private static NetMsg instance = new NetMsg();
        public static NetMsg Instance { get { return instance; } }

        // 使用强类型字典存储委托，避免DynamicInvoke
        private Dictionary<int, HashSet<Action<IMessage>>> netEventDic = new Dictionary<int, HashSet<Action<IMessage>>>(128);
        private Dictionary<Type, Dictionary<Delegate, Action<IMessage>>> delegateWrappers = new Dictionary<Type, Dictionary<Delegate, Action<IMessage>>>();

        private Queue<ValueTuple<int, int, byte[]>> queueNetMsg = new Queue<ValueTuple<int, int, byte[]>>();
        public static object lockQueueNetMsg = new object();

        private Queue<Action> queueEventFromNet = new Queue<Action>();
        public static object lockQueueEventFromNet = new object();

        private NetMsg() { }

        private static Dictionary<int, Type> cmd2MsgTypeDict = new Dictionary<int, Type>();

        #region RegisterMsgEvent

        public void RegNetMsgEvent<T>(int cmd, Action<T> callback) where T : IMessage
        {
            // 创建类型安全的包装委托，避免DynamicInvoke
            Action<IMessage> wrappedCallback = (message) => { callback((T)message); };

            // 缓存包装委托以便后续取消注册时使用
            if (!delegateWrappers.ContainsKey(typeof(T)))
            {
                delegateWrappers[typeof(T)] = new Dictionary<Delegate, Action<IMessage>>();
            }
            delegateWrappers[typeof(T)][callback] = wrappedCallback;
            InterRegNetMsgEvent(cmd, wrappedCallback);
            SetTypeByCmd(cmd, callback);
        }

        private void InterRegNetMsgEvent(int cmd, Action<IMessage> callback)
        {
            if (netEventDic.ContainsKey(cmd))
            {
                if (!netEventDic[cmd].Contains(callback))
                    netEventDic[cmd].Add(callback);
            }
            else
            {
                netEventDic.Add(cmd, new HashSet<Action<IMessage>>() { callback });
            }
        }
        #endregion

        #region UnregisterCMD

        public void UnregisterCMD<T>(int cmd, Action<T> callback) where T : IMessage
        {
            Dictionary<Delegate, Action<IMessage>> wrapperDict;
            Action<IMessage> wrappedCallback;
            if (delegateWrappers.TryGetValue(typeof(T), out wrapperDict) &&
                wrapperDict.TryGetValue(callback, out wrappedCallback))
            {
                InterUnregisterCMD(cmd, wrappedCallback);
                wrapperDict.Remove(callback);
            }
        }

        private void InterUnregisterCMD(int cmd, Action<IMessage> callback)
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
            catch (Exception ex)
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
                OverlayManager.PopTip("错误:" + errMsg);
                App.log.Error("错误:" + errMsg);
            }

            if (err > ErrorCode.ErrorOk)
                return;

            HashSet<Action<IMessage>> eventList = GetNetEventDicList(cmd);
            if (eventList != null && eventList.Count > 0)
            {
                // 获取消息类型
                Type protoType = GetTypeByCmd(cmd);
                if (protoType == null)
                {
                    App.log.Error($"无法确定命令 {cmd} 的消息类型");
                    return;
                }
                IMessage protobufMsg = ProtoBufHelper.DeSerizlizeFromPool(arg, protoType);

                try
                {
                    // 使用强类型调用，避免DynamicInvoke的性能开销
                    foreach (Action<IMessage> callback in eventList)
                    {
                        try
                        {
                            callback(protobufMsg);
                        }
                        catch (Exception e)
                        {
                            App.log.Error($"处理网络消息时出错 (CMD: {cmd}): {e}");
                        }
                    }
                }
                finally
                {
                    ProtoBufHelper.ReleaseToPool(protobufMsg);
                }
            }
        }
        #endregion
        /// <summary>
        /// 设置网络消息类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="callback"></param>
        private static void SetTypeByCmd<T>(int cmd, Action<T> callback) where T : IMessage
        {
            var paramters = callback.Method.GetParameters();
            if (paramters.Length != 0)
            {
                var protoType = paramters[0].ParameterType;
                if (!protoType.IsInterface && !protoType.IsAbstract)
                    cmd2MsgTypeDict[cmd] = protoType;
            }
        }
        private static Type GetTypeByCmd(int cmd)
        {
            Type type;
            if (cmd2MsgTypeDict.TryGetValue(cmd, out type)) return type;
            return null;
        }

        /// <summary>
        /// 获取所有事件
        /// </summary>
        private HashSet<Action<IMessage>> GetNetEventDicList(int cmd)
        {
            HashSet<Action<IMessage>> tempList;
            if (netEventDic.TryGetValue(cmd, out tempList) && tempList != null)
                return tempList;
            return null;
        }
    }
}