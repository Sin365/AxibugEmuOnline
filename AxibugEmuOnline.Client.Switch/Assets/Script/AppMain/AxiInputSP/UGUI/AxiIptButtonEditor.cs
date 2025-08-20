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
            // ��ȡ���л�����
            _axiBtnTypeListProp = serializedObject.FindProperty("axiBtnTypeList");

            // ��ʼ�� ReorderableList
            _itemList = new ReorderableList(
                serializedObject,
                _axiBtnTypeListProp,
                true, true, true, true
            );

            // �Զ����б���ƣ�����ԭ���߼���
            _itemList.drawHeaderCallback = rect => GUI.Label(rect, "��λ����");
            _itemList.drawElementCallback = (rect, index, active, focused) => {
                var element = _axiBtnTypeListProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. ���Ȼ����Զ���ļ�λ����
            EditorGUILayout.LabelField("�Զ����λ����", EditorStyles.boldLabel);
            _itemList.DoLayoutList();

            // 2. ��ӷָ���
            EditorGUILayout.Space(10);
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(5);

            // 3. ����ԭ�� Button ���ԣ��ų��Ѵ���� axiBtnTypeList��
            EditorGUILayout.LabelField("ԭ����ť����", EditorStyles.boldLabel);
            DrawDefaultInspectorExcluding("axiBtnTypeList");

            serializedObject.ApplyModifiedProperties();
        }

        // ���������������ų�ָ���ֶε�Ĭ�� Inspector
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