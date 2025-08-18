#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class AxiProjectTools_AssetsAutoSetting : EditorWindow
{
	[MenuItem("Axibug移植工具/AssetsAutoSetting/自动设置TextureMaxSize为图片大小1倍")]
	public static void AutoSettTextureSize_1x() { SetTextureSite(1f); }

	[MenuItem("Axibug移植工具/AssetsAutoSetting/自动设置TextureMaxSize为图片大小2分之1倍")]
	public static void AutoSettTextureSize_1_2x() { SetTextureSite(1f / 2f); }
	[MenuItem("Axibug移植工具/AssetsAutoSetting/自动设置TextureMaxSize为图片大小4分之1倍")]
	public static void AutoSettTextureSize_1_4x() { SetTextureSite(1f / 4f); }

	public static void SetTextureSite(float Scale)
	{
		Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
		if (textures.Length == 0)
		{
			Debug.LogWarning("请先选择目录，或者Texture资源");
			return;
		}

		AssetDatabase.StartAssetEditing(); // 开启批量编辑模式
		foreach (var texture in textures)
		{
			string path = AssetDatabase.GetAssetPath(texture);
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer != null)
			{
				int size = Mathf.Max(texture.width, texture.height);
				int maxsize = Mathf.ClosestPowerOfTwo((int)(size * Scale)); // Unity内置方法适配2的幂次方
				importer.maxTextureSize = maxsize;
				importer.SaveAndReimport();
			}
		}
		AssetDatabase.StopAssetEditing(); // 结束批量编辑
		Debug.Log($"Updated {textures.Length} textures.");
	}
}
#endif