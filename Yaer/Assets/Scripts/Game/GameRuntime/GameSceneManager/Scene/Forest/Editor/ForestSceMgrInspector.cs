using Game.GameRuntime.GameSceneManager.Base.Editor;
using UnityEditor;

namespace Game.GameRuntime.GameSceneManager.Scene.Forest.Editor
{
    [CustomEditor(typeof(ForestSceneManager), true)]
    public class ForestSceMgrInspector : BaseGameSceneMgrInspector
    {
        private SerializedProperty slime1;
        private SerializedProperty slime2;

        protected override void OnEnable()
        {
            base.OnEnable();

            // slime1 = serializedObject.FindProperty("slime1");
            // slime2 = serializedObject.FindProperty("slime2");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // EditorGUILayout.Space();
            // EditorUI.DrawBoltLabel("SceneObjects", Color.white);
            //
            // EditorGUILayout.PropertyField(slime1);
            // EditorGUILayout.PropertyField(slime2);
            //
            // serializedObject.ApplyModifiedProperties();
        }
    }
}