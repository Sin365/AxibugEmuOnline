using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class LaunchUI : MonoBehaviour
    {
        [SerializeField]
        RectTransform MainMenuRoot;
        [SerializeField]
        MainMenuController MainMenu;
        [SerializeField]
        Image XMBBackground;
        [SerializeField]
        Image RomPreviewBigPic;
        [SerializeField]
        CanvasGroup XMBCG_For_RomPreviewBigPic;

        Vector2 m_mainLayoutPosition;
        [SerializeField]
        float m_detailLayoutPosition_x = 55;
        [SerializeField]
        float m_LayoutChangeSpeed = 10;

        public static LaunchUI Instance { get; private set; }

        TweenerCore<Vector2, Vector2, VectorOptions> m_layoutTween;
        AlphaWraper romPreviewWraper;

        private void Awake()
        {
            Instance = this;
            m_mainLayoutPosition = MainMenuRoot.anchoredPosition;
            MainMenu.ListenControlAction = true;
            
            romPreviewWraper = new AlphaWraper(XMBCG_For_RomPreviewBigPic, RomPreviewBigPic.GetComponent<CanvasGroup>(), false);
        }

        private void Start()
        {
            CommandDispatcher.Instance.Mode = CommandListener.ScheduleType.Normal;
        }

        private void Update()
        {
            if (CommandDispatcher.Instance.Mode == CommandListener.ScheduleType.Gaming && App.emu.Core.IsNull())
                CommandDispatcher.Instance.Mode = CommandListener.ScheduleType.Normal;
        }

        public void HideMainMenu()
        {
            XMBBackground.gameObject.SetActiveEx(false);
            MainMenuRoot.gameObject.SetActiveEx(false);
            RomPreviewBigPic.gameObject.SetActiveEx(false);
        }

        public void ShowMainMenu()
        {
            XMBBackground.gameObject.SetActiveEx(true);
            MainMenuRoot.gameObject.SetActiveEx(true);

            if (romPreviewWraper.On)
            {
                XMBCG_For_RomPreviewBigPic.gameObject.SetActive(false);
                RomPreviewBigPic.gameObject.SetActive(true);
            }
            else if (!romPreviewWraper.On)
            {
                XMBCG_For_RomPreviewBigPic.gameObject.SetActive(true);
                XMBCG_For_RomPreviewBigPic.alpha = 1;
                RomPreviewBigPic.gameObject.SetActive(false);
            }
        }

        public void HideRomPreview()
        {
            romPreviewWraper.On = false;
        }

        public void SetRomPreview(Sprite sp)
        {
            if (MainMenu.ListenControlAction) return;

            RomPreviewBigPic.sprite = sp;
            romPreviewWraper.On = true;
        }

        public void ToDetailMenuLayout()
        {
            if (m_layoutTween != null)
            {
                m_layoutTween.Kill();
                m_layoutTween = null;
            }
            m_layoutTween = DOTween
                .To(
                () => MainMenuRoot.anchoredPosition,
                (x) => MainMenuRoot.anchoredPosition = x,
                new Vector2(m_detailLayoutPosition_x, MainMenuRoot.anchoredPosition.y),
                m_LayoutChangeSpeed)
                .SetSpeedBased();
            MainMenu.ListenControlAction = false;
            MainMenu.EnterDetailState();
        }

        public void ToMainMenuLayout()
        {
            if (m_layoutTween != null)
            {
                m_layoutTween.Kill();
                m_layoutTween = null;
            }
            m_layoutTween = DOTween.To(
                () => MainMenuRoot.anchoredPosition,
                (x) => MainMenuRoot.anchoredPosition = x,
                m_mainLayoutPosition,
                m_LayoutChangeSpeed)
                .SetSpeedBased();
            MainMenu.ListenControlAction = true;
            MainMenu.ExitDetailState();

            HideRomPreview();
        }
    }
}
