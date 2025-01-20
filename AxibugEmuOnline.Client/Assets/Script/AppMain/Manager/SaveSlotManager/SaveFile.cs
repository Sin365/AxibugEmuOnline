using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AxibugEmuOnline.Client
{
    /// <summary> 存档文件管理类 </summary>
    public class SaveFile
    {
        /// <summary> 指示该存档是否是自动存档 </summary>
        public bool AutoSave => SlotIndex == 0;
        /// <summary> 指示该存档所在槽位 </summary>
        public int SlotIndex { get; private set; }
        /// <summary> 指示该存档所属Rom的ID </summary>
        public int RomID { get; private set; }
        /// <summary> 指示该存档所属模拟器平台 </summary>
        public RomPlatformType EmuPlatform { get; private set; }
        /// <summary> 指示该存档是否为空 </summary>
        public bool IsEmpty { get; }

        /// <summary> 存档文件路径 </summary>
        public string FilePath
        {
            get
            {
                var path = App.PersistentDataPath(EmuPlatform);
                path = $"{path}/Slot/{EmuPlatform}/{RomID}";

                Directory.CreateDirectory(path);

                var filePath = $"{path}/slot{SlotIndex}.SlotSav";
                return filePath;
            }
        }

        public SaveFile(int romID, RomPlatformType platform, int slotIndex)
        {
            RomID = romID;
            EmuPlatform = platform;
            SlotIndex = slotIndex;

            IsEmpty = File.Exists(FilePath);
        }

        public unsafe void GetSavData(out byte[] savData, out byte[] screenShotData)
        {
            savData = null;
            screenShotData = null;

            if (!File.Exists(FilePath)) return;

            var raw = File.ReadAllBytes(FilePath);
            int headerSize = Marshal.SizeOf(typeof(Header));

            if (raw.Length < headerSize)
            {
                App.log.Warning("无效存档");
                return;
            }

            var header = new Header();
            IntPtr ptr = Marshal.AllocHGlobal(headerSize);
            Marshal.StructureToPtr(header, ptr, false);
            Marshal.Copy(raw, 0, ptr, headerSize);
            Marshal.FreeHGlobal(ptr);

            savData = new byte[header.DataLength];
            Array.Copy(raw, headerSize, savData, 0, savData.Length);
            screenShotData = new byte[header.ScreenShotLength];
            Array.Copy(raw, headerSize + savData.Length, screenShotData, 0, screenShotData.Length);

            return;
        }

        public unsafe void Save(byte[] savData, byte[] screenShotData)
        {
            var filePath = FilePath;

            var header = new Header { EmuPlatform = (byte)EmuPlatform, SlotIndex = (byte)SlotIndex, RomID = RomID, DataLength = savData.Length, ScreenShotLength = screenShotData.Length };
            int headerSize = Marshal.SizeOf(typeof(Header));
            IntPtr ptr = Marshal.AllocHGlobal(headerSize);

            var totalSize = headerSize + savData.Length + screenShotData.Length;
            byte[] raw = new byte[totalSize];

            try
            {
                Marshal.StructureToPtr(header, ptr, false);
                Marshal.Copy(ptr, raw, 0, headerSize);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            Array.Copy(savData, 0, raw, headerSize, savData.Length);
            Array.Copy(screenShotData, 0, raw, headerSize + savData.Length, screenShotData.Length);

            File.WriteAllBytes(filePath, raw);
        }

        [StructLayout(LayoutKind.Explicit, Size = 14)]
        struct Header
        {
            [FieldOffset(0)]
            public byte EmuPlatform;
            [FieldOffset(1)]
            public byte SlotIndex;
            [FieldOffset(2)]
            public int RomID;
            [FieldOffset(6)]
            public int DataLength;
            [FieldOffset(10)]
            public int ScreenShotLength;
        }
    }
}