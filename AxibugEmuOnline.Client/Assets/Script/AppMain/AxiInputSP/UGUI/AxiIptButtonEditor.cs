#if UNITY_EDITOR
using AxiInputSP.UGUI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AxibugEmuOnline.Editors
{
    [CustomEditor(typeof(AxiIptButton))]
    public class AxiIptButtonEditor : Editor
    {
        private ReorderableList _itemList;
        private SerializedProperty _axiBtnTypeListProp;

        private void OnEnable()
        {
            // 获取序列化属性
            _axiBtnTypeListProp = serializedObject.FindProperty("axiBtnTypeList");

            // 初始化 ReorderableList
            _itemList = new ReorderableList(
                serializedObject,
                _axiBtnTypeListProp,
                true, true, true, true
            );

            // 自定义列表绘制（保持原有逻辑）
            _itemList.drawHeaderCallback = rect => GUI.Label(rect, "键位数组");
            _itemList.drawElementCallback = (rect, index, active, focused) => {
                var element = _axiBtnTypeListProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. 优先绘制自定义的键位数组
            EditorGUILayout.LabelField("自定义键位配置", EditorStyles.boldLabel);
            _itemList.DoLayoutList();

            // 2. 添加分割线
            EditorGUILayout.Space(10);
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(5);

            // 3. 绘制原生 Button 属性（排除已处理的 axiBtnTypeList）
            EditorGUILayout.LabelField("原生按钮属性", EditorStyles.boldLabel);
            DrawDefaultInspectorExcluding("axiBtnTypeList");

            serializedObject.ApplyModifiedProperties();
        }

        // 辅助方法：绘制排除指定字段的默认 Inspector
        private void DrawDefaultInspectorExcluding(params string[] excludePaths)
        {
            var prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (System.Array.IndexOf(excludePaths, prop.name) >= 0) continue;
                EditorGUILayout.PropertyField(prop, true);
            }
        }
    }
}
#endif