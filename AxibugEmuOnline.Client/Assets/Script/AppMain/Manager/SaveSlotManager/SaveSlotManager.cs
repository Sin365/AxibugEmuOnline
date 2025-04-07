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
        public SavCloudApi CloudApi { get; private set; } = new SavCloudApi();

        Dictionary<int, SaveFile[]> m_saveFileDict = new Dictionary<int, SaveFile[]>();

        public void Update()
        {
            foreach (var saveFiles in m_saveFileDict.Values)
            {
                foreach (var file in saveFiles)
                {
                    file.Update();
                }
            }
        }

        public List<SaveFile> GetSlotSaves(int romID, RomPlatformType platform)
        {
            List<SaveFile> result = new List<SaveFile>();
            for (int i = 0; i < MAX_SLOT_COUNT; i++)
            {
                result.Add(GetSaveFile(romID, platform, i));
            }

            return result;
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