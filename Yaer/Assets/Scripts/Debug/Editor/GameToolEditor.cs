using System;
using System.Collections.Generic;
using GameDebug;
using GameDebug.Editor.Components;
using GameFramework.CoreExtend.Component;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.GameTool
{
    public class GameManagerToolEditor : EditorWindow
    {
        private ComponentSystem componentSystem;
        private string logMsg;

        private Dictionary<BaseGTEditorComponent, bool> componentState = new Dictionary<BaseGTEditorComponent, bool>();

        [MenuItem("Editor/GameManagerTool %G")]
        public static void OpenWindow()
        {
            GameManagerToolEditor window = GetWindow<GameManagerToolEditor>("GameManagerTool");
            window.position = new Rect(0, 0, 600, 400);
            window.Show();
        }

        private int selectScene = 0;
        private GameDebug.GameTool gameTool;

        private void OnEnable()
        {
            if (gameTool == null) gameTool = new GameDebug.GameTool();

            componentSystem = new ComponentSystem();
            InitAddComponents();
            componentSystem.InitComponents();
        }

        private void InitAddComponents()
        {
            AddComponent<AddItemEditorComponent>();
            AddComponent<ChangeClothesEditorComponentGT>();
        }

        private T GetComponent<T>() where T : BaseGTEditorComponent
        {
            return componentSystem.GetComponent<T>();
        }

        private void AddComponent<T>() where T : BaseGTEditorComponent, new()
        {
            var component = new T();
            component.SetGameTool(gameTool);
            componentSystem.AddComponent(component);
            componentState.Add(component, false);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(gameTool.LogMsg);

            EditorGUILayout.Space();
            if (GUILayout.Button("跳过开头"))
            {
                gameTool.SkipInitScene();
            }

            EditorGUILayout.BeginHorizontal();
            selectScene = EditorGUILayout.Popup(selectScene, gameTool.SceneNames);
            if (GUILayout.Button("跳转到目标场景"))
            {
                gameTool.SkipScene(gameTool.SceneNames[selectScene]);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                // 获取所有组件列表
                List<BaseGTEditorComponent> keys = new List<BaseGTEditorComponent>(componentState.Keys);
                // 遍历每个组件，显示 Toggle
                for (int i = 0; i < keys.Count; i++)
                {
                    var comp = keys[i];
                    // 显示组件名称，Toggle 选中状态取自字典
                    bool selected = EditorGUILayout.ToggleLeft(comp.name, componentState[comp], GUILayout.Width(50));
                    // 如果选中状态发生变化，并且现在被选中，则取消其他选项
                    if (selected && !componentState[comp])
                    {
                        // 将所有组件状态置为 false
                        foreach (var key in keys)
                        {
                            componentState[key] = false;
                        }

                        // 将当前组件置为 true
                        componentState[comp] = true;
                    }
                    // 如果当前组件未被选中，则保持状态为 false
                    else if (!selected)
                    {
                        componentState[comp] = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 可选：显示当前选中的组件名称
            foreach (var kvp in componentState)
            {
                if (kvp.Value)
                {
                    kvp.Key.OnGUI();
                }
            }
        }
    }
}