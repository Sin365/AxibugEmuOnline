using MyNes.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.Input
{
    public class LocalKeyMapper : KeyMapper
    {
        private Dictionary<KeyCode, EnumKeyKind> m_mapper = new Dictionary<KeyCode, EnumKeyKind>();
        private Dictionary<EnumKeyKind, KeyCode> m_mapperOpp = new Dictionary<EnumKeyKind, KeyCode>();
        private Dictionary<EnumKeyKind, int> m_keyIndexTable = new Dictionary<EnumKeyKind, int>();
        private EnumKeyKind[] m_focusKeys;
        private bool[] m_keyStates;

        public override void Init()
        {
            SetKeyMapper(KeyCode.W, EnumKeyKind.Up);
            SetKeyMapper(KeyCode.S, EnumKeyKind.Down);
            SetKeyMapper(KeyCode.A, EnumKeyKind.Left);
            SetKeyMapper(KeyCode.D, EnumKeyKind.Right);
            SetKeyMapper(KeyCode.V, EnumKeyKind.Select);
            SetKeyMapper(KeyCode.B, EnumKeyKind.Start);
            SetKeyMapper(KeyCode.J, EnumKeyKind.B);
            SetKeyMapper(KeyCode.K, EnumKeyKind.A);
            SetKeyMapper(KeyCode.U, EnumKeyKind.TurboB);
            SetKeyMapper(KeyCode.I, EnumKeyKind.TurboA);
            SetComplete();
        }

        void SetKeyMapper(KeyCode inputKeycode, EnumKeyKind joyKey)
        {
            if (m_mapperOpp.TryGetValue(joyKey, out KeyCode keyCode))//如果该映射已设置过,移除之前的映射
            {
                m_mapperOpp.Remove(joyKey);
                m_mapper.Remove(keyCode);
            }
            m_mapper[inputKeycode] = joyKey;
            m_mapperOpp[joyKey] = inputKeycode;
        }

        void SetComplete()
        {
            m_focusKeys = m_mapperOpp.Keys.ToArray();
            m_keyStates = new bool[m_focusKeys.Length];

            m_keyIndexTable.Clear();
            for (int i = 0; i < m_focusKeys.Length; i++)
            {
                m_keyIndexTable[m_focusKeys[i]] = i;
            }
        }

        public override void Update()
        {
            if (m_focusKeys == null) return;

            for (int i = 0; i < m_focusKeys.Length; i++)
            {
                var keyCode = m_mapperOpp[m_focusKeys[i]];
                m_keyStates[i] = UnityEngine.Input.GetKey(keyCode);
            }
        }

        public override bool IsPressing(EnumKeyKind keyKind)
        {
            if (!m_keyIndexTable.TryGetValue(keyKind, out int index)) return false;//没有设置映射,直接false

            return m_keyStates[index];
        }
    }
}
