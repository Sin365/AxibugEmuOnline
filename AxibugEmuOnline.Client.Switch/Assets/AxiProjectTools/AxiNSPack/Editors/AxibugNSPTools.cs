#if UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AxibugEmuOnline.Editors
{
    public class AxibugNSPTools : Editor
    {
        static string WorkRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "AxiProjectTools/AxiNSPack"));
        static string switch_keys = Path.GetFullPath(Path.Combine(Application.dataPath, "AxiProjectTools/AxiNSPack/switch_keys"));
        static string hacpack_root = Path.GetFullPath(Path.Combine(Application.dataPath, "AxiProjectTools/AxiNSPack/hacpack"));
        static Dictionary<string, string> tools = new Dictionary<string, string>();
        static string prodKeysPath;

        static void InitToolPath()
        {
            #region 初始化工具路径
            // 获取环境变量（需要添加环境变量检查）
            string sdkRoot = Environment.GetEnvironmentVariable("NINTENDO_SDK_ROOT");
            tools["authoringTool"] = Path.Combine(sdkRoot, "Tools/CommandLineTools/AuthoringTool/AuthoringTool.exe");
            tools["hacPack"] = Path.Combine(hacpack_root, "hacpack");
            #endregion
        }

        [MenuItem("Axibug移植工具/Switch/AxibugNSPTools/RepackNSP(仅重新构建NSP）")]
        static void RepackNSP()
        {
            if (!CheckEnvironmentVariable())
                return;

            string path = EditorUtility.OpenFilePanel(
                title: "选择 .nsp 文件",
                directory: Path.Combine(Application.dataPath, ".."), // 默认路径为项目 Assets 目录
                extension: "nsp" // 限制文件类型为 .nsp
            );

            if (string.IsNullOrEmpty(path))
                return;

            RepackNSP(path);
        }


        //[MenuItem("Axibug移植工具/Switch/AxibugNSPTools/UnpackNSP(解包工具）")]
        //static void UnpackNSP()
        //{
        //    InitToolPath();
        //    if (!CheckEnvironmentVariable())
        //        return;

        //    string nspFilePath = EditorUtility.OpenFilePanel(
        //        title: "选择 .nsp 文件",
        //        directory: Path.Combine(Application.dataPath, ".."), // 默认路径为项目 Assets 目录
        //        extension: "nsp" // 限制文件类型为 .nsp
        //    );

        //    if (string.IsNullOrEmpty(nspFilePath))
        //        return;

        //    string nspParentDir = Path.GetDirectoryName(nspFilePath);
        //    string extractPath = Path.Combine(nspParentDir, "repacker_extract");
        //    ExecuteCommand($"{tools["authoringTool"]} extract -o \"{extractPath}\" \"{nspFilePath}\"", nspParentDir);
        //}

        [MenuItem("Axibug移植工具/Switch/AxibugNSPTools/Build With RepackNSP(打包NSP并重新构建NSP）")]
        public static void BuildWithRepackNSP()
        {
            if (!CheckEnvironmentVariable())
                return;

            if (!EditorUtility.DisplayDialog("打包", $"确认打包NSP?", "继续", "取消"))
                return;

            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    levels.Add(scene.path);
            }

            var buildOpt = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
                buildOpt |= BuildOptions.EnableDeepProfilingSupport;
            if (EditorUserBuildSettings.allowDebugging)
                buildOpt |= BuildOptions.AllowDebugging;

            //勾选构建NSP包
            EditorUserBuildSettings.switchCreateRomFile = true;

#if UNITY_2018_1_OR_NEWER && !UNITY_6000_0_OR_NEWER
            string titleid = PlayerSettings.Switch.applicationID;
#else
            string titleid = "null";
