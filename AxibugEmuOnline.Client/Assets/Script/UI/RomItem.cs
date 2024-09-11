using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class RomItem : MenuItem, IVirtualItem
    {
        [SerializeField]
        Image m_romImage;

        [SerializeField]
        GameObject DownloadingFlag;
        [SerializeField]
        Slider DownProgress;
        [SerializeField]
        GameObject FileReadyFlag;

        public int Index { get; set; }

        private RomLib m_romlib => App.nesRomLib;
        private RomFile m_romfile;

        public void SetData(object data)
        {
            Reset();

            m_romfile = (RomFile)data;
            m_romfile.OnInfoFilled += OnRomInfoFilled;
            m_romImage.sprite = null;

            UpdateView();

            if (!m_romfile.InfoReady)
            {
                m_romlib.BeginFetchRomInfo(m_romfile);
            }

            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);
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
                App.CacheMgr.GetSpriteCache(m_romfile.ImageURL, (img, url) =>
                {
                    if (url != m_romfile.ImageURL) return;

                    m_romImage.sprite = img;
                });
            }
        }

        public override bool OnEnterItem()
        {
            if (!m_romfile.RomReady)
            {
                m_romfile.BeginDownload();
                return false;
            }
            else
            {
                App.BeginGame(m_romfile);

                return false;
            }
        }

        private void Update()
        {
            DownloadingFlag.SetActiveEx(false);
            FileReadyFlag.SetActiveEx(false);

            if (m_romfile == null) return;
            if (!m_romfile.InfoReady) return;

            if (m_romfile.IsDownloading)
            {
                DownloadingFlag.SetActiveEx(true);
                DownProgress.value = m_romfile.Progress;
            }
            else if (m_romfile.RomReady)
            {
                FileReadyFlag.SetActiveEx(true);
            }
        }
    }
}
