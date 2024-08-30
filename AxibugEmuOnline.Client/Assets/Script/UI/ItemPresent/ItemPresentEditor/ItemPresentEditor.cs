using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ItemPresent))]
public class ItemPresentEditor : GridLayoutGroupEditor
{
    public override void OnInspectorGUI()
    {
        ItemPresent behaviour = target as ItemPresent;
        var itemTemplate = serializedObject.FindProperty("ItemTemplate");
        var viewRect = serializedObject.FindProperty("ViewRect");
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(itemTemplate, new GUIContent("元素"), true);
        if (GUILayout.Button("同步大小", GUILayout.Width(80)))
        {
            behaviour.cellSize = behaviour.ItemTemplate.rect.size;
            GUIUtility.keyboardControl = 0;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(viewRect, true);

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
