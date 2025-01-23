using MAME.Core;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILongClickButton : Button
{
    public bool bHotKey { get; private set; } = false;
    public long[] Key;
    protected override void OnEnable()
    {
        base.OnEnable();
        bHotKey = false;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        bHotKey = true;
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        bHotKey = false;
    }
}
