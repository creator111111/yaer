using Game.GameRuntime.GameSceneManager.Base.Editor;
using GameFramework.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.SelectClothes.Editor
{
    [CustomEditor(typeof(SelectClothesSceneManager), true)]
    public class SelectClothesSceneMgrInsp : BaseGameSceneMgrInspector
    {
        private SerializedProperty changingClothingTarget;

        protected override void OnEnable()
        {
            base.OnEnable();

            changingClothingTarget = serializedObject.FindProperty("changingClothingTarget");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorUI.DrawBoltLabel("SelectClothesSceneManager", Color.white);
            EditorGUILayout.PropertyField(changingClothingTarget);
        }
    }
}