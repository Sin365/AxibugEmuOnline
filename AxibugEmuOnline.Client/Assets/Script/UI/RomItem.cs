using AxibugEmuOnline.Client.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class RomItem : MenuItem, IVirtualItem
    {
        [SerializeField]
        Image m_romImage;

        public int Index { get; set; }

        private RomLib m_romlib;
        private RomFile m_romfile;

        public void SetData(object data)
        {
            SetSelectState(true);

            m_romfile = (RomFile)data;

            UpdateView();

            if (!m_romfile.InfoReady)
            {
                m_romfile.OnInfoFilled += OnRomInfoFilled;
                m_romlib.BeginFetchRomInfo(m_romfile);
            }
        }

        public void SetDependencyProperty(object data)
        {
            m_romlib = (RomLib)data;
        }

        public void Release()
        {
            m_romfile.OnInfoFilled -= OnRomInfoFilled;
            SetSelectState(false);
        }

        private void OnRomInfoFilled()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            if (!m_romfile.InfoReady)
            {
                SetBaseInfo(string.Empty, string.Empty, null);
            }
            else
            {
                SetBaseInfo(m_romfile.Alias, m_romfile.Descript, null);
            }
        }
    }
}
