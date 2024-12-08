using AxibugEmuOnline.Client.ClientCore;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
                return "Assets/StreamingAssets/NES/Roms";
#elif UNITY_PSP2
                return $"{Application.dataPath}/StreamingAssets/NES/Roms";
#else
                return $"{Application.streamingAssetsPath}/NES/Roms";
#endif
			}
        }

        public Stream OpenRom(string fname)
		{
			Debug.Log($" OpenRom -> {RomDirectoryPath}/{fname}");
			try
            {
                var stream = File.Open($"{RomDirectoryPath}/{fname}", FileMode.Open,FileAccess.Read);
				Debug.Log($" OpenRom -> OK!");
				return stream;
            }
            catch (Exception ex)
            {
				Debug.Log($" OpenRom -> Err!");
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
            return new MemoryStream(Resources.Load<TextAsset>("NES/Disksys.rom").bytes);
        }

        public void SaveSRAMToFile(byte[] sramContent, string romName)
        {
            string sramDirectoryPath = $"{CorePath.DataPath}/sav";
            Directory.CreateDirectory(sramDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{sramDirectoryPath}/{romName}.sav", sramContent);
        }

        public void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            string diskFileDirectoryPath = $"{CorePath.DataPath}/dsv";
            Directory.CreateDirectory(diskFileDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{diskFileDirectoryPath}/{romName}.dsv", diskFileContent);
        }

        public EmulatorConfig Config { get; private set; } = new EmulatorConfig();

        public void PrepareDirectory(string directPath)
        {
            Directory.CreateDirectory($"{CorePath.DataPath}/{directPath}");
        }

        public void SaveFile(byte[] fileData, string directPath, string fileName)
        {
            PrepareDirectory(directPath);

            var fileFullpath = $"{CorePath.DataPath}/{directPath}/{fileName}";
            File.WriteAllBytes(fileFullpath, fileData);
        }

        public Stream OpenFile(string directPath, string fileName)
        {
            try
            {
                var data = File.ReadAllBytes($"{CorePath.DataPath}/{directPath}/{fileName}");
                if (data == null) return null;
                return new MemoryStream(data);
            }
            catch
            {
                return null;
            }

        }

        public bool TryGetMapperNo(ROM rom, out int mapperNo)
        {
            var db = Resources.Load<RomDB>("NES/ROMDB");
            return db.GetMapperNo(rom.GetPROM_CRC(), out mapperNo);
        }

        public ControllerState GetControllerState()
        {
            var mapper = NesControllerMapper.Get();
            return mapper.CreateState();
        }
    }
}
