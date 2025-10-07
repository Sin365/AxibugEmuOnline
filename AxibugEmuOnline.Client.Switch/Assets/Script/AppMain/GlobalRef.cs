using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

public class GlobalRef : MonoBehaviour
{
    public CanvasGroup FilterPreview;
    public CanvasGroup XMBBg;

    private void Awake()
    {
        //初始化后第一时间播放开机音效
        App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Launch);
    }
}
