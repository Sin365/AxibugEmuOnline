using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RomFile
    {
        private HttpAPI.Resp_RomInfo webData;
        /// <summary> 依赖的Rom文件 </summary>
        private List<RomFile> dependencies = new List<RomFile>();
        RomPlatformType m_defaultPlatform;

        /// <summary> 指示该Rom是否是多文件Rom </summary>
        public bool MultiFileRom
        {
            get
            {
                switch (Platform)
                {
                    case RomPlatformType.Nes:
                    case RomPlatformType.MasterSystem:
                    case RomPlatformType.GameGear:
                    case RomPlatformType.GameBoy:
                    case RomPlatformType.GameBoyColor:
                    case RomPlatformType.ColecoVision:
                    case RomPlatformType.Sc3000:
                    case RomPlatformType.Sg1000:
                        return false;
                    case RomPlatformType.Igs:
                    case RomPlatformType.Cps1:
                    case RomPlatformType.Cps2:
                    case RomPlatformType.Neogeo:
                    case RomPlatformType.ArcadeOld:
                        return true;
                    default: throw new NotImplementedException($"未实现的平台{Platform}");
                }
            }
        }

        public string FileExtentionName
        {
            get
            {
                switch (Platform)
                {
                    case RomPlatformType.Nes: return ".nes";

                    case RomPlatformType.MasterSystem: //return ".sms";
                    case RomPlatformType.GameGear:
                    case RomPlatformType.GameBoy: //return ".gb";
                    case RomPlatformType.GameBoyColor: //return ".gbc";
                    case RomPlatformType.ColecoVision: 
                    case RomPlatformType.Sc3000:
                    case RomPlatformType.Sg1000:
                        throw new NotImplementedException($"该平台使用核心内zip解压{Platform}");


                    default: throw new NotImplementedException($"未实现的平台{Platform}");
                }
            }
        }

        bool m_hasLocalFile;
        public bool HasLocalFile
        {
            get
            {
                if (!m_hasLocalFile) return false;

                foreach (var depRom in dependencies)
                {
                    if (!depRom.HasLocalFile) return false;
                }

                return true;
            }
        }

        /// <summary> 指示该Rom文件的存放路径 </summary>
        public string LocalFilePath => $"{App.PersistentDataPath(Platform)}/RemoteRoms/{FileName}";

        /// <summary> 指示该Rom文件是否已下载完毕 </summary>
        public bool RomReady
        {
            get
            {
                if (!HasLocalFile) return false;

                foreach (var depRom in dependencies)
                {
                    if (!depRom.RomReady) return false;
                }

                return true;
            }
        }
        /// <summary> 指示是否正在下载Rom文件 </summary>
        public bool IsDownloading
        {
            get
            {
                if (!InfoReady) return false;

                var progress = App.FileDownloader.GetDownloadProgress(webData.url);
                if (progress.HasValue) return true;

                foreach (var depRom in dependencies)
                {
                    if (depRom.IsDownloading) return true;
                }

                return false;
            }
        }

        public float Progress
        {
            get
            {
                if (!IsDownloading) return 0;

                float progress = 0f;

                if (m_hasLocalFile) progress = 1f;
                else
                {
                    var downloadProgress = App.FileDownloader.GetDownloadProgress(webData.url);
                    progress = downloadProgress.HasValue ? downloadProgress.Value : 0;
                }

                foreach (var depRom in dependencies)
                {
                    progress += depRom.Progress;
                }

                return progress / (dependencies.Count + 1);
            }
        }

        public RomPlatformType Platform => webData != null ? (RomPlatformType)webData.ptype : m_defaultPlatform;
        /// <summary> 指示该Rom信息是否已填充 </summary>
        public bool InfoReady
        {
            get
            {
                if (webData == null) return false;

                foreach (var depRom in dependencies)
                {
                    if (!depRom.InfoReady) return false;
                }

                return true;
            }
        }
        /// <summary> 唯一标识 </summary>
        public int ID => webData != null ? webData.id : -1;
        /// <summary> 别名 </summary>
        public string Alias => webData.romName;
        /// <summary> 描述 </summary>
        public string Descript => webData.desc;
        /// <summary> 游戏类型 </summary>
        public string GameTypeDes => webData.gType;
        /// <summary> 小图URL </summary>
        public string ImageURL => webData.imgUrl;

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

        public RomFile(int index, int insidePage, RomPlatformType defaultPlatform)
        {
            Index = index;
            Page = insidePage;
            m_defaultPlatform = defaultPlatform;
        }

        public void CheckLocalFileState()
        {
            if (webData == null) m_hasLocalFile = false;
            else
            {
                if (App.FileDownloader.GetDownloadProgress(webData.url) == null)
                {
                    if (MultiFileRom)
                        m_hasLocalFile = AxiIO.Directory.Exists(LocalFilePath);
                    else
                        m_hasLocalFile = AxiIO.File.Exists(LocalFilePath);
                }
            }

            foreach (var depRom in dependencies)
                depRom.CheckLocalFileState();
        }

        public void BeginDownload()
        {
            if (RomReady) return;
            if (IsDownloading) return;

            //检查依赖Rom的下载情况            
            foreach (var depRom in dependencies)
            {
                depRom.BeginDownload();
            }
            App.FileDownloader.BeginDownload(webData.url, (bytes) =>
            {
                HandleRomFilePostProcess(bytes, webData.romName);
            });
        }

        private void HandleRomFilePostProcess(byte[] bytes, string romName)
        {
            if (bytes == null) return;

            if (MultiFileRom)
            {
                Dictionary<string, byte[]> unzipFiles = new Dictionary<string, byte[]>();
                //多rom文件的平台,下载下来的数据直接解压放入文件夹内
                var zip = new ZipInputStream(new System.IO.MemoryStream(bytes));

                List<string> depth0Files = new List<string>();
                while (true)
                {
                    var currentEntry = zip.GetNextEntry();
                    if (currentEntry == null) break;

                    if (currentEntry.IsDirectory) continue;

                    var buffer = new byte[1024];
                    System.IO.MemoryStream output = new System.IO.MemoryStream();
                    while (true)
                    {
                        var size = zip.Read(buffer, 0, buffer.Length);
                        if (size == 0) break;
                        else output.Write(buffer, 0, size);
                    }
                    output.Flush();
                    unzipFiles[$"{LocalFilePath}/{currentEntry.Name}"] = output.ToArray();
                }

                string rootDirName = null;
                //如果第一层目录只有一个文件并且是文件夹,则所有文件层级外提一层
                if (depth0Files.Count == 1 && depth0Files[0].Contains('.'))
                {
                    rootDirName = depth0Files[0];
                }

                foreach (var item in unzipFiles)
                {
                    var path = rootDirName != null ? item.Key.Substring(0, rootDirName.Length + 1) : item.Key;
                    var data = item.Value;
                    AxiIO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    AxiIO.File.WriteAllBytes(path, data);
                }
            }
            else
            {
                var directPath = System.IO.Path.GetDirectoryName(LocalFilePath);
                AxiIO.Directory.CreateDirectory(directPath);

                AxiIO.File.WriteAllBytes(LocalFilePath, bytes);
            }
            OverlayManager.PopTip($"下载完毕:[{romName}]");
            Eventer.Instance.PostEvent(EEvent.OnRomFileDownloaded, ID);
            OnDownloadOver?.Invoke(this);
        }

        public byte[] GetRomFileData()
        {
            Debug.Assert(!MultiFileRom, "仅供单文件Rom使用的接口");

            if (webData == null) throw new Exception("Not Valid Rom");
            if (!RomReady) throw new Exception("Rom File Not Downloaded");

            var bytes = AxiIO.File.ReadAllBytes(LocalFilePath);
            if (System.IO.Path.GetExtension(LocalFilePath).ToLower() == ".zip")
            {
                var zip = new ZipInputStream(new System.IO.MemoryStream(bytes));
                while (true)
                {
                    var currentEntry = zip.GetNextEntry();
                    if (currentEntry == null) break;

                    //当前平台单文件rom扩展名判断
                    if (!currentEntry.Name.ToLower().EndsWith(FileExtentionName)) continue;

                    var buffer = new byte[1024];
                    System.IO.MemoryStream output = new System.IO.MemoryStream();
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

        public void SetWebData(HttpAPI.Resp_RomInfo resp_RomInfo)
        {
            webData = resp_RomInfo;
            FileName = MultiFileRom ? System.IO.Path.GetFileNameWithoutExtension(webData.url) : System.IO.Path.GetFileName(webData.url);
            FileName = System.Net.WebUtility.UrlDecode(FileName);

            //收集依赖Rom
            if (webData.parentRomIdsList != null)
            {
                dependencies.Clear();
                foreach (var romID in webData.parentRomIdsList)
                {
                    var romFile = new RomFile(Index, Page, m_defaultPlatform);
                    dependencies.Add(romFile);
                    App.StartCoroutine(App.httpAPI.GetRomInfo(romID, (romInfo) =>
                    {
                        romFile.SetWebData(romInfo);
                    }));
                }
            }

            CheckLocalFileState();

            App.StartCoroutine(WaitInfoReady());
        }

        private IEnumerator WaitInfoReady()
        {
            while (!InfoReady) yield return null;

            OnInfoFilled?.Invoke();
        }
    }
}
