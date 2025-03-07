using AxiInputSP.UGUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatingJoystick : Joystick
{
    public Image mImgBg;
    public Image mImgHandle;
    public Image mImgArrow;

    public bool mIsMounseDown = false;

    readonly static Color ShowColor = new Color(1F, 1f, 1F, 0.7f);
    readonly static Color HideColor = new Color(1F, 1f, 1F, 0.3f);
    AxiIptJoystick mAxiIptJoystick;
    //一次新的摇杆移动
    public static bool bNewTouchWithSkill = false;

    protected override void Start()
    {
        base.Start();
        mImgBg = background.transform.GetComponent<Image>();
        mImgHandle = background.transform.Find("Handle").GetComponent<Image>();
        mArrow = transform.Find("Background/Arrow").GetComponent<RectTransform>();
        mImgArrow = transform.Find("Background/Arrow/imgArrow").GetComponent<Image>();
        mArrow.gameObject.SetActive(false);
        //background.gameObject.SetActive(false);
        background.gameObject.SetActive(true);
        background.transform.localPosition = new Vector3(256f, 256f, 0);
        mImgBg.color = HideColor;
        mImgHandle.color = HideColor;
        mImgArrow.color = HideColor;
        mIsMounseDown = false;
    }

    void OnEnable()
    {
        if (mAxiIptJoystick == null)
        {
            mAxiIptJoystick = new AxiIptJoystick(GetJoyRaw);
        }
    }

    private void OnDisable()
    {
        if (mAxiIptJoystick != null)
        {
            mAxiIptJoystick.Dispose();
            mAxiIptJoystick = null;
        }
    }

    private Vector2Int GetJoyRaw()
    {
        return this.RawInputV2;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        //background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        //background.gameObject.SetActive(true);
        mImgBg.color = ShowColor;
        mImgHandle.color = ShowColor;
        mImgArrow.color = ShowColor;
        mIsMounseDown = true;
        base.OnPointerDown(eventData);
        bNewTouchWithSkill = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        //background.gameObject.SetActive(false);
        PointerUp();
        bNewTouchWithSkill = false;
    }

    public override void PointerUp()
    {
        if (background == null)
            return;

        background.transform.localPosition = new Vector3(256f, 256f, 0);
        mImgBg.color = HideColor;
        mImgHandle.color = HideColor;
        mIsMounseDown = false;
        base.PointerUp();
    }
}