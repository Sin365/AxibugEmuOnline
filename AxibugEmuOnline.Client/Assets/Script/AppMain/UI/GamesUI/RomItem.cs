using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.UI;
using Coffee.UIExtensions;
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
        [SerializeField]
        UIShiny DownloadComplete;
        [SerializeField]
        GameObject Star;

        public int Index { get; set; }

        public bool IsStar
        {
            get
            {
                return m_romfile != null && m_romfile.InfoReady ? m_romfile.Star : false;
            }
        }
        public int RomID { get { return m_romfile != null && m_romfile.InfoReady ? m_romfile.ID : -1; } }

        private RomLib m_romlib => App.GetRomLib(m_romfile.Platform);

        public bool RomInfoReady => m_romfile != null && m_romfile.InfoReady;

        private RomFile m_romfile;

        protected override void OnEnable()
        {
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRomFileDownloaded, OnRomDownloaded);
        }

        protected override void OnDisable()
        {
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRomFileDownloaded, OnRomDownloaded);
        }

        private void OnRomDownloaded(int romID)
        {
            if (m_romfile == null) return;

            m_romfile.CheckLocalFileState();
            if (m_romfile.RomReady)
                DownloadComplete.Play();
        }

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

            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot && ((ThirdMenuRoot)data).SelectIndex == Index);
        }

        protected override void OnSelected(float progress)
        {
            base.OnSelected(progress);

            if (m_romImage.sprite == null) LaunchUI.Instance.HideRomPreview();
            else LaunchUI.Instance.SetRomPreview(m_romImage.sprite);
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
                SetBaseInfo("正在拉取", "---", "---");
            }
            else
            {
                SetBaseInfo(m_romfile.Alias, m_romfile.Descript, m_romfile.GameTypeDes);
                App.CacheMgr.GetSpriteCache(m_romfile.ImageURL, (img, url) =>
                {
                    if (!m_romfile.InfoReady || url != m_romfile.ImageURL) return;

                    m_romImage.sprite = img;
                    if (m_select) LaunchUI.Instance.SetRomPreview(img);
                });
                Star.SetActiveEx(m_romfile.Star);
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
                App.emu.BeginGame(m_romfile);

                return false;
            }
        }


        protected override void Update()
        {
            DownloadingFlag.SetActiveEx(false);
            FileReadyFlag.SetActiveEx(false);
            Star.SetActiveEx(IsStar);

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

            base.Update();
        }
    }
}
