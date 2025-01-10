using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public partial class GamePadManager
    {
        #region Events
        public delegate void GamePadConnectedHandle(GamePad newConnectGamePad);
        /// <summary> 当一个手柄连接时触发 </summary>
        public event GamePadConnectedHandle OnGamePadConnected;

        public delegate void GamePadDisConnectedHandle(GamePad disConnectGamePad);
        /// <summary> 当一个手柄断开时触发 </summary>
        public event GamePadDisConnectedHandle OnGamePadDisConnected;
        #endregion

        Dictionary<GamePadInfo, GamePad> m_gamePads = new Dictionary<GamePadInfo, GamePad>();
        HashSet<GamePadInfo> m_temp = new HashSet<GamePadInfo>();

        public void Update()
        {
            m_temp.Clear();
            foreach (var info in m_gamePads.Keys)
                m_temp.Add(info); //记录需要被移除的手柄

            var devices = Input.GetJoystickNames();
            for (int i = 0; i < devices.Length; i++)
            {
                var info = new GamePadInfo { Index = i, Name = devices[i] };
                m_temp.Remove(info);

                if (!m_gamePads.ContainsKey(info))
                {
                    m_gamePads[info] = new GamePad(info);
                    OnGamePadConnected?.Invoke(m_gamePads[info]);
                };
            }

            foreach (var info in m_temp)
            {
                if (m_gamePads.TryGetValue(info, out GamePad gp))
                {
                    m_gamePads.Remove(info);
                    gp.Offline = true;
                    OnGamePadDisConnected?.Invoke(gp);
                }
            }
        }

        /// <summary>
        /// 获取所有已连接的手柄,返回的结果顺序与手柄序号无关
        /// </summary>
        /// <returns></returns>
        public GamePad[] GetGamePads()
        {
            return m_gamePads.Values.ToArray();
        }

        internal struct GamePadInfo : IEquatable<GamePadInfo>, IComparable<GamePadInfo>
        {
            internal int Index;
            internal string Name;

            public override bool Equals(object obj)
            {
                if (obj is GamePadInfo)
                {
                    return Equals((GamePadInfo)obj);
                }
                return false;
            }

            public bool Equals(GamePadInfo other)
            {
                return Index == other.Index && Name == other.Name;
            }

            public override int GetHashCode()
            {
                // Custom hash code implementation without HashCombine
                int hash = 17;
                hash = hash * 31 + Index.GetHashCode();
                hash = hash * 31 + (Name != null ? Name.GetHashCode() : 0);
                return hash;
            }

            public int CompareTo(GamePadInfo other)
            {
                int indexComparison = Index.CompareTo(other.Index);
                if (indexComparison != 0)
                {
                    return indexComparison;
                }
                return string.Compare(Name, other.Name, StringComparison.Ordinal);
            }

            public static bool operator ==(GamePadInfo left, GamePadInfo right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GamePadInfo left, GamePadInfo right)
            {
                return !(left == right);
            }

            public static bool operator <(GamePadInfo left, GamePadInfo right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(GamePadInfo left, GamePadInfo right)
            {
                return left.CompareTo(right) > 0;
            }
        }
    }
}