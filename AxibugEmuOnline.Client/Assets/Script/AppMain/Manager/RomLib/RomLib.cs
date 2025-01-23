using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AxibugEmuOnline.Client.HttpAPI;

namespace AxibugEmuOnline.Client
{
    public class RomLib
    {
        /// <summary> Rom请求,一页的大小 </summary>
        private const int PAGE_SIZE = 10;

        /// <summary> 请求指令 </summary>
        private HashSet<int> FetchPageCmd = new HashSet<int>();
        private RomFile[] RomFetchList;
        private Dictionary<int, RomFile> RomFileIdMapper = new Dictionary<int, RomFile>();
        private Dictionary<string, RomFile> RomFileNameMapper = new Dictionary<string, RomFile>();
        private HttpAPI.GetRomListAPI m_romGetFunc;
        private HttpAPI.SearchRomListAPI m_romSearchFunc;
        private RomPlatformType m_platform;
        private string lastSearchKey;

        public RomLib(RomPlatformType platform)
        {
            m_platform = platform;
            m_romGetFunc = App.httpAPI.GetRomList;
            m_romSearchFunc = App.httpAPI.SearchRomList;

            Eventer.Instance.RegisterEvent<int, bool>(EEvent.OnRomStarStateChanged, OnRomStarStateChanged);
        }

        /// <summary> 无参构造函数将会创建一个管理收藏的Rom列表 </summary>
        public RomLib() : this(RomPlatformType.All)
        {
            m_romGetFunc = App.httpAPI.GetMarkList;
            m_romSearchFunc = App.httpAPI.SearchMarkList;
        }

        private void OnRomStarStateChanged(int romID, bool star)
        {
            if (RomFetchList == null) return;

            var targetRom = RomFetchList.FirstOrDefault(rom => rom.ID == romID);
            if (targetRom == null) return;

            targetRom.Star = star;
        }

        public RomFile GetRomFile(string romFileName)
        {
            RomFile romFile;
            RomFileNameMapper.TryGetValue(romFileName, out romFile);
            return romFile;
        }

        /// <summary> 清除所有下载的Rom文件 </summary>
        public void ClearRomFile()
        {
            var path = $"{App.PersistentDataPath(m_platform)}/RemoteRoms";
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        /// <summary> 移除一个已下载的Rom </summary>
        public void RemoveOneRomFile(RomFile romFile)
        {
            if (romFile.RomReady)
                File.Delete(romFile.LocalFilePath);
        }

        /// <summary>
        /// 获得所有Rom文件
        /// </summary>
        public void FetchRomCount(Action<RomFile[]> callback, string searchKey = null)
        {
            lastSearchKey = searchKey;
            if (string.IsNullOrWhiteSpace(searchKey))
            {
                m_romGetFunc((page, romList) =>
                {
                    FetchPageCmd.Clear();
                    RomFileIdMapper.Clear();
                    RomFileNameMapper.Clear();

                    if (romList != null)
                        RomFetchList = new RomFile[romList.resultAllCount];
                    else
                        RomFetchList = new RomFile[0];

                    for (int i = 0; i < RomFetchList.Length; i++)
                    {
                        //以后考虑用对象池实例化RomFile
                        RomFetchList[i] = new RomFile(i, i / PAGE_SIZE);
                    }
                    SaveRomInfoFromWeb(romList);

                    callback.Invoke(RomFetchList);
                }, m_platform, 0, PAGE_SIZE);
            }
            else
            {
                m_romSearchFunc((page, romList) =>
                {
                    FetchPageCmd.Clear();
                    RomFileIdMapper.Clear();
                    RomFileNameMapper.Clear();

                    if (romList != null)
                        RomFetchList = new RomFile[romList.resultAllCount];
                    else
                        RomFetchList = new RomFile[0];

                    for (int i = 0; i < RomFetchList.Length; i++)
                    {
                        //以后考虑用对象池实例化RomFile
                        RomFetchList[i] = new RomFile(i, i / PAGE_SIZE);
                    }
                    SaveRomInfoFromWeb(romList);

                    callback.Invoke(RomFetchList);
                }, m_platform, searchKey, 0, PAGE_SIZE);
            }
        }

        bool m_needFetch = false;
        public void BeginFetchRomInfo(RomFile romFile)
        {
            if (romFile.InfoReady) return;

            if (FetchPageCmd.Add(romFile.Page))
                m_needFetch = true;
        }

        public void ExecuteFetchRomInfo()
        {
            if (FetchPageCmd.Count == 0) return;
            if (!m_needFetch) return;

            foreach (var pageNo in FetchPageCmd)
            {
                if (!string.IsNullOrEmpty(lastSearchKey))
                {
                    m_romSearchFunc((page, resp) =>
                    {
                        FetchPageCmd.Remove(page);
                        SaveRomInfoFromWeb(resp);
                    }, m_platform, lastSearchKey, pageNo, PAGE_SIZE);
                }
                else
                {
                    m_romGetFunc((page, resp) =>
                    {
                        FetchPageCmd.Remove(page);
                        SaveRomInfoFromWeb(resp);
                    }, m_platform, pageNo, PAGE_SIZE);
                }
            }

            m_needFetch = false;
        }

        private void SaveRomInfoFromWeb(Resp_GameList resp)
        {
            if (resp == null) return;

            for (int i = 0; i < resp.gameList.Count; i++)
            {
                var webData = resp.gameList[i];
                RomFile targetRomFile = RomFetchList[webData.orderid];

                targetRomFile.SetWebData(webData);
                RomFileIdMapper[webData.id] = RomFetchList[webData.orderid];
                RomFileNameMapper[targetRomFile.FileName] = targetRomFile;
            }
        }

        public static string CalcHash(byte[] data)
        {
            return Helper.FileMD5Hash(data);
        }

        public void AddRomFile(RomFile rom)
        {
            RomFileNameMapper[rom.FileName] = rom;
        }
    }
}
