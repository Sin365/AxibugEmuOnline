using System;
using UnityEngine;
using UnityEngine.UI;

public class UEGVideoPlayer : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int mScreenSize = Vector2Int.one;
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
    Texture2D defaultTex;
    bool bTexInit = false;

    private TimeSpan lastElapsed;
    public double videoFPS { get; private set; }
    public ulong mFrame { get; private set; }
    bool bInit = false;
    bool bHadData = false;

    private void Awake()
    {
        mFrame = 0;
        m_drawCanvas = GameObject.Find("GameRawImage").GetComponent<RawImage>();
        m_drawCanvasrect = m_drawCanvas.GetComponent<RectTransform>();
        defaultTex = new Texture2D(1, 1, TextureFormat.BGRA32, false);//直接初始化好了。分辨率是固定的呢
        m_rawBufferWarper = defaultTex;
        m_rawBufferWarper.filterMode = FilterMode.Point;
    }

    public void Initialize()
    {
        m_drawCanvas.color = Color.white;

        //if (m_rawBufferWarper == null)
        if(!bTexInit)
        {
            mDataLenght = mWidth * mHeight * 4;
            m_rawBufferWarper = new Texture2D(mWidth, mHeight, TextureFormat.BGRA32, false);
            m_rawBufferWarper.filterMode = FilterMode.Point;
            bTexInit = true;
        }
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
        if (!bHadData)
            return;

        if (!bInit)
        {
            Initialize();
            return;
        }
        m_rawBufferWarper.LoadRawTextureData(mFrameDataPtr, mDataLenght);
        m_rawBufferWarper.Apply();
    }

    //public void SubmitVideo(int width, int height, byte[] data, long frame_number)
    public void SubmitVideo(int width, int height, IntPtr ptr, long frame_number)
    {

        mFrame = (ulong)frame_number;
        var current = UEssgee.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        videoFPS = 1d / delta.TotalSeconds;
        //mFrameData = data;
        mFrameDataPtr = ptr;

        if (!bHadData)
        {
            mScreenSize = new Vector2Int(width, height);
            mWidth = width;
            mHeight = height;
            bHadData = true;
        }
        //Debug.Log($"frame_number -> {frame_number}");
    }

    public byte[] GetScreenImg()
    {
        return (m_drawCanvas.texture as Texture2D).EncodeToJPG();
    }
}
