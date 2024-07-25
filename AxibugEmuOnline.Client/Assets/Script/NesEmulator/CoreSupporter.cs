using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class CoreSupporter : ISupporterImpl
    {
        private static string RomDirectoryPath
        {
            get
            {
#if UNITY_EDITOR
                return "Assets/StreamingAssets/Roms";
#else
                return $"{Application.streamingAssetsPath}/Roms";
#endif
            }
        }

        public Stream OpenRom(string fname)
        {
            try
            {
                var stream = File.Open($"{RomDirectoryPath}/{fname}", FileMode.Open);
                return stream;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        public void GetRomPathInfo(string fname, out string fullPath, out string directPath)
        {
            directPath = RomDirectoryPath;
            fullPath = $"{directPath}/{fname}";
        }

        public Stream OpenFile_DISKSYS()
        {
            return File.Open($"{Application.streamingAssetsPath}/Disksys.rom", FileMode.Open, FileAccess.Read);
        }

        public void SaveSRAMToFile(byte[] sramContent, string romName)
        {
            string sramDirectoryPath = $"{Application.persistentDataPath}/sav";
            Directory.CreateDirectory(sramDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{sramDirectoryPath}/{romName}.sav", sramContent);
        }

        public void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            string diskFileDirectoryPath = $"{Application.persistentDataPath}/dsv";
            Directory.CreateDirectory(diskFileDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{diskFileDirectoryPath}/{romName}.dsv", diskFileContent);
        }
    }
}
