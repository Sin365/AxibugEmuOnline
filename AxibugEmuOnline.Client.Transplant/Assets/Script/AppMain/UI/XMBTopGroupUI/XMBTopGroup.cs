using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using System;
using UnityEngine;
using UnityEngine.UI;

public class XMBTopGroup : MonoBehaviour
{
    public Text txtDateTime;
    public Image imgPowerRoot;
    public Image imgPower1;
    public Image imgPower2;
    public Image imgPower3;
    public Text DelayValue;
    public Text OnlinePlayerCount;
    public Text FPS;

    void OnEnable()
    {
        TickLoop.LoopAction_1s += RefreshAll;
        RefreshAll();
    }

    void OnDisable()
    {
        TickLoop.LoopAction_1s -= RefreshAll;
    }

    void RefreshAll()
    {
        RefreshTime();
        RefreshPower();
        RefreshDelay();
        RefreshFps();
    }

    (uint lastFrame, float lastTime) m_lastFrameInfo;
    private void RefreshFps()
    {
        if (App.emu.Core.IsNull())
            FPS.gameObject.SetActiveEx(false);
        else
        {
            FPS.gameObject.SetActiveEx(true);
            var gap = App.emu.Core.Frame - m_lastFrameInfo.lastFrame;
            var time = Time.realtimeSinceStartup - m_lastFrameInfo.lastTime;
            var fps = gap / time;
            FPS.text = $"FPS:{fps:.#}";

            m_lastFrameInfo.lastFrame = App.emu.Core.Frame;
            m_lastFrameInfo.lastTime = Time.realtimeSinceStartup;
        }
    }

    private void RefreshDelay()
    {
        if (App.user == null)
        {
            DelayValue.text = $"-";
            OnlinePlayerCount.text = $"-";
            return;
        }
        else
        {
            if (App.user.IsLoggedIn)
            {
                DelayValue.text = $"{App.tickLoop.AveNetDelay * 1000:0}ms";
                OnlinePlayerCount.text = App.user.OnlinePlayerCount.ToString();
            }
            else
            {
                DelayValue.text = "-";
                OnlinePlayerCount.text = $"-";
            }
        }
    }

    void RefreshTime()
    {
        txtDateTime.text = DateTime.Now.ToString("MM/dd HH:mm");
    }

    void RefreshPower()
    {
        float battery = SystemInfo.batteryLevel;

        if (Application.platform == RuntimePlatform.WindowsPlayer
            ||
            Application.platform == RuntimePlatform.WindowsEditor
            ||
            Application.platform == RuntimePlatform.OSXPlayer
            ||
            Application.platform == RuntimePlatform.OSXEditor
            )
        {
            battery = 1f;
        }

        if (battery > 0.80f)
        {
            imgPower1.gameObject.SetActive(true);
            imgPower2.gameObject.SetActive(true);
            imgPower2.gameObject.SetActive(true);
        }
        else if (battery > 0.50f)
        {
            imgPower1.gameObject.SetActive(false);
            imgPower2.gameObject.SetActive(true);
            imgPower2.gameObject.SetActive(true);
        }
        else if (battery > 0.20f)
        {
            imgPower1.gameObject.SetActive(false);
            imgPower2.gameObject.SetActive(false);
            imgPower2.gameObject.SetActive(true);
        }
        else
        {
            imgPower1.gameObject.SetActive(false);
            imgPower2.gameObject.SetActive(false);
            imgPower2.gameObject.SetActive(false);
        }
    }
}
