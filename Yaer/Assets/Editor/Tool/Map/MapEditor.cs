using Game.GameRuntime.Entities.Component.Map;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Map
{
    [CustomEditor(typeof(MapLimit))]
    public class MapEditor : UnityEditor.Editor
    {
        private MapLimit script;

        private void OnEnable()
        {
            script = (MapLimit)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Editor Map Limit"))
            {
                MapEditorWindow.Open(script);
            }
        }
    }
}