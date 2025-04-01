#if UNITY_EDITOR
using AxibugEmuOnline.Editors;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AxiAutoBuild
{
    [MenuItem("Axibug移植工具/AutoBuild/Build ALL")]
    public static void Build_Build_ALL()
    {
        Build_Global(BuildTarget.Android);
        Build_Global(BuildTarget.iOS);
        Build_Global(BuildTarget.StandaloneWindows);
        Build_Global(BuildTarget.StandaloneLinux64);
        Build_Global(BuildTarget.WSAPlayer);
        AxibugNSPTools.BuildWithRepackNSP();
    }

    [MenuItem("Axibug移植工具/AutoBuild/Android")]
    public static void Build_Android()
    {
        Build_Global(BuildTarget.Android);
    }

    [MenuItem("Axibug移植工具/AutoBuild/IOS")]
    public static void Build_IOS()
    {
        Build_Global(BuildTarget.iOS);
    }
    [MenuItem("Axibug移植工具/AutoBuild/PC")]
    public static void Build_PC()
    {
        Build_Global(BuildTarget.StandaloneWindows);
    }
    [MenuItem("Axibug移植工具/AutoBuild/Linux")]
    public static void Build_Linux64()
    {
        Build_Global(BuildTarget.StandaloneLinux64);
    }
    [MenuItem("Axibug移植工具/AutoBuild/UWP")]
    public static void Build_UWP()
    {
        Build_Global(BuildTarget.WSAPlayer);
    }

    [MenuItem("Axibug移植工具/AutoBuild/EmbeddedLinux")]
    public static void Build_EmbeddedLinux()
    {
        Build_Global(BuildTarget.EmbeddedLinux);
    }

    [MenuItem("Axibug移植工具/AutoBuild/Switch")]
    public static void Build_Switch()
    {
        AxibugNSPTools.BuildWithRepackNSP();
    }


    public static void Build_Global(BuildTarget target)
    {
        if (!EditorUtility.DisplayDialog("打包", $"确认打包{target}?", "继续", "取消"))
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

        string targetName = $"{Application.productName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

        targetName += target switch
        {
            BuildTarget.Android => ".apk",
            BuildTarget.iOS => ".ipa",
            BuildTarget.StandaloneWindows => ".exe",
            _ => "",
        };


        string _locationPathName = $"Output/{target}/{targetName}";
        string FullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", _locationPathName));
        string dirPath = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var options = new BuildPlayerOptions
        {
            scenes = levels.ToArray(),
            locationPathName = _locationPathName,
            target = target,
            options = buildOpt
        };

        try
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AutoBuild] Unity Build {target} 错误:{ex.ToString()}");
            return;
        }
    }

}
#endif