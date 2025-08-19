using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using SyncingFileRecord = System.Collections.Generic.Dictionary<AxibugProtobuf.RomPlatformType, System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<AxibugEmuOnline.Client.SaveFile.SyncingFiles>>>;

namespace AxibugEmuOnline.Client
{
    /// <summary> 存档文件抽象类 </summary>
    public partial class SaveFile
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
        public bool IsEmpty { get; private set; }
        /// <summary> 存档文件路径 </summary>
        public string FilePath
        {
            get
            {
                var path = App.UserPersistenDataPath(EmuPlatform);
                path = $"{path}/Slot/{RomID}";

                AxiIO.Directory.CreateDirectory(path);

                var filePath = $"{path}/slot{SlotIndex}.SlotSav";
                return filePath;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (FSM.CurrentState is DownloadingState) return true;
                else if (FSM.CurrentState is UploadingState) return true;

                return false;
            }
        }

        public SimpleFSM<SaveFile>.State GetCurrentState()
        {
            return FSM.CurrentState;
        }

        public event Action OnSavSuccessed;
        public event Action OnStateChanged;

        /// <summary> 存档顺序号,用于判断云存档和本地存档的同步状态,是否存在冲突 </summary>
        public uint Sequecen { get; private set; }

        public SimpleFSM<SaveFile> FSM { get; private set; }
        byte[] m_savDataCaches;
        byte[] m_screenShotCaches;
        Header m_headerCache;
        bool m_cacheOutdate = true;

        public SaveFile(int romID, RomPlatformType platform, int slotIndex)
        {
            RomID = romID;
            EmuPlatform = platform;
            SlotIndex = slotIndex;
            FSM = new SimpleFSM<SaveFile>(this);
            FSM.AddState<IdleState>();
            FSM.AddState<CheckingNetworkState>();//？
            FSM.AddState<CheckingState>();
            FSM.AddState<DownloadingState>();
            FSM.AddState<ConflictState>();
            FSM.AddState<UploadingState>();
            FSM.AddState<SyncedState>();
            FSM.AddState<SyncFailedState>();
            FSM.OnStateChanged += FSM_OnStateChanged;

            IsEmpty = !AxiIO.File.Exists(FilePath);

            if (IsEmpty) Sequecen = 0;
            else //从文件头读取存储序号
            {
                byte[] saveOrderData = new byte[4];

                int res = AxiIO.File.ReadBytesToArr(FilePath, saveOrderData, 0, 4);
                if (res < 4) //无效的存档文件
                {
                    IsEmpty = true;
                    AxiIO.File.Delete(FilePath);
                }
                else
                {
                    Sequecen = BitConverter.ToUInt32(saveOrderData, 0);
                }
            }

            FSM.ChangeState<IdleState>();
        }

        private void FSM_OnStateChanged()
        {
            OnStateChanged?.Invoke();
        }

        public void Update()
        {
            FSM.Update();
        }

        /// <summary>
        /// 获得存档的保存时间(UTC)
        /// </summary>
        /// <returns></returns>
        public DateTime GetSavTimeUTC()
        {
            GetSavData(out _, out _);
            return new DateTime((long)m_headerCache.SavTicks, DateTimeKind.Utc);
        }

        public unsafe void GetSavData(out byte[] savData, out byte[] screenShotData)
        {
            if (!m_cacheOutdate)
            {
                savData = m_savDataCaches;
                screenShotData = m_screenShotCaches;
                return;
            }

            m_cacheOutdate = false;

            savData = null;
            screenShotData = null;

            if (!AxiIO.File.Exists(FilePath)) return;

            var raw = AxiIO.File.ReadAllBytes(FilePath);
            int headerSize = Marshal.SizeOf(typeof(Header));

            if (raw.Length < headerSize)
            {
                App.log.Warning("无效存档");
                return;
            }

            m_headerCache = new Header();
            fixed (Header* headPtr = &m_headerCache)
            {
                var headP = (byte*)headPtr;
                Marshal.Copy(raw, 0, (IntPtr)headP, sizeof(Header));
            }

            savData = new byte[m_headerCache.DataLength];
            Array.Copy(raw, headerSize, savData, 0, savData.Length);
            screenShotData = new byte[m_headerCache.ScreenShotLength];
            Array.Copy(raw, headerSize + savData.Length, screenShotData, 0, screenShotData.Length);

            m_savDataCaches = savData;
            m_screenShotCaches = screenShotData;

            return;
        }

