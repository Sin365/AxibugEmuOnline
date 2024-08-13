using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AxibugEmuOnline.Client
{
    public class HttpAPI
    {
        public string WebSite = "http://emu.axibug.com/api";

        public void GetNesRomList(Action<Resp_GameList> callback, int page, int pageSize = 10)
        {
            AppAxibugEmuOnline.StartCoroutine(GetNesRomListFlow(page, pageSize, callback));
        }

        private IEnumerator GetNesRomListFlow(int page, int pageSize, Action<Resp_GameList> callback)
        {
            UnityWebRequest request = new UnityWebRequest($"{WebSite}/NesRomList?Page={page}&PageSize={pageSize}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback.Invoke(null);
                yield break;
            }

            var resp = JsonUtility.FromJson<Resp_GameList>(request.downloadHandler.text);
            callback.Invoke(resp);
        }

        public class Resp_GameList
        {
            public int Page { get; set; }
            public int MaxPage { get; set; }
            public int ResultAllCount { get; set; }
            public List<Resp_RomInfo> GameList { get; set; }
        }

        public class Resp_RomInfo
        {
            public int ID { get; set; }
            public string Hash { get; set; }
            public string RomName { get; set; }
            public string Url { get; set; }
            public string ImgUrl { get; set; }
        }
    }
}
