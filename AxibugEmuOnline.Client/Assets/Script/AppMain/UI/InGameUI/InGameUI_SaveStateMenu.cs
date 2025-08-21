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

            public override string Name => SavFile.AutoSave ? "自动存档" : $"存档{SavFile.SlotIndex}";

            SaveMenuItem saveMENU;
            LoadMenuItem loadMENU;
            RetryMenuItem retryMENU;
            UseLocalSaveMenuItem useLocalMENU;
            UseRemoteSaveMenuItem useRemoteMENU;

            public SaveSlotMenu(InGameUI inGameui, SaveFile savFile)
            {
                SavFile = savFile;

                saveMENU = new SaveMenuItem(inGameui, savFile);
                loadMENU = new LoadMenuItem(inGameui, savFile);
                retryMENU = new RetryMenuItem(inGameui, savFile);
                useLocalMENU = new UseLocalSaveMenuItem(inGameui, savFile);
                useRemoteMENU = new UseRemoteSaveMenuItem(inGameui, savFile);
            }

            public override void OnShow(OptionUI_MenuItem ui)
            {
                base.OnShow(ui);

                SavFile.TrySync();
            }

            protected override List<InternalOptionMenu> GetOptionMenus()
            {
                var menus = new List<InternalOptionMenu>();

                if (SavFile.GetCurrentState() is SaveFile.ConflictState)
                {
                    menus.Add(useRemoteMENU);
                    menus.Add(useLocalMENU);
                }
                else
                {
                    if (SavFile.GetCurrentState() is SaveFile.SyncFailedState) menus.Add(retryMENU);
                    if (!SavFile.AutoSave) menus.Add(saveMENU);
                    if (!SavFile.IsEmpty) menus.Add(loadMENU);
                }

                return menus;
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
                    if (m_savFile.IsBusy)
                    {
                        OverlayManager.PopTip("存档正在同步中");
                        cancelHide = true;
                        return;
                    }

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

            public class RetryMenuItem : ExecuteMenu
            {
                SaveFile m_savFile;
                InGameUI m_ingameUI;
                public override string Name => "重试";
                public override bool Visible => m_savFile.GetCurrentState() is SaveFile.SyncFailedState;

                public RetryMenuItem(InGameUI inGameui, SaveFile savFile)
                {
                    m_ingameUI = inGameui;
                    m_savFile = savFile;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    cancelHide = true;
                    m_savFile.TrySync();
                }
            }

            public class UseRemoteSaveMenuItem : ExecuteMenu
            {
                SaveFile m_savFile;
                InGameUI m_ingameUI;
                public override string Name => "使用云端存档";

                public UseRemoteSaveMenuItem(InGameUI inGameui, SaveFile savFile)
                {
                    m_ingameUI = inGameui;
                    m_savFile = savFile;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    if (m_savFile.GetCurrentState() is not SaveFile.ConflictState) return;
                    cancelHide = true;
                    m_savFile.FSM.ChangeState<SaveFile.DownloadingState>();
                }
            }

            public class UseLocalSaveMenuItem : ExecuteMenu
            {
                SaveFile m_savFile;
                InGameUI m_ingameUI;
                public override string Name => "使用本地存档";

                public UseLocalSaveMenuItem(InGameUI inGameui, SaveFile savFile)
                {
                    m_ingameUI = inGameui;
                    m_savFile = savFile;
                }

                public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
                {
                    if (m_savFile.GetCurrentState() is not SaveFile.ConflictState) return;
                    cancelHide = true;
                    m_savFile.FSM.ChangeState<SaveFile.UploadingState>();
                }
            }
        }
    }
}