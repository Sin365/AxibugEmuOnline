using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class RomLib
    {
        private List<RomFile> nesRomFiles = new List<RomFile>();

        public void GetNesRomFile(int index, Action<RomFile> callback)
        {
            if (nesRomFiles.Count <= index)
            {
                int pageSize = 10;
                int page = index / pageSize;
                int indexInPage = index % page;

                //填充空的RomFile数据
                var needFill = index - nesRomFiles.Count + 1;
                needFill += pageSize - indexInPage - 1;
                for (int i = 0; i < needFill; i++)
                {
                    nesRomFiles.Add(new RomFile(EnumPlatform.NES));
                }

                AppAxibugEmuOnline.httpAPI.GetNesRomList((romList) =>
                {
                    if (romList == null)
                    {
                        callback.Invoke(null);
                    }
                    else
                    {
                        for (int i = 0; i < romList.GameList.Count; i++)
                        {
                            nesRomFiles[pageSize * (page - 1) + i].SetWebData(romList.GameList[i]);
                        }
                    }
                }, page, pageSize);
            }
        }
    }
}
