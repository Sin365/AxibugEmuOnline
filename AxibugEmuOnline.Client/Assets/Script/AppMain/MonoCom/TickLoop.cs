using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class TickLoop : MonoBehaviour
    {
        public static Action LoopAction_tick;
        public static Action LoopAction_1s;
        public static Action LoopAction_3s;
        public Stopwatch sw = Stopwatch.StartNew();
        public TimeSpan LastStartPingTime;
        public int LastPingSeed;
        public double AveNetDelay;
        public double MinNetDelay;
        public double MaxNetDelay;
        public List<double> NetDelays = new List<double>();
        public const int NetAveDelayCount = 3;
        private void Awake()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdPing, OnCmdPing);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdPong, OnCmdPong);

            LoopAction_3s += Ping;

            SetFrameRate(60);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    OnApplicationQuit();
            };
#endif
        }
        float LastLoopTime_1s;
        float LastLoopTime_3s;
        private void Update()
        {
            NetMsg.Instance.DequeueNesMsg();

            LoopAction_tick?.Invoke();

            if (Time.time - LastLoopTime_1s > 1)
            {
                LastLoopTime_1s = Time.time;
                LoopAction_1s?.Invoke();
            }

            if (Time.time - LastLoopTime_3s > 3)
            {
                LastLoopTime_3s = Time.time;
                LoopAction_3s?.Invoke();
            }
        }

        void OnApplicationQuit()
        {
            App.log.Debug("OnApplicationQuit");
            App.network.bAutoReConnect = false;

            if (App.network.isConnected)
            {
                App.network.CloseConntect();
            }
        }


        void Ping()
        {
            if (!App.network.isConnected)
                return;

            int randSeed = new System.Random().Next(0, int.MaxValue);
            LastPingSeed = randSeed;
            LastStartPingTime = App.tick.sw.Elapsed;
            Protobuf_Ping resp = new Protobuf_Ping()
            {
                Seed = randSeed,
            };
            App.network.SendToServer((int)CommandID.CmdPing, ProtoBufHelper.Serizlize(resp));
        }


        public void OnCmdPing(byte[] reqData)
        {
            //App.log.Debug($"OnCmdPing");
            Protobuf_Ping msg = ProtoBufHelper.DeSerizlize<Protobuf_Ping>(reqData);
            Protobuf_Pong resp = new Protobuf_Pong()
            {
                Seed = msg.Seed,
            };
            App.network.SendToServer((int)CommandID.CmdPong, ProtoBufHelper.Serizlize(resp));
        }

        public void OnCmdPong(byte[] reqData)
        {
            //App.log.Debug($"OnCmdPong");
            Protobuf_Pong msg = ProtoBufHelper.DeSerizlize<Protobuf_Pong>(reqData);

            if (LastPingSeed == msg.Seed)
            {
                TimeSpan current = App.tick.sw.Elapsed;
                TimeSpan delta = current - LastStartPingTime;
                NetDelays.Add(delta.TotalSeconds);

                while (NetDelays.Count > NetAveDelayCount)
                    NetDelays.RemoveAt(0);

                double tempMin = double.MaxValue;
                double tempMax = double.MinValue;
                for (int i = 0; i < NetDelays.Count; i++)
                {
                    tempMin = Math.Min(NetDelays[i], tempMin);
                    tempMax = Math.Max(NetDelays[i], tempMax);
                }
                MinNetDelay = tempMin;
                MaxNetDelay = tempMax;
                AveNetDelay = NetDelays.Average(w => w);
            }
        }

        internal object GetDateTimeStr()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        internal void SetFrameRate(int rate)
        {//关闭垂直同步
            QualitySettings.vSyncCount = 0;
            //设为60帧
            Application.targetFrameRate = rate;
        }
    }
}
