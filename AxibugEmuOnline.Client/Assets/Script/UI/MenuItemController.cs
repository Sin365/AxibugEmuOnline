using AxibugEmuOnline.Client.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class MenuItemController : MonoBehaviour
    {
        private int m_selectIndex;
        protected List<MenuItem> m_runtimeMenuUI = new List<MenuItem>();

        public int SelectIndex
        {
            get => m_selectIndex;
            set
            {
                value = Mathf.Clamp(value, 0, m_runtimeMenuUI.Count - 1);
                m_selectIndex = value;

                OnSelectMenuChanged();
            }
        }

        [SerializeField]
        protected Transform m_menuItemRoot;

        protected virtual void Start()
        {
            for (int i = 0; i < m_menuItemRoot.childCount; i++)
            {
                Transform child = m_menuItemRoot.GetChild(i);
                m_runtimeMenuUI.Add(child.GetComponent<MenuItem>());
            }

            Canvas.ForceUpdateCanvases();
            SelectIndex = 0;
        }

        protected virtual void Update() { }

        protected abstract void OnSelectMenuChanged();
    }
}
