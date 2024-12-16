using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SpriteProcessor : EditorWindow
{
    [MenuItem("Tools/修复多个材质精灵")]
    public static void FixMultipleMaterialSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:sprite");
        List<Sprite> spritesToFix = new List<Sprite>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            // 检查是否有多个材质
            if (IsUsingMultipleMaterials(sprite))
            {
                spritesToFix.Add(sprite);
                Debug.Log("Found sprite with multiple materials: " + path);
            }
        }

        // 修复每个找到的Sprite
        foreach (var sprite in spritesToFix)
        {
            FixSprite(sprite);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", "已修复多个材质精灵。", "OK");
    }

    private static bool IsUsingMultipleMaterials(Sprite sprite)
    {
        if (sprite == null) return false;

        // 获取精灵的材质
        var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;

        return textureImporter != null && textureImporter.spriteImportMode == SpriteImportMode.Multiple;
    }

    private static void FixSprite(Sprite sprite)
    {
        // 获取Sprite的路径
        string path = AssetDatabase.GetAssetPath(sprite);
        var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        if (textureImporter != null)
        {
            // 保存当前切割信息
            SpriteMetaData[] originalMetaData = textureImporter.spritesheet;

            // 临时禁用Sprite导入
            textureImporter.spriteImportMode = SpriteImportMode.None;
            textureImporter.SaveAndReimport();

            // 重新启用Sprite导入并保持原样切割参数
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritesheet = originalMetaData; // 恢复原来的切割信息

            // 重新导入以应用更改
            textureImporter.SaveAndReimport();
        }
    }
}
