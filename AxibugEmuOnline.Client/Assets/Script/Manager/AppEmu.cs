using MyNes.Core;
using Palmmedia.ReportGenerator.Core;
using SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        INes _nes;

        public static WINSettings Settings { get; private set; }

        private bool isMaxing;

        private bool isMoving;

        private bool pausedByMainWindow;

        private bool isMouseVisible;

        private int mouseHiderCounter;

        private const int mouseHiderReload = 1;

        private bool gameLoaded;

        private IContainer components;

        public AppEmu()
        {
        }

        public void Init()
        {
            _nes = new INes();
        }

        internal void LoadGame(string filePath)
        {
            bool success = false;
            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".nes":
                    NesEmu.LoadGame(filePath, out success);
                    break;
                case ".7z":
                case ".zip":
                case ".rar":
                case ".gzip":
                case ".tar":
                case ".bzip2":
                case ".xz":
                    {
                        string text = filePath;
                        string text2 = Path.GetTempPath() + "\\MYNES\\";
                        SevenZipExtractor sevenZipExtractor;
                        try
                        {
                            sevenZipExtractor = new SevenZipExtractor(text);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Resources.Message35 + ": \n" + ex.Message);
                            return;
                        }
                        if (sevenZipExtractor.ArchiveFileData.Count == 1)
                        {
                            if (sevenZipExtractor.ArchiveFileData[0].FileName.Substring(sevenZipExtractor.ArchiveFileData[0].FileName.Length - 4, 4).ToLower() == ".nes")
                            {
                                sevenZipExtractor.ExtractArchive(text2);
                                text = text2 + sevenZipExtractor.ArchiveFileData[0].FileName;
                            }
                        }
                        else
                        {
                            List<string> list = new List<string>();
                            foreach (ArchiveFileInfo archiveFileDatum in sevenZipExtractor.ArchiveFileData)
                            {
                                list.Add(archiveFileDatum.FileName);
                            }
                            FormFilesList formFilesList = new FormFilesList(list.ToArray());
                            if (formFilesList.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                            string[] fileNames = new string[1] { formFilesList.SelectedRom };
                            sevenZipExtractor.ExtractFiles(text2, fileNames);
                            text = text2 + formFilesList.SelectedRom;
                        }
                        NesEmu.LoadGame(text, out success);
                        break;
                    }
            }
            if (success)
            {
                if (Settings.Misc_RecentFiles == null)
                {
                    Settings.Misc_RecentFiles = new string[0];
                }
                List<string> list2 = new List<string>(Settings.Misc_RecentFiles);
                if (list2.Contains(filePath))
                {
                    list2.Remove(filePath);
                }
                list2.Insert(0, filePath);
                if (list2.Count > 19)
                {
                    list2.RemoveAt(list2.Count - 1);
                }
                Settings.Misc_RecentFiles = list2.ToArray();
                gameLoaded = true;
                if (Settings.Win_StartInFullscreen)
                {
                    if (base.WindowState != FormWindowState.Maximized)
                    {
                        MyNesMain.VideoProvider.ResizeBegin();
                        Thread.Sleep(100);
                        MyNesMain.RendererSettings.Vid_Fullscreen = true;
                        base.FormBorderStyle = FormBorderStyle.None;
                        menuStrip1.Visible = false;
                        base.WindowState = FormWindowState.Maximized;
                        MyNesMain.VideoProvider.ResizeEnd();
                    }
                }
                else if (MyNesMain.RendererSettings.Vid_AutoStretch)
                {
                    ApplyStretch(applyRegion: true);
                }
            }
            ApplyWindowTitle();
        }


        #region Setting

        internal void LoadSettings()
        {
            base.Location = new Point(Program.Settings.Win_Location_X, Program.Settings.Win_Location_Y);
            base.Size = new Size(Program.Settings.Win_Size_W, Program.Settings.Win_Size_H);
            for (int i = 0; i < Program.SupportedLanguages.Length / 3; i++)
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Text = Program.SupportedLanguages[i, 2];
                toolStripMenuItem.Checked = Program.SupportedLanguages[i, 0] == Program.Settings.InterfaceLanguage;
                interfaceLanguageToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
            }
        }
        #endregion
    }
}
