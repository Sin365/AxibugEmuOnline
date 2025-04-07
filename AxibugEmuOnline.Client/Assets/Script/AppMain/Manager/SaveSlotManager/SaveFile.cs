using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;
using AxibugProtobuf;
using MAME.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AxibugEmuOnline.Client
{
    /// <summary> 存档文件管理类 </summary>
    public class SaveFile
    {
        public SavCloudApi CloudAPI => App.SavMgr.CloudApi;

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

        /// <summary> 存档顺序号,用于判断云存档和本地存档的同步状态,是否存在冲突 </summary>
        public uint Sequecen { get; private set; }
        SimpleFSM<SaveFile> FSM;

        public SaveFile(int romID, RomPlatformType platform, int slotIndex)
        {
            RomID = romID;
            EmuPlatform = platform;
            SlotIndex = slotIndex;
            FSM = new SimpleFSM<SaveFile>(this);
            FSM.AddState<UnkownState>();
            FSM.AddState<CheckingState>();
            FSM.AddState<DownloadingState>();
            FSM.AddState<UploadingState>();
            FSM.AddState<SyncedState>();

            IsEmpty = File.Exists(FilePath);

            if (IsEmpty) Sequecen = 0;
            else //从文件头读取存储序号
            {
                byte[] saveOrderData = new byte[4];
                var streaming = File.OpenRead(FilePath);
                int res = streaming.Read(saveOrderData, 0, 4);
                if (res != 4) //无效的存档文件
                {
                    IsEmpty = true;
                    File.Delete(FilePath);
                }
                else
                {
                    Sequecen = BitConverter.ToUInt32(saveOrderData, 0);
                }

                streaming.Dispose();
            }

            FSM.ChangeState<UnkownState>();
        }

        public void Update()
        {
            FSM.Update();
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

        public unsafe void Save(uint sequence, byte[] savData, byte[] screenShotData)
        {
            var filePath = FilePath;

            var header = new Header
            {
                Sequence = sequence,
                RomID = RomID,
                DataLength = savData.Length,
                ScreenShotLength = screenShotData.Length
            };
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
            Sequecen = sequence;
        }

        /// <summary>
        /// 尝试同步存档
        /// </summary>
        public void TrySync()
        {
            if (FSM.CurrentState is not UnkownState && FSM.CurrentState is not SyncedState) return;

            FSM.ChangeState<CheckingState>();
        }


        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct Header
        {
            [FieldOffset(0)]
            public uint Sequence;
            [FieldOffset(4)]
            public int RomID;
            [FieldOffset(8)]
            public int DataLength;
            [FieldOffset(12)]
            public int ScreenShotLength;
        }

        public enum EnumState
        {
            Unkown,
            Checking,
            Downloading,
            Uploading,
            Synced
        }
    }
}