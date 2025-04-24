
using Essgee;
using Essgee.Metadata;
using System;

public static class EmuStandInfo
{
    //À´×ÔmetaData
    //public static string datDirectoryPath;
    //public static string metadataDatabaseFilePath;


    public static string jsonConfigFileName;//= "Config.json";
    public static string saveDataDirectoryName;//= "Saves";
    public static string screenshotDirectoryName;//= "Screenshots";
    public static string saveStateDirectoryName;//= "Savestates";
    public static string extraDataDirectoryName;//= "Extras";
    public static string ProductName;//= "";

    public static string programDataDirectory;// = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ProductName);
    public static string programConfigPath;// = Path.Combine(programDataDirectory, jsonConfigFileName);

    public static Configuration Configuration { get; set; }

    public static string ShaderPath;//= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Shaders");
    public static string SaveDataPath;//= Path.Combine(programDataDirectory, saveDataDirectoryName);
    public static string ScreenshotPath;//= Path.Combine(programDataDirectory, screenshotDirectoryName);
    public static string SaveStatePath;//= Path.Combine(programDataDirectory, saveStateDirectoryName);
    public static string ExtraDataPath;//= Path.Combine(programDataDirectory, extraDataDirectoryName);

    static Random mRandom;
    public static Random Random
    {
        get
        {
            if (mRandom == null)
            {
                mRandom = new Random();
            }
            return mRandom;
        }
    }

    public static string ProductVersion { get; set; }
}