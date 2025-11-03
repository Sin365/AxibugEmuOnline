using AxibugEmuOnline.Client.ClientCore;
using AxiInputSP.UGUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OverlayManager : MonoBehaviour
    {
        static OverlayManager s_ins;


        [SerializeField] InputUI m_InputUI;
        [SerializeField] OptionUI m_OptionUI;
        [SerializeField] AxiScreenGamepad m_screenGamepad;
        [SerializeField] PopTipsUI m_popTipsUI;

        private void Awake()
        {
            s_ins = this;

            m_InputUI.gameObject.SetActive(false);

            m_screenGamepad.gameObject.SetActive(App.bUseGUIButton);
        }

        public static void HideOrShwoGUIButton()
        {
            App.bUseGUIButton = !App.bUseGUIButton;
            s_ins.m_screenGamepad.gameObject.SetActive(App.bUseGUIButton);
        }

        public static void Input(Action<string> callback, string placeHolder, string defaultText)
        {
#if UNITY_PSP2 && !UNITY_EDITOR
            App.sonyVitaCommonDialog.ShowPSVitaIME(callback, placeHolder, defaultText);
#else
            s_ins.m_InputUI.Show(new ValueTuple<Action<string>, string, string>(callback, placeHolder, defaultText));
#endif
        }

        public static void PopSideBar<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : InternalOptionMenu
        {
            s_ins.m_OptionUI.Pop(menus, defaultIndex, onClose);
        }

        public static void HideSideBar()
        {
            s_ins.m_OptionUI.Hide();
        }

        public static void PopTip(string msg)
        {
            s_ins.m_popTipsUI.Pop(msg);
        }

    }
}
