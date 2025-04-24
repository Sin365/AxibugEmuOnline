#if UNITY_SWITCH
using nn.fs;
#endif

public class AxiNSIO
{
	string save_name => AxiNS.instance.mount.SaveMountName;
	public string save_path => $"{save_name}:/";
#if UNITY_SWITCH
	private FileHandle fileHandle = new nn.fs.FileHandle();
#endif
	/// <summary>
	/// 检查Path是否存在
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	public bool CheckPathExists(string filePath)
	{
#if !UNITY_SWITCH
        return false;
#else
		nn.fs.EntryType entryType = 0;
		nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
		//result.abortUnlessSuccess();
		//这个异常捕获。真的别扭
		return nn.fs.FileSystem.ResultPathAlreadyExists.Includes(result);
#endif
	}
	/// <summary>
	/// 检查Path是否不存在
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	public bool CheckPathNotFound(string filePath)
	{
#if !UNITY_SWITCH
        return false;
#else
		nn.fs.EntryType entryType = 0;
		nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
		//这个异常捕获。真的别扭
		return nn.fs.FileSystem.ResultPathNotFound.Includes(result);
#endif
	}
	/// <summary>
	/// 创建目录，目录存在也会返回true
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	public bool CreateDir(string filePath)
	{
#if !UNITY_SWITCH
        return false;
#else
		// 使用封装函数检查和创建父目录
		if (!EnsureParentDirectory(filePath, true))
		{
			UnityEngine.Debug.LogError($"无法确保父目录，文件写入取消: {filePath}");
			return false;
		}
		return true;
#endif
	}



	/// <summary>
	/// 保存并创建文件（如果目录不存在回先自动创建目录）
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="bw"></param>
	/// <returns></returns>
	public bool FileToSaveWithCreate(string filePath, System.IO.MemoryStream ms)
	{
		return FileToSaveWithCreate(filePath, ms.ToArray());
	}
	/// <summary>
	/// 保存并创建文件（如果目录不存在回先自动创建目录）
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="data"></param>
	/// <returns></returns>
	public AxiNSWait_FileToSaveByMSWithCreate FileToSaveWithCreateAsync(string filePath, System.IO.MemoryStream ms)
	{
		var wait = new AxiNSWait_FileToSaveByMSWithCreate(filePath, ms);
		AxiNS.instance.wait.AddWait(wait);
		return wait;
	}
	/// <summary>
	/// 保存并创建文件（如果目录不存在回先自动创建目录）
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="data"></param>
	/// <returns></returns>
	public bool FileToSaveWithCreate(string filePath, byte[] data)
	{
#if !UNITY_SWITCH
        return false;
#else
		if (!AxiNS.instance.mount.SaveIsMount)
		{
			UnityEngine.Debug.LogError($"Save 尚未挂载，无法存储 {filePath}");
			return false;
		}

		nn.Result result;
#if UNITY_SWITCH && !UNITY_EDITOR
        // 阻止用户在保存时，退出游戏
        // Switch 条例 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif
		// 使用封装函数检查和创建父目录
		if (!EnsureParentDirectory(filePath, true))
		{
			UnityEngine.Debug.LogError($"无法确保父目录，文件写入取消: {filePath}");
			return false;
		}

		//string directoryPath = System.IO.Path.GetDirectoryName(filePath.Replace(save_path, ""));
		//string fullDirectoryPath = $"{save_path}{directoryPath}";
		//UnityEngine.Debug.Log($"检查父目录: {fullDirectoryPath}");

		//nn.fs.EntryType entryType = 0;
		//result = nn.fs.FileSystem.GetEntryType(ref entryType, fullDirectoryPath);
		//if (!result.IsSuccess() && nn.fs.FileSystem.ResultPathNotFound.Includes(result))
		//{
		//	UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 不存在，尝试创建 (判断依据 result=>{result.ToString()})");
		//	result = nn.fs.Directory.Create(fullDirectoryPath);
		//	if (!result.IsSuccess())
		//	{
		//		UnityEngine.Debug.LogError($"创建父目录失败: {result.GetErrorInfo()}");
		//		return false;
		//	}
		//	UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 创建成功");
		//}
		//else if (result.IsSuccess() && entryType != nn.fs.EntryType.Directory)
		//{
		//	UnityEngine.Debug.LogError($"路径 {fullDirectoryPath} 已存在，但不是目录");
		//	return false;
		//}
		//else if (!result.IsSuccess())
		//{
		//	UnityEngine.Debug.LogError($"检查父目录失败: {result.GetErrorInfo()}");
		//	return false;
		//}

		if (CheckPathNotFound(filePath))
		{
			UnityEngine.Debug.Log($"文件({filePath})不存在需要创建");
			result = nn.fs.File.Create(filePath, data.Length); //this makes a file the size of your save journal. You may want to make a file smaller than this.
															   //result.abortUnlessSuccess();
			if (!result.IsSuccess())
			{
				UnityEngine.Debug.LogError($"创建文件失败 {filePath} : " + result.GetErrorInfo());
				return false;
			}
		}
		else
			UnityEngine.Debug.Log($"文件({filePath})存在，不必创建");

		result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);
		//result.abortUnlessSuccess();
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"失败 File.Open(ref filehandle, {filePath}, OpenFileMode.Write): " + result.GetErrorInfo());
			return false;
		}
		UnityEngine.Debug.Log($"成功 File.Open(ref filehandle, {filePath}, OpenFileMode.Write)");

		//nn.fs.WriteOption.Flush 应该就是覆盖写入
		result = nn.fs.File.Write(fileHandle, 0, data, data.Length, nn.fs.WriteOption.Flush); // Writes and flushes the write at the same time
																							  //result.abortUnlessSuccess();
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError("写入文件失败: " + result.GetErrorInfo());
			return false;
		}
		UnityEngine.Debug.Log("写入文件成功: " + filePath);

		nn.fs.File.Close(fileHandle);

		//必须得提交，否则没有真实写入
		result = FileSystem.Commit(save_name);
		//result.abortUnlessSuccess();
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		UnityEngine.Debug.Log($"FileSystem.Commit({save_name}) 成功: ");


