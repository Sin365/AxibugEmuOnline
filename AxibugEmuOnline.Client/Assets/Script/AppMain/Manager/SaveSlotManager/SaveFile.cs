using AxibugProtobuf;
using System;

namespace AxibugEmuOnline.Client
{
    /// <summary> 存档文件类 </summary>
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

        public SaveFile(int romID, RomPlatformType platform, int slotIndex)
        {
            RomID = romID;
            EmuPlatform = platform;
            SlotIndex = slotIndex;
        }

        internal void Save(byte[] savData, byte[] screenShotData)
        {
            throw new NotImplementedException();
        }
    }
}