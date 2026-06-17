using UnityEngine;

[CreateAssetMenu(fileName = "VerScriptable", menuName = "Scriptable Objects/VerScriptable")]
public class VerScriptable : ScriptableObject
{
    public int MainVer;
    public int SubVer;
    public int FixVer;
    public string Tag;

    public string GetFullVersionStr()
    {
        string verStr = MainVer + "." + SubVer + "." + FixVer;
        if (!string.IsNullOrEmpty(Tag))
            verStr += " " + Tag;
        return verStr;
    }

    public string GetBundleVersionStr()
    {
        string verStr = MainVer + "." + SubVer + "." + FixVer;
        return verStr;
    }
}