#if UNITY_SWITCH && !UNITY_EDITOR
        // 停止阻止用户退出游戏
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif

		return true;
#endif
	}
	/// <summary>
	/// 保存并创建文件（如果目录不存在回先自动创建目录）
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="data"></param>
	/// <returns></returns>
	public AxiNSWait_FileToSaveWithCreate FileToSaveWithCreateAsync(string filePath, byte[] data)
	{
		var wait = new AxiNSWait_FileToSaveWithCreate(filePath, data);
		AxiNS.instance.wait.AddWait(wait);
		return wait;
	}
	public byte[] LoadSwitchDataFile(string filename)
	{
		LoadSwitchDataFile(filename, out byte[] outputData);
		return outputData;
    }

    public bool LoadSwitchDataFile(string filename, ref System.IO.MemoryStream ms)
	{
		if (LoadSwitchDataFile(filename, out byte[] outputData))
		{
			using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(ms))
			{
				writer.Write(outputData);
			}
			return true;
		}
		return false;
	}
	public bool LoadSwitchDataFile(string filename, out byte[] outputData)
	{
#if !UNITY_SWITCH
        outputData = null;
        return false;
#else
		outputData = null;
		if (!AxiNS.instance.mount.SaveIsMount)
		{
			UnityEngine.Debug.LogError($"Save 尚未挂载，无法读取 {filename}");
			return false;
		}
		if (CheckPathNotFound(filename))
			return false;

		nn.Result result;
		result = nn.fs.File.Open(ref fileHandle, filename, nn.fs.OpenFileMode.Read);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Open 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;   // Could not open file. This can be used to detect if this is the first time a user has launched your game. 
							// (However, be sure you are not getting this error due to your file being locked by another process, etc.)
		}
		UnityEngine.Debug.Log($"nn.fs.File.Open 成功 {filename}");
		long iFileSize = 0;
		result = nn.fs.File.GetSize(ref iFileSize, fileHandle);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.GetSize 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		UnityEngine.Debug.Log($"nn.fs.File.GetSize 成功 {filename},size=>{iFileSize}");

		byte[] loadedData = new byte[iFileSize];
		result = nn.fs.File.Read(fileHandle, 0, loadedData, iFileSize);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Read 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		UnityEngine.Debug.Log($"nn.fs.File.Read 成功 {filename}");

		nn.fs.File.Close(fileHandle);

		//for (int i = 0; i < loadedData.Length; i++)
		//{
		//	UnityEngine.Debug.Log($"data[{i}]:{loadedData[i]}");
		//}

		outputData = loadedData;
		return true;
