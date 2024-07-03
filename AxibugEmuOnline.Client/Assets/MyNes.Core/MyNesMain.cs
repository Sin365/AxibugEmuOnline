using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MyNes.Core;

public class MyNesMain
{
	public static EmuSettings EmuSettings { get; private set; }

	public static RendererSettings RendererSettings { get; private set; }

	public static string AppPath { get; private set; }

	public static string WorkingFolder { get; private set; }

	internal static List<Board> Boards { get; private set; }

	public static List<IVideoProvider> VideoProviders { get; private set; }

	public static List<IAudioProvider> AudioProviders { get; private set; }

	public static IVideoProvider VideoProvider { get; private set; }

	public static IAudioProvider AudioProvider { get; private set; }

	public static WaveRecorder WaveRecorder { get; private set; }

	public static void Initialize(bool setupRenderers)
	{
		Tracer.WriteLine("Initializing My Nes Core ....");
		AppPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
		if (AppPath == "")
		{
			AppPath = Path.GetFullPath(".\\");
		}
		WorkingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MyNes");
		Directory.CreateDirectory(WorkingFolder);
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
		VideoProviders = new List<IVideoProvider>();
		AudioProviders = new List<IAudioProvider>();
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "*", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			try
			{
				if (!(Path.GetExtension(text).ToLower() == ".exe") && !(Path.GetExtension(text).ToLower() == ".dll"))
				{
					continue;
				}
				Tracer.WriteLine("Reading assembly: " + text);
				Assembly assembly = Assembly.LoadFile(text);
				if (!(assembly != null))
				{
					continue;
				}
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsSubclassOf(typeof(Board)) && !type.IsAbstract)
					{
						Board board = Activator.CreateInstance(type) as Board;
						Boards.Add(board);
						Tracer.WriteLine("Board added: " + board.Name + " [ Mapper " + board.MapperNumber + "]");
					}
					else if (type.GetInterface("MyNes.Core.IVideoProvider") != null)
					{
						IVideoProvider videoProvider = Activator.CreateInstance(type) as IVideoProvider;
						VideoProviders.Add(videoProvider);
						Tracer.WriteLine("Video provider added: " + videoProvider.Name + " [" + videoProvider.ID + "]");
					}
					else if (type.GetInterface("MyNes.Core.IAudioProvider") != null)
					{
						IAudioProvider audioProvider = Activator.CreateInstance(type) as IAudioProvider;
						AudioProviders.Add(audioProvider);
						Tracer.WriteLine("Audio provider added: " + audioProvider.Name + " [" + audioProvider.ID + "]");
					}
				}
			}
			catch (Exception ex)
			{
				Tracer.WriteLine("ERROR: " + ex.ToString());
			}
		}
		Tracer.WriteInformation("Done.");
		Tracer.WriteInformation("Total of " + Boards.Count + " board found.");
		Tracer.WriteInformation("Total of " + VideoProviders.Count + " video provider found.");
		Tracer.WriteInformation("Total of " + AudioProviders.Count + " audio provider found.");
		if (setupRenderers)
		{
			SetVideoProvider();
			SetAudioProvider();
			SetRenderingMethods();
		}
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

	public static IVideoProvider GetVideoProvider(string id)
	{
		foreach (IVideoProvider videoProvider in VideoProviders)
		{
			if (videoProvider.ID == id)
			{
				return videoProvider;
			}
		}
		return null;
	}

	public static IAudioProvider GetAudioProvider(string id)
	{
		foreach (IAudioProvider audioProvider in AudioProviders)
		{
			if (audioProvider.ID == id)
			{
				return audioProvider;
			}
		}
		return null;
	}

	public static void SetVideoProvider()
	{
		Tracer.WriteLine("Looking for the video provider that set in the settings...");
		VideoProvider = GetVideoProvider(RendererSettings.Video_ProviderID);
		if (VideoProvider == null)
		{
			Tracer.WriteError("ERROR: cannot find the video provider that set in the settings");
			Tracer.WriteWarning("Deciding video provider");
			if (VideoProviders.Count > 0)
			{
				RendererSettings.Video_ProviderID = VideoProviders[0].ID;
				VideoProvider = VideoProviders[0];
				if (VideoProvider != null)
				{
					Tracer.WriteInformation("Video provider set to " + VideoProvider.Name + " [" + VideoProvider.ID + "]");
					VideoProvider.Initialize();
				}
				else
				{
					Tracer.WriteError("ERROR: cannot set video provider.");
				}
			}
			else
			{
				Tracer.WriteError("ERROR: cannot set video provider, no video providers located.");
			}
		}
		else
		{
			Tracer.WriteInformation("Video provider set to " + VideoProvider.Name + " [" + VideoProvider.ID + "]");
			VideoProvider.Initialize();
		}
	}

	public static void SetAudioProvider()
	{
		Tracer.WriteLine("Looking for the audio provider that set in the settings...");
		AudioProvider = GetAudioProvider(RendererSettings.Audio_ProviderID);
		if (AudioProvider == null)
		{
			Tracer.WriteError("ERROR: cannot find the audio provider that set in the settings");
			Tracer.WriteWarning("Deciding audio provider");
			if (AudioProviders.Count > 0)
			{
				RendererSettings.Audio_ProviderID = AudioProviders[0].ID;
				AudioProvider = AudioProviders[0];
				if (AudioProvider != null)
				{
					Tracer.WriteInformation("Audio provider set to " + AudioProvider.Name + " [" + AudioProvider.ID + "]");
					AudioProvider.Initialize();
				}
				else
				{
					Tracer.WriteError("ERROR: cannot set audio provider.");
				}
			}
			else
			{
				Tracer.WriteError("ERROR: cannot set audio provider, no audio providers located.");
			}
		}
		else
		{
			Tracer.WriteInformation("Audio provider set to " + AudioProvider.Name + " [" + AudioProvider.ID + "]");
			AudioProvider.Initialize();
		}
	}

	public static void SetRenderingMethods()
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
