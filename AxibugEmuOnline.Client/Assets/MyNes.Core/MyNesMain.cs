using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MyNes.Core
{
    public class MyNesMain
    {
        public static EmuSettings EmuSettings { get; private set; }
        public static RendererSettings RendererSettings { get; private set; }
        public static IFileManager FileManager { get; private set; }
        public static string WorkingFolder { get; private set; }

        internal static List<Board> Boards { get; private set; }

        public static IVideoProvider VideoProvider { get; private set; }

        public static IAudioProvider AudioProvider { get; private set; }

        public static WaveRecorder WaveRecorder { get; private set; }

        public static void Initialize(IFileManager fileManager, IVideoProvider videoProvider, IAudioProvider audioProvider)
        {
            Tracer.WriteLine("Initializing My Nes Core ....");
            FileManager = fileManager;
            WorkingFolder = fileManager.GetWorkingFolderPath();
            Tracer.WriteLine("Loading emu settings ...");
            EmuSettings = new EmuSettings(Path.Combine(WorkingFolder, "emusettings.ini"));
            EmuSettings.LoadSettings();
            Tracer.WriteLine("Emu settings loaded successfully.");
            Tracer.WriteLine("Loading renderer settings ...");
            RendererSettings = new RendererSettings(Path.Combine(WorkingFolder, "renderersettings.ini"));
            RendererSettings.LoadSettings();
            Tracer.WriteLine("Renderer settings loaded successfully.");
            Tracer.WriteLine("Locating boards and providers ...");
            WaveRecorder = new WaveRecorder();
            Boards = new List<Board>();

            var allTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(ass => ass.GetTypes());

            VideoProvider = videoProvider;
            AudioProvider = audioProvider;
            foreach (var type in allTypes)
            {
                if (type.IsSubclassOf(typeof(Board)) && !type.IsAbstract)
                {
                    Board board = Activator.CreateInstance(type) as Board;
                    Boards.Add(board);
                    Tracer.WriteLine("Board added: " + board.Name + " [ Mapper " + board.MapperNumber + "]");
                }
            }

            Tracer.WriteInformation("Done.");
            Tracer.WriteInformation("Total of " + Boards.Count + " board found.");
            SetVideoProvider();
            SetAudioProvider();
            SetRenderingMethods();

            NesEmu.Initialize();
        }

        public static void Shutdown()
        {
            if (NesEmu.ON)
            {
                NesEmu.ShutDown();
            }
            if (VideoProvider != null)
            {
                VideoProvider.ShutDown();
            }
            if (AudioProvider != null)
            {
                AudioProvider.ShutDown();
            }
            Tracer.WriteLine("Saving settings ...");
            EmuSettings.SaveSettings();
            RendererSettings.SaveSettings();
            Tracer.WriteLine("Settings saved successfully.");
            Tracer.WriteLine("Exiting My Nes.");
        }

        internal static bool IsBoardExist(int mapper)
        {
            foreach (Board board in Boards)
            {
                if (board.MapperNumber == mapper)
                {
                    return true;
                }
            }
            return false;
        }

        internal static Board GetBoard(int mapper)
        {
            foreach (Board board in Boards)
            {
                if (board.MapperNumber == mapper)
                {
                    return board;
                }
            }
            return null;
        }

        public static void MakeWorkingFolder()
        {
            WorkingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MyNes");
            Directory.CreateDirectory(WorkingFolder);
        }

        public static BoardInfoObject[] GetBoardsList(bool includeUnsupportedMappers)
        {
            List<BoardInfoObject> list = new List<BoardInfoObject>();
            if (includeUnsupportedMappers)
            {
                for (int i = 0; i < 256; i++)
                {
                    bool flag = false;
                    foreach (Board board in Boards)
                    {
                        if (board.MapperNumber == i)
                        {
                            BoardInfoObject boardInfoObject = new BoardInfoObject();
                            boardInfoObject.Name = board.Name;
                            boardInfoObject.MapperNumber = board.MapperNumber;
                            boardInfoObject.IsSupported = true;
                            boardInfoObject.HasIssues = board.HasIssues;
                            boardInfoObject.Issues = board.Issues;
                            list.Add(boardInfoObject);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        BoardInfoObject boardInfoObject2 = new BoardInfoObject();
                        boardInfoObject2.Name = "N/A";
                        boardInfoObject2.MapperNumber = i;
                        boardInfoObject2.IsSupported = false;
                        boardInfoObject2.HasIssues = false;
                        boardInfoObject2.Issues = "";
                        list.Add(boardInfoObject2);
                    }
                }
            }
            else
            {
                foreach (Board board2 in Boards)
                {
                    if (board2.MapperNumber >= 0)
                    {
                        BoardInfoObject boardInfoObject3 = new BoardInfoObject();
                        boardInfoObject3.Name = board2.Name;
                        boardInfoObject3.MapperNumber = board2.MapperNumber;
                        boardInfoObject3.IsSupported = true;
                        boardInfoObject3.HasIssues = board2.HasIssues;
                        boardInfoObject3.Issues = board2.Issues;
                        list.Add(boardInfoObject3);
                    }
                }
            }
            return list.ToArray();
        }

        static void SetVideoProvider()
        {
            if (VideoProvider != null) VideoProvider.Initialize();
            else Tracer.WriteError("VideoProvider is null");
        }

        static void SetAudioProvider()
        {
            Tracer.WriteLine("Looking for the audio provider that set in the settings...");
            if (AudioProvider != null) AudioProvider.Initialize();
            else Tracer.WriteError("AudioProvider is null");
        }

        static void SetRenderingMethods()
        {
            if (VideoProvider != null && AudioProvider != null)
            {
                NesEmu.SetupRenderingMethods(VideoProvider.SubmitFrame, AudioProvider.SubmitSamples, AudioProvider.TogglePause, AudioProvider.GetIsPlaying);
            }
            else
            {
                Tracer.WriteError("ERROR: unable to setup rendering methods, one (or both) of the providers is not set (video and/or audio provider)");
            }
        }

        public static void RecordWave()
        {
            if (NesEmu.ON && NesEmu.SoundEnabled)
            {
                string text = Path.Combine(EmuSettings.WavesFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath) + ".wav");
                int num = 0;
                while (File.Exists(text))
                {
                    text = Path.Combine(EmuSettings.WavesFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath) + "_" + num + ".wav");
                    num++;
                }
                WaveRecorder.Record(text, 1, 16, RendererSettings.Audio_Frequency);
            }
            else if (VideoProvider != null)
            {
                VideoProvider.WriteErrorNotification("Cannot record sound when the emu is off/sound is not enabled.", instant: false);
            }
        }
    }
    public interface IFileManager
    {
        string GetWorkingFolderPath();
        public Stream OpenDatabaseFile();
        public Stream OpenPaletteFile();
        public Stream OpenRomFile(string path);
    }
}
