using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SaveStateMenu : ExpandMenu
    {
        private InGameUI m_gameUI;

        public override string Name => "存档";

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
                result.Add(new SaveSlotMenu(m_gameUI, savFile));
            }
            return result;
        }

        /// <summary> 存档侧边选项UI </summary>
        public class SaveSlotMenu : ExpandMenu
        {
            public override Type MenuUITemplateType => typeof(OptionUI_SavSlotItem);
            public SaveFile SavFile { get; private set; }

            List<InternalOptionMenu> m_subOptions = new List<InternalOptionMenu>();
            public override string Name => SavFile.AutoSave ? "自动存档" : $"存档{SavFile.SlotIndex}";

            public SaveSlotMenu(InGameUI inGameui, SaveFile savFile)
            {
                SavFile = savFile;

                //非自动存档,增加保存选项
                if (!savFile.AutoSave) m_subOptions.Add(new SaveMenuItem(inGameui, savFile));
                //添加读取选项
                m_subOptions.Add(new LoadMenuItem(inGameui, savFile));
            }

            protected override List<InternalOptionMenu> GetOptionMenus()
            {
                return m_subOptions;
            }

            public class SaveMenuItem : ExecuteMenu
            {
                SaveFile m_savFile;
                InGameUI m_ingameUI;
                public override string Name => "保存";

                public SaveMenuItem(InGameUI inGameui, SaveFile savFile)
                {
                    m_ingameUI = inGameui;
                    m_savFile = savFile;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    var stateData = m_ingameUI.Core.GetStateBytes();
                    var tex = m_ingameUI.Core.OutputPixel;
                    var screenData = tex.ToJPG(m_ingameUI.Core.DrawCanvas.transform.localScale);

                    m_savFile.Save(m_savFile.Sequecen, stateData, screenData);
                }
            }

            public class LoadMenuItem : ExecuteMenu
            {
                SaveFile m_savFile;
                InGameUI m_ingameUI;
                public override string Name => "读取";

                public LoadMenuItem(InGameUI inGameui, SaveFile savFile)
                {
                    m_ingameUI = inGameui;
                    m_savFile = savFile;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    cancelHide = true;
                    m_savFile.GetSavData(out byte[] savData, out var _);
                    if (savData != null)
                    {
                        m_ingameUI.Core.LoadStateFromBytes(savData);
                    }

                    OverlayManager.HideSideBar();
                }
            }
        }
    }
}