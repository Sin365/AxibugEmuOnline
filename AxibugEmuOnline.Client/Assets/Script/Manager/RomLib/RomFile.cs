using AxibugEmuOnline.Client.ClientCore;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AxibugEmuOnline.Client
{
    public class RomFile
    {
        private HttpAPI.Resp_RomInfo webData;
        private bool hasLocalFile;
        private string fileName;
        private EnumPlatform platform;

        /// <summary> 指示该Rom文件的存放路径 </summary>
        public string LocalFilePath => $"{CoreSupporter.PersistentDataPath}/RemoteRoms/{platform}/{fileName}";
        /// <summary> 指示该Rom文件是否已下载完毕 </summary>
        public bool RomReady => hasLocalFile;
        /// <summary> 指示是否正在下载Rom文件 </summary>
        public bool IsDownloading { get; private set; }

        /// <summary> 指示该Rom信息是否已填充 </summary>
        public bool InfoReady => webData != null;
        /// <summary> 唯一标识 </summary>
        public int ID => webData != null ? webData.id : -1;
        /// <summary> 别名 </summary>
        public string Alias => webData.romName;
        /// <summary> 描述 </summary>
        public string Descript => webData.desc;

        /// <summary> 文件名 </summary>
        public string FileName => fileName;
        /// <summary> 在查询结果中的索引 </summary>
        public int Index { get; private set; }
        /// <summary> 在查询结果中的所在页 </summary>
        public int Page { get; private set; }

        public event Action OnDownloadOver;
        public event Action OnInfoFilled;

        public RomFile(EnumPlatform platform, int index, int insidePage)
        {
            this.platform = platform;
            Index = index;
            Page = insidePage;
        }

        public void BeginDownload()
        {
            if (RomReady) return;
            if (IsDownloading) return;

            IsDownloading = true;
            AppAxibugEmuOnline.StartCoroutine(DownloadRemoteRom((bytes) =>
            {
                IsDownloading = false;

                if (bytes != null)
                {
                    var directPath = Path.GetDirectoryName(LocalFilePath);
                    Directory.CreateDirectory(directPath);

                    File.WriteAllBytes(LocalFilePath, bytes);
                    hasLocalFile = true;
                }
                OnDownloadOver?.Invoke();
            }));
        }

        public byte[] GetRomFileData()
        {
            if (webData == null) throw new Exception("Not Valid Rom");
            if (!RomReady) throw new Exception("Rom File Not Downloaded");

            var bytes = File.ReadAllBytes(LocalFilePath);
            if (Path.GetExtension(LocalFilePath).ToLower() == ".zip")
            {
                var zip = new ZipInputStream(new MemoryStream(bytes));
                while (zip.GetNextEntry() is ZipEntry entry)
                {
                    if (!entry.Name.ToLower().EndsWith(".nes")) continue;

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
            UnityWebRequest uwr = UnityWebRequest.Get($"{AppAxibugEmuOnline.httpAPI.DownSite}/{webData.url}");
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback(null);
            }
            else
            {
                callback(uwr.downloadHandler.data);
            }
        }

        public void SetWebData(HttpAPI.Resp_RomInfo resp_RomInfo)
        {
            webData = resp_RomInfo;
            fileName = Path.GetFileName(webData.url);
            fileName = System.Net.WebUtility.UrlDecode(fileName);

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
