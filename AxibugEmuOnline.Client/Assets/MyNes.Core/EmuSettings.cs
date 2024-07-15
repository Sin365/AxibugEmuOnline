using System.IO;

namespace MyNes.Core
{
    public class EmuSettings : ISettings
    {
    	public string SnapsFolder = "Snaps";

    	public string WavesFolder = "SoundRecords";

    	public string SnapsFormat = ".png";

    	public bool SnapsReplace;

    	public int RegionSetting;

    	public string StateFolder = "States";

    	public string GameGenieFolder = "GMCodes";

    	public string SRAMFolder = "Srams";

    	public bool SaveSRAMAtEmuShutdown = true;

    	public EmuSettings(string path)
    		: base(path)
    	{
    	}

    	public override void LoadSettings()
    	{
    		base.LoadSettings();
    		if (MyNesMain.WorkingFolder == null)
    		{
    			MyNesMain.MakeWorkingFolder();
    		}
    		if (SnapsFolder == "Snaps")
    		{
    			SnapsFolder = Path.Combine(MyNesMain.WorkingFolder, "Snaps");
    		}
    		if (StateFolder == "States")
    		{
    			StateFolder = Path.Combine(MyNesMain.WorkingFolder, "States");
    		}
    		if (GameGenieFolder == "GMCodes")
    		{
    			GameGenieFolder = Path.Combine(MyNesMain.WorkingFolder, "GMCodes");
    		}
    		if (SRAMFolder == "Srams")
    		{
    			SRAMFolder = Path.Combine(MyNesMain.WorkingFolder, "Srams");
    		}
    		if (WavesFolder == "SoundRecords")
    		{
    			WavesFolder = Path.Combine(MyNesMain.WorkingFolder, "SoundRecords");
    		}
    		try
    		{
    			Directory.CreateDirectory(WavesFolder);
    		}
    		catch
    		{
    			Tracer.WriteError("Cannot create sound records folder !!");
    		}
    		try
    		{
    			Directory.CreateDirectory(SnapsFolder);
    		}
    		catch
    		{
    			Tracer.WriteError("Cannot create snaps folder !!");
    		}
    		try
    		{
    			Directory.CreateDirectory(StateFolder);
    		}
    		catch
    		{
    			Tracer.WriteError("Cannot create states folder !!");
    		}
    		try
    		{
    			Directory.CreateDirectory(SRAMFolder);
    		}
    		catch
    		{
    			Tracer.WriteError("Cannot create srams folder !!");
    		}
    		try
    		{
    			Directory.CreateDirectory(GameGenieFolder);
    		}
    		catch
    		{
    			Tracer.WriteError("Cannot create game genie codes folder !!");
    		}
    		StateHandler.StateFolder = StateFolder;
    	}
    }
}
