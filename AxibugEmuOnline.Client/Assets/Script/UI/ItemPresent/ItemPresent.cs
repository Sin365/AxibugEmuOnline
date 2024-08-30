using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPresent : GridLayoutGroup, IVirtualLayout
{
    public RectTransform ItemTemplate;
    public RectTransform ViewRect;

    private Dictionary<GameObject, ScripteInterface> _cacheItemScripts = new Dictionary<GameObject, ScripteInterface>();
    private List<object> _dataList;
    private object _dependencyProperty;
    private Vector2 _layoutCellsize;

    public Action OnItemDelayShowPorcessComplete;


#if UNITY_EDITOR
    public int EditorOnlyItemCount
    {
        get => ItemCount;
        set => ItemCount = value;
    }
#endif

    private int ItemCount
    {
        get => children.Count;
        set
        {
            if (value == ItemCount) return;

            if (value <= 0)
            {
                foreach (var child in children)
                {
                    child.Dispose();
                }

                children.Clear();
            }
            else
            {
                var gap = value - children.Count;

                if (gap > 0)
                {
                    for (int i = 0; i < gap; i++)
                    {
                        ItemProxy item = new ItemProxy(ItemTemplate, this);
                        children.Add(item);
                        item.Width = cellSize.x;
                        item.Height = cellSize.y;
                        item.Index = children.Count - 1;
                    }
                }
                else if (gap < 0)
                {
                    for (int i = 0; i < -gap; i++)
                    {
                        int removeIndex = children.Count - 1;
                        children[removeIndex].Dispose();
                        children.RemoveAt(removeIndex);
                    }
                }
            }

            SetDirty();
        }
    }
    private List<ItemProxy> children = new List<ItemProxy>();

    private List<ItemProxy> handleChildren = new List<ItemProxy>();
    private bool m_dataDirty;

    public override void CalculateLayoutInputHorizontal()
    {
        handleChildren.Clear();
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (child.IsDestroyed || !child.Visible)
                continue;

            handleChildren.Add(child);
        }
        m_Tracker.Clear();
        updateFixHeightAndWidth();

        int minColumns = 0;
        int preferredColumns = 0;
        if (m_Constraint == Constraint.FixedColumnCount)
        {
            minColumns = preferredColumns = m_ConstraintCount;
        }
        else if (m_Constraint == Constraint.FixedRowCount)
        {
            minColumns = preferredColumns = Mathf.CeilToInt(handleChildren.Count / (float)m_ConstraintCount - 0.001f);
        }
        else
        {
            minColumns = 1;
            preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(handleChildren.Count));
        }

        SetLayoutInputForAxis(
            padding.horizontal + (_layoutCellsize.x + spacing.x) * minColumns - spacing.x,
            padding.horizontal + (_layoutCellsize.x + spacing.x) * preferredColumns - spacing.x,
            -1, 0);
    }

    public void MoveToScrollViewCenter(ScrollRect scrollRect, int dataIndex)
    {
        if (m_dataDirty)
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                child.UpdateLayout();
            }
            Canvas.ForceUpdateCanvases();
        }

        var targetProxy = children[dataIndex];
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        // Item is here
        var itemCenterPositionInScroll = GetWorldPointInWidget(scrollRect.transform as RectTransform, GetWidgetWorldPoint(targetProxy));
        //Debug.Log("Item Anchor Pos In Scroll: " + itemCenterPositionInScroll);
        // But must be here
        var targetPositionInScroll = GetWorldPointInWidget(scrollRect.transform as RectTransform, GetWidgetWorldPoint(scrollRect.viewport));
        //Debug.Log("Target Anchor Pos In Scroll: " + targetPositionInScroll);
        // So it has to move this distance
        var difference = targetPositionInScroll - itemCenterPositionInScroll;
        difference.z = 0f;

        var newNormalizedPosition = new Vector2(difference.x / (rectTransform.rect.width - scrollRect.viewport.rect.width),
            difference.y / (rectTransform.rect.height - scrollRect.viewport.rect.height));

        newNormalizedPosition = scrollRect.normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        scrollRect.normalizedPosition = newNormalizedPosition;
        //DOTween.To(() => scrollRect.normalizedPosition, x => scrollRect.normalizedPosition = x, newNormalizedPosition, 0.2f);
    }

    Vector3 GetWidgetWorldPoint(RectTransform target)
    {
        //pivot position + item size has to be included
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);
    }

    Vector3 GetWidgetWorldPoint(ItemProxy proxy)
    {
        Vector3[] temp = new Vector3[4];
        rectTransform.GetLocalCorners(temp);
        var pos = (Vector2)temp[1] + proxy.AnchoredPosition;
        pos = rectTransform.TransformPoint(pos);

        return pos;
    }

    Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
    {
        return target.InverseTransformPoint(worldPoint);
    }

    public override void CalculateLayoutInputVertical()
    {
        int minRows = 0;
        if (m_Constraint == Constraint.FixedColumnCount)
        {
            minRows = Mathf.CeilToInt(handleChildren.Count / (float)m_ConstraintCount - 0.001f);
        }
        else if (m_Constraint == Constraint.FixedRowCount)
        {
            minRows = m_ConstraintCount;
        }
        else
        {
            float width = rectTransform.rect.width;
            int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (_layoutCellsize.x + spacing.x)));
            minRows = Mathf.CeilToInt(handleChildren.Count / (float)cellCountX);
        }

        float minSpace = padding.vertical + (_layoutCellsize.y + spacing.y) * minRows - spacing.y;
        SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        SetProxyCellsAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetProxyCellsAlongAxis(1);

        foreach (var item in handleChildren)
        {
            item.UpdateLayout();
        }
    }



    private void SetProxyCellsAlongAxis(int axis)
    {
        // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
        // and only vertical values when invoked for the vertical axis.
        // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
        // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
        // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
        var proxyChildCount = handleChildren.Count;
        if (axis == 0)
        {
            // Only set the sizes when invoked for horizontal axis, not the positions.

            for (int i = 0; i < proxyChildCount; i++)
            {
                ItemProxy proxy = handleChildren[i];

                proxy.Width = _layoutCellsize.x;
                proxy.Height = _layoutCellsize.y;
            }
            return;
        }

        float width = rectTransform.rect.size.x;
        float height = rectTransform.rect.size.y;

        int cellCountX = 1;
        int cellCountY = 1;
        if (m_Constraint == Constraint.FixedColumnCount)
        {
            cellCountX = m_ConstraintCount;

            if (proxyChildCount > cellCountX)
                cellCountY = proxyChildCount / cellCountX + (proxyChildCount % cellCountX > 0 ? 1 : 0);
        }
        else if (m_Constraint == Constraint.FixedRowCount)
        {
            cellCountY = m_ConstraintCount;

            if (proxyChildCount > cellCountY)
                cellCountX = proxyChildCount / cellCountY + (proxyChildCount % cellCountY > 0 ? 1 : 0);
        }
        else
        {
            if (_layoutCellsize.x + spacing.x <= 0)
                cellCountX = int.MaxValue;
            else
                cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

            if (_layoutCellsize.y + spacing.y <= 0)
                cellCountY = int.MaxValue;
            else
                cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
        }

        int cornerX = (int)startCorner % 2;
        int cornerY = (int)startCorner / 2;

        int cellsPerMainAxis, actualCellCountX, actualCellCountY;
        if (startAxis == Axis.Horizontal)
        {
            cellsPerMainAxis = cellCountX;
            actualCellCountX = Mathf.Clamp(cellCountX, 1, proxyChildCount);
            actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(proxyChildCount / (float)cellsPerMainAxis));
        }
        else
        {
            cellsPerMainAxis = cellCountY;
            actualCellCountY = Mathf.Clamp(cellCountY, 1, proxyChildCount);
            actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(proxyChildCount / (float)cellsPerMainAxis));
        }

        Vector2 requiredSpace = new Vector2(
            actualCellCountX * _layoutCellsize.x + (actualCellCountX - 1) * spacing.x,
            actualCellCountY * _layoutCellsize.y + (actualCellCountY - 1) * spacing.y
        );
        Vector2 startOffset = new Vector2(
            GetStartOffset(0, requiredSpace.x),
            GetStartOffset(1, requiredSpace.y)
        );

        for (int i = 0; i < proxyChildCount; i++)
        {
            int positionX;
            int positionY;
            if (startAxis == Axis.Horizontal)
            {
                positionX = i % cellsPerMainAxis;
                positionY = i / cellsPerMainAxis;
            }
            else
            {
                positionX = i / cellsPerMainAxis;
                positionY = i % cellsPerMainAxis;
            }

            if (cornerX == 1)
                positionX = actualCellCountX - 1 - positionX;
            if (cornerY == 1)
                positionY = actualCellCountY - 1 - positionY;

            SetProxyChildAlongAxis(handleChildren[i], 0, startOffset.x + (_layoutCellsize[0] + spacing[0]) * positionX, _layoutCellsize[0]);
            SetProxyChildAlongAxis(handleChildren[i], 1, startOffset.y + (_layoutCellsize[1] + spacing[1]) * positionY, _layoutCellsize[1]);
        }
    }

    private void SetProxyChildAlongAxis(ItemProxy proxy, int axis, float pos, float size)
    {
        var scaleFactor = 1.0f;

        if (proxy == null)
            return;

        Vector2 sizeDelta = new Vector2(proxy.Width, proxy.Height);
        sizeDelta[axis] = size;
        proxy.Width = sizeDelta.x;
        proxy.Height = sizeDelta.y;

        Vector2 anchoredPosition = proxy.AnchoredPosition;
        anchoredPosition[axis] = (axis == 0) ? (pos + size * proxy.Pivot[axis] * scaleFactor) : (-pos - size * (1f - proxy.Pivot[axis]) * scaleFactor);
        proxy.AnchoredPosition = anchoredPosition;
    }

    public void UpdateProxyVisualState()
    {
        if (m_dataDirty)
        {
            foreach (var proxy in children)
            {
                proxy.UpdateView(true);
            }
            Canvas.ForceUpdateCanvases();

            m_dataDirty = false;
        }

        if (ViewRect == null)
        {
            foreach (var proxy in children)
            {
                proxy.IsInViewRect = true;
            }
            return;
        }

        Vector3[] corners = new Vector3[4];
        ViewRect.GetLocalCorners(corners);
        Rect parentRect = ViewRect.rect;
        parentRect.position = corners[0];

        rectTransform.GetLocalCorners(corners);
        Vector2 leftUpCorner = corners[1];

        foreach (var proxy in children)
        {
            var localPos = leftUpCorner + proxy.AnchoredPosition;
            localPos.x -= proxy.Width * 0.5f;
            localPos.y -= proxy.Height * 0.5f;
            localPos = transform.localToWorldMatrix.MultiplyPoint(localPos);
            localPos = ViewRect.worldToLocalMatrix.MultiplyPoint(localPos);

            Rect proxyRect = new Rect(localPos, new Vector2(proxy.Width, proxy.Height));

            if (parentRect.Overlaps(proxyRect)) proxy.IsInViewRect = true;
            else proxy.IsInViewRect = false;
        }
    }

    public bool PauseUpdateView;
    private void LateUpdate()
    {
        if (!PauseUpdateView)
        {
            updateFixHeightAndWidth();

            UpdateProxyVisualState();
            HandleProxyShow();
        }
    }

    private void updateFixHeightAndWidth()
    {
        _layoutCellsize = cellSize;
    }

    private List<ItemProxy> NeedDelayShowItems = new List<ItemProxy>();
    private bool hasNeedShowInLastFrame = false;
    private float stepDuration = 0f;
    private void HandleProxyShow(bool allLoad = true, float delayStep = 0.02f)
    {
        if (allLoad)
        {
            foreach (var proxy in children)
            {
                if (proxy.NeedHide)
                    proxy.UpdateView();
                else if (proxy.NeedShow)
                    proxy.UpdateView();
            }
        }
        else
        {
            NeedDelayShowItems.Clear();
            foreach (var proxy in children)
            {
                if (proxy.NeedHide)
                    proxy.UpdateView();
                else if (proxy.NeedShow && !proxy.firstShow)
                    proxy.UpdateView();
                else if (proxy.NeedShow && proxy.firstShow)
                {
                    NeedDelayShowItems.Add(proxy);
                }
            }

            if (NeedDelayShowItems.Count == 0 && hasNeedShowInLastFrame)
            {
                // Debug.Log("Show Complete!", gameObject);
                OnItemDelayShowPorcessComplete?.Invoke();
            }
            hasNeedShowInLastFrame = NeedDelayShowItems.Count > 0;

            stepDuration += Time.deltaTime;
            while (stepDuration >= delayStep)
            {
                foreach (var proxy in NeedDelayShowItems)
                {
                    if (proxy.NeedShow)
                    {
                        proxy.UpdateView();
                        break;
                    }
                }
                stepDuration -= delayStep;
            }
        }
    }

    protected override void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        foreach (var proxy in children)
            proxy.Dispose();

        children.Clear();
    }


    public void SetData(object dataList)
    {
        Clear();

        if (dataList == null)
        {
            ItemCount = 0;
            if (_dataList != null)
                _dataList.Clear();
        }
        else if (dataList is IEnumerable ienumrable)
        {
            List<object> temp = new List<object>();
            foreach (var item in ienumrable)
            {
                temp.Add(item);
            }
            ItemCount = temp.Count;
            _dataList = temp;
        }
        else
        {
            Debug.LogException(new Exception("ItemPresent SetData 传递的参数类型不受支持"), gameObject);
            return;
        }

        m_dataDirty = true;
    }

    public void UpdateDependencyProperty(object dp)
    {
        _dependencyProperty = dp;
        foreach (var proxy in children)
        {
            proxy.UpdateDP();
        }

        m_dataDirty = true;
    }

    public Dictionary<GameObject, ScripteInterface> CacheItemScripts => _cacheItemScripts;

    public object DependencyProperty => _dependencyProperty;

    public RectTransform RectTransform => rectTransform;
    public Vector2 GetItemAnchorePos(int index)
    {
        var proxy = children[index];
        return proxy.AnchoredPosition;
    }
    public RectTransform GetItemUIIfExist(int index)
    {
        if (children.Count <= index) return null;

        var proxy = children[index];
        return proxy.RuntimeItemUI;
    }
    public List<object> DataList
    {
        get => _dataList;
        set => SetData(value);
    }

    private ScrollRect _scrollRect;
    public RectTransform GetItemUIByDataIndex(int dataIndex)
    {
        if (_scrollRect == null)
        {
            _scrollRect = GetComponentInParent<ScrollRect>();
        }
        if (_scrollRect != null) MoveToScrollViewCenter(_scrollRect, dataIndex);

        return this.GetItemUIIfExist(dataIndex);
    }
}
