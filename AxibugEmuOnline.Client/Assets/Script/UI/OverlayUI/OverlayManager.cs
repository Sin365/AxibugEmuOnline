using System;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OverlayManager : MonoBehaviour
    {
        static OverlayManager s_ins;

        [SerializeField]
        InputUI m_InputUI;

        private void Awake()
        {
            s_ins = this;

            m_InputUI.gameObject.SetActive(false);
        }

        public static InputUI Input((Action<string> callback, string placeHolder, string defaultText) param)
        {
            s_ins.m_InputUI.Show(param);

            return s_ins.m_InputUI;
        }
    }
}
