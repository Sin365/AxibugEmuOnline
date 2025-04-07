#if UNITY_SWITCH
using nn.account;
#endif
public class AxiNSMount
{
    static bool bInMount = false;
    internal static string m_SaveMountName;
    static bool bInMountForDebug = false;
    internal static string m_SaveMountForDebugName;

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

    public bool MountSDForDebug(string mountName = "sd")
    {
#if !UNITY_SWITCH
        return false;
#else
        if (bInMountForDebug)
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
        m_SaveMountForDebugName = mountName;
        bInMountForDebug = true;
        return true;
#endif
    }

    public bool MountSD(string mountName = "sd")
    {
#if !UNITY_SWITCH
        return false;
#else
        if (bInMountForDebug)
            return true;
        nn.Result result;
        result = nn.fs.SdCard.Mount(mountName);
        //result.abortUnlessSuccess();
        if (!result.IsSuccess())
        {
            UnityEngine.Debug.LogError($"nn_fs_MountSdCard->挂载{mountName}:/ 失败: " + result.ToString());
            return false;
        }
        UnityEngine.Debug.Log($"nn_fs_MountSdCard->挂载{mountName}:/ 成功 ");
        m_SaveMountForDebugName = mountName;
        bInMountForDebug = true;
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

    public void UnmountSaveForDebug()
    {
#if UNITY_SWITCH
        if (!bInMountForDebug)
        {
            UnityEngine.Debug.LogError($"{m_SaveMountForDebugName}:/ 没有被挂载，无需卸载");
            return;
        }
        nn.fs.FileSystem.Unmount(m_SaveMountForDebugName);
        UnityEngine.Debug.LogError($"UnmountSaveForDebufa->已卸载{m_SaveMountForDebugName}:/ ");
        bInMountForDebug = false;
#endif
    }
}