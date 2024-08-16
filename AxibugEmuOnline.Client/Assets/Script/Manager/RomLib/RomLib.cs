using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class RomLib
    {
        private Dictionary<int, RomFile> nesRomFileIdMapper = new Dictionary<int, RomFile>();
        private Dictionary<string, RomFile> nesRomFileNameMapper = new Dictionary<string, RomFile>();

        public RomFile GetNesRomFile(string romFileName)
        {
            RomFile romFile;
            nesRomFileNameMapper.TryGetValue(romFileName, out romFile);
            return romFile;
        }

        public void GetNesRomFile(int page, int pageSize, Action<List<RomFile>> callback)
        {
            AppAxibugEmuOnline.httpAPI.GetNesRomList((romList) =>
            {
                if (romList == null)
                {
                    callback.Invoke(null);
                }
                else
                {
                    List<RomFile> result = new List<RomFile>();
                    for (int i = 0; i < romList.gameList.Count; i++)
                    {
                        var webData = romList.gameList[i];
                        RomFile targetRomFile;
                        nesRomFileIdMapper.TryGetValue(webData.id, out targetRomFile);
                        if (targetRomFile == null)
                        {
                            targetRomFile = new RomFile(EnumPlatform.NES);
                            targetRomFile.SetWebData(webData);
                            nesRomFileIdMapper[webData.id] = targetRomFile;

                            nesRomFileNameMapper[targetRomFile.FileName] = targetRomFile;
                        }

                        result.Add(targetRomFile);
                    }

                    callback(result);
                }
            }, page, pageSize);
        }

        public static string CalcHash(byte[] data)
        {
            return string.Empty; //todo : 等待远程仓库敲定hash算法
            //var hashBytes = MD5.Create().ComputeHash(data);
            //StringBuilder sb = new StringBuilder();
            //foreach (byte b in hashBytes)
            //{
            //    sb.Append(b.ToString("x2"));
            //}

            //return sb.ToString();
        }
    }
}