        public unsafe void Save(uint sequence, byte[] savData, byte[] screenShotData)
        {
            if (IsBusy) return;

            var filePath = FilePath;

            var header = new Header
            {
                Sequence = sequence,
                RomID = RomID,
                SavTicks = (ulong)DateTime.UtcNow.Ticks,
                DataLength = (uint)savData.Length,
                ScreenShotLength = (uint)screenShotData.Length,
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

            AxiIO.File.WriteAllBytes(filePath, raw);
            Sequecen = sequence;

            m_headerCache = header;
            m_savDataCaches = savData;
            m_screenShotCaches = screenShotData;

            IsEmpty = false;

            OnSavSuccessed?.Invoke();
        }

        /// <summary>
        /// 尝试同步存档
        /// </summary>
        public void TrySync()
        {
            if (FSM.CurrentState is not IdleState && FSM.CurrentState is not SyncedState && FSM.CurrentState is not SyncFailedState) return;

            FSM.ChangeState<CheckingNetworkState>();
        }



        private void SetSavingFlag()
        {
            SyncingFilesUtility.Add(this);
        }

        private void ClearSavingFlag()
        {
            SyncingFilesUtility.Remove(this);
        }

        public override string ToString()
        {
            return $"{EmuPlatform}|{RomID}|{SlotIndex}";
        }

        public static class SyncingFilesUtility
        {
            static SyncingFileRecord m_syncFiles = new SyncingFileRecord();

            public static void ReadFromPlayprefs()
            {
                var jsonStr = AxiPlayerPrefs.GetString("SYNCING_SAVE");
                if (string.IsNullOrEmpty(jsonStr)) return;

                var temp = JsonUtility.FromJson<SyncingFilesList>(jsonStr);
                if (temp.List == null || temp.List.Count == 0) return;

                foreach (var file in temp.List)
                {
                    Add(file);
                }
            }

            public static void ContinueSync()
            {
                var allFiles = m_syncFiles.Values.SelectMany(v => v.Values).SelectMany(v => v);
                foreach (var file in allFiles)
                {
                    var savFile = App.SavMgr.GetSaveFile(file.romID, file.platform, file.slotIndex);
                    savFile.TrySync();
                }
            }

            public static void Add(SaveFile savFile)
            {
                SyncingFiles file = new SyncingFiles { romID = savFile.RomID, platform = savFile.EmuPlatform, slotIndex = savFile.SlotIndex };
                Add(file);
            }

            private static void Add(SyncingFiles file)
            {
                if (!m_syncFiles.TryGetValue(file.platform, out var mapper))
                {
                    mapper = new Dictionary<int, HashSet<SyncingFiles>>();
                    m_syncFiles.Add(file.platform, mapper);
                }

                if (!mapper.TryGetValue(file.romID, out var syncingTables))
                {
                    syncingTables = new HashSet<SyncingFiles>();
                    mapper[file.romID] = syncingTables;
                }

                if (syncingTables.Add(file))
                {
                    WritePlayerprefs();
                }
            }

            public static void Remove(SaveFile savFile)
            {
                SyncingFiles file = new SyncingFiles { romID = savFile.RomID, platform = savFile.EmuPlatform, slotIndex = savFile.SlotIndex };
                if (!m_syncFiles.TryGetValue(file.platform, out var mapper))
                {
                    return;
                }

                if (!mapper.TryGetValue(savFile.RomID, out var syncingTables))
                {
                    return;
                }

                if (syncingTables.Remove(file))
                {
                    WritePlayerprefs();
                }
            }

            private static void WritePlayerprefs()
            {
                var allFiles = m_syncFiles.Values.SelectMany(v => v.Values).SelectMany(v => v);
                var temp = new SyncingFilesList { List = new List<SyncingFiles>(allFiles) };

                var jsonStr = JsonUtility.ToJson(temp);
                AxiPlayerPrefs.SetString("SYNCING_SAVE", jsonStr);
            }
        }

        [Serializable]
        class SyncingFilesList
        {
            [SerializeField]
            public List<SyncingFiles> List;
        }

        [Serializable]
        public struct SyncingFiles
        {
            public int romID;
            public RomPlatformType platform;
            public int slotIndex;

            public override int GetHashCode()
            {
                return HashCode.Combine(romID, platform, slotIndex);
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetHashCode() != GetHashCode()) return false;

                return true;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 24)]
        struct Header
        {
            [FieldOffset(0)]
            public uint Sequence;
            [FieldOffset(4)]
            public int RomID;
            [FieldOffset(8)]
            public ulong SavTicks;
            [FieldOffset(16)]
            public uint DataLength;
            [FieldOffset(20)]
            public uint ScreenShotLength;
        }
    }
}