#endif
	}
	public AxiNSWait_LoadSwitchDataFile LoadSwitchDataFileAsync(string filename)
	{
		var wait = new AxiNSWait_LoadSwitchDataFile(filename);
		AxiNS.instance.wait.AddWait(wait);
		return wait;
	}
	public bool DeletePathFile(string filename)
	{
#if !UNITY_SWITCH
        return false;
#else


#if UNITY_SWITCH && !UNITY_EDITOR
        // This next line prevents the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

		if (CheckPathNotFound(filename))
			return false;
		nn.Result result;
		result = nn.fs.File.Delete(filename);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Delete 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		result = nn.fs.FileSystem.Commit(save_name);
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		return true;

#if UNITY_SWITCH && !UNITY_EDITOR
        // End preventing the user from quitting the game while saving.
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif

#endif
	}
	public AxiNSWait_DeletePathFile DeletePathFileAsync(string filename)
	{
		var wait = new AxiNSWait_DeletePathFile(filename);
		AxiNS.instance.wait.AddWait(wait);
		return wait;
	}
	public bool DeletePathDir(string filename)
	{
#if !UNITY_SWITCH
        return false;
#else

#if UNITY_SWITCH && !UNITY_EDITOR
        // This next line prevents the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

		if (CheckPathNotFound(filename))
			return false;
		nn.Result result;
		result = nn.fs.Directory.Delete(filename);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Delete 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		result = nn.fs.FileSystem.Commit(save_name);
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		return true;

#if UNITY_SWITCH && !UNITY_EDITOR
        // End preventing the user from quitting the game while saving.
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
#endif
	}
	public AxiNSWait_DeletePathDir DeletePathDirAsync(string filename)
	{
		var wait = new AxiNSWait_DeletePathDir(filename);
		AxiNS.instance.wait.AddWait(wait);
		return wait;
	}

	/// <summary>
	/// 递归删除目录
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public bool DeleteRecursivelyPathDir(string filename)
	{
#if !UNITY_SWITCH
        return false;
#else

#if UNITY_SWITCH && !UNITY_EDITOR
        // This next line prevents the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

		if (CheckPathNotFound(filename))
			return false;
		nn.Result result;
		result = nn.fs.Directory.DeleteRecursively(filename);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.DeleteRecursively 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		result = nn.fs.FileSystem.Commit(save_name);
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		return true;

#if UNITY_SWITCH && !UNITY_EDITOR
        // End preventing the user from quitting the game while saving.
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
#endif
	}

	/// <summary>
	/// 递归删除情况
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public bool CleanRecursivelyPathDir(string filename)
	{
#if !UNITY_SWITCH
        return false;
#else

#if UNITY_SWITCH && !UNITY_EDITOR
        // This next line prevents the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

		if (CheckPathNotFound(filename))
			return false;
		nn.Result result;
		result = nn.fs.Directory.CleanRecursively(filename);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.DeleteRecursively 失败 {filename} : result=>{result.GetErrorInfo()}");
			return false;
		}
		result = nn.fs.FileSystem.Commit(save_name);
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		return true;

#if UNITY_SWITCH && !UNITY_EDITOR
        // End preventing the user from quitting the game while saving.
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
#endif
	}

	public bool RenameDir(string oldpath, string newpath)
	{
#if !UNITY_SWITCH
        return false;
#else

#if UNITY_SWITCH && !UNITY_EDITOR
        // This next line prevents the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

		if (CheckPathNotFound(oldpath))
			return false;

		nn.Result result;
		result = nn.fs.Directory.Rename(oldpath, newpath);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Rename 失败 {oldpath} to {newpath} : result=>{result.GetErrorInfo()}");
			return false;
		}
		result = nn.fs.FileSystem.Commit(save_name);
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + result.GetErrorInfo());
			return false;
		}
		return true;

