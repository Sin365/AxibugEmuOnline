//#define UNITY_SWITCH

#if UNITY_SWITCH
using nn.fs;
using System.Text.RegularExpressions;
#endif

using System.Collections.Generic;
using System;
using System.Diagnostics;

public class AxiNSIO
{
    string save_name => AxiNS.instance.mount.SaveMountName;
    public string save_path => $"{save_name}:/";
    //#if UNITY_SWITCH
    //    private FileHandle fileHandle = new nn.fs.FileHandle();
    //#endif

    static object commitLock = new object();

    static bool bDirty = false;

    #region debug用
    static bool m_bDebugStepBreak = false;
    static E_AxiNS_dgbBk m_breakType = E_AxiNS_dgbBk.None;
    public enum E_AxiNS_dgbBk
    {
        None = 0,
        Dir,
        SaveFile,
        LoadFile
    }
    static int m_StepBreakIdx = 0;
    public static void SetDebugStep(E_AxiNS_dgbBk type, int step)
    {
        m_bDebugStepBreak = true;
        m_breakType = type;
        m_StepBreakIdx = Math.Max(0, step);
        UnityEngine.Debug.Log("[AxiNSIO]设置" + type.ToString() + "步进中断数" + step);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("UNITY_SWITCH")]
    public static void CheckCanStep(E_AxiNS_dgbBk type, int stepIdx,
        string path, string method = "")
    {
        string temp = $"调用 {method} do->{path}";

        if (!m_bDebugStepBreak || type != m_breakType)
        {
            UnityEngine.Debug.Log("[AxiNSIO]正常步进推进" + type.ToString() + "[" + stepIdx + "]" + ":" + temp + "");
            return;
        }

        if (stepIdx >= m_StepBreakIdx)
        {
            UnityEngine.Debug.Log("[AxiNSIO]步进中断" + type.ToString() + "[" + stepIdx + "]" + ":" + temp + "");
            throw new Exception("[AxiNSIO]步进中断" + type.ToString() + "[" + stepIdx + "]" + ":" + temp + "");
            return;
        }
        UnityEngine.Debug.Log("[AxiNSIO]允许步进进行" + type.ToString() + "[" + stepIdx + "]" + ":" + temp + "");
        return;
    }

    public static void ClearDbgStep()
    {
        m_bDebugStepBreak = false;
    }

    #endregion
    bool MustCommitSave()
    {
        SetCommitDirty();
        return CommitSave();
    }

    bool CommitSave()
    {
        lock (commitLock)
        {
            if (!bDirty)
                return false;
#if UNITY_SWITCH && !UNITY_EDITOR
            using (AxiNSIOKeepingDisposable.Acquire())
            {
                nn.Result ret = FileSystem.Commit(save_name);
                if (!ret.IsSuccess())
                {
                    UnityEngine.Debug.LogError($"FileSystem.Commit({save_name}) 失败: " + ret.GetErrorInfo());
                    return false;
                }
                bDirty = false;
                return true;
            }
#else
            return false;
#endif
        }

    }

    void SetCommitDirty()
    {
        lock (commitLock)
        {
            bDirty = true;
        }
    }

    public void ApplyAutoCommit()
    {
        CommitSave();
    }


    /// <summary>
    /// 检查File是否存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool CheckFilePathExists(string filePath)
    {
        string ametaPath = GetFileLengthMetaPath(filePath);
        return (DoCheckPathExists(ametaPath) && DoCheckPathExists(filePath));
    }

    /// <summary>
    /// 检查Dir是否存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool CheckDirPathExists(string dirPath)
    {
        return DoCheckPathExists(dirPath);
    }

    /// <summary>
    /// 检查Path是否存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool DoCheckPathExists(string filePath)
    {
#if !UNITY_SWITCH
        return false;
#else

        string mountRoot = null;
        int colonSlashIndex = filePath.IndexOf(":/");
        if (colonSlashIndex > 0)
        {
            mountRoot = filePath.Substring(0, colonSlashIndex + 1); // 例如 "save:"
        }
        // 检查挂载状态
        if (!IsMountPointAccessible(mountRoot + "/"))
        {
            UnityEngine.Debug.LogError($"挂载点 {mountRoot + "/"} 未挂载，无法操作路径 {filePath}");
            return false;
        }

        nn.fs.EntryType entryType = 0;
        nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
        //result.abortUnlessSuccess();
        //这个异常捕获。真的别扭

        //日，FileSystem.ResultPathAlreadyExists 貌似不太行
        //return nn.fs.FileSystem.ResultPathAlreadyExists.Includes(result);
        return !nn.fs.FileSystem.ResultPathNotFound.Includes(result);
#endif
    }
    /// <summary>
    /// 检查Path是否不存在
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool CheckPathNotFound(string filePath)
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
    /// <param name="dirpath"></param>
    /// <returns></returns>
    public bool CreateDir(string dirpath)
    {
        UnityEngine.Debug.Log($"CreateDir: {dirpath}");
        lock (commitLock)
        {

#if !UNITY_SWITCH
            return false;
#else
            // 使用封装函数检查和创建父目录
            if (!EnsureParentDirectory(dirpath, true))
            {
                UnityEngine.Debug.LogError($"无法确保父目录，文件写入取消: {dirpath}");
                return false;
            }
            return true;
#endif
        }
    }

