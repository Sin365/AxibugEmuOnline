using MyNes.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.Input
{
    public class InputManager : MonoBehaviour
    {
        private KeyMapper m_p1Mapper = new KeyMapper();
        private KeyMapper m_p2Mapper = new KeyMapper();
        private KeyMapper m_p3Mapper = new KeyMapper();
        private KeyMapper m_p4Mapper = new KeyMapper();

        private void Awake()
        {
            m_p1Mapper.SetKeyMapper(KeyCode.W, EnumKeyKind.Up);
            m_p1Mapper.SetKeyMapper(KeyCode.S, EnumKeyKind.Down);
            m_p1Mapper.SetKeyMapper(KeyCode.A, EnumKeyKind.Left);
            m_p1Mapper.SetKeyMapper(KeyCode.D, EnumKeyKind.Right);
            m_p1Mapper.SetKeyMapper(KeyCode.V, EnumKeyKind.Select);
            m_p1Mapper.SetKeyMapper(KeyCode.B, EnumKeyKind.Start);
            m_p1Mapper.SetKeyMapper(KeyCode.J, EnumKeyKind.B);
            m_p1Mapper.SetKeyMapper(KeyCode.K, EnumKeyKind.A);
            m_p1Mapper.SetKeyMapper(KeyCode.U, EnumKeyKind.TurboB);
            m_p1Mapper.SetKeyMapper(KeyCode.I, EnumKeyKind.TurboA);
            m_p1Mapper.SetComplete();
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
