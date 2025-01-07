using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public interface IVirtualLayout
{
    Dictionary<GameObject, ScripteInterface> CacheItemScripts { get; }
    List<object> DataList { get; }
    object DependencyProperty { get; }
    RectTransform RectTransform { get; }
    RectTransform GetTemplate(object data);
    Vector2 GetItemAnchorePos(int index);
    RectTransform GetItemUIIfExist(int index);

    void UpdateProxyVisualState();
    void UpdateDependencyProperty(object dp);
    void SetData(IEnumerable dataList);
    void MoveToScrollViewCenter(ScrollRect scrollRect, int dataIndex);
}

public class ItemProxy
{
    public bool IsDestroyed;
    public bool Visible = true;
    public int Index;
    public bool IsInViewRect;

    public Vector2 Pivot => _template.pivot;
    public Vector2 AnchoredPosition;

    public float Width;
    public float Height;

    private IVirtualLayout _parent;

    private RectTransform _template => _parent.GetTemplate(_parent.DataList[Index]);
    private RectTransform _runtimeInstance;
    private LayoutGroup _layoutElement;

    public RectTransform RuntimeItemUI => _runtimeInstance;
    public bool firstShow { get; private set; } = true;

    public float PreferredWidth
    {
        get
        {
            if (_layoutElement == null) return 0;

            return _layoutElement.preferredWidth;
        }
    }
    public float PreferredHeight
    {
        get
        {
            if (_layoutElement == null) return 0;

            return _layoutElement.preferredHeight;
        }
    }

    public ScripteInterface GetLuaObj()
    {
        if (_runtimeInstance == null) return null;

        ScripteInterface lfi;

        _parent.CacheItemScripts.TryGetValue(_runtimeInstance.gameObject, out lfi);

        return lfi;
    }

    public ItemProxy(IVirtualLayout parent)
    {
        _parent = parent;
    }

    public void Dispose()
    {
        if (_runtimeInstance != null)
        {
            if (Application.isPlaying)
            {
                GameObjectPool.Release(_runtimeInstance.gameObject);
            }
            else
                GameObject.DestroyImmediate(_runtimeInstance.gameObject);
        }
    }

    public bool NeedShow
    {
        get
        {
            if (IsInViewRect && _runtimeInstance == null) return true;
            else
            {
                if (_runtimeInstance != null && IsInViewRect && _runtimeInstance.anchoredPosition != AnchoredPosition)
                    return true;
                else
                    return false;
            }
        }

    }
    public bool NeedHide => !IsInViewRect && _runtimeInstance != null;

    public void UpdateView(bool force = false)
    {
        if (IsInViewRect)
        {
            if (_runtimeInstance == null)
            {
                _runtimeInstance = GetInstance();
                _layoutElement = _runtimeInstance.GetComponent<LayoutGroup>();
                UpdateViewData();
            }
            else if (force)
            {
                UpdateViewData();
            }

            UpdateLayout();
        }
        else
        {
            ReleaseInstance();
        }
    }

    public void UpdateLayout()
    {
        if (_runtimeInstance != null)
        {
            _runtimeInstance.gameObject.SetActive(true);
            _runtimeInstance.anchorMax = Vector2.up;
            _runtimeInstance.anchorMin = Vector2.up;
            _runtimeInstance.anchoredPosition = AnchoredPosition;
            _runtimeInstance.sizeDelta = new Vector2(Width, Height);
        }
        if (_layoutElement != null)
        {
            _layoutElement.CalculateLayoutInputHorizontal();
            _layoutElement.CalculateLayoutInputVertical();
            _layoutElement.SetLayoutHorizontal();
            _layoutElement.SetLayoutVertical();
        }

    }

    private void UpdateViewData()
    {
        if (Application.isPlaying)
        {
            if (!_parent.CacheItemScripts.ContainsKey(_runtimeInstance.gameObject))
            {
                var vItem = _runtimeInstance.gameObject.GetComponent<IVirtualItem>();
                ScripteInterface newSI = new ScripteInterface(vItem);
                _parent.CacheItemScripts[_runtimeInstance.gameObject] = newSI;
            }

            ScripteInterface si;

            _parent.CacheItemScripts.TryGetValue(_runtimeInstance.gameObject, out si);
            si.SetDataList(_parent.DataList[Index], Index);
            if (_parent.DependencyProperty != null)
                si.SetDependencyProperty(_parent.DependencyProperty);
        }
    }

    public void UpdateDP()
    {
        if (_runtimeInstance == null) return;
        ScripteInterface luaInterface;
        _parent.CacheItemScripts.TryGetValue(_runtimeInstance.gameObject, out luaInterface);
        if (luaInterface == null) return;

        if (_parent.DependencyProperty != null)
            luaInterface.SetDependencyProperty(_parent.DependencyProperty);
    }

    private RectTransform GetInstance()
    {
        var res = GameObjectPool.GetInstance(_template.gameObject, _parent.RectTransform).GetComponent<RectTransform>();
        return res;
    }

    private void ReleaseInstance()
    {
        if (_runtimeInstance == null) return;

        _layoutElement = null;

        if (Application.isPlaying)
        {
            ScripteInterface si;
            _parent.CacheItemScripts.TryGetValue(_runtimeInstance.gameObject, out si);
            if (si != null) si.Release();
            GameObjectPool.Release(_runtimeInstance.gameObject);
            _runtimeInstance = null;
        }
        else
        {
            GameObject.DestroyImmediate(_runtimeInstance.gameObject);
            _runtimeInstance = null;
        }
    }
}

public class ScripteInterface
{
    private IVirtualItem _itemInstance;
    public IVirtualItem ItemInstance => _itemInstance;

    public ScripteInterface(IVirtualItem lc)
    {
        _itemInstance = lc;
    }

    public void SetDataList(object dataItem, int index)
    {
        if (_itemInstance == null) return;
        _itemInstance.Index = index;
        _itemInstance.SetData(dataItem);
    }
    public void Release()
    {
        if (_itemInstance == null) return;
        _itemInstance.Release();
    }

    public void SetDependencyProperty(object dependencyProperty)
    {
        if (_itemInstance == null) return;

        _itemInstance.SetDependencyProperty(dependencyProperty);
    }

}

public interface IVirtualItem
{
    GameObject gameObject { get; }
    int Index { get; set; }
    void SetData(object data);
    void SetDependencyProperty(object data);
    void Release();
}
