using GameFramework.Editor.UI;
using UnityEditor;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect.Editor
{
    [CustomEditor(typeof(ColliderResponder))]
    public class ColliderResponderInspector : UnityEditor.Editor
    {
        private SerializedProperty isChild;
        private void OnEnable()
        {
            isChild = serializedObject.FindProperty("isChild");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorUI.DrawScriptAsset(serializedObject);
            
            EditorGUILayout.PropertyField(isChild);

            if (isChild.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("parent"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("entityLogic"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}