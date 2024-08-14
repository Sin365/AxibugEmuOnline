using AxibugEmuOnline.Client.ClientCore;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using System.Linq;
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

        public string LocalFilePath => $"{Application.persistentDataPath}/RemoteRoms/{platform}/{fileName}";
        public bool FileReady => hasLocalFile;
        public int ID => webData != null ? webData.id : -1;
        public bool IsDownloading { get; private set; }
        public string Alias => webData.romName;
        public string FileName => fileName;

        public event Action OnDownloadOver;

        public RomFile(EnumPlatform platform)
        {
            this.platform = platform;
        }

        public void BeginDownload()
        {
            if (FileReady) return;
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
            if (!FileReady) throw new Exception("Rom File Not Downloaded");

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
        }
    }
}
