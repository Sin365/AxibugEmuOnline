using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using MAME.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UniVideoPlayer : MonoBehaviour, IVideoPlayer
{
    public Vector2Int mScreenSize { get; private set; }
    [SerializeField]
    private int mDataLenght;
    [SerializeField]
    private Texture2D m_rawBufferWarper;
    [SerializeField]
    private RawImage m_drawCanvas;
    [SerializeField]
    private RectTransform m_drawCanvasrect;
    //int[] mFrameData;
    IntPtr mFrameDataPtr;

    private TimeSpan lastElapsed;
    public double videoFPS { get; private set; }
    public ulong mFrame { get; private set; }
    bool bInit = false;

    public Texture2D rawBufferWarper => m_rawBufferWarper;
    public RawImage DrawCanvas => m_drawCanvas;

    private void Awake()
    {
        mFrame = 0;
        m_drawCanvas = GameObject.Find("GameRawImage").GetComponent<RawImage>();
        m_drawCanvasrect = m_drawCanvas.GetComponent<RectTransform>();
    }

    public void Initialize(int width, int height, IntPtr framePtr)
    {
        m_drawCanvas.color = Color.white;

        if (m_rawBufferWarper == null || mScreenSize.x != width || mScreenSize.y != height)
        {
            mScreenSize = new Vector2Int(width, height);
            mDataLenght = width * height * 4;
            //mFrameData = new int[mWidth * mHeight];
            //MAME来的是BGRA32，好好好 BGRA->RGBA
            m_rawBufferWarper = new Texture2D(mScreenSize.x, mScreenSize.y, TextureFormat.RGBA32, false);
            m_rawBufferWarper.filterMode = FilterMode.Point;
        }

        mFrameDataPtr = framePtr;
        m_drawCanvas.texture = m_rawBufferWarper;
        bInit = true;

        //float targetWidth = ((float)mScreenSize.x/ mScreenSize.y) * m_drawCanvasrect.rect.height ;
        //m_drawCanvasrect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    public void StopVideo()
    {
        bInit = false;
        m_drawCanvas.color = new Color(0, 0, 0, 0);
    }

    //void Update()
    //{
    //    if (!bInit) return;
    //    //m_rawBufferWarper.LoadRawTextureData(mFrameDataPtr, mDataLenght);
    //    //m_rawBufferWarper.Apply();
    //}

    public void SubmitVideo(int[] data, long frame_number)
    {
        mFrame = (ulong)frame_number;
        UMAME.instance.mTimeSpan.SetTick(mFrame);
        var current = UMAME.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        videoFPS = 1d / delta.TotalSeconds;
        //mFrameData = data;

        //Debug.Log($"frame_number -> {frame_number}");
        m_rawBufferWarper.LoadRawTextureData(mFrameDataPtr, mDataLenght);
        m_rawBufferWarper.Apply();

    }

    public byte[] GetScreenImg()
    {
        return (m_drawCanvas.texture as Texture2D).EncodeToJPG();
    }


    public void ApplyFilterEffect()
    {
        App.settings.Filter.ExecuteFilterRender(m_rawBufferWarper, m_drawCanvas);
    }

    public void ApplyScreenScaler()
    {
        App.settings.ScreenScaler.CalcScale(m_drawCanvas, UMAME.instance.Platform);
    }
}
