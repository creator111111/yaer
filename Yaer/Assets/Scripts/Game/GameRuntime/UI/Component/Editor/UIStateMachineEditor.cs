using GameFramework.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.UI.Component.Editor
{
    [CustomEditor(typeof(UIStateMachine))]
    public class UIStateMachineEditor : UnityEditor.Editor
    {
        private SerializedProperty currentStateName;

        private string newStateName = "New State";
        private UIStateMachine script;
        private SerializedProperty stateNames;
        private int index;

        private void OnEnable()
        {
            script = (UIStateMachine)target;

            script.RefreshStateNames();

            currentStateName = serializedObject.FindProperty("currentStateName");
            stateNames = serializedObject.FindProperty("stateNames");
            
            index = script.GetStateNames().IndexOf(script.InitStateName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorUI.DrawScriptAsset(serializedObject);

            // 默认状态
            if (script.GetStateNames().Count > 0)
            {
                var newIndex = EditorGUILayout.Popup("默认状态：", index, script.GetStateNames().ToArray());
                if (newIndex != index)
                {
                    index = newIndex;
                    script.SetDefaultState(script.GetStateNames()[index]);
                } 
            }

            // 显示每个状态
            EditorGUILayout.LabelField("Now States: " + currentStateName.stringValue, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (stateNames != null)
                for (var i = 0; i < stateNames.arraySize; i++)
                {
                    // 获取当前状态的名字
                    var stateNameProp = stateNames.GetArrayElementAtIndex(i);

                    // 显示当前状态的名字
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(stateNameProp.stringValue)) script.ChangeTo(stateNameProp.stringValue);

                    // 删除按钮
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        // Remove state from dictionary
                        script.RemoveState(stateNameProp.stringValue);

                        // Update serialized object
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();
                }

            // 添加新状态的输入框和按钮
            EditorGUILayout.BeginHorizontal();
            newStateName = EditorGUILayout.TextField(newStateName);
            if (GUILayout.Button("Add State"))
            {
                script.RegisterState(newStateName);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();

            // 如果界面有修改，则标记预制体被修改，从而可以保存
            if (GUI.changed)
            {
                // 标记当前目标对象已修改
                EditorUtility.SetDirty(target);
                // 对于预制体实例，记录修改以便保存
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}