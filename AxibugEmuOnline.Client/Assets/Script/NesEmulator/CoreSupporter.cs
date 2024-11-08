using AxibugEmuOnline.Client.ClientCore;
using AxiReplay;
using System;
using System.IO;
using System.Runtime.InteropServices;
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
                var romFile = App.nesRomLib.GetRomFile(fname);
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
            var romFile = App.nesRomLib.GetRomFile(fname);
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
            string sramDirectoryPath = $"{App.PersistentDataPath}/sav";
            Directory.CreateDirectory(sramDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{sramDirectoryPath}/{romName}.sav", sramContent);
        }

        public void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            string diskFileDirectoryPath = $"{App.PersistentDataPath}/dsv";
            Directory.CreateDirectory(diskFileDirectoryPath);
            romName = Path.GetFileNameWithoutExtension(romName);
            File.WriteAllBytes($"{diskFileDirectoryPath}/{romName}.dsv", diskFileContent);
        }

        public EmulatorConfig Config { get; private set; } = new EmulatorConfig();

        public void PrepareDirectory(string directPath)
        {
            Directory.CreateDirectory($"{App.PersistentDataPath}/{directPath}");
        }

        public void SaveFile(byte[] fileData, string directPath, string fileName)
        {
            PrepareDirectory(directPath);

            var fileFullpath = $"{App.PersistentDataPath}/{directPath}/{fileName}";
            File.WriteAllBytes(fileFullpath, fileData);
        }

        public Stream OpenFile(string directPath, string fileName)
        {
            try
            {
                var data = File.ReadAllBytes($"{App.PersistentDataPath}/{directPath}/{fileName}");
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

        private ControllerState m_sampledState;
        public ControllerState GetControllerState()
        {
            return m_sampledState;
        }

        public void SampleInput()
        {
            if (InGameUI.Instance.IsNetPlay)
            {
                if (App.roomMgr.netReplay.NextFrame(out var replayData, out int _))
                {
                    m_sampledState = FromNet(replayData);
                    var localState = NesControllerMapper.Get().CreateState();
                    var rawData = ToNet(localState);
                    App.roomMgr.SendRoomSingelPlayerInput((uint)App.roomMgr.netReplay.mCurrPlayFrame, rawData);
                }
                else
                {
                    m_sampledState = default;
                }
            }
            else
            {
                m_sampledState = NesControllerMapper.Get().CreateState();
            }
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
