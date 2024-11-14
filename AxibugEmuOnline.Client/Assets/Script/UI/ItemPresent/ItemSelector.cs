using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSelector : MonoBehaviour
{
    [SerializeField]
    protected List<RectTransform> ItemList;

    public RectTransform GetItemTemplate(object data)
    {
        return OnGetTemplate(data);
    }

    protected abstract RectTransform OnGetTemplate(object data);
}

