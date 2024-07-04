using AxibugEmuOnline.Client.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class NesCoreProxy : MonoBehaviour
    {
        private AppEmu m_appEnum = new AppEmu();

        private void Start()
        {
            m_appEnum.Init();
        }
    }
}
