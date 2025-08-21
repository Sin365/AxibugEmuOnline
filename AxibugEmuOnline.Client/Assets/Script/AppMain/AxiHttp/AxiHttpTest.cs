using System;
using System.Collections;
using UnityEngine;

public class AxiHttpTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //for (int i = 0; i < 1000; i++)
                StartCoroutine(DownloadFromURL("http://emu.axibug.com/UserSav/12/Nes/190/1/1.sav", "D:/1.bin", null));
        }
    }

    IEnumerator DownloadFromURL(string url, string path, Action<byte[]> callback)
    {

        for (int i = 0; i < 1000; i++)
        {
            AxiHttpProxy.SendDownLoadProxy request = AxiHttpProxy.GetDownLoad(url);

            while (!request.downloadHandler.isDone)
            {
                Debug.Log($"下载进度：{request.downloadHandler.DownLoadPr} ->{request.downloadHandler.loadedLenght}/{request.downloadHandler.NeedloadedLenght}");
                yield return null;
            }
            AxiHttpProxy.ShowAxiHttpDebugInfo(request.downloadHandler);
            Debug.Log($"下载进度完毕:data.Length=>" + request.downloadHandler.data.Length);
        }
            

        //if (!request.downloadHandler.bHadErr)
        //{
        //    AxiIO.Directory.CreateDirectory(path);
        //    AxiIO.File.WriteAllBytes($"{path}/{url.GetHashCode()}", request.downloadHandler.data, false);
        //    callback?.Invoke(request.downloadHandler.data);
        //}
        //else
        //{
        //    callback?.Invoke(null);
        //}
    }
}
