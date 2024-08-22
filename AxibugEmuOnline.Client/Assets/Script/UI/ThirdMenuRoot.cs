using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class ThirdMenuRoot : MonoBehaviour
    {
        private RectTransform m_rect;
        private RectTransform m_parent;

        [SerializeField]
        float WidthFix = 50;

        private void Awake()
        {
            m_rect = transform as RectTransform;
            m_parent = transform.parent as RectTransform;
        }

        private void LateUpdate()
        {
            SyncRectToLaunchUI();
        }

        Vector3[] corner = new Vector3[4];
        private void SyncRectToLaunchUI()
        {
            if (LaunchUI.Instance == null) return;
            var launchUIRect = LaunchUI.Instance.transform as RectTransform;

            m_rect.pivot = new Vector2(1, 0.5f);
            m_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, launchUIRect.rect.width);
            m_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, launchUIRect.rect.height);
            m_rect.position = launchUIRect.position;
            var temp = m_rect.localPosition;
            var offsetX = (m_rect.pivot.x - 0.5f) * m_rect.rect.size.x;
            temp.x += offsetX;
            var offsetY = (m_rect.pivot.y - 0.5f) * m_rect.rect.size.y;
            temp.y += offsetY;
            m_rect.localPosition = temp;
            m_rect.localScale = launchUIRect.localScale;

            m_parent.GetWorldCorners(corner);
            var parentPosition = corner[0];
            parentPosition = launchUIRect.InverseTransformPoint(parentPosition);
            launchUIRect.GetWorldCorners(corner);
            var rootPosition = corner[0];
            rootPosition = launchUIRect.InverseTransformPoint(rootPosition);

            var widthGap = parentPosition.x - rootPosition.x;
            m_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, launchUIRect.rect.width - widthGap - WidthFix);
        }
    }
}
