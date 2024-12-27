using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIFilterPreviewer : MonoBehaviour
{
    private RawImage m_rawImg;
    private Texture m_src;

    private void Awake()
    {
        m_rawImg = GetComponent<RawImage>();
        m_src = m_rawImg.texture;
    }

    private void Update()
    {
        App.settings.ScreenScaler.CalcScale(m_rawImg);
        App.settings.Filter.ExecuteFilterRender(m_src, m_rawImg);
    }
}
