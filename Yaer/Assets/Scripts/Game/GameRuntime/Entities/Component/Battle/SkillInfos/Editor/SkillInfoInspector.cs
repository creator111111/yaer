using GameFramework.Editor.UI;
using UnityEditor;

namespace Game.GameRuntime.Entities.Component.Battle.SkillInfos.Editor
{
    [CustomEditor(typeof(SkillInfo))]
    public class SkillInfoInspector : UnityEditor.Editor
    {
        private SkillInfo script;
        private void OnEnable()
        {
            script = (SkillInfo)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorUI.DrawScriptAsset(serializedObject);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("data"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shapeType"));

            switch (script.shapeType)
            {
                case SkillShapeType.Circle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
                    break;

                case SkillShapeType.Rectangle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));
                    break;

                case SkillShapeType.Triangle:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("baseLength"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("triangleHeight"));
                    break;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("openGizmos"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}