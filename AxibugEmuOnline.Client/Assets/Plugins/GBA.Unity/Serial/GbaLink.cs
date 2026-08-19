using System.Collections.Generic;

namespace OptimeGBA
{
    /// <summary>
    /// 本机 2～4 核虚拟线。Multi / Normal 字交换；网络仍由外层负责。
    /// </summary>
    public sealed class GbaLink
    {
        readonly List<Gba> _gbas = new List<Gba>(4);

        // Normal 交换缓冲（2P 简化：按 PlayerId 存最近一次 send）
        readonly byte[] _normal8 = new byte[4];
        readonly uint[] _normal32 = new uint[4];
        readonly bool[] _normal8Ready = new bool[4];
        readonly bool[] _normal32Ready = new bool[4];

        public int AttachedCount => _gbas.Count;

        public GbaLink(Gba gba0, Gba gba1, Gba gba2 = null, Gba gba3 = null)
        {
            Attach(gba0, 0);
            Attach(gba1, 1);
            if (gba2 != null) Attach(gba2, 2);
            if (gba3 != null) Attach(gba3, 3);
        }

        void Attach(Gba gba, int id)
        {
            if (gba == null) return;
            _gbas.Add(gba);
            gba.Serial.PlayerId = id;
            gba.Serial.IsChild = id != 0;
            gba.Serial.Link = this;
        }

        public void DetachAll()
        {
            foreach (var g in _gbas)
            {
                if (g?.Serial != null)
                    g.Serial.Link = null;
            }
            _gbas.Clear();
        }

        public void NotifyParentStartedTransfer()
        {
            for (int i = 0; i < _gbas.Count; i++)
            {
                var s = _gbas[i].Serial;
                // 只让 Child 进入 Busy；Parent 已在 ApplySiocntWrite 里置 Busy
                if (!s.IsChild) continue;
                if (s.Mode == SerialGba.SioMode.Multi)
                    s.BeginMultiTransferFromLink();
            }
        }

        public void CollectMultiplayerWords(ushort[] words)
        {
            for (int i = 0; i < 4; i++)
                words[i] = 0xFFFF;

            for (int i = 0; i < _gbas.Count && i < 4; i++)
                words[i] = _gbas[i].Serial.MultiSend;
        }

        public void FinishAllMultiplayer(ushort[] words)
        {
            for (int i = 0; i < _gbas.Count; i++)
            {
                var s = _gbas[i].Serial;
                // Busy 或 Multi 都完成，避免 Mode 瞬间不一致丢包
                if (s.Mode == SerialGba.SioMode.Multi || (s.Siocnt & (1 << 7)) != 0)
                    s.FinishMultiplayerTransfer(words);
            }
        }

        public void NotifyNormalStart(SerialGba who)
        {
            int id = who.PlayerId & 3;
            if (who.Mode == SerialGba.SioMode.Normal8)
            {
                _normal8[id] = (byte)(who.MultiSend & 0xFF);
                _normal8Ready[id] = true;
            }
            else if (who.Mode == SerialGba.SioMode.Normal32)
            {
                _normal32[id] = who.GetNormal32();
                _normal32Ready[id] = true;
            }
        }

        /// <summary>2P：与对方交换；多机时与 id^1 交换（可按需再改）。</summary>
        public byte ExchangeNormal8(byte mine, int id)
        {
            id &= 3;
            _normal8[id] = mine;
            _normal8Ready[id] = true;

            int other = id == 0 ? 1 : 0;
            if (other < _gbas.Count && _normal8Ready[other])
                return _normal8[other];
            return mine; // 对方尚未 Start：回环，避免卡死
        }

        public uint ExchangeNormal32(uint mine, int id)
        {
            id &= 3;
            _normal32[id] = mine;
            _normal32Ready[id] = true;

            int other = id == 0 ? 1 : 0;
            if (other < _gbas.Count && _normal32Ready[other])
                return _normal32[other];
            return mine;
        }
    }
}