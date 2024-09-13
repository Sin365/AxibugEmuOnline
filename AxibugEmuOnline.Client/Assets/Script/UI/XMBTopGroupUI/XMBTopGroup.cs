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
    void Awake()
    {
        txtDateTime = this.transform.Find("txtDateTime").GetComponent<Text>();
        imgPowerRoot = this.transform.Find("imgPowerRoot").GetComponent<Image>();
        imgPower1 = imgPowerRoot.transform.Find("PowerGroup/imgPower1").GetComponent<Image>();
        imgPower2 = imgPowerRoot.transform.Find("PowerGroup/imgPower1").GetComponent<Image>();
        imgPower3 = imgPowerRoot.transform.Find("PowerGroup/imgPower1").GetComponent<Image>();
    }

    void OnEnable()
    {
        TickLoop.LoopAction_1s += RefreshAll;
    }

    void OnDisable()
    {
        TickLoop.LoopAction_1s -= RefreshAll;
    }

    void RefreshAll()
    {
        RefreshTime();
        RefreshPower();
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
