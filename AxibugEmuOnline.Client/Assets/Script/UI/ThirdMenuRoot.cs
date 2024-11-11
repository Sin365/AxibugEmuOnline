using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class ThirdMenuRoot : SubMenuItemGroup
    {
        private RectTransform m_rect;
        private RectTransform m_parent;

        [SerializeField]
        private RectTransform m_selectArrow; 
        [SerializeField]
        float ArrowOffset = 50;
        [SerializeField]
        float WidthFix = 50;
        [SerializeField]
        public ItemPresent itemGroup;
        [SerializeField]
        ScrollRect srollRect;

        public override int SelectIndex
        {
            get => m_selectIndex;
            set
            {
                if (itemGroup.DataList == null) return;

                value = Mathf.Clamp(value, 0, itemGroup.DataList.Count - 1);
                if (m_selectIndex == value) return;
                bool useAnim = m_selectIndex != -1;
                m_selectIndex = value;

                RollToIndex(m_selectIndex, useAnim);
                OnSelectMenuChanged();
            }
        }

        public void ResetToFirst()
        {
            m_selectIndex = -1;
            SelectIndex = 0;
        }

        protected override MenuItem GetItemUIByIndex(int index)
        {
            return itemGroup.GetItemUIByDataIndex(index)?.GetComponent<MenuItem>();
        }

        protected override void Awake()
        {
            m_rect = transform as RectTransform;
            m_parent = transform.parent as RectTransform;

            base.Awake();
        }

        public override void Init(List<MenuData> menuDataList) { }

        protected override bool OnCmdEnter()
        {
            var item = GetItemUIByIndex(SelectIndex);
            if (item != null)
                return item.OnEnterItem();
            else
                return true;
        }

        protected override void OnCmdBack()
        {
            var item = GetItemUIByIndex(SelectIndex);
            item?.OnExitItem();
        }

        private void LateUpdate()
        {
            SyncRectToLaunchUI();
        }

        protected override void OnSelectMenuChanged()
        {
            itemGroup.UpdateDependencyProperty(this);
        }

        void RollToIndex(int index, bool useAnim = false)
        {
            SyncRectToLaunchUI();
            Vector2 itemPos = itemGroup.GetItemAnchorePos(index);

            Vector2 targetPos = itemGroup.transform.InverseTransformPoint(m_selectArrow.position);
            Vector3[] corners = new Vector3[4];
            itemGroup.RectTransform.GetLocalCorners(corners);
            targetPos = targetPos - (Vector2)corners[1];

            float gap = targetPos.y - itemPos.y;

            srollRect.velocity = Vector2.zero;
            if (!useAnim)
                srollRect.content.anchoredPosition += new Vector2(0, gap);
            else
            {
                var endValue = srollRect.content.anchoredPosition + new Vector2(0, gap);
                DOTween.To(() => srollRect.content.anchoredPosition, (x) => srollRect.content.anchoredPosition = x, endValue, 0.125f);
            }
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

            m_selectArrow.position = m_parent.transform.position;
            temp = m_selectArrow.anchoredPosition;
            temp.x += ArrowOffset;
            m_selectArrow.anchoredPosition = temp;
        }

        protected override void OnCmdSelectItemDown()
        {
            base.OnCmdSelectItemDown();
        }

        protected override void OnCmdSelectItemUp()
        {
            base.OnCmdSelectItemUp();
        }
    }
}
