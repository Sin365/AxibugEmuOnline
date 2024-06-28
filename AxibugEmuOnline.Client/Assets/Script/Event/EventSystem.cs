using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client.Event
{
    public class EventData
    {
        private static long BaseUid = 0;
        private static long GenUid()
        {
            return (++BaseUid);
        }

        /// <summary>
        /// 唯一id
        /// </summary>
        public long uid { get; private set; }

        /// <summary>
        /// 回调
        /// </summary>
        public Delegate callback { get; private set; }

        public EventData(Delegate d)
        {
            uid = GenUid();
            callback = d;
        }
    }

    public class EventSystem
    {
        private static EventSystem instance = new EventSystem();
        public static EventSystem Instance { get { return instance; } }

        private Dictionary<EEvent, List<Delegate>> eventDic = new Dictionary<EEvent, List<Delegate>>(128);

        private EventSystem() { }


        #region RegisterEvent
        public void RegisterEvent(EEvent evt, Action callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1>(EEvent evt, Action<T1> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2>(EEvent evt, Action<T1, T2> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2, T3>(EEvent evt, Action<T1, T2, T3> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        public void RegisterEvent<T1, T2, T3, T4>(EEvent evt, Action<T1, T2, T3, T4> callback)
        {
            InterRegisterEvent(evt, callback);
        }

        private void InterRegisterEvent(EEvent evt, Delegate callback)
        {
            if (eventDic.ContainsKey(evt))
            {
                if (eventDic[evt].IndexOf(callback) < 0)
                {
                    eventDic[evt].Add(callback);
                }
            }
            else
            {
                eventDic.Add(evt, new List<Delegate>() { callback });
            }
        }
        #endregion

        #region UnregisterEvent

        public void UnregisterEvent(EEvent evt, Action callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1>(EEvent evt, Action<T1> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2>(EEvent evt, Action<T1, T2> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2, T3>(EEvent evt, Action<T1, T2, T3> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        public void UnregisterEvent<T1, T2, T3, T4>(EEvent evt, Action<T1, T2, T3, T4> callback)
        {
            Delegate tempDelegate = callback;
            InterUnregisterEvent(evt, tempDelegate);
        }

        private void InterUnregisterEvent(EEvent evt, Delegate callback)
        {
            if (eventDic.ContainsKey(evt))
            {
                eventDic[evt].Remove(callback);
                if (eventDic[evt].Count == 0) eventDic.Remove(evt);
            }
        }
        #endregion

        #region PostEvent
        public void PostEvent<T1, T2, T3, T4>(EEvent evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2, T3, T4>)callback)(arg1, arg2, arg3, arg4);
                    }
                    catch (Exception e)
                    {
                        AppAxibugEmuOnline.log.Error(e.Message);
                    }
                }
            }
        }

        public void PostEvent<T1, T2, T3>(EEvent evt, T1 arg1, T2 arg2, T3 arg3)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2, T3>)callback)(arg1, arg2, arg3);
                    }
                    catch (Exception e)
                    {
                        AppAxibugEmuOnline.log.Error(e.Message);
                    }
                }
            }
        }

        public void PostEvent<T1, T2>(EEvent evt, T1 arg1, T2 arg2)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T1, T2>)callback)(arg1, arg2);
                    }
                    catch (Exception e)
                    {
                        AppAxibugEmuOnline.log.Error(e.Message);
                    }
                }
            }
        }

        public void PostEvent<T>(EEvent evt, T arg)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action<T>)callback)(arg);
                    }
                    catch (Exception e)
                    {
                        AppAxibugEmuOnline.log.Error(e.Message + ", method name : " + callback.Method);
                    }
                }
            }

        }

        public void PostEvent(EEvent evt)
        {
            List<Delegate> eventList = GetEventList(evt);
            if (eventList != null)
            {
                foreach (Delegate callback in eventList)
                {
                    try
                    {
                        ((Action)callback)();
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
        /// <param name="evt"></param>
        /// <returns></returns>
        private List<Delegate> GetEventList(EEvent evt)
        {
            if (eventDic.ContainsKey(evt))
            {
                List<Delegate> tempList = eventDic[evt];
                if (null != tempList)
                {
                    return tempList;
                }
            }
            return null;
        }
    }
}
