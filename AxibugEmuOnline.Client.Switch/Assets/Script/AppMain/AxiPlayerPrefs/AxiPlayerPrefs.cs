using AxibugEmuOnline.Client.ClientCore;

public static class AxiPlayerPrefs
{
    //#if UNITY_SWITCH && !UNITY_EDITOR
    //	public static string SaveDataRootDirPath = "save:/axibug";
    //#elif UNITY_PSP2 && !UNITY_EDITOR
    //	public static string SaveDataRootDirPath = "ux0:data/axibug";
    //#else
    //    public static string SaveDataRootDirPath = UnityEngine.Application.persistentDataPath;
    //#endif

    //使用统一的平台宏区分目录
    public static string SaveDataRootDirPath => App.PersistentDataRootPath();

    static IAxiPlayerPrefs m_axiPlayerPrefs;
    static IAxiPlayerPrefs axiPlayerPrefs
    {
        get
        {
            if (m_axiPlayerPrefs == null)
            {
#if UNITY_SWITCH || UNITY_PSP2
                m_axiPlayerPrefs = new AxiPlayerPrefsForFileSystem();
#else
                m_axiPlayerPrefs = new AxiPlayerPrefsForUnity();
#endif
            }
            return m_axiPlayerPrefs;
        }
    }

    public static float GetFloat(string key) { return axiPlayerPrefs.GetFloat(key); }
    public static void SetFloat(string key, float value) { axiPlayerPrefs.SetFloat(key, value); }
    public static float GetFloat(string key, float defaultValue) { return axiPlayerPrefs.GetFloat(key, defaultValue); }
    public static int GetInt(string key) { return axiPlayerPrefs.GetInt(key); }
    public static void SetInt(string key, int value) { axiPlayerPrefs.SetInt(key, value); }
    public static int GetInt(string key, int defaultValue) { return axiPlayerPrefs.GetInt(key, defaultValue); }
    public static string GetString(string key) { return axiPlayerPrefs.GetString(key); }
    public static void SetString(string key, string value) { axiPlayerPrefs.SetString(key, value); }
    public static string GetString(string key, string defaultValue) { return axiPlayerPrefs.GetString(key, defaultValue); }

    internal static void DeleteAll() { axiPlayerPrefs.DeleteAll(); }
}
