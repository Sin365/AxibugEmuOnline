using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Text;

namespace AxibugEmuOnline.Editors
{
    public class AxibugNSPTools : Editor
    {
        static string WorkRoot = Path.GetFullPath(Path.Combine(Application.dataPath,"AxiProjectTools/AxiNSPack"));
        static string switch_keys = Path.GetFullPath(Path.Combine(Application.dataPath, "AxiProjectTools/AxiNSPack/switch_keys"));
        static string hacpack_root = Path.GetFullPath(Path.Combine(Application.dataPath, "AxiProjectTools/AxiNSPack/hacpack"));
        static Dictionary<string, string> tools = new Dictionary<string, string>();
        static string prodKeysPath;
        
        [MenuItem("Axibug移植工具/Switch/AxibugNSPTools/RepackNSP")]
        static void RepackNSP()
        {
            if (!CheckEnvironmentVariable())
                return;

            string path = EditorUtility.OpenFilePanel(
                title: "选择 .nsp 文件",
                directory: Path.Combine(Application.dataPath,".."), // 默认路径为项目 Assets 目录
                extension: "nsp" // 限制文件类型为 .nsp
            );

            if (string.IsNullOrEmpty(path))
                return;

            RepackNSP(path);
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
            #region 初始化工具路径
            // 获取环境变量（需要添加环境变量检查）
            string sdkRoot = Environment.GetEnvironmentVariable("NINTENDO_SDK_ROOT");
            tools["authoringTool"] = Path.Combine(sdkRoot, "Tools/CommandLineTools/AuthoringTool/AuthoringTool.exe");
            tools["hacPack"] = hacpack_root;
            #endregion

            #region 处理NSP文件路径
            string nspFilePath = nspFile;
            string nspFileName = Path.GetFileName(nspFilePath);
            string nspParentDir = Path.GetDirectoryName(nspFilePath);
            #endregion

            #region 提取Title ID
            string titleID = ExtractTitleID(nspFilePath) ?? ManualTitleIDInput();
            Debug.Log($"[AxibugNSPTools]Using Title ID: {titleID}");
            #endregion

            #region 清理临时目录
            CleanDirectory(Path.Combine(nspParentDir, "repacker_extract"));
            CleanDirectory(Path.Combine(Path.GetTempPath(), "NCA"));
            CleanDirectory(Path.Combine(WorkRoot, "hacpack_backup"));
            #endregion

            #region 解包NSP文件
            string extractPath = Path.Combine(nspParentDir, "repacker_extract");
            ExecuteCommand($"{tools["authoringTool"]} extract -o \"{extractPath}\" \"{nspFilePath}\"");

            var (controlPath, programPath) = FindNACPAndNPDPaths(extractPath);
            if (controlPath == null || programPath == null)
            {
                Debug.LogError("[AxibugNSPTools] Critical directory structure not found!");
                return;
            }
            #endregion

            #region 重建NCA/NSP
            string tmpPath = Path.Combine(Path.GetTempPath(), "NCA");
            string programNCA = BuildProgramNCA(tmpPath, titleID, programPath);
            string controlNCA = BuildControlNCA(tmpPath, titleID, controlPath);
            BuildMetaNCA(tmpPath, titleID, programNCA, controlNCA);

            string outputNSP = BuildFinalNSP(nspFilePath, nspParentDir, tmpPath, titleID);
            Debug.Log($"[AxibugNSPTools]Repacking completed: {outputNSP}");
            #endregion
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

        static string ManualTitleIDInput()
        {
            Console.Write("Enter Title ID manually: ");
            return Console.ReadLine().Trim();
        }

        static void CleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                while (Directory.Exists(path)) ; // 等待删除完成
            }
        }

        static (string, string) FindNACPAndNPDPaths(string basePath)
        {
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                if (File.Exists(Path.Combine(dir, "fs0/control.nacp")))
                    return (dir, null);
                if (File.Exists(Path.Combine(dir, "fs0/main.npdm")))
                    return (null, dir);
            }
            return (null, null);
        }

        static string ExecuteCommand(string command)
        {
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
                    StandardErrorEncoding = Encoding.UTF8
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
        static string BuildProgramNCA(string tmpPath, string titleID, string programDir)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype program --exefsdir \"{programDir}/fs0\" " +
                          $"--romfsdir \"{programDir}/fs1\" --logodir \"{programDir}/fs2\"";

            string output = ExecuteCommand($"{tools["hacPack"]} {args}");
            return ParseNCAOutput(output, "Program");
        }

        static string BuildControlNCA(string tmpPath, string titleID, string controlDir)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype control --romfsdir \"{controlDir}/fs0\"";

            string output = ExecuteCommand($"{tools["hacPack"]} {args}");
            return ParseNCAOutput(output, "Control");
        }

        static void BuildMetaNCA(string tmpPath, string titleID, string programNCA, string controlNCA)
        {
            string args = $"-k \"{prodKeysPath}\" -o \"{tmpPath}\" --titleid {titleID} " +
                          $"--type nca --ncatype meta --titletype application " +
                          $"--programnca \"{programNCA}\" --controlnca \"{controlNCA}\"";

            ExecuteCommand($"{tools["hacPack"]} {args}");
        }

        static string BuildFinalNSP(string origPath, string parentDir, string tmpPath, string titleID)
        {
            string outputPath = origPath.Replace(".nsp", "_repacked.nsp");
            if (File.Exists(outputPath)) File.Delete(outputPath);

            string args = $"-k \"{prodKeysPath}\" -o \"{parentDir}\" --titleid {titleID} " +
                          $"--type nsp --ncadir \"{tmpPath}\"";

            ExecuteCommand($"{tools["hacPack"]} {args}");
            File.Move(Path.Combine(parentDir, $"{titleID}.nsp"), outputPath);
            return outputPath;
        }

        static string ParseNCAOutput(string output, string type)
        {
            var line = output.Split('\n')
                .FirstOrDefault(l => l.Contains($"Created {type} NCA:"));

            return line?.Split(':').Last().Trim();
        }
        #endregion
    }
}
