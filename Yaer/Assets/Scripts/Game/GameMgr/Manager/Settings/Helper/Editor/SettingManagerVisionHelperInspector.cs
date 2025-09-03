using System.Collections.Generic;
using UnityEditor;

namespace Game.GameMgr.Manager.Settings.Helper.Editor
{
    [CustomEditor(typeof(SettingManagerVisionHelper))]
    public class SettingManagerVisionHelperInspector : UnityEditor.Editor
    {
        private Dictionary<string, string> info = new Dictionary<string, string>(); // <key, value>
        private SerializedProperty infoDic;

        private void OnEnable()
        {
            infoDic = serializedObject.FindProperty("infoDic");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            foreach (var kv in infoDic)
            {
            }

            EditorGUILayout.PropertyField(infoDic);
            serializedObject.ApplyModifiedProperties();
        }
    }
}