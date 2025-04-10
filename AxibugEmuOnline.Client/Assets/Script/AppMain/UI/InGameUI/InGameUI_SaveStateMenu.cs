using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                result.Add(new SaveSlotMenu(m_gameUI, savFile));
            }
            return result;
        }

        /// <summary> 存档侧边选项UI </summary>
        public class SaveSlotMenu : ExecuteMenu
        {
            public override Type MenuUITemplateType => typeof(OptionUI_SavSlotItem);
            public SaveFile SavFile { get; private set; }

            private InGameUI m_ingameUI;

            public override bool Visible => !SavFile.AutoSave;

            public SaveSlotMenu(InGameUI inGameui, SaveFile savFile)
            {
                SavFile = savFile;
                m_ingameUI = inGameui;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                cancelHide = true;
                var stateData = m_ingameUI.Core.GetStateBytes();
                var tex = m_ingameUI.Core.OutputPixel;
                var screenData = tex.ToJPG();

                SavFile.Save(SavFile.Sequecen, stateData, screenData);
            }

            public override string Name => SavFile.AutoSave ? "自动存档" : $"存档{SavFile.SlotIndex}";
        }
    }
}