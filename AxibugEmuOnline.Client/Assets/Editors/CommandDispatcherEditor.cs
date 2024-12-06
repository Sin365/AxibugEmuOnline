using AxibugEmuOnline.Client;
using UnityEditor;
using UnityEngine;
namespace AxibugEmuOnline.Editors
{
    [CustomEditor(typeof(CommandDispatcher))]
    public class CommandDispatcherEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var dispacather = target as CommandDispatcher;

            dispacather.GetRegisters(out var normal, out var solo);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("NORMAL");
            foreach (var item in normal)
            {
                Draw(item);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("SOLO");
            foreach (var item in solo)
            {
                Draw(item);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField(dispacather.Current.Name);

            Repaint();
        }

        private void Draw(CommandExecuter item)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            using (new EditorGUI.DisabledGroupScope(!item.Enable))
                EditorGUILayout.ObjectField(item.gameObject, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
        }
    }
}