    /// <summary>
    /// 保存并创建文件（如果目录不存在回先自动创建目录）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bw"></param>
    /// <returns></returns>
    public bool FileToSaveWithCreate(string filePath, System.IO.MemoryStream ms)
    {
        return FileToSaveWithCreate(filePath, ms.ToArray(), false);
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
    /// 获取amate文件路径，即描述文件大小的文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    string GetFileLengthMetaPath(string filePath)
    {
        string lengthFilePath = filePath + ".ameta";
        return lengthFilePath;
    }
    public bool GetFileLength(string filePath,out long length)
    {
        string ametaPath = GetFileLengthMetaPath(filePath);
        UnityEngine.Debug.Log($"准备进行amete文件读取:{ametaPath}");
        if (!DoLoadSwitchDataFile(ametaPath, null, out byte[] ametedata))
        {
            UnityEngine.Debug.Log($"amete文件读取失败:{ametaPath}");
            length = -1;
            return false;
        }
        int realLength = BitConverter.ToInt32(ametedata);
        UnityEngine.Debug.Log($"amete得到真实大小:{realLength}");
        length = realLength;
        return true;
    }

    /// <summary>
    /// 保存并创建文件（如果目录不存在回先自动创建目录）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    /// <param name="immediatelyCommit">是否立即Commit到物理存储</param>
    /// <returns></returns>
    public bool FileToSaveWithCreate(string filePath, byte[] data, bool immediatelyCommit)
    {
        CheckCanStep(E_AxiNS_dgbBk.SaveFile, -1000, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (DoFileToSaveWithCreate(filePath, data, immediatelyCommit))
        {
            CheckCanStep(E_AxiNS_dgbBk.SaveFile, -999, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            string fileLengthMetaPath = GetFileLengthMetaPath(filePath);
            CheckCanStep(E_AxiNS_dgbBk.SaveFile, -998, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            byte[] lengthData = BitConverter.GetBytes(data.Length);
            CheckCanStep(E_AxiNS_dgbBk.SaveFile, -997, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (DoFileToSaveWithCreate(fileLengthMetaPath, lengthData, immediatelyCommit))
            {
                CheckCanStep(E_AxiNS_dgbBk.SaveFile, -996, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                UnityEngine.Debug.Log($"写入AMate文件 {fileLengthMetaPath}：标记长度{data.Length}");
                return true;
            }
            else
            {
                CheckCanStep(E_AxiNS_dgbBk.SaveFile, -995, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                UnityEngine.Debug.Log($"写入AMate文件 失败 {fileLengthMetaPath}：标记长度{data.Length}");
                return false;
            }
        }
        return false;
    }

    private const int AlignmentSize = 4096; // 4K
    bool DoFileToSaveWithCreate(string filePath, byte[] data, bool immediatelyCommit)
    {
        // 计算需要对齐后的总长度
        long alignedLength = ((data.Length + AlignmentSize - 1) / AlignmentSize) * AlignmentSize;

        UnityEngine.Debug.Log($"FileToSaveWithCreate: {filePath}");
#if !UNITY_SWITCH
        return false;
#else
        lock (commitLock)
        {
            using (AxiNSIOKeepingDisposable.Acquire())
            {
                CheckCanStep(E_AxiNS_dgbBk.SaveFile, 1, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                if (!AxiNS.instance.mount.SaveIsMount)
                {
                    UnityEngine.Debug.LogError($"Save 尚未挂载，无法存储 {filePath}");
                    return false;
                }

                nn.Result result;

                //取出父级目录
                string dirpath = string.Empty;
                string mountRoot = null;
                int colonSlashIndex = filePath.IndexOf(":/");
                if (colonSlashIndex > 0)
                    mountRoot = filePath.Substring(0, colonSlashIndex + 1); // 例如 "save:"

                int lastSlashIndex = filePath.LastIndexOf('/');
                if (lastSlashIndex >= 0)
                {
                    string parent = filePath.Substring(0, lastSlashIndex);
                    if (mountRoot != null && !parent.Equals(mountRoot, StringComparison.OrdinalIgnoreCase))
                        dirpath = parent;
                }
                if (!string.IsNullOrWhiteSpace(dirpath))
                {
                    // 使用封装函数检查和创建父目录
                    if (!EnsureParentDirectory(dirpath, true))
                    {
                        UnityEngine.Debug.LogError($"无法确保父目录，文件写入取消: {filePath}");
                        return false;
                    }
                }

                CheckCanStep(E_AxiNS_dgbBk.SaveFile, 2, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                FileHandle fileHandle = new nn.fs.FileHandle();

                if (CheckPathNotFound(filePath))
                {
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 3, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    UnityEngine.Debug.Log($"文件({filePath})不存在需要创建");
                    result = nn.fs.File.Create(filePath, alignedLength); //this makes a file the size of your save journal. You may want to make a file smaller than this.
                                                                         //result.abortUnlessSuccess();
                    if (!result.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"创建文件失败 {filePath} : " + result.GetErrorInfo());
                        return false;
                    }

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 4, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

                    //读取文件Handle
                    result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);

                    if (!result.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"文件OpenHandle失败 {filePath} : " + result.GetErrorInfo());
                        return false;
                    }

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 5, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
                else
                {
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 6, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    //读取文件Handle
                    result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);
                    if (!result.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"文件OpenHandle失败 {filePath} : " + result.GetErrorInfo());
                        return false;
                    }
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 7, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

                    long currsize = 0;
                    result = File.GetSize(ref currsize, fileHandle);
                    if (!result.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"文件File.GetSize失败 {filePath} : " + result.GetErrorInfo());
                        return false;
                    }
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 8, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    if (currsize == alignedLength)
                    {
                        UnityEngine.Debug.Log($"文件({filePath})存在,长度一致，不用重新创建");
                        CheckCanStep(E_AxiNS_dgbBk.SaveFile, 9, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    }
                    else
                    {
                        CheckCanStep(E_AxiNS_dgbBk.SaveFile, 10, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        UnityEngine.Debug.Log($"文件({filePath})存在,长度不一致，先删除再重建");
                        nn.fs.File.Close(fileHandle);
                        //删除
                        result = File.Delete(filePath);
                        if (!result.IsSuccess())
                        {
                            UnityEngine.Debug.LogError($"删除旧文件失败 {filePath} : " + result.GetErrorInfo());
                            return false;
                        }
                        CheckCanStep(E_AxiNS_dgbBk.SaveFile, 11, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        //重新创建
                        result = nn.fs.File.Create(filePath, alignedLength);
                        if (!result.IsSuccess())
                        {
                            UnityEngine.Debug.LogError($"创建文件失败 {filePath} : " + result.GetErrorInfo());
                            return false;
                        }
                        CheckCanStep(E_AxiNS_dgbBk.SaveFile, 12, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        //重新读取文件Handle
                        result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);
                        if (!result.IsSuccess())
                        {
                            UnityEngine.Debug.LogError($"文件OpenHandle失败 {filePath} : " + result.GetErrorInfo());
                            return false;
                        }
                        CheckCanStep(E_AxiNS_dgbBk.SaveFile, 13, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    }
                }

                //  //OpenFileMode.AllowAppend 好像不可用
                //  //result = File.Open(ref fileHandle, filePath, OpenFileMode.AllowAppend);
                //  result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);

                //result.abortUnlessSuccess();
                //if (!result.IsSuccess())
                //{
                //    UnityEngine.Debug.LogError($"失败 File.Open(ref filehandle, {filePath}, OpenFileMode.Write): " + result.GetErrorInfo());
                //    return false;
                //}
                UnityEngine.Debug.Log($"成功 File.Open(ref filehandle, {filePath}, OpenFileMode.Write)");

                CheckCanStep(E_AxiNS_dgbBk.SaveFile, 14, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

                /* 不padding
                //nn.fs.WriteOption.Flush 应该就是覆盖写入
                result = nn.fs.File.Write(fileHandle, 0, data, data.Length, nn.fs.WriteOption.Flush); // Writes and flushes the write at the same time
                //result.abortUnlessSuccess();
                if (!result.IsSuccess())
                {
                    UnityEngine.Debug.LogError($"写入失败 {filePath} : {result.GetErrorInfo()}");
                    nn.fs.File.Close(fileHandle);
                    return false;
                }
                nn.fs.File.Close(fileHandle);
                UnityEngine.Debug.Log($"写入成功: {filePath} (原始大小: {alignedLength} 字节)");
                */

                #region padding 2次写入（一次真实数据，一次padding）
                CheckCanStep(E_AxiNS_dgbBk.SaveFile, 15, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                result = nn.fs.File.Write(fileHandle, 0, data, data.Length, nn.fs.WriteOption.Flush); // Writes and flushes the write at the same time
                if (!result.IsSuccess())
                {
                    UnityEngine.Debug.LogError($"写入真实数据失败 {filePath} : {result.GetErrorInfo()}");
                    nn.fs.File.Close(fileHandle);
                    return false;
                }
                UnityEngine.Debug.Log($"写入真实数据成功: {filePath} (原始大小: {data.Length} 字节)");
                // 计算需要补齐的字节数
                long padding = alignedLength - data.Length;
                if (padding > 0)
                {
                    byte[] zeroBytes = new byte[padding];
                    UnityEngine.Debug.Log($"需要padding，长度{padding}");
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 16, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    result = nn.fs.File.Write(fileHandle, data.Length, zeroBytes, zeroBytes.Length, nn.fs.WriteOption.Flush); // Writes and flushes the write at the same time
                    if (!result.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"写入padding失败 长度{padding} {filePath} : {result.GetErrorInfo()}");
                        nn.fs.File.Close(fileHandle);
                        return false;
                    }
                    UnityEngine.Debug.Log($"写入padding成功 长度{padding}: {filePath} (总大小: {alignedLength} 字节)");
                }
                else
                {
                    UnityEngine.Debug.Log($"不需要padding");
                }
                nn.fs.File.Close(fileHandle);
                #endregion

                #region 4K 对齐分块写入 
                /*
                // ====================== 4K 对齐分块写入 ======================
                const long Alignment = 4096;
                const long MaxChunkSize = 2 * 1024 * 1024;  // 2MB

                long offset = 0;

                nn.Result resultLoop;
                bool bHadWrite = false;
                while (offset < data.Length)
                {
                    long remaining = data.Length - offset;
                    long chunkSize = System.Math.Min(MaxChunkSize, remaining);

                    //// 最后一块需要 padding 到 4K
                    //long writeSize = (remaining <= MaxChunkSize)
                    //    ? ((chunkSize + 4095) / Alignment) * Alignment
                    //    : chunkSize;

                    // 最后一块需要 padding 到 4K
                    long writeSize = (remaining <= MaxChunkSize)
                        ? ((chunkSize + 4095) / Alignment) * Alignment
                        : chunkSize;

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 15, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    byte[] writeBuffer = new byte[writeSize];

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 16, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    // 拷贝真实数据
                    int copyLen = (int)System.Math.Min(chunkSize, writeBuffer.Length);
                    Buffer.BlockCopy(data, (int)offset, writeBuffer, 0, copyLen);
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 17, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

                    // 显式清零 padding
                    Array.Clear(writeBuffer, (int)chunkSize, (int)(writeSize - chunkSize));

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 18, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    // 执行写入
                    resultLoop = nn.fs.File.Write(fileHandle, offset, writeBuffer, writeSize, nn.fs.WriteOption.Flush);

                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 19, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    if (!resultLoop.IsSuccess())
                    {
                        UnityEngine.Debug.LogError($"写入失败 {filePath} offset={offset}: {resultLoop.GetErrorInfo()}");
                        nn.fs.File.Close(fileHandle);
                        return false;
                    }

                    offset += chunkSize;   // 按原始数据长度前进
                    bHadWrite = true;
                }

                nn.fs.File.Close(fileHandle);
                UnityEngine.Debug.Log($"写入成功: {filePath} (原始大小: {data.Length} 字节)");
                //====================== End ：4K 对齐分块写入 ======================

                if (!bHadWrite)
                    UnityEngine.Debug.Log($"是空文件");//不过也允许，允许创建空文件
                */
                #endregion

                CheckCanStep(E_AxiNS_dgbBk.SaveFile, 50, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                UnityEngine.Debug.Log("写入文件成功: " + filePath);
                if (immediatelyCommit)
                {
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 51, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    //必须得提交，否则没有真实写入
                    return MustCommitSave();
                }
                else
                {
                    CheckCanStep(E_AxiNS_dgbBk.SaveFile, 52, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    SetCommitDirty();
                    return true;
                }
            }
        }
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
        byte[] outputData;
        LoadSwitchDataFile(filename, out outputData);
        return outputData;
    }

    public bool LoadSwitchDataFile(string filename, ref System.IO.MemoryStream ms)
    {
        byte[] outputData;
        if (LoadSwitchDataFile(filename, out outputData))
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
        CheckCanStep(E_AxiNS_dgbBk.LoadFile, -500, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        string ametaPath = GetFileLengthMetaPath(filename);
        UnityEngine.Debug.Log($"准备进行amete文件读取:{ametaPath}");
        CheckCanStep(E_AxiNS_dgbBk.LoadFile, -499, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (!DoLoadSwitchDataFile(ametaPath, null, out byte[] ametedata))
        {
            UnityEngine.Debug.Log($"amete文件读取失败:{ametaPath}");
            CheckCanStep(E_AxiNS_dgbBk.LoadFile, -498, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
            outputData = null;
            return false;
        }
        CheckCanStep(E_AxiNS_dgbBk.LoadFile, -497, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        int realLength = BitConverter.ToInt32(ametedata);
        UnityEngine.Debug.Log($"amete得到真实大小:{realLength}");
        CheckCanStep(E_AxiNS_dgbBk.LoadFile, -496, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        return DoLoadSwitchDataFile(filename, realLength, out outputData);
    }

    bool DoLoadSwitchDataFile(string filename, int? realLength, out byte[] outputData)
    {
#if !UNITY_SWITCH //|| UNITY_EDITOR
        outputData = null;
        return false;
#else

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 1, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);

        outputData = null;
        if (!AxiNS.instance.mount.SaveIsMount)
        {
            UnityEngine.Debug.LogError($"Save 尚未挂载，无法读取 {filename}");
            return false;
        }
        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 2, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (CheckPathNotFound(filename))
            return false;

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 3, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        FileHandle fileHandle = new nn.fs.FileHandle();
        nn.Result result;
        result = nn.fs.File.Open(ref fileHandle, filename, nn.fs.OpenFileMode.Read);
        if (result.IsSuccess() == false)
        {
            UnityEngine.Debug.LogError($"nn.fs.File.Open 失败 {filename} : result=>{result.GetErrorInfo()}");
            return false;   // Could not open file. This can be used to detect if this is the first time a user has launched your game. 
                            // (However, be sure you are not getting this error due to your file being locked by another process, etc.)
        }
        UnityEngine.Debug.Log($"nn.fs.File.Open 成功 {filename}");

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 4, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        long iFileSize = 0;
        result = nn.fs.File.GetSize(ref iFileSize, fileHandle);
        if (result.IsSuccess() == false)
        {
            UnityEngine.Debug.LogError($"nn.fs.File.GetSize 失败 {filename} : result=>{result.GetErrorInfo()}");
            return false;
        }

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 5, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        UnityEngine.Debug.Log($"nn.fs.File.GetSize 成功 {filename},size=>{iFileSize}");

        /*byte[] loadedData = new byte[iFileSize];
		result = nn.fs.File.Read(fileHandle, 0, loadedData, iFileSize);
		if (result.IsSuccess() == false)
		{
			UnityEngine.Debug.LogError($"nn.fs.File.Read 失败 {filename} : result=>{result.GetErrorInfo()}");
            //到达这里文件一定都open了 需要Close
            nn.fs.File.Close(fileHandle);
            return false;
		}
        
		nn.fs.File.Close(fileHandle);
		outputData = loadedData;
        */

        // ==================== 安全版核心 ====================
        if (realLength != null)
            iFileSize = realLength.Value;

        outputData = new byte[iFileSize];
        long offset = 0;
        const long MaxChunk = 1 * 1024 * 1024L;   // 1MB 分块，更保守

        byte[] buffer = new byte[MaxChunk];

        while (offset < (long)iFileSize)
        {
            long toRead = System.Math.Min(MaxChunk, iFileSize - offset);

            result = nn.fs.File.Read(fileHandle, offset, buffer, toRead);
            if (!result.IsSuccess())
            {
                UnityEngine.Debug.LogError($"Read 失败 {filename} offset={offset}: {result.GetErrorInfo()}");
                nn.fs.File.Close(fileHandle);
                outputData = null;
                return false;
            }

            Buffer.BlockCopy(buffer, 0, outputData, (int)offset, (int)toRead);
            offset += toRead;
        }

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 6, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        UnityEngine.Debug.Log($"nn.fs.File.Read 成功 {filename}");
        nn.fs.File.Close(fileHandle);

        CheckCanStep(E_AxiNS_dgbBk.LoadFile, 7, filename, System.Reflection.MethodBase.GetCurrentMethod().Name);
        return true;
#endif
    }
    public AxiNSWait_LoadSwitchDataFile LoadSwitchDataFileAsync(string filename)
    {
        var wait = new AxiNSWait_LoadSwitchDataFile(filename);
        AxiNS.instance.wait.AddWait(wait);
        return wait;
    }

    public bool GetDirectoryFiles(string path, out string[] entrys)
    {
#if !UNITY_SWITCH || UNITY_EDITOR

        entrys = null;
        return false;
#else
		return GetDirectoryEntrys(path,nn.fs.OpenDirectoryMode.File,out entrys);
#endif
    }

    public bool GetDirectoryDirs(string path, out string[] entrys)
    {
#if !UNITY_SWITCH || UNITY_EDITOR
        entrys = null;
        return false;
#else
        return GetDirectoryEntrys(path, nn.fs.OpenDirectoryMode.Directory, out entrys);
#endif
    }

#if UNITY_SWITCH
    bool GetDirectoryEntrys(string path, nn.fs.OpenDirectoryMode type, out string[] entrys)
    {
        nn.fs.DirectoryHandle eHandle = new nn.fs.DirectoryHandle();
        nn.Result result = nn.fs.Directory.Open(ref eHandle, path, type);
        if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            UnityEngine.Debug.Log($"目录 {path} 不存在");
            entrys = null;
            return false;
        }
        long entryCount = 0;
        nn.fs.Directory.GetEntryCount(ref entryCount, eHandle);
        nn.fs.DirectoryEntry[] entries = new nn.fs.DirectoryEntry[entryCount];
        long actualEntries = 0;
        result = nn.fs.Directory.Read(ref actualEntries, entries, eHandle, entryCount);
        if (result.IsSuccess() == false)
        {
            UnityEngine.Debug.LogError($"nn.fs.Directory.Read 失败 {path} : result=>{result.GetErrorInfo()}");
            //到达这里一定都open了 需要Close
            nn.fs.Directory.Close(eHandle);
            entrys = null;
            return false;
        }

        entrys = new string[actualEntries];
        for (int i = 0; i < actualEntries; i++)
        {
            entrys[i] = System.IO.Path.Combine(path, entries[i].name);
        }
        nn.fs.Directory.Close(eHandle);
        return true;
    }
#endif

    bool GetDirectoryEntrysFullRecursion(string path, out string[] entrys)
    {
#if UNITY_SWITCH

        nn.fs.DirectoryHandle eHandle = new nn.fs.DirectoryHandle();
        nn.Result result = nn.fs.Directory.Open(ref eHandle, path, OpenDirectoryMode.All);
        if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            UnityEngine.Debug.Log($"目录 {path} 不存在");
            entrys = null;
            return false;
        }
        long entryCount = 0;
        nn.fs.Directory.GetEntryCount(ref entryCount, eHandle);
        nn.fs.DirectoryEntry[] entries = new nn.fs.DirectoryEntry[entryCount];
        long actualEntries = 0;
        result = nn.fs.Directory.Read(ref actualEntries, entries, eHandle, entryCount);

        if (result.IsSuccess() == false)
        {
            UnityEngine.Debug.LogError($"nn.fs.Directory.Read 失败 {path} : result=>{result.GetErrorInfo()}");
            //到达这里一定都open了 需要Close
            nn.fs.Directory.Close(eHandle);
            entrys = null;
            return false;
        }

        List<string> temp = new List<string>();
        for (int i = 0; i < actualEntries; i++)
        {
            string singlePath = System.IO.Path.Combine(path, entries[i].name);
            temp.Add(singlePath);
            if (entries[i].entryType == EntryType.Directory && GetDirectoryEntrysFullRecursion(singlePath, out string[] singleEntryList))
            {
                temp.AddRange(singleEntryList);
            }
        }
        nn.fs.Directory.Close(eHandle);
        entrys = temp.ToArray();
        return true;
#else
        entrys = null;
        return false;
#endif
    }
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
#if !UNITY_SWITCH || UNITY_EDITOR
        yield break;
#else
    // 将通配符转换为正则表达式（支持*和?）
    var regexPattern = "^" + 
        Regex.Escape(searchPattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") 
        + "$";

    var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

		if (!GetDirectoryEntrys(path, nn.fs.OpenDirectoryMode.File, out string[] entrys))
		{
            yield break;
        }

		for (int i = 0; i < entrys.Length; i++)
		{
			if (regex.IsMatch(System.IO.Path.GetFileName(entrys[i])))
			{
				yield return entrys[i];
			}
        }
#endif
    }

    public bool DeletePathFile(string filename)
    {
        string ametapath = GetFileLengthMetaPath(filename);
        DoDeletePathFile(ametapath);
        return DoDeletePathFile(filename);
    }

    bool DoDeletePathFile(string filename)
    {
#if !UNITY_SWITCH
        return false;
#else
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(filename))
                return false;
            nn.Result result;
            result = nn.fs.File.Delete(filename);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.Delete 失败 {filename} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
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
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(filename))
                return false;
            nn.Result result;
            result = nn.fs.Directory.Delete(filename);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.Delete 失败 {filename} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
#endif
    }
    public AxiNSWait_DeletePathDir DeletePathDirAsync(string filename)
    {
        var wait = new AxiNSWait_DeletePathDir(filename);
        AxiNS.instance.wait.AddWait(wait);
        return wait;
    }
    public bool DeletePathDirRecursively(string filename)
    {
#if !UNITY_SWITCH
        return false;
#else
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(filename))
                return false;
            nn.Result result;
            result = nn.fs.Directory.DeleteRecursively(filename);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.Recursively 失败 {filename} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
#endif
    }
    public AxiNSWait_DeletePathDirRecursively DeletePathDirRecursivelyAsync(string filename)
    {
        var wait = new AxiNSWait_DeletePathDirRecursively(filename);
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
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(filename))
                return false;
            nn.Result result;
            result = nn.fs.Directory.DeleteRecursively(filename);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.DeleteRecursively 失败 {filename} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
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
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(filename))
                return false;
            nn.Result result;
            result = nn.fs.Directory.CleanRecursively(filename);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.DeleteRecursively 失败 {filename} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
#endif
    }

    public bool RenameDir(string oldpath, string newpath)
    {
#if !UNITY_SWITCH
        return false;
#else
        using (AxiNSIOKeepingDisposable.Acquire())
        {
            if (CheckPathNotFound(oldpath))
                return false;
            nn.Result result;
            result = nn.fs.Directory.Rename(oldpath, newpath);
            if (result.IsSuccess() == false)
            {
                UnityEngine.Debug.LogError($"nn.fs.File.Rename 失败 {oldpath} to {newpath} : result=>{result.GetErrorInfo()}");
                return false;
            }
            //return CommitSave();
            SetCommitDirty();
            return true;
        }
#endif
    }

#if UNITY_SWITCH
    bool CreateLoopDir(string path)
    {
        // 检查路径是否存在及其类型
        nn.fs.EntryType entryType = 0;
        nn.Result result;
        List<string> needcreatedirs = new List<string>();
        string node = path;
        while (true)
        {
            if (string.IsNullOrEmpty(node) || node.EndsWith(":/"))
                break;


            if (!CheckPathNotFound(node))
                needcreatedirs.Insert(0, node);
            else
                break;

            result = nn.fs.FileSystem.GetEntryType(ref entryType, node);
            if (!result.IsSuccess())
                needcreatedirs.Insert(0, node);
            else
                break;//文件已存在

            node = System.IO.Path.GetDirectoryName(node);
        }

        for (int i = 0; i < needcreatedirs.Count; i++)
        {
            UnityEngine.Debug.LogError($"需要创建的目录: {needcreatedirs[i]}");
        }

        for (int i = 0; i < needcreatedirs.Count; i++)
        {
            //result = nn.fs.Directory.Create(needcreatedirs[i]);
            //if (!result.IsSuccess())
            //{
            //    UnityEngine.Debug.LogError($"创建父目录失败: {result.GetErrorInfo()}");
            //    return false;
            //}
            //UnityEngine.Debug.Log($"父目录 {needcreatedirs[i]} 创建成功");
            //CommitSave();
        }

        return false;
        return true;
    }
#endif

    /// <summary>
    /// 解析路径并获取其所有父级目录（从直接父目录到根目录），并排除存储设备挂载根节点（如"save:"或"sd:"）。
    /// 专为Switch平台设计，正确处理如"save:/"或"sd:/"开头的路径。
    /// </summary>
    /// <param name="inputPath">输入的绝对路径</param>
    /// <param name="resolvedDirectory">输出参数，解析出的最直接目录（文件所在目录或目录自身）</param>
    /// <param name="parentDirectories">输出参数，从直接父目录到挂载点下一级的列表（不包含挂载根节点）</param>
    /// <returns>操作是否成功（路径格式基本有效）</returns>
    bool TryGetDirectoryAndParentsExcludingRoot(string inputPath, out string resolvedDirectory, out List<string> parentDirectories)
    {
        // 捕获路径中包含非法字符引发的异常
        //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->{inputPath}");
        resolvedDirectory = null;
        parentDirectories = new List<string>();

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->string.IsNullOrWhiteSpace({inputPath})==false");
            return false;
        }
        //string normalizedPath = SetSafePath(inputPath);
        string normalizedPath = inputPath;
        //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->normalizedPath=>{normalizedPath}");
        try
        {
            // 1. 判断路径类型并解析出最直接的目标目录
            bool isLikelyFile = false;

            if (normalizedPath.EndsWith("/"))
            {
                //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->normalizedPath.EndsWith(\"/\") == true");
                resolvedDirectory = normalizedPath.TrimEnd('/');
            }
            else
            {
                int lastSeparatorIndex = normalizedPath.LastIndexOf('/');
                //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->lastSeparatorIndex->{lastSeparatorIndex}");
                string lastPart = (lastSeparatorIndex >= 0) ? normalizedPath.Substring(lastSeparatorIndex + 1) : normalizedPath;

                if (string.IsNullOrEmpty(lastPart) || lastPart.Equals("..") || !lastPart.Contains("."))
                {
                    //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step1");
                    resolvedDirectory = normalizedPath;
                }
                else
                {
                    //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step2");
                    isLikelyFile = true;
                    //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->lastSeparatorIndex=>{lastSeparatorIndex}");
                    if (lastSeparatorIndex >= 0)
                    {
                        resolvedDirectory = normalizedPath.Substring(0, lastSeparatorIndex);
                    }
                    else
                    {
                        resolvedDirectory = normalizedPath; // 可能是根目录下的文件，但这种情况在Switch特殊路径中较少
                    }
                }
            }

            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->resolvedDirectory=>{resolvedDirectory}");
            if (string.IsNullOrEmpty(resolvedDirectory))
            {
                //UnityEngine.Debug.LogError($"TryGetDirectoryAndParentsExcludingRoot->string.IsNullOrEmpty(resolvedDirectory) == false");
                return false;
            }

            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step3");
            // 2. 提取并检查挂载根节点（如 "save:" 或 "sd:"）
            string mountRoot = null;
            int colonSlashIndex = resolvedDirectory.IndexOf(":/");
            if (colonSlashIndex > 0)
            {
                mountRoot = resolvedDirectory.Substring(0, colonSlashIndex + 1); // 例如 "save:"
            }
            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->mountRoot=>{mountRoot}");

            // 检查挂载状态
            if (!IsMountPointAccessible(mountRoot + "/"))
            {
                UnityEngine.Debug.LogError($"挂载点 {mountRoot + "/"} 未挂载，无法操作路径 {inputPath}");
                return false;
            }

            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step4");
            // 3. 手动分割路径，收集父目录，并在到达挂载根节点时停止
            string currentPath = resolvedDirectory;
            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->currentPath=>{currentPath}");
            while (!string.IsNullOrEmpty(currentPath))
            {
                //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step5");
                // 检查当前路径是否已经是挂载根节点（例如 "save:"）
                if (mountRoot != null && currentPath.Equals(mountRoot, StringComparison.OrdinalIgnoreCase))
                {
                    break; // 停止添加，不将挂载根节点加入父目录列表
                }

                parentDirectories.Add(currentPath); // 将当前层级的路径加入列表

                int lastSlashIndex = currentPath.LastIndexOf('/');
                if (lastSlashIndex < 0)
                {
                    break; // 没有分隔符了，跳出循环
                }

                string nextParent = currentPath.Substring(0, lastSlashIndex);

                // 检查下一级父目录是否就是挂载根节点（例如 "save:"），如果是，则停止
                if (mountRoot != null && nextParent.Equals(mountRoot, StringComparison.OrdinalIgnoreCase))
                {
                    // 可以选择是否将 mountRoot 加入列表，这里根据需求不加入
                    break;
                }

                currentPath = nextParent;
            }
            //UnityEngine.Debug.Log($"TryGetDirectoryAndParentsExcludingRoot->step6 True Return");

            return true;
        }
        catch (ArgumentException ex)
        {
            // 捕获路径中包含非法字符引发的异常
            UnityEngine.Debug.LogError($"路径中包含非法字符: {ex.Message}");
            return false;
        }
    }

    bool EnsureParentDirectory(string filePath, bool bAutoCreateDir = true)
    {
#if !UNITY_SWITCH
        return false;
#else

        CheckCanStep(E_AxiNS_dgbBk.Dir, 1, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

        if (!TryGetDirectoryAndParentsExcludingRoot(filePath, out string fullDirectoryPath, out List<string> parentDirectories))
        {
            UnityEngine.Debug.LogError($"TryGetDirectoryAndParentsExcludingRoot 操作失败:{filePath}");
            return false;
        }

        CheckCanStep(E_AxiNS_dgbBk.Dir, 2, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);

        UnityEngine.Debug.Log($"检查父目录: {fullDirectoryPath}");

        // 检查路径是否存在及其类型
        nn.fs.EntryType entryType = 0;
        nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, fullDirectoryPath);
        if (!result.IsSuccess() && nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            CheckCanStep(E_AxiNS_dgbBk.Dir, 3, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (bAutoCreateDir)
            {
                // 路径不存在，尝试创建
                UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 不存在，尝试创建 (判断依据 result=>{result.ToString()})");

                for (int i = parentDirectories.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Debug.Log($">>待检查目录: {parentDirectories[i]}");
                }
                for (int i = parentDirectories.Count - 1; i >= 0; i--)
                {
                    string dir = parentDirectories[i];
                    if (CheckPathNotFound(dir))
                    {
                        CheckCanStep(E_AxiNS_dgbBk.Dir, 4, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        UnityEngine.Debug.Log($"需要创建的目录: {dir}");
                        using (AxiNSIOKeepingDisposable.Acquire())
                        {
                            result = nn.fs.Directory.Create(dir);
                        }
                        if (!result.IsSuccess())
                        {
                            UnityEngine.Debug.LogError($"创建父 {dir} 目录失败: {result.GetErrorInfo()}");
                            return false;
                        }
                        UnityEngine.Debug.Log($"父目录 {dir} 创建成功");
                        CheckCanStep(E_AxiNS_dgbBk.Dir, 4, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"目录已存在，无需创建: {dir}");
                    }
                }
                CheckCanStep(E_AxiNS_dgbBk.Dir, 6, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
                UnityEngine.Debug.Log($"父目录 {fullDirectoryPath} 创建成功");
                return true;
            }
            return false;
        }
        else if (result.IsSuccess() && entryType != nn.fs.EntryType.Directory)
        {
            CheckCanStep(E_AxiNS_dgbBk.Dir, 7, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            // 路径存在，但不是目录
            UnityEngine.Debug.LogError($"路径 {fullDirectoryPath} 已存在，但不是目录");
            return false;
        }
        else if (!result.IsSuccess())
        {
            CheckCanStep(E_AxiNS_dgbBk.Dir, 8, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
            // 其他错误
            UnityEngine.Debug.LogError($"检查父目录失败: {result.GetErrorInfo()}");
            return false;
        }
        CheckCanStep(E_AxiNS_dgbBk.Dir, 9, filePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
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