using GameFramework.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Base.Editor
{
    [CustomEditor(typeof(BaseGameSceneManager), true)]
    public class BaseGameSceneMgrInspector : UnityEditor.Editor
    {
        private SerializedProperty map;

        private SerializedProperty sceneObjs;
        private GUIStyle titleStyle;
        private BaseGameSceneManager Target => target as BaseGameSceneManager;

        protected virtual void OnEnable()
        {
            titleStyle = new GUIStyle { fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.white;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorUI.DrawScriptAsset(serializedObject);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("config"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnterPosConfig"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}