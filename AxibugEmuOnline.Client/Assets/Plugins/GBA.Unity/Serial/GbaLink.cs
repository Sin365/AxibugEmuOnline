using System.Collections.Generic;

namespace OptimeGBA
{
    /// <summary>
    /// 本机 2～4 核虚拟线。只做 Multi 字交换；网络由外层负责。
    /// 完成路径对齐 mGBA finishMultiplayer + MultiplayerFinishTransfer。
    /// </summary>
    public sealed class GbaLink
    {
        readonly List<Gba> _gbas = new List<Gba>(4);

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
            // Ready 在读 SIOCNT 时体现
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

        /// <summary>收集各核 MultiSend → words[0..3]，缺席 0xFFFF。</summary>
        public void CollectMultiplayerWords(ushort[] words)
        {
            for (int i = 0; i < 4; i++)
                words[i] = 0xFFFF;

            for (int i = 0; i < _gbas.Count && i < 4; i++)
                words[i] = _gbas[i].Serial.MultiSend;
        }

        /// <summary>对所有已挂接且 Multi 模式的核执行 Finish。</summary>
        public void FinishAllMultiplayer(ushort[] words)
        {
            for (int i = 0; i < _gbas.Count; i++)
            {
                var s = _gbas[i].Serial;
                if (s.Mode == SerialGba.SioMode.Multi)
                    s.FinishMultiplayerTransfer(words);
            }
        }
    }
}