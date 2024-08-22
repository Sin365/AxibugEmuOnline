using AxibugEmuOnline.Client.ClientCore;
using System;
using System.IO;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class CoreSupporter : ISupporterImpl
    {
        public Stream OpenRom(string fname)
        {
            try
            {
                var romFile = AppAxibugEmuOnline.nesRomLib.GetRomFile(fname);
                var bytes = romFile.GetRomFileData();
                Debug.Log($"Open {romFile.Alias}");
                return new MemoryStream(bytes);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        public void GetRomPathInfo(string fname, out string fullPath, out string directPath)
        {
            var romFile = AppAxibugEmuOnline.nesRomLib.GetRomFile(fname);
            UnityEngine.Debug.Assert(romFile != null);

            fullPath = romFile.LocalFilePath;
            directPath = Path.GetDirectoryName(fullPath);
        }

        public Stream OpenFile_DISKSYS()
        {
            return new MemoryStream(Resources.Load<TextAsset>("NES/Disksys.rom").bytes);
        }

        public void SaveSRAMToFile(byte[] sramContent, string romName)
        {
            string sramDirectoryPath = $"{AppAxibugEmuOnline.PersistentDataPath}/sav";
            Directory.CreateDirectory(sramDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{sramDirectoryPath}/{romName}.sav", sramContent);
        }

        public void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            string diskFileDirectoryPath = $"{AppAxibugEmuOnline.PersistentDataPath}/dsv";
            Directory.CreateDirectory(diskFileDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{diskFileDirectoryPath}/{romName}.dsv", diskFileContent);
        }

        public EmulatorConfig Config { get; private set; } = new EmulatorConfig();

        public void PrepareDirectory(string directPath)
        {
            Directory.CreateDirectory($"{AppAxibugEmuOnline.PersistentDataPath}/{directPath}");
        }

        public void SaveFile(byte[] fileData, string directPath, string fileName)
        {
            PrepareDirectory(directPath);

            var fileFullpath = $"{AppAxibugEmuOnline.PersistentDataPath}/{directPath}/{fileName}";
            File.WriteAllBytes(fileFullpath, fileData);
        }

        public Stream OpenFile(string directPath, string fileName)
        {
            try
            {
                var data = File.ReadAllBytes($"{AppAxibugEmuOnline.PersistentDataPath}/{directPath}/{fileName}");
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
