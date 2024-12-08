using UnityEngine;

public static class CorePath
{
#if UNITY_EDITOR
	public static string DataPath => Application.persistentDataPath;
#elif UNITY_PSP2
	public static string DataPath => Application.dataPath;
#else
	public static string DataPath => Application.persistentDataPath;
#endif
}
