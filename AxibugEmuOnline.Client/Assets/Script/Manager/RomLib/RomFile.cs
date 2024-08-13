using System;
using System.IO;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RomFile
    {
        private HttpAPI.Resp_RomInfo webData;
        private bool hasLocalFile;
        private string romName;
        private EnumPlatform platform;

        public string LocalFilePath => $"{Application.persistentDataPath}/RemoteRoms/{platform}/{romName}";


        public RomFile(EnumPlatform platform)
        {
            this.platform = platform;
        }

        public void GetRomFileData(Action<byte[]> callback)
        {
            if (webData == null) { callback.Invoke(null); return; }
            if (hasLocalFile)
            {
                var path = LocalFilePath;
            }
        }

        public void SetWebData(HttpAPI.Resp_RomInfo resp_RomInfo)
        {
            webData = resp_RomInfo;
            romName = Path.GetFileName(webData.Url);

            if (File.Exists(LocalFilePath))
            {
                hasLocalFile = true;
            }
            else
            {
                hasLocalFile = false;
            }
        }
    }
}
