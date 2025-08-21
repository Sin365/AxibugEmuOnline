﻿using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class CoreSupporter : ISupporterImpl
    {

        public System.IO.Stream OpenRom(string fname)
        {
            try
            {
                var romFile = App.GetRomLib(RomPlatformType.Nes).GetRomFile(fname);
                var bytes = romFile.GetRomFileData();
                Debug.Log($"Open {romFile.Alias}");
                return new System.IO.MemoryStream(bytes);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        public void GetRomPathInfo(string fname, out string fullPath, out string directPath)
        {
            var romFile = App.GetRomLib(RomPlatformType.Nes).GetRomFile(fname);
            UnityEngine.Debug.Assert(romFile != null);

            fullPath = romFile.LocalFilePath;
            directPath = System.IO.Path.GetDirectoryName(fullPath);
        }

        public System.IO.Stream OpenFile_DISKSYS()
        {
            return new System.IO.MemoryStream(Resources.Load<TextAsset>("NES/Disksys.rom").bytes);
        }

        public void SaveSRAMToFile(byte[] sramContent, string romName)
        {
            string sramDirectoryPath = $"{App.PersistentDataPath(AxibugProtobuf.RomPlatformType.Nes)}/{Config.path.szSavePath}";
            AxiIO.Directory.CreateDirectory(sramDirectoryPath);
            romName = System.IO.Path.GetFileNameWithoutExtension(romName);
            AxiIO.File.WriteAllBytes($"{sramDirectoryPath}/{romName}.sav", sramContent);
        }

        public void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            string diskFileDirectoryPath = $"{App.PersistentDataPath(AxibugProtobuf.RomPlatformType.Nes)}/dsv";
            AxiIO.Directory.CreateDirectory(diskFileDirectoryPath);
            romName = System.IO.Path.GetFileNameWithoutExtension(romName);
            AxiIO.File.WriteAllBytes($"{diskFileDirectoryPath}/{romName}.dsv", diskFileContent);
        }

        public EmulatorConfig Config { get; private set; } = new EmulatorConfig();
        public void PrepareDirectory(string directPath)
        {
            AxiIO.Directory.CreateDirectory($"{App.PersistentDataPath(AxibugProtobuf.RomPlatformType.Nes)}/{directPath}");
        }

        public void SaveFile(byte[] fileData, string directPath, string fileName)
        {
            PrepareDirectory(directPath);

            var fileFullpath = $"{App.PersistentDataPath(AxibugProtobuf.RomPlatformType.Nes)}/{directPath}/{fileName}";
            AxiIO.File.WriteAllBytes(fileFullpath, fileData);
        }

        public System.IO.Stream OpenFile(string directPath, string fileName)
        {
            try
            {
                var path = $"{App.PersistentDataPath(AxibugProtobuf.RomPlatformType.Nes)}/{directPath}/{fileName}";
                var data = AxiIO.File.ReadAllBytes(path);
                if (data == null) return null;
                return new System.IO.MemoryStream(data);
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


        public ControllerState FromNet(AxiReplay.ReplayStep step)
        {
            var temp = new ServerInputSnapShot();
            var result = new ControllerState();
            temp.all = step.InPut;
            result.raw0 = temp.p1;
            result.raw1 = temp.p2;
            result.raw2 = temp.p3;
            result.raw3 = temp.p4;
            result.valid = true;

            return result;
        }

        public uint ToNet(ControllerState state)
        {
            var temp = new ServerInputSnapShot();
            temp.p1 = (byte)state.raw0;
            temp.p2 = (byte)state.raw1;
            temp.p3 = (byte)state.raw2;
            temp.p4 = (byte)state.raw3;
            return (uint)temp.all;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        struct ServerInputSnapShot
        {
            [FieldOffset(0)]
            public UInt64 all;
            [FieldOffset(0)]
            public byte p1;
            [FieldOffset(1)]
            public byte p2;
            [FieldOffset(2)]
            public byte p3;
            [FieldOffset(3)]
            public byte p4;
        }
    }
}
