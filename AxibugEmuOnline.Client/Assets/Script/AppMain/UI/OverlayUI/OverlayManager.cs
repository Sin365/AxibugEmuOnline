using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OverlayManager : MonoBehaviour
    {
        static OverlayManager s_ins;

        [SerializeField]
        InputUI m_InputUI;
        [SerializeField]
        OptionUI m_OptionUI;

        private void Awake()
        {
            s_ins = this;

            m_InputUI.gameObject.SetActive(false);
        }

        public static InputUI Input(Action<string> callback, string placeHolder, string defaultText)
        {
            s_ins.m_InputUI.Show((callback, placeHolder, defaultText));

            return s_ins.m_InputUI;
        }
        public static void Pop<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : OptionMenu
        {
            s_ins.m_OptionUI.Pop(menus, defaultIndex, onClose);
        }

        public static void PopMsg(string msg)
        {

        }
    }
}
