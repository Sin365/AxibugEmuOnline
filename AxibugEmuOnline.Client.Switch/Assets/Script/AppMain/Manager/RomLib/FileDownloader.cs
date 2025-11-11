using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class FileDownloader
    {
        Dictionary<string, AxiHttpProxy.SendDownLoadProxy> m_downloadingTasks = new Dictionary<string, AxiHttpProxy.SendDownLoadProxy>();
        Dictionary<string, Action<byte[]>> m_completeCallback = new Dictionary<string, Action<byte[]>>();
        public void BeginDownload(string url, Action<byte[]> callback)
        {
            AxiHttpProxy.SendDownLoadProxy downloadProxy;
            if (m_downloadingTasks.TryGetValue(url, out downloadProxy)) return;

            m_completeCallback[url] = callback;
            var downloadRequest = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{url}");
            m_downloadingTasks[url] = downloadRequest;
        }

        public float? GetDownloadProgress(string url)
        {
            AxiHttpProxy.SendDownLoadProxy proxy;
            m_downloadingTasks.TryGetValue(url, out proxy);
            if (proxy == null) return null;

            return Mathf.Clamp01(proxy.downloadHandler.downLoadPr);
        }

        HashSet<string> temp = new HashSet<string>();
        public void Update()
        {
            temp.Clear();

            foreach (var item in m_downloadingTasks)
            {
                var url = item.Key;
                var proxy = item.Value;
                if (proxy.downloadHandler.isDone)
                {
                    temp.Add(url);
                }
            }

            foreach (var url in temp)
            {
                var overTask = m_downloadingTasks[url];
                m_downloadingTasks.Remove(url);

                if (!overTask.downloadHandler.bHadErr)
                {
                    m_completeCallback[url].Invoke(overTask.downloadHandler.data);
                    m_completeCallback.Remove(url);
                }
                else
                {
                    Debug.LogError($"{overTask.downloadHandler.errInfo}:{overTask.downloadHandler.url}");
                }
            }
        }
    }
}