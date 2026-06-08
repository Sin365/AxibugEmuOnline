#if UNITY_EDITOR
using System;
using UnityEngine;

public static partial class AxiAutoBuild
{
    static AxiAutoBuild()
    {
        CustomSetVersionName += SetVersionNameWithAxiEmuVer;
        CustomSetbundleVersion += SetCustomSetbundleVersion;
    }

    private static string SetCustomSetbundleVersion()
    {
        var versionInfo = Resources.Load<VerScriptable>("Version/VersionInfo");
        return versionInfo.GetBundleVersionStr();
    }

    private static string SetVersionNameWithAxiEmuVer(string srcName)
    {
        var versionInfo = Resources.Load<VerScriptable>("Version/VersionInfo");
        return srcName + "_" + versionInfo.GetFullVersionStr();
    }
}
#endif