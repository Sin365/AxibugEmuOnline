using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public abstract class OptionUI_MenuItem : MonoBehaviour
    {
        [SerializeField] Text m_MenuNameTxt;
        [SerializeField] Image m_Icon;

        public bool Visible => m_Menu.Visible;

        protected OptionMenu m_Menu;

        public void SetData(OptionMenu menuData)
        {
            m_Menu = menuData;
            m_MenuNameTxt.text = menuData.Name;
            if (menuData.Icon == null) m_Icon.gameObject.SetActiveEx(false);
            else
            {
                m_Icon.gameObject.SetActiveEx(true);
                m_Icon.sprite = menuData.Icon;
            }

            OnSetData(menuData);
        }

        protected virtual void OnSetData(OptionMenu menuData) { }

        public abstract void OnExecute();
        public abstract void OnFocus();
    }

    public abstract class OptionUI_MenuItem<T> : OptionUI_MenuItem
        where T : OptionMenu
    {
        protected T MenuData => m_Menu as T;
    }
}
