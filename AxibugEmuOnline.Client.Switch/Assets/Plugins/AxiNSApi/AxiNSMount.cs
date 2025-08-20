#if UNITY_SWITCH
using nn.account;
#endif
public class AxiNSMount
{
    static bool bInMount = false;
    internal static string m_SaveMountName;
    static bool bInSdCardMount = false;
    internal static string m_SdCardMountName;
	static bool bInSdCardDebugMount = false;
	internal static string m_SdCardDebugMountName;

	public bool SaveIsMount => bInMount;
    public string SaveMountName
    {
        get
        {
            if (!bInMount)
                return string.Empty;
            else
                return m_SaveMountName;
        }
    }

#if UNITY_SWITCH
	public bool MountSave(Uid userId, string mountName = "save")
	{
		if (bInMount)
			return true;

		if (!nn.fs.SaveData.IsExisting(userId))
		{
			UnityEngine.Debug.LogError($"{userId.ToString()}存档不存在！");
			return false;
		}
		UnityEngine.Debug.Log($"{userId.ToString()}存档确保存在！");

		nn.Result result;
		result = nn.fs.SaveData.Mount(mountName, userId);
		//result.abortUnlessSuccess();

		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"MountSave->挂载{mountName}:/ 失败: " + result.ToString());
			return false;
		}
		UnityEngine.Debug.Log($"MountSave->挂载{mountName}:/ 成功 ");
		m_SaveMountName = mountName;
		bInMount = true;
		return true;
	}
#endif

	public bool MountSDForDebug(string mountName = "dbgsd")
    {
#if !UNITY_SWITCH
        return false;
#else
        if (bInSdCardDebugMount)
            return true;
        nn.Result result;
        result = nn.fs.SdCard.MountForDebug(mountName);
        //result.abortUnlessSuccess();
        if (!result.IsSuccess())
        {
            UnityEngine.Debug.LogError($"nn_fs_MountSdCardForDebug->挂载{mountName}:/ 失败: " + result.ToString());
            return false;
        }
        UnityEngine.Debug.Log($"nn_fs_MountSdCardForDebug->挂载{mountName}:/ 成功 ");
		m_SdCardDebugMountName = mountName;
		bInSdCardDebugMount = true;
        return true;
#endif
    }
    public bool MountSD(string mountName = "sd")
    {
#if !UNITY_SWITCH
        return false;
#else
        if (bInSdCardMount)
            return true;
        nn.Result result;
        result = AxiNSSDCard.Mount(mountName);
        if (!result.IsSuccess())
        {
            UnityEngine.Debug.LogError($"nn_fs_MountSdCard->挂载{mountName}:/ 失败: " + result.ToString());
            return false;
        }
        UnityEngine.Debug.Log($"nn_fs_MountSdCard->挂载{mountName}:/ 成功 ");
        m_SdCardMountName = mountName;
        bInSdCardMount = true;
        return true;
#endif
    }
    public void UnmountSave()
    {
#if UNITY_SWITCH
        if (!bInMount)
        {
            UnityEngine.Debug.LogError($"{m_SaveMountName}:/ 没有被挂载，无需卸载");
            return;
        }
        nn.fs.FileSystem.Unmount(m_SaveMountName);
        UnityEngine.Debug.LogError($"UnmountSaveForDebufa->已卸载{m_SaveMountName}:/ ");
        bInMount = false;
#endif
    }
    public void UnmountSDCardForDebug()
    {
#if UNITY_SWITCH
        if (!bInSdCardDebugMount)
        {
            UnityEngine.Debug.LogError($"{m_SdCardDebugMountName}:/ 没有被挂载，无需卸载");
            return;
        }
        nn.fs.FileSystem.Unmount(m_SdCardDebugMountName);
        UnityEngine.Debug.LogError($"UnmountSDCardForDebug->已卸载{m_SdCardDebugMountName}:/ ");
		bInSdCardDebugMount = false;
#endif
    }
	public void UnmountSDCard()
	{
#if UNITY_SWITCH
		if (!bInSdCardMount)
		{
			UnityEngine.Debug.LogError($"{m_SdCardMountName}:/ 没有被挂载，无需卸载");
			return;
		}
		nn.fs.FileSystem.Unmount(m_SdCardMountName);
		UnityEngine.Debug.LogError($"UnmountSDCard->已卸载{m_SdCardMountName}:/ ");
		bInSdCardMount = false;
#endif
	}
}