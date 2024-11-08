using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using System;
using System.Collections.Generic;
using System.IO;
using static AxibugEmuOnline.Client.HttpAPI;

namespace AxibugEmuOnline.Client
{
    public class RomLib
    {
        /// <summary> Rom请求,一页的大小 </summary>
        private const int PAGE_SIZE = 10;

        /// <summary> 请求指令 </summary>
        private HashSet<int> FetchPageCmd = new HashSet<int>();
        private RomFile[] nesRomFetchList;
        private Dictionary<int, RomFile> nesRomFileIdMapper = new Dictionary<int, RomFile>();
        private Dictionary<string, RomFile> nesRomFileNameMapper = new Dictionary<string, RomFile>();
        private HttpAPI.GetRomListAPI m_romGetFunc;
        private EnumPlatform m_platform;

        public RomLib(EnumPlatform platform)
        {
            m_platform = platform;
            switch (platform)
            {
                case EnumPlatform.NES:
                    m_romGetFunc = App.httpAPI.GetNesRomList;
                    break;
            }
        }

        public RomFile GetExistRom(string fileName)
        {
            var res = RomFile.CreateExistRom(m_platform, fileName);

            nesRomFileNameMapper[res.FileName] = res;

            return res;
        }

        public RomFile GetRomFile(string romFileName)
        {
            RomFile romFile;
            nesRomFileNameMapper.TryGetValue(romFileName, out romFile);
            return romFile;
        }



        /// <summary> 清除所有下载的Rom文件 </summary>
        public void ClearRomFile()
        {
            var path = $"{App.PersistentDataPath}/RemoteRoms/{m_platform}";
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
        /// <param name="callback"></param>
        public void FetchRomCount(Action<RomFile[]> callback)
        {
            m_romGetFunc((romList) =>
            {
                FetchPageCmd.Clear();
                nesRomFileIdMapper.Clear();
                nesRomFileNameMapper.Clear();
                nesRomFetchList = new RomFile[romList.resultAllCount];
                for (int i = 0; i < nesRomFetchList.Length; i++)
                {
                    //以后考虑用对象池实例化RomFile
                    nesRomFetchList[i] = new RomFile(m_platform, i, i / PAGE_SIZE);
                }
                SaveRomInfoFromWeb(romList);

                callback.Invoke(nesRomFetchList);
            }, 0, PAGE_SIZE);
        }

        public void BeginFetchRomInfo(RomFile romFile)
        {
            if (romFile.InfoReady) return;

            FetchPageCmd.Add(romFile.Page);
        }

        public void ExecuteFetchRomInfo()
        {
            if (FetchPageCmd.Count == 0) return;

            foreach (var pageNo in FetchPageCmd)
            {
                m_romGetFunc(SaveRomInfoFromWeb, pageNo, PAGE_SIZE);
            }
            FetchPageCmd.Clear();
        }

        private void SaveRomInfoFromWeb(Resp_GameList resp)
        {
            for (int i = 0; i < resp.gameList.Count; i++)
            {
                var webData = resp.gameList[i];
                RomFile targetRomFile = nesRomFetchList[webData.orderid];

                targetRomFile.SetWebData(webData);
                nesRomFileIdMapper[webData.id] = nesRomFetchList[webData.orderid];
                nesRomFileNameMapper[targetRomFile.FileName] = targetRomFile;
            }
        }

        public static string CalcHash(byte[] data)
        {
            return Helper.FileMD5Hash(data);
        }

        public void AddRomFile(RomFile rom)
        {
            nesRomFileNameMapper[rom.FileName] = rom;
        }
    }
}
