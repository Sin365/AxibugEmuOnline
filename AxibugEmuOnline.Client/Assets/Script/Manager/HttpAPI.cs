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
        public string DownSite = "http://emu.axibug.com";

        public delegate void GetRomListAPI(Action<Resp_GameList> callback, int page, int pageSize = 10);

        public void GetNesRomList(Action<Resp_GameList> callback, int page, int pageSize = 10)
        {
            AppAxibugEmuOnline.StartCoroutine(GetNesRomListFlow(page, pageSize, callback));
        }

        private IEnumerator GetNesRomListFlow(int page, int pageSize, Action<Resp_GameList> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{WebSite}/NesRomList?Page={page}&PageSize={pageSize}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback.Invoke(null);
                yield break;
            }

            var resp = JsonUtility.FromJson<Resp_GameList>(request.downloadHandler.text);
            callback.Invoke(resp);
        }

        enum GameType : byte
        {
            NONE = 0,
            ACT,
            ARPG,
            AVG,
            ETC,
            FTG,
            PUZ,
            RAC,
            RPG,
            SLG,
            SPG,
            SRPG,
            STG,
            TAB,
            /// <summary>
            /// 合卡
            /// </summary>
            ALLINONE,
        }

        [Serializable]
        public class Resp_GameList
        {
            public int page;
            public int maxPage;
            public int resultAllCount;
            public List<Resp_RomInfo> gameList;
        }

        [Serializable]
        public class Resp_RomInfo
        {
            public int orderid;
            public int id;
            public string romName;
            public string gType;
            public string desc;
            public string url;
            public string imgUrl;
            public string hash;
            public int stars;
        }
    }
}
