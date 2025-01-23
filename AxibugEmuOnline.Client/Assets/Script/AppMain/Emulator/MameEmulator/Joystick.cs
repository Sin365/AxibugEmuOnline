using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{

    public Vector2Int RawInputV2
    {
        get
        {
            return InputForEmu;
        }
    }

    public float HandleRange
    {
        get { return handleRange; }
        set { handleRange = Mathf.Abs(value); }
    }

    public float DeadZone
    {
        get { return deadZone; }
        set { deadZone = Mathf.Abs(value); }
    }

    public AxisOptions AxisOptions { get { return AxisOptions; } set { axisOptions = value; } }

    [SerializeField] private float handleRange = 1;
    [SerializeField] private float deadZone = 0f;
    [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
    [SerializeField] private bool snapX = false;
    [SerializeField] private bool snapY = false;

    [SerializeField] protected RectTransform background = null;
    [SerializeField] private RectTransform handle = null;
    [SerializeField] protected RectTransform mArrow;
    private RectTransform baseRect = null;

    private Canvas canvas;
    private Camera cam;

    private Vector2 input = Vector2.zero;
    public Vector2Int InputForEmu = Vector2Int.zero;

    public static Joystick joystickInstance;


    protected virtual void Start()
    {

        HandleRange = handleRange;
        DeadZone = deadZone;
        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");
        else
        {
            if (cam == null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    cam = canvas.worldCamera;
            }
        }

        Vector2 center = new Vector2(0.5f, 0.5f);
        background.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;

        joystickInstance = this;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    Vector2 mArrowTop = new Vector2(0,1);
    public void OnDrag(PointerEventData eventData)
    {
        if (cam == null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera;
        }

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * canvas.scaleFactor);
        //FormatInput();
        HandleInput(input.magnitude, input.normalized, radius, cam);
        handle.anchoredPosition = input * radius * handleRange;
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > deadZone)
        {
            if (magnitude > 1)
            {
                input = normalised;
            }
        }
        else
        {
            input = Vector2.zero;
        }


        if (magnitude > deadZone && input != Vector2.zero)
        {
            if (!mArrow.gameObject.activeSelf) mArrow.gameObject.SetActive(true);
            float angle = Vector2.Angle(mArrowTop, input.normalized);

            if (input.x > 0)
                angle *= -1;
            mArrow.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
            if (mArrow.gameObject.activeSelf) mArrow.gameObject.SetActive(false);


        if (input.x < -1 * deadZone)
            InputForEmu.x = -1;
        else if (input.x > deadZone)
            InputForEmu.x = 1;
        else
            InputForEmu.x = 0;

        if (input.y < -1 * deadZone)
            InputForEmu.y = -1;
        else if (input.y > deadZone)
            InputForEmu.y = 1;
        else
            InputForEmu.y = 0;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        PointerUp();
    }

    public virtual void PointerUp()
    {
        input = Vector2.zero;
        InputForEmu = Vector2Int.zero;
        handle.anchoredPosition = Vector2.zero;
        mArrow.gameObject.SetActive(false);
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }
}

public enum AxisOptions { Both, Horizontal, Vertical }