#if UNITY_SWITCH && !UNITY_EDITOR
        // End preventing the user from quitting the game while saving.
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
#endif
	}
	bool EnsureParentDirectory(string filePath, bool bAutoCreateDir = true)
	{
#if !UNITY_SWITCH
        return false;
#else
		// 参数校验
		if (string.IsNullOrEmpty(filePath))
		{
			UnityEngine.Debug.LogError($"无效参数：filePath={filePath}");
			return false;
		}

		// 提取路径前缀（如 save:/、sd:/）
		int prefixEndIndex = filePath.IndexOf(":/");
		if (prefixEndIndex == -1)
		{
			UnityEngine.Debug.LogError($"文件路径 {filePath} 格式无效，未找到 ':/' 前缀");
			return false;
		}
		string pathPrefix = filePath.Substring(0, prefixEndIndex + 2); // 提取前缀，例如 "save:/"
		string relativePath = filePath.Substring(prefixEndIndex + 2); // 移除前缀，得到相对路径

		// 检查挂载状态
		if (!IsMountPointAccessible(pathPrefix))
		{
			UnityEngine.Debug.LogError($"挂载点 {pathPrefix} 未挂载，无法操作路径 {filePath}");
			return false;
		}

		// 提取父目录路径
		string directoryPath = System.IO.Path.GetDirectoryName(relativePath); // 获取父目录相对路径
		if (string.IsNullOrEmpty(directoryPath))
		{
			UnityEngine.Debug.Log($"文件路径 {filePath} 无需创建父目录（位于根目录）");
			return true; // 根目录无需创建
		}

		string fullDirectoryPath = $"{pathPrefix}{directoryPath}"; // 拼接完整父目录路径
		UnityEngine.Debug.Log($"检查父目录: {fullDirectoryPath}");

		// 检查路径是否存在及其类型
		nn.fs.EntryType entryType = 0;
		nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, fullDirectoryPath);
		if (!result.IsSuccess() && nn.fs.FileSystem.ResultPathNotFound.Includes(result))
		{
			if (bAutoCreateDir)
			{
				// 路径不存在，尝试创建
				UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 不存在，尝试创建 (判断依据 result=>{result.ToString()})");
				result = nn.fs.Directory.Create(fullDirectoryPath);
				if (!result.IsSuccess())
				{
					UnityEngine.Debug.LogError($"创建父目录失败: {result.GetErrorInfo()}");
					return false;
				}
				UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 创建成功");
				return true;
			}
			return false;
		}
		else if (result.IsSuccess() && entryType != nn.fs.EntryType.Directory)
		{
			// 路径存在，但不是目录
			UnityEngine.Debug.LogError($"路径 {fullDirectoryPath} 已存在，但不是目录");
			return false;
		}
		else if (!result.IsSuccess())
		{
			// 其他错误
			UnityEngine.Debug.LogError($"检查父目录失败: {result.GetErrorInfo()}");
			return false;
		}
		// 路径存在且是目录
		UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 已存在且有效");
		return true;

#endif
	}
	/// <summary>
	/// 检查指定挂载点是否可访问
	/// </summary>
	/// <param name="pathPrefix">路径前缀，例如 "save:/" 或 "sd:/"</param>
	/// <returns>挂载点是否可访问</returns>
	bool IsMountPointAccessible(string pathPrefix)
	{
#if !UNITY_SWITCH
        return false;
#else
		if (string.IsNullOrEmpty(pathPrefix))
		{
			UnityEngine.Debug.LogError($"无效挂载点: {pathPrefix}");
			return false;
		}

		// 根据前缀判断挂载点类型并检查挂载状态
		if (pathPrefix == $"{save_name}:/")
		{
			if (!AxiNS.instance.mount.SaveIsMount)
			{
				UnityEngine.Debug.LogError($"{save_name}:/ 未挂载");
				return false;
			}
			return true;
		}
		else if (pathPrefix == "sd:/")
		{
			long freeSpace = 0;
			// 检查 SD 卡挂载状态（示例，需根据实际实现调整）
			nn.Result result = nn.fs.FileSystem.GetFreeSpaceSize(ref freeSpace, "sd:/");
			if (!result.IsSuccess())
			{
				UnityEngine.Debug.LogError($"sd:/ 未挂载或无法访问: {result.GetErrorInfo()}");
				return false;
			}
			return true;
		}
		else
		{
			UnityEngine.Debug.LogWarning($"未知挂载点 {pathPrefix}，假定已挂载");
			return true; // 其他挂载点需根据实际需求实现
		}
#endif
	}
}
