using MyNes.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.Input
{
    public class InputManager : MonoBehaviour
    {
        private KeyMapper m_p1Mapper = new LocalKeyMapper();
        private KeyMapper m_p2Mapper = new NetKeyMapper();
        private KeyMapper m_p3Mapper = new NetKeyMapper();
        private KeyMapper m_p4Mapper = new NetKeyMapper();

        private void Awake()
        {
            m_p1Mapper.Init();
            m_p2Mapper.Init();
            m_p3Mapper.Init();
            m_p4Mapper.Init();
        }

        private void Update()
        {
            m_p1Mapper.Update();
            m_p2Mapper.Update();
            m_p3Mapper.Update();
            m_p4Mapper.Update();
        }

        public bool IsKeyPress(EnumJoyIndex joyIndex, EnumKeyKind keyKind)
        {
            switch (joyIndex)
            {
                case EnumJoyIndex.P1: return m_p1Mapper.IsPressing(keyKind);
                case EnumJoyIndex.P2: return m_p2Mapper.IsPressing(keyKind);
                case EnumJoyIndex.P3: return m_p3Mapper.IsPressing(keyKind);
                case EnumJoyIndex.P4: return m_p4Mapper.IsPressing(keyKind);
                default: return default;
            }
        }
    }
}
