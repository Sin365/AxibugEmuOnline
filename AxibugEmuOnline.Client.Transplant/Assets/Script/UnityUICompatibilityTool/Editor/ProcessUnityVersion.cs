using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ProcessUnityVersion
{
    public static Dictionary<string, string> prefab_replaceDict = new Dictionary<string, string>()
    {
		//GraphicRaycaster
        {"m_Script: {fileID: 11500000, guid: dc42784cf147c0c48a680349fa168899, type: 3}", "m_Script: {fileID: 11500000, guid: dd73c3b3bc600af4baad2151ea75fa2c, type: 3}"},
		//CanvasScaler
        {"m_Script: {fileID: 11500000, guid: 0cd44c1031e13a943bb63640046fad76, type: 3}", "m_Script: {fileID: 11500000, guid: 8b447dd23c860344d836be9d9aec15f0, type: 3}"},
		//Text
        {"m_Script: {fileID: 11500000, guid: 5f7201a12d95ffc409449d95f23cf332, type: 3}", "m_Script: {fileID: 11500000, guid: 880218c1f11e0054597c4282005949df, type: 3}"},
        {"m_Script: {fileID: 11500000, guid: 880218c1f11e0054597c4282005949df, type: 3}", "m_Script: {fileID: 11500000, guid: 880218c1f11e0054597c4282005949df, type: 3}"},
        {"m_Script: {fileID: 708705254, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: 880218c1f11e0054597c4282005949df, type: 3}"},
		//Image
        {"m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}", "m_Script: {fileID: 11500000, guid: b2899f217128f0d46b73ba0f23cb55d0, type: 3}"},
		//Button
        {"m_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}", "m_Script: {fileID: 11500000, guid: 901c84e239f2fb9469f458b142fff636, type: 3}"},
        {"m_Script: {fileID: 1392445389, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: 901c84e239f2fb9469f458b142fff636, type: 3}"},
		//RawImage
        {"m_Script: {fileID: 11500000, guid: 1344c3c82d62a2a41a3576d8abb8e3ea, type: 3}", "m_Script: {fileID: 11500000, guid: 5d926e078698e40438e354128b5acd58, type: 3}"},
		//Dropdown
        {"m_Script: {fileID: 11500000, guid: 0d0b652f32a2cc243917e4028fa0f046, type: 3}", "m_Script: {fileID: 11500000, guid: 5cc1d029a0a45e04f94112215706fd34, type: 3}"},
		//ScrollRect
        {"m_Script: {fileID: 11500000, guid: 1aa08ab6e0800fa44ae55d278d1423e3, type: 3}", "m_Script: {fileID: 11500000, guid: fb6f7756c81d4814f9e31a48b1d39433, type: 3}"},
		//Toggle
        {"m_Script: {fileID: 11500000, guid: 9085046f02f69544eb97fd06b6048fe2, type: 3}", "m_Script: {fileID: 11500000, guid: db771345252fdde47ad268c9d300daae, type: 3}"},
        {"m_Script: {fileID: 2109663825, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: db771345252fdde47ad268c9d300daae, type: 3}"},
		//Scrollbar
        {"m_Script: {fileID: 11500000, guid: 2a4db7a114972834c8e4117be1d82ba3, type: 3}", "m_Script: {fileID: 11500000, guid: 4d63c9838d79fa3429c5ee15808b19eb, type: 3}"},
		//Slider
        {"m_Script: {fileID: 11500000, guid: 67db9e8f0e2ae9c40bc1e2b64352a6b4, type: 3}", "m_Script: {fileID: 11500000, guid: caba9dd549ff58c4c927b755957be186, type: 3}"},
        {"m_Script: {fileID: -113659843, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: caba9dd549ff58c4c927b755957be186, type: 3}"},
		//Mask
        {"m_Script: {fileID: 11500000, guid: 31a19414c41e5ae4aae2af33fee712f6, type: 3}", "m_Script: {fileID: 11500000, guid: 498b1c91abcaea34a89cf3d6b968867b, type: 3}"},
        {"m_Script: {fileID: -1200242548, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: 498b1c91abcaea34a89cf3d6b968867b, type: 3}"},
		//RectMask2D
        {"m_Script: {fileID: 11500000, guid: 3312d7739989d2b4e91e6319e9a96d76, type: 3}", "m_Script: {fileID: 11500000, guid: a734a3407c17c0841967c6f9cce8eac7, type: 3}"},
		//ContentSizeFitter
        {"m_Script: {fileID: 11500000, guid: 3245ec927659c4140ac4f8d17403cc18, type: 3}", "m_Script: {fileID: 11500000, guid: 91ed9c8a3832de340916866a0a1bad20, type: 3}"},
        {"m_Script: {fileID: 1741964061, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: 91ed9c8a3832de340916866a0a1bad20, type: 3}"},
		//GridLayoutGroup
        {"m_Script: {fileID: 11500000, guid: 8a8695521f0d02e499659fee002a26c2, type: 3}", "m_Script: {fileID: 11500000, guid: ed4588221c168b34f84d23c06c2d2d49, type: 3}"},
		//EventSystem
        {"m_Script: {fileID: 11500000, guid: 76c392e42b5098c458856cdf6ecaaaa1, type: 3}", "m_Script: {fileID: 11500000, guid: ab4a01a487c8b1046b5d74f81c4caa42, type: 3}"},
		//StandaloneInputModule
        {"m_Script: {fileID: 11500000, guid: 4f231c4fb786f3946a6b90b886c48677, type: 3}", "m_Script: {fileID: 11500000, guid: c337dc6c95c72134abc4afe4607cb3b9, type: 3}"},
		//Outline
        {"m_Script: {fileID: 11500000, guid: e19747de3f5aca642ab2be37e372fb86, type: 3}", "m_Script: {fileID: 11500000, guid: d9a7125dc73df8b42872961c2c4803f7, type: 3}"},
        {"m_Script: {fileID: -900027084, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: d9a7125dc73df8b42872961c2c4803f7, type: 3}"},
        //VideoPlayer
        {"m_Script: {fileID: -765806418, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}"},
        //VerticalLayoutGroup
        {"m_Script: {fileID: 11500000, guid: 59f8146938fff824cb5fd77236b75775, type: 3}", "m_Script: {fileID: 11500000, guid: 7d1bb376a8d85044ab7bce5994d5cdff, type: 3}"},
        {"m_Script: {fileID: 1297475563, guid: f70555f144d8491a825f0804e09c671c, type: 3}", "m_Script: {fileID: 11500000, guid: 7d1bb376a8d85044ab7bce5994d5cdff, type: 3}"},
        //HorizontalLayoutGroup
        {"m_Script: {fileID: 11500000, guid: 30649d3a9faa99c48a7b1166b86bf2a0, type: 3}", "m_Script: {fileID: 11500000, guid: f64d5bed43ac5a84eac12353031fa310, type: 3}"},
        //EventTrigger
        {"m_Script: {fileID: 11500000, guid: d0b148fe25e99eb48b9724523833bab1, type: 3}", "m_Script: {fileID: 11500000, guid: 3189a1a2831d7e04eafb4daf5c8db0b3, type: 3}"},
        //LayoutElement
        {"m_Script: {fileID: 11500000, guid: 306cc8c2b49d7114eaa3623786fc2126, type: 3}", "m_Script: {fileID: 11500000, guid: 104bbc5315760e248864e117de3e8c24, type: 3}"},
    };

    [MenuItem("Tools/替换UI引用")]
    public static void Process()
    {
        ProcessAllPrefabs("*.prefab");
        ProcessAllPrefabs("*.anim");
    }

    [MenuItem("Tools/反向替换UI引用")]
    public static void ReverseProcess()
    {
        ProcessAllPrefabs("*.prefab", true);
        ProcessAllPrefabs("*.anim", true);
    }

    private static void ProcessAllPrefabs(string form, bool reverse = false)
    {
        List<GameObject> prefabs = new List<GameObject>();
        var resourcesPath = Application.dataPath;
        var absolutePaths = Directory.GetFiles(resourcesPath, form, SearchOption.AllDirectories);
        for (int i = 0; i < absolutePaths.Length; i++)
        {
            Debug.Log("prefab name: " + absolutePaths[i]);
            foreach (var VARIABLE in prefab_replaceDict)
            {
                string oldValue = reverse ? VARIABLE.Value : VARIABLE.Key;
                string newValue = reverse ? VARIABLE.Key : VARIABLE.Value;
                ReplaceValue(absolutePaths[i], oldValue, newValue);
            }
            EditorUtility.DisplayProgressBar("处理预制体……", "处理预制体中……", (float)i / absolutePaths.Length);
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 替换值
    /// </summary>
    /// <param name="strFilePath">文件路径</param>
    private static void ReplaceValue(string strFilePath, string oldLine, string newLine)
    {
        if (File.Exists(strFilePath))
        {
            string[] lines = File.ReadAllLines(strFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace(oldLine, newLine);
            }
            File.WriteAllLines(strFilePath, lines);
        }
    }
}