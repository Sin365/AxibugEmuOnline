using AxibugProtobuf;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 游戏存档管理器
    /// </summary>
    public class SaveSlotManager
    {
        const int MAX_SLOT_COUNT = 4;

        SavCloudApi m_cloudApi = new SavCloudApi();
        Dictionary<int, SaveFile[]> m_saveFileDict = new Dictionary<int, SaveFile[]>();

        public void Save(int romID, RomPlatformType platform, int slotIndex, byte[] savData, byte[] screenShotData)
        {
            var fileIns = GetSaveFile(romID, platform, slotIndex);
            fileIns.Save(savData, screenShotData);
        }

        SaveFile GetSaveFile(int romID, RomPlatformType platform, int slotIndex)
        {
            if (!m_saveFileDict.TryGetValue(romID, out SaveFile[] files))
            {
                if (files == null) files = new SaveFile[MAX_SLOT_COUNT];
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = new SaveFile(romID, platform, i);
                }
                m_saveFileDict[romID] = files;
            }

            return files[slotIndex];
        }
    }
}