#endif
            string targetName = $"{Application.productName}_{titleid}.nsp";

            string _locationPathName = $"Output/NSPBuild/{DateTime.Now.ToString("yyyyMMddHHmmss")}/{targetName}";
            var options = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = _locationPathName,
                target = BuildTarget.Switch,
                options = buildOpt
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AxibugNSPTools] Unity Build NSP 错误:{ex.ToString()}");
                return;
            }

            string NSPFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", _locationPathName));
            RepackNSP(NSPFullPath);
        }

        static bool CheckEnvironmentVariable()
        {
            // 获取环境变量（需要添加环境变量检查）
            string sdkRoot = Environment.GetEnvironmentVariable("NINTENDO_SDK_ROOT");
            if (string.IsNullOrEmpty(sdkRoot))
            {
                Debug.LogError($"[AxibugNSPTools]请先正确配置环境变量:NINTENDO_SDK_ROOT,(若已配置，则保证配置后彻底重启Unity Hub和Unity)");
                return false;
            }

            #region 获取prod.keys文件路径
            prodKeysPath = Path.Combine(
                switch_keys,
                "prod.keys"
            );

            if (!File.Exists(prodKeysPath))
            {
                Debug.LogError($"[AxibugNSPTools]未找到 prod.keys! 请先准备文件，path:{prodKeysPath}");
                return false;
            }
            #endregion

            return true;
        }

        static void RepackNSP(string nspFile)
        {
            InitToolPath();

            #region 处理NSP文件路径
            string nspFilePath = nspFile;
            string nspFileName = Path.GetFileName(nspFilePath);
            string nspParentDir = Path.GetDirectoryName(nspFilePath);
            #endregion

            #region 提取Title ID
            string titleID = ExtractTitleID(nspFilePath);
            if (string.IsNullOrEmpty(titleID))
            {
                Debug.LogError($"[AxibugNSPTools]NSP文件名一部分，必须包含TitleID");
                return;
            }
            Debug.Log($"[AxibugNSPTools]Using Title ID: {titleID}");
            #endregion

            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"清理临时目录", 0);
            #region 清理临时目录
            CleanDirectory(Path.Combine(nspParentDir, "repacker_extract"));
            CleanDirectory(Path.Combine(Path.GetTempPath(), "NCA"));
            CleanDirectory(Path.Combine(nspParentDir, "hacpack_backup"));
            #endregion

            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"解包NSP文件", 0.2f);
            #region 解包NSP文件
            string extractPath = Path.Combine(nspParentDir, "repacker_extract");
            ExecuteCommand($"{tools["authoringTool"]} extract -o \"{extractPath}\" \"{nspFilePath}\"", nspParentDir);

            string controlPath = null;
            string programPath = null;
            FindNACPAndNPDPaths(extractPath, ref controlPath, ref programPath);
            if (controlPath == null || programPath == null)
            {
                Debug.LogError("[AxibugNSPTools] Critical directory structure not found!");
                return;
            }
            #endregion

            #region 重建NCA/NSP
            string tmpPath = Path.Combine(Path.GetTempPath(), "NCA");
            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"重建 Program NCA", 0.3f);
            string programNCA = BuildProgramNCA(tmpPath, titleID, programPath, nspParentDir);
            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"重建 Control NCA", 0.4f);
            string controlNCA = BuildControlNCA(tmpPath, titleID, controlPath, nspParentDir);
            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"重建 Meta NCA", 0.5f);
            BuildMetaNCA(tmpPath, titleID, programNCA, controlNCA, nspParentDir);
            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"重建NSP", 0.6f);
            string outputNSP = BuildFinalNSP(nspFilePath, nspParentDir, tmpPath, titleID, nspParentDir);
            EditorUtility.DisplayProgressBar("AxibugNSPTools", $"重建NSP", 0.9f);
            Debug.Log($"[AxibugNSPTools]Repacking completed: {outputNSP}");

			#endregion

			EditorUtility.DisplayProgressBar("AxibugNSPTools", $"清理临时目录", 1);
			#region 清理临时目录
			CleanDirectory(Path.Combine(nspParentDir, "repacker_extract"));
			CleanDirectory(Path.Combine(Path.GetTempPath(), "NCA"));
			CleanDirectory(Path.Combine(nspParentDir, "hacpack_backup"));
			#endregion
			System.Diagnostics.Process.Start("explorer", "/select,\"" + outputNSP.Trim() + "\"");
			EditorUtility.ClearProgressBar();
        }

        #region 辅助方法
        static string GetUserInput()
        {
            Console.Write("Enter the NSP filepath: ");
            return Console.ReadLine();
        }
        static string ExtractTitleID(string path)
        {
            var match = Regex.Match(path, @"0100[\dA-Fa-f]{12}");
            return match.Success ? match.Value : null;
        }

        static void CleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                while (Directory.Exists(path)) ; // 等待删除完成
            }
        }

        static void FindNACPAndNPDPaths(string basePath, ref string controlPath, ref string programPath)
        {
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                if (File.Exists(Path.Combine(dir, "fs0/control.nacp")))
                    controlPath = dir;
                if (File.Exists(Path.Combine(dir, "fs0/main.npdm")))
                    programPath = dir;
            }
        }

        static string ExecuteCommand(string command, string workdir)
        {
            Debug.Log($"调用cmd=>{command}");
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,  // 增加错误流重定向
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,  // 明确指定编码
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = workdir
                }
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // 使用事件处理程序捕获实时输出
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    outputBuilder.AppendLine(args.Data);
                    Debug.Log($"[AxibugNSPTools]{args.Data}");
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    errorBuilder.AppendLine(args.Data);
                    if (args.Data.Contains("[WARN]"))
                        Debug.LogWarning($"[AxibugNSPTools]{args.Data}");
                    else
                        Debug.LogError($"[AxibugNSPTools]{args.Data}");
                }
            };

            process.Start();

            // 开始异步读取输出
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 等待进程退出（此时流已关闭）
            process.WaitForExit();

            // 将错误信息附加到主输出
            if (errorBuilder.Length > 0)
            {
                outputBuilder.AppendLine("\nError Output:");
                outputBuilder.Append(errorBuilder);
            }

            return outputBuilder.ToString();
        }
        #endregion

        #region NCA构建逻辑
        static string BuildProgramNCA(string tmpPath, string titleID, string programDir, string workdir)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype program --exefsdir \"{programDir}/fs0\" " +
                          $"--romfsdir \"{programDir}/fs1\" --logodir \"{programDir}/fs2\"";

            string output = ExecuteCommand($"{tools["hacPack"]} {args}", workdir);
            return ParseNCAOutput(output, "Program");
        }

        static string BuildControlNCA(string tmpPath, string titleID, string controlDir, string workdir)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype control --romfsdir \"{controlDir}/fs0\"";

            string output = ExecuteCommand($"{tools["hacPack"]} {args}", workdir);

            return ParseNCAOutput(output, "Control");
        }

        static void BuildMetaNCA(string tmpPath, string titleID, string programNCA, string controlNCA, string workdir)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype meta --titletype application " +
                          $"--programnca \"{programNCA}\" --controlnca \"{controlNCA}\"";

            ExecuteCommand($"{tools["hacPack"]} {args}", workdir);
        }

        static string BuildFinalNSP(string origPath, string parentDir, string tmpPath, string titleID, string workdir)
        {
            string outputPath = origPath.Replace(".nsp", "_repacked.nsp");
            if (File.Exists(outputPath)) File.Delete(outputPath);

            string args = $"-k \"{prodKeysPath}\" -o \"{parentDir}\" --titleid {titleID} " +
                          $"--type nsp --ncadir \"{tmpPath}\"";

            ExecuteCommand($"{tools["hacPack"]} {args}", workdir);
            File.Move(Path.Combine(parentDir, $"{titleID}.nsp"), outputPath);
            return outputPath;
        }

        static string ParseNCAOutput(string output, string type)
        {
            var line = output.Split('\n')
                .FirstOrDefault(l => l.Contains($"Created {type} NCA:"));
            //return line?.Split(':').Last().Trim();
            return line?.Substring(line.IndexOf("NCA:") + "NCA:".Length).Trim();

        }
        #endregion
    }
}
#endif