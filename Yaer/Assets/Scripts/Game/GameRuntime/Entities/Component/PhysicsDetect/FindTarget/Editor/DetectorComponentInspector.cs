using GameFramework.Editor.UI;
using UnityEditor;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget.Editor
{
    [CustomEditor(typeof(TargetDetector))]
    public class DetectorComponentInspector : UnityEditor.Editor
    {
        private TargetDetector script;
        private void OnEnable()
        {
            script = (TargetDetector)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorUI.DrawScriptAsset(serializedObject);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectorName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detectRangeType"));
            
            switch (script.detectRangeType)
            {
                case DetectRangeType.Circle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
                    break;

                case DetectRangeType.Rectangle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));
                    break;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("openGizmos"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}