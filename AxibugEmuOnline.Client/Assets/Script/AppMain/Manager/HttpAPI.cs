using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class HttpAPI
    {
        public string WebHost = "http://emu.axibug.com";
        public string WebSiteApi => WebHost + "/api";

        public delegate void GetRomListAPI(Action<int, Resp_GameList> callback, int page, int pageSize = 10);
        public delegate void SearchRomListAPI(Action<int, Resp_GameList> callback, string searchKey, int page, int pageSize = 10);

        public void GetNesRomList(Action<int, Resp_GameList> callback, int page, int pageSize = 10)
        {
            App.StartCoroutine(GetNesRomListFlow(page, pageSize, callback));
        }

        public void SearchNesRomList(Action<int, Resp_GameList> callback, string searchKey, int page, int pageSize = 10)
        {
            App.StartCoroutine(SearchNesRomListFlow(searchKey, page, pageSize, callback));
        }
        private IEnumerator SearchNesRomListFlow(string searchKey, int page, int pageSize, Action<int, Resp_GameList> callback)
        {
            if (!string.IsNullOrEmpty(searchKey))
            {
                string oldsearch = searchKey;
                //searchKey = System.Net.WebUtility.UrlEncode(searchKey);
                searchKey = AxiHttp.UrlEncode(searchKey);
                App.log.Info($"search->{oldsearch} ->{searchKey}");
                //searchKey =  HttpUtility.UrlDecode(searchKey);
            }
            //避免特殊字符和个别文字编码问题
            //byte[] gb2312Bytes = Encoding.Default.GetBytes(searchKey);
            //byte[] utf8Bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, gb2312Bytes);
            //// 将UTF-8编码的字节数组转换回字符串（此时是UTF-8编码的字符串）
            //string utf8String = Encoding.UTF8.GetString(utf8Bytes);
            //searchKey = UrlEncode(utf8String);
            //App.log.Info($"search->{utf8String} ->{searchKey}");
            string url = $"{WebSiteApi}/NesRomList?Page={page}&PageSize={pageSize}&SearchKey={searchKey}";
            App.log.Info($"GetRomList=>{url}");
            AxiHttpProxy.SendWebRequestProxy request = AxiHttpProxy.Get(url);
            yield return request.SendWebRequest;
            if (!request.downloadHandler.isDone)
            {
                callback.Invoke(page, null);
                yield break;
            }

            if (!request.downloadHandler.bHadErr)
            {
                var resp = JsonUtility.FromJson<Resp_GameList>(request.downloadHandler.text);
                callback.Invoke(page, resp);
                yield break;
            }

            App.log.Error(request.downloadHandler.ErrInfo);
            callback.Invoke(page, null);

            /*
            UnityWebRequest request = UnityWebRequest.Get($"{WebSiteApi}/NesRomList?Page={page}&PageSize={pageSize}&SearchKey={searchKey}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback.Invoke(null);
                yield break;
            }*/

        }
        private IEnumerator GetNesRomListFlow(int page, int pageSize, Action<int, Resp_GameList> callback)
        {
            string url = $"{WebSiteApi}/NesRomList?Page={page}&PageSize={pageSize}";
            App.log.Info($"GetRomList=>{url}");
            AxiHttpProxy.SendWebRequestProxy request = AxiHttpProxy.Get(url);
            yield return request.SendWebRequest;
            if (!request.downloadHandler.isDone)
            {
                callback.Invoke(page, null);
                yield break;
            }

            //请求成功
            if (!request.downloadHandler.bHadErr)
            {
                var resp = JsonUtility.FromJson<Resp_GameList>(request.downloadHandler.text);
                callback.Invoke(page, resp);
                yield break;
            }

            App.log.Error(request.downloadHandler.ErrInfo);
            callback.Invoke(page, null);
            /*
            UnityWebRequest request = UnityWebRequest.Get($"{WebSiteApi}/NesRomList?Page={page}&PageSize={pageSize}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback.Invoke(null);
                yield break;
            }
            */
        }

        public IEnumerator GetNesRomInfo(int RomID, Action<Resp_RomInfo> callback)
        {

            AxiHttpProxy.SendWebRequestProxy request = AxiHttpProxy.Get($"{WebSiteApi}/RomInfo?PType={PlatformType.Nes}&RomID={RomID}");
            yield return request.SendWebRequest;
            if (!request.downloadHandler.isDone)
            {
                callback.Invoke(null);
                yield break;
            }

            //成功
            if (!request.downloadHandler.bHadErr)
            {
                var resp = JsonUtility.FromJson<Resp_RomInfo>(request.downloadHandler.text);
                callback.Invoke(resp);
                yield break;
            }

            App.log.Error(request.downloadHandler.ErrInfo);
            callback.Invoke(null);

            /*
            UnityWebRequest request = UnityWebRequest.Get($"{WebSiteApi}/RomInfo?PType={PlatformType.Nes}&RomID={RomID}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback.Invoke(null);
                yield break;
            }*/

        }

        enum PlatformType : byte
        {
            All = 0,
            Nes = 1,
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
        [Serializable]
        public class Resp_CheckStandInfo
        {
            public int needUpdateClient;
            public string serverIp;
            public ushort serverPort;
            public string clientVersion;
            public string downLoadUrl;
        }
    }
}
