using System;
using System.Collections.Generic;
using Game.GameRuntime.UI.FormLogic.KnockbackTestPanel;
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
            AddComponent<AddDateEditorComponentGT>();
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

    /// <summary>
    /// 受击/击退调试窗口：与 GameManagerTool 同文件、同命名空间，确保与已有 Editor 菜单项一起编译。
    /// Unity 通过 MenuItem("Editor/...") 把入口挂到顶部菜单「Editor」下；本类继承 EditorWindow，在 OnGUI 里画参数并调用 KnockbackTestRunner.TryApply。
    /// </summary>
    public class KnockbackTestEditorWindow : EditorWindow
    {
        private KnockbackTestRunner.TestMode _mode = KnockbackTestRunner.TestMode.FullNormal;
        private float _dirX = 1f;
        private float _dirY;
        private float _breakWidth = 2f;
        private float _breakHight = 0.5f;
        private float _breakTime = 0.5f;
        private float _bounceFrequency = 2f;
        private string _lastMessage;

        [MenuItem("Editor/受击击退测试")]
        public static void OpenKnockbackTest()
        {
            var win = GetWindow<KnockbackTestEditorWindow>("受击击退测试");
            win.minSize = new Vector2(420, 360);
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("需在运行模式（Play）下使用，且场景已生成玩家。", EditorStyles.wordWrappedLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("请先进入 Play 模式，再点击执行。", MessageType.Info);
            }

            EditorGUILayout.Space(8);
            _mode = (KnockbackTestRunner.TestMode)EditorGUILayout.EnumPopup("测试模式", _mode);

            EditorGUILayout.Space(4);
            _dirX = EditorGUILayout.FloatField("dirPos.x（伤害来源）", _dirX);
            _dirY = EditorGUILayout.FloatField("dirPos.y", _dirY);
            _breakWidth = EditorGUILayout.FloatField("breakWidth", _breakWidth);
            _breakHight = EditorGUILayout.FloatField("breakHight", _breakHight);
            _breakTime = EditorGUILayout.FloatField("breakTime（击退时长）", _breakTime);
            _bounceFrequency = EditorGUILayout.FloatField("bounceFrequency", _bounceFrequency);

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Normal：需 breakHight>0 才走 KnockBack；Break：击飞不走 KnockBack 曲线；纯击退：仅 KnockBackComponent。",
                MessageType.Info);

            EditorGUILayout.Space(6);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("执行受击/击退", GUILayout.Height(36)))
                {
                    _lastMessage = null;
                    var dir = new Vector2(_dirX, _dirY);
                    if (KnockbackTestRunner.TryApply(_mode, dir, _breakWidth, _breakHight, _breakTime, _bounceFrequency,
                            out var err))
                    {
                        _lastMessage = "已执行。";
                    }
                    else
                    {
                        _lastMessage = err;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_lastMessage))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.HelpBox(_lastMessage,
                    _lastMessage.StartsWith("已") ? MessageType.Info : MessageType.Warning);
            }
        }
    }
}