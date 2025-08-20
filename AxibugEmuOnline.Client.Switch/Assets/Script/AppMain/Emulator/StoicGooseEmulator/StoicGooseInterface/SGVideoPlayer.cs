using System;
using UnityEngine;
using UnityEngine.UI;

public class SGVideoPlayer : MonoBehaviour
{

    [SerializeField]
    private int mWidth;
    [SerializeField]
    private int mHeight;
    [SerializeField]
    private int mDataLenght;
    [SerializeField]
    private Texture2D m_rawBufferWarper;
    [SerializeField]
    private RawImage m_drawCanvas;
    [SerializeField]
    private RectTransform m_drawCanvasrect;
    //byte[] mFrameData;
    IntPtr mFrameDataPtr;
    public Texture2D rawBufferWarper => m_rawBufferWarper;
    public RawImage DrawCanvas => m_drawCanvas;
    private TimeSpan lastElapsed;
    public double videoFPS { get; private set; }
    public ulong mFrame { get; private set; }
    bool bInit = false;
    bool bHadData = false;

    private void Awake()
    {
        bHadData = false;
        mFrame = 0;
        m_drawCanvas = GameObject.Find("GameRawImage").GetComponent<RawImage>();
        m_drawCanvasrect = m_drawCanvas.GetComponent<RectTransform>();
    }

    public void Initialize()
    {
        m_drawCanvas.color = Color.white;

        if (m_rawBufferWarper == null)
        {
            mDataLenght = mWidth * mHeight * 4;
            //mFrameData = new byte[mDataLenght];

            //// 固定数组，防止垃圾回收器移动它  
            //var bitmapcolorRect_handle = GCHandle.Alloc(mFrameData, GCHandleType.Pinned);
            //// 获取数组的指针  
            //mFrameDataPtr = bitmapcolorRect_handle.AddrOfPinnedObject();


            //MAME来的是BGRA32，好好好
            m_rawBufferWarper = new Texture2D(mWidth, mHeight, TextureFormat.BGRA32, false);
            //m_rawBufferWarper = new Texture2D(mWidth, mHeight, TextureFormat.ARGB32, false);
            m_rawBufferWarper.filterMode = FilterMode.Point;
        }

        //mFrameDataPtr = framePtr;
        m_drawCanvas.texture = m_rawBufferWarper;
        bInit = true;

        float targetWidth = ((float)mWidth / mHeight) * m_drawCanvasrect.rect.height;
        m_drawCanvasrect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    public void StopVideo()
    {
        bInit = false;
        m_drawCanvas.color = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        if (!bHadData
            ||
            !UStoicGoose.instance.emulatorHandler.IsRunning)
            return;

        if (!bInit)
        {
            Initialize();
            return;
        }
        m_rawBufferWarper.LoadRawTextureData(mFrameDataPtr, mDataLenght);
        m_rawBufferWarper.Apply();
    }


    public byte[] GetScreenImg()
    {
        return (m_drawCanvas.texture as Texture2D).EncodeToJPG();
    }

    public bool IsVerticalOrientation { get; internal set; }

    internal void UpdateScreen(IntPtr ptr, long frame_number)
    {
        var current = UStoicGoose.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        videoFPS = 1d / delta.TotalSeconds;
        mFrameDataPtr = ptr;
        if (!bHadData)
            bHadData = true;
    }

    internal void SetSize(int screenWidth, int screenHeight)
    {
        mWidth = screenWidth;
        mHeight = screenHeight;
    }
}
