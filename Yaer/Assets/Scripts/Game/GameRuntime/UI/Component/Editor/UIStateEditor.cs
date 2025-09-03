using GameFramework.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.UI.Component.Editor
{
    [CustomEditor(typeof(UIState))]
    public class UIStateEditor : UnityEditor.Editor
    {
        private int index;
        private UIState script;

        private void OnEnable()
        {
            script = (UIState)target;
            script.Init();

            if (script.isInit == false) return;

            index = script.stateMachine.GetStateNames().IndexOf(script.currentStateName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorUI.DrawScriptAsset(serializedObject);

            script.stateMachine = EditorGUILayout.ObjectField("StateMachine", script.stateMachine, typeof(UIStateMachine), true) as UIStateMachine;

            if (script.stateMachine != null && script.isInit == false) script.Init();

            if (script.isInit == false) return;

            var newIndex = EditorGUILayout.Popup("状态：", index, script.stateMachine.GetStateNames().ToArray());
            if (newIndex != index)
            {
                index = newIndex;
                script.Enter(script.stateMachine.GetStateNames()[index]);
            }

            EditorUI.DrawFgx();

            var data = script.dataList.Find(x => x.stateName == script.currentStateName);
            if (data != null)
            {
                data.controlActive = EditorGUILayout.Toggle("控制激活", data.controlActive);

                if (data.controlActive)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        data.isActive = EditorGUILayout.Toggle("激活", data.isActive);
                        var disabled = !data.isActive;
                        disabled = EditorGUILayout.Toggle("禁用", disabled); // 直接用反转的值, 简化逻辑.
                        data.isActive = !disabled;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorUI.DrawFgx();

                data.controlTsf = EditorGUILayout.Toggle("控制Tsf", data.controlTsf);

                if (data.controlTsf)
                {
                    data.position = EditorGUILayout.Vector3Field("位置", data.position);
                    data.size = EditorGUILayout.Vector2Field("大小", data.size);
                    data.rotation = EditorGUILayout.Vector3Field("旋转", data.rotation);
                    data.scale = EditorGUILayout.Vector3Field("缩放", data.scale);
                }

                EditorUI.DrawFgx();

                // data.controlCanvasGroup = EditorGUILayout.Toggle("控制CanvasGroup", data.controlCanvasGroup);
                // var cg = EditorGUILayout.ObjectField("script", script.GetComponent<CanvasGroup>(), typeof(CanvasGroup), true);
                // if (cg is null)
                // {
                //     script.gameObject.AddComponent<CanvasGroup>();
                // }
                // if (data.controlCanvasGroup)
                // {
                //     data.alpha = EditorGUILayout.Slider("Alpha", data.alpha, 0, 1);
                // }
                
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
}