using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SaveStateMenu : ExpandMenu
    {
        private InGameUI m_gameUI;

        public override string Name => "保存";

        public InGameUI_SaveStateMenu(InGameUI inGameUI)
        {
            m_gameUI = inGameUI;
        }

        protected override List<InternalOptionMenu> GetOptionMenus()
        {
            var saveFiles = App.SavMgr.GetSlotSaves(m_gameUI.RomFile.ID, m_gameUI.RomFile.Platform);
            List<InternalOptionMenu> result = new List<InternalOptionMenu>();
            foreach (var savFile in saveFiles)
            {
                if (savFile.AutoSave) continue;
                result.Add(new SaveSlotMenu(savFile));
            }
            return result;
        }

        /// <summary> 存档侧边选项UI </summary>
        public class SaveSlotMenu : ExecuteMenu
        {
            public override Type MenuUITemplateType => typeof(OptionUI_SavSlotItem);
            public SaveFile SavFile { get; private set; }

            public override bool Visible => !SavFile.AutoSave;

            public SaveSlotMenu(SaveFile savFile)
            {
                SavFile = savFile;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                cancelHide = true;//保存后不关闭
            }

            public override string Name => SavFile.AutoSave ? "自动存档" : $"存档{SavFile.SlotIndex}";
        }
    }
}