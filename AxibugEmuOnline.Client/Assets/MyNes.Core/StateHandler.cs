using System;
using System.IO;
using System.Text;

namespace MyNes.Core
{
    public class StateHandler
    {
    	public static int Slot = 0;

    	internal static string StateFolder = "States";

    	private const byte state_version = 7;

    	private static bool IsSavingState = false;

    	private static bool IsLoadingState = false;

    	public static void SaveState(string fileName, bool saveImage)
    	{
    		if (!NesEmu.ON)
    		{
    			Tracer.WriteError("Can't save state, emu is off.");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error1, instant: false);
    			return;
    		}
    		if (!File.Exists(NesEmu.CurrentFilePath))
    		{
    			Tracer.WriteError("Can't save state, no rom file is loaded.");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error2, instant: false);
    			return;
    		}
    		if (IsLoadingState)
    		{
    			Tracer.WriteError("Can't save state while loading a state !");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error3, instant: false);
    			return;
    		}
    		if (IsSavingState)
    		{
    			Tracer.WriteError("Already saving state !!");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error4, instant: false);
    			return;
    		}
    		IsSavingState = true;
    		BinaryWriter bin = new BinaryWriter(new MemoryStream());
    		bin.Write(Encoding.ASCII.GetBytes("MNS"));
    		bin.Write((byte)7);
    		for (int i = 0; i < NesEmu.SHA1.Length; i += 2)
    		{
    			string value = NesEmu.SHA1.Substring(i, 2).ToUpper();
    			bin.Write(Convert.ToByte(value, 16));
    		}
    		NesEmu.WriteStateData(ref bin);
    		byte[] outData = new byte[0];
    		ZlipWrapper.CompressData(((MemoryStream)bin.BaseStream).GetBuffer(), out outData);
    		FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
    		fileStream.Write(outData, 0, outData.Length);
    		MyNesMain.VideoProvider.TakeSnapshotAs(fileName.Replace(".mns", ".jpg"), ".jpg");
    		bin.Flush();
    		bin.Close();
    		fileStream.Flush();
    		fileStream.Close();
    		IsSavingState = false;
    		Tracer.WriteInformation("State saved at slot " + Slot);
    		MyNesMain.VideoProvider.WriteInfoNotification(MNInterfaceLanguage.Message_Info1 + " " + Slot, instant: false);
    	}

    	public static void SaveState(int Slot)
    	{
    		if (StateFolder == "States")
    		{
    			StateFolder = Path.Combine(MyNesMain.WorkingFolder, "States");
    		}
    		Directory.CreateDirectory(StateFolder);
    		SaveState(Path.Combine(StateFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath)) + "_" + Slot + ".mns", saveImage: false);
    	}

    	public static void SaveState()
    	{
    		SaveState(Slot);
    	}

    	public static void LoadState()
    	{
    		LoadState(Slot);
    	}

    	public static void LoadState(string fileName)
    	{
    		if (!NesEmu.ON)
    		{
    			Tracer.WriteError("Can't load state, emu is off.");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error5, instant: false);
    			return;
    		}
    		if (!File.Exists(NesEmu.CurrentFilePath))
    		{
    			Tracer.WriteError("Can't load state, no rom file is loaded.");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error6, instant: false);
    			return;
    		}
    		if (IsSavingState)
    		{
    			Tracer.WriteError("Can't load state while saving a state !");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error7, instant: false);
    			return;
    		}
    		if (IsLoadingState)
    		{
    			Tracer.WriteError("Already loading state !!");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error8, instant: false);
    			return;
    		}
    		if (!File.Exists(fileName))
    		{
    			Tracer.WriteError("No state found in slot " + Slot);
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error9 + " " + Slot, instant: false);
    			return;
    		}
    		IsLoadingState = true;
    		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
    		byte[] array = new byte[fileStream.Length];
    		byte[] outData = new byte[0];
    		fileStream.Read(array, 0, array.Length);
    		fileStream.Close();
    		ZlipWrapper.DecompressData(array, out outData);
    		BinaryReader bin = new BinaryReader(new MemoryStream(outData));
    		byte[] array2 = new byte[3];
    		bin.Read(array2, 0, array2.Length);
    		if (Encoding.ASCII.GetString(array2) != "MNS")
    		{
    			Tracer.WriteError("Unable load state at slot " + Slot + "; Not My Nes State File !");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error10 + " " + Slot + "; " + MNInterfaceLanguage.Message_Error11, instant: false);
    			IsLoadingState = false;
    			return;
    		}
    		if (bin.ReadByte() != 7)
    		{
    			Tracer.WriteError("Unable load state at slot " + Slot + "; Not compatible state file version !");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error10 + " " + Slot + "; " + MNInterfaceLanguage.Message_Error12, instant: false);
    			IsLoadingState = false;
    			return;
    		}
    		string text = "";
    		for (int i = 0; i < NesEmu.SHA1.Length; i += 2)
    		{
    			text += bin.ReadByte().ToString("X2");
    		}
    		if (text.ToLower() != NesEmu.SHA1.ToLower())
    		{
    			Tracer.WriteError("Unable load state at slot " + Slot + "; This state file is not for this game; not same SHA1 !");
    			MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Message_Error10 + " " + Slot + "; " + MNInterfaceLanguage.Message_Error13, instant: false);
    			IsLoadingState = false;
    		}
    		else
    		{
    			NesEmu.ReadStateData(ref bin);
    			bin.Close();
    			IsLoadingState = false;
    			Tracer.WriteInformation("State loaded from slot " + Slot);
    			MyNesMain.VideoProvider.WriteInfoNotification(MNInterfaceLanguage.Message_Info2 + " " + Slot, instant: false);
    		}
    	}

    	public static void LoadState(int Slot)
    	{
    		if (StateFolder == "States")
    		{
    			StateFolder = Path.Combine(MyNesMain.WorkingFolder, "States");
    		}
    		Directory.CreateDirectory(StateFolder);
    		LoadState(Path.Combine(StateFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath)) + "_" + Slot + ".mns");
    	}

    	public static string GetStateFile(int slot)
    	{
    		if (File.Exists(NesEmu.CurrentFilePath))
    		{
    			return Path.Combine(StateFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath)) + "_" + slot + ".mns";
    		}
    		return "";
    	}

    	public static string GetStateImageFile(int slot)
    	{
    		if (File.Exists(NesEmu.CurrentFilePath))
    		{
    			return Path.Combine(StateFolder, Path.GetFileNameWithoutExtension(NesEmu.CurrentFilePath)) + "_" + slot + ".jpg";
    		}
    		return "";
    	}
    }
}
