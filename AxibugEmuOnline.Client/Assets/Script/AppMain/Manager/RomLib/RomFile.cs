using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RomFile
    {
        private HttpAPI.Resp_RomInfo webData;
        private bool hasLocalFile;
        //private UnityWebRequest downloadRequest;
        private AxiHttpProxy.SendDownLoadProxy downloadRequest;

        public bool IsUserRom { get; private set; }

        /// <summary> 指示该Rom文件的存放路径 </summary>
        public string LocalFilePath =>
                                    IsUserRom ?
                                    $"{App.PersistentDataPath(Platform)}/UserRoms/{FileName}" :
                                    $"{App.PersistentDataPath(Platform)}/RemoteRoms/{FileName}";

        /// <summary> 指示该Rom文件是否已下载完毕 </summary>
        public bool RomReady => hasLocalFile;
        ///// <summary> 指示是否正在下载Rom文件 </summary>
        //public bool IsDownloading => downloadRequest != null && downloadRequest.result == UnityWebRequest.Result.InProgress;
        //public float Progress => IsDownloading ? downloadRequest.downloadProgress : 0;

        /// <summary> 指示是否正在下载Rom文件 </summary>
        public bool IsDownloading => downloadRequest != null && !downloadRequest.downloadHandler.isDone;
        public float Progress => IsDownloading ? downloadRequest.downloadHandler.DownLoadPr : 0;


        public RomPlatformType Platform => webData != null ? (RomPlatformType)webData.ptype : RomPlatformType.Invalid;
        /// <summary> 指示该Rom信息是否已填充 </summary>
        public bool InfoReady => webData != null;
        /// <summary> 唯一标识 </summary>
        public int ID => webData != null ? webData.id : -1;
        /// <summary> 别名 </summary>
        public string Alias => IsUserRom ? Path.GetFileNameWithoutExtension(FileName) : webData.romName;
        /// <summary> 描述 </summary>
        public string Descript => IsUserRom ? string.Empty : webData.desc;
        /// <summary> 游戏类型 </summary>
        public string GameTypeDes => IsUserRom ? string.Empty : webData.gType;
        /// <summary> 小图URL </summary>
        public string ImageURL => IsUserRom ? string.Empty : webData.imgUrl;

        /// <summary> 文件名 </summary>
        public string FileName { get; private set; }
        /// <summary> 在查询结果中的索引 </summary>
        public int Index { get; private set; }
        /// <summary> 在查询结果中的所在页 </summary>
        public int Page { get; private set; }
        public string Hash => webData != null ? webData.hash : string.Empty;
        /// <summary> 标记是否收藏 </summary>
        public bool Star
        {
            get { return webData != null ? webData.isStar > 0 : false; }
            set
            {
                if (webData == null) return;
                webData.isStar = value ? 1 : 0;
            }
        }

        public event Action<RomFile> OnDownloadOver;
        public event Action OnInfoFilled;

        public RomFile(int index, int insidePage)
        {
            Index = index;
            Page = insidePage;
        }

        public void BeginDownload()
        {
            if (RomReady) return;
            if (IsDownloading) return;

            App.StartCoroutine(DownloadRemoteRom((bytes) =>
            {
                if (bytes != null)
                {
                    var directPath = Path.GetDirectoryName(LocalFilePath);
                    Directory.CreateDirectory(directPath);

                    File.WriteAllBytes(LocalFilePath, bytes);
                    hasLocalFile = true;

                    Eventer.Instance.PostEvent(EEvent.OnRomFileDownloaded, ID);
                }
                OnDownloadOver?.Invoke(this);
            }));
        }

        public byte[] GetRomFileData()
        {
            if (!IsUserRom && webData == null) throw new Exception("Not Valid Rom");
            if (!RomReady) throw new Exception("Rom File Not Downloaded");

            var bytes = File.ReadAllBytes(LocalFilePath);
            if (Path.GetExtension(LocalFilePath).ToLower() == ".zip")
            {
                var zip = new ZipInputStream(new MemoryStream(bytes));
                while (true)
                {
                    var currentEntry = zip.GetNextEntry();
                    if (currentEntry == null) break;

                    if (!currentEntry.Name.ToLower().EndsWith(".nes")) continue;

                    var buffer = new byte[1024];
                    MemoryStream output = new MemoryStream();
                    while (true)
                    {
                        var size = zip.Read(buffer, 0, buffer.Length);
                        if (size == 0) break;
                        else output.Write(buffer, 0, size);
                    }
                    output.Flush();
                    return output.ToArray();
                }
            }
            else
            {
                return bytes;
            }

            throw new Exception("Not Valid Rom Data");
        }

        private IEnumerator DownloadRemoteRom(Action<byte[]> callback)
        {
            downloadRequest = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{webData.url}");

            while (!downloadRequest.downloadHandler.isDone)
            {
                yield return null;
                Debug.Log($"下载进度：{downloadRequest.downloadHandler.DownLoadPr} ->{downloadRequest.downloadHandler.loadedLenght}/{downloadRequest.downloadHandler.NeedloadedLenght}");
            }
            AxiHttpProxy.ShowAxiHttpDebugInfo(downloadRequest.downloadHandler);

            var request = downloadRequest;
            downloadRequest = null;

            if (!request.downloadHandler.bHadErr)
                callback(request.downloadHandler.data);
            else
                callback(null);

            //downloadRequest = UnityWebRequest.Get($"{App.httpAPI.WebHost}/{webData.url}");
            //yield return downloadRequest.SendWebRequest();

            //var request = downloadRequest;
            //downloadRequest = null;

            //if (request.result != UnityWebRequest.Result.Success)
            //{
            //	callback(null);
            //}
            //else
            //{
            //	callback(request.downloadHandler.data);
            //}
        }

        public void SetWebData(HttpAPI.Resp_RomInfo resp_RomInfo)
        {
            webData = resp_RomInfo;
            FileName = Path.GetFileName(webData.url);
            FileName = System.Net.WebUtility.UrlDecode(FileName);

            if (File.Exists(LocalFilePath))
            {
                var fileBytes = File.ReadAllBytes(LocalFilePath);
                var localHash = RomLib.CalcHash(fileBytes);

                hasLocalFile = localHash == webData.hash;
                if (!hasLocalFile)
                    File.Delete(LocalFilePath);
            }
            else
            {
                hasLocalFile = false;
            }

            OnInfoFilled?.Invoke();
        }

    }
}
