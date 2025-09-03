using System;
using GameFramework.Editor.UI;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor.GFExtend.ComponentSystemEditor
{
    [CustomEditor(typeof(ComponentSystemMono), true)]
    public class ComponentSystemMonoInspector : UnityEditor.Editor
    {
        private ComponentSystemMono script;
        protected Type selectedType;

        protected virtual void OnEnable()
        {
            script = (ComponentSystemMono)target;
            selectedType = typeof(BaseGFComponentMono);
            
            script.CreateComponentRoot();
        }

        public override void OnInspectorGUI()
        {
            EditorUI.DrawScriptAsset(serializedObject);

            // 显示组件列表
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);

            script.RefreshComponents();

            var components = script.GetAddComponents();
            for (var i = 0; i < components.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var componentMono = components[i];
                // 显示组件名称
                EditorGUILayout.ObjectField(componentMono, typeof(BaseGFComponentMono), false);

                // 显示优先级字段
                EditorGUILayout.LabelField(componentMono.Priority.ToString(), GUILayout.Width(20));

                // 拖拽排序按钮
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    Undo.RecordObject(script, "Move Component Up");
                    componentMono.Priority++;
                    script.SortComponent();
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    Undo.RecordObject(script, "Move Component Down");
                    componentMono.Priority--;
                    script.SortComponent();
                }

                // 删除组件按钮
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    Undo.RecordObject(script, "Remove Component");
                    script.RemoveComponent(componentMono);
                }

                EditorGUILayout.EndHorizontal();
            }

            // 添加组件按钮
            if (GUILayout.Button("Add Component"))
            {
                Undo.RecordObject(script, "Add Component");
                FindComponentEditor.OpenWindow(script, selectedType);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}