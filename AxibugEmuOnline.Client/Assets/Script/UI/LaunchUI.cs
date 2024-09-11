using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class LaunchUI : MonoBehaviour
    {
        [SerializeField]
        RectTransform MainMenuRoot;
        [SerializeField]
        MainMenuController MainMenu;

        Vector2 m_mainLayoutPosition;
        [SerializeField]
        Vector2 m_detailLayoutPosition;
        [SerializeField]
        float m_LayoutChangeSpeed = 10;

        public static LaunchUI Instance { get; private set; }

        TweenerCore<Vector2, Vector2, VectorOptions> m_layoutTween;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(Camera.main.gameObject);
            m_mainLayoutPosition = MainMenuRoot.anchoredPosition;
            MainMenu.ListenControlAction = true;
        }

        public void HideMainMenu()
        {
            MainMenuRoot.gameObject.SetActiveEx(false);
        }

        public void ShowMainMenu()
        {
            MainMenuRoot.gameObject.SetActiveEx(true);
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
                m_detailLayoutPosition,
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
        }
    }
}
