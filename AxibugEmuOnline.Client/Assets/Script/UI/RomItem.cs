using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class RomItem : MenuItem, IVirtualItem, IPointerClickHandler
    {
        [SerializeField]
        Image m_romImage;

        public int Index { get; set; }

        private RomLib m_romlib;
        private RomFile m_romfile;

        public void SetData(object data)
        {
            m_romfile = (RomFile)data;
            m_romfile.OnInfoFilled += OnRomInfoFilled;
            m_romImage.sprite = null;

            UpdateView();

            if (!m_romfile.InfoReady)
            {
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
        }

        private void OnRomInfoFilled()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            if (!m_romfile.InfoReady)
            {
                SetBaseInfo(string.Empty, string.Empty);
            }
            else
            {
                SetBaseInfo(m_romfile.Alias, m_romfile.Descript);
                AppAxibugEmuOnline.CacheMgr.GetSpriteCache(m_romfile.ImageURL, (img, url) =>
                {
                    if (url != m_romfile.ImageURL) return;

                    m_romImage.sprite = img;
                });
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!m_romfile.RomReady)
                m_romfile.BeginDownload();
            else
            {
                AppAxibugEmuOnline.SceneLoader.BeginLoad("Scene/EmuTest", () =>
                {
                    var nesEmu = GameObject.FindObjectOfType<NesEmulator>();
                });
            }
        }
    }
}
