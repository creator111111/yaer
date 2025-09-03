using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor.GFExtend.ComponentSystemEditor
{
    public class FindComponentEditor : EditorWindow
    {
        private static List<Type> allComponents = new List<Type>(); // 所有可用的组件类型
        private ComponentSystemMono script;
        private Vector2 scrollPosition = Vector2.zero; // 滚动位置
        private string searchQuery = ""; // 搜索关键字

        private void OnGUI()
        {
            GUILayout.Label("Search Components", EditorStyles.boldLabel);

            // 搜索框
            searchQuery = EditorGUILayout.TextField("Search:", searchQuery);

            // 过滤组件列表
            var filteredComponents = string.IsNullOrEmpty(searchQuery)
                ? allComponents
                : allComponents.Where(t => t.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            // 显示组件列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var componentType in filteredComponents)
                if (GUILayout.Button(componentType.Name, GUILayout.Height(20)))
                    AddComponentToSelectedGameObject(componentType);

            EditorGUILayout.EndScrollView();
        }

        public static void OpenWindow(ComponentSystemMono script, Type cpnType)
        {
            var w = GetWindow<FindComponentEditor>("Add Component Search");
            w.script = script;
            // 获取所有 MonoBehaviour 类型
            allComponents = GetAllMonoBehaviours(cpnType);
        }

        private static List<Type> GetAllMonoBehaviours(Type cpnType)
        {
            // 获取当前程序集中所有的 MonoBehaviour 类型
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(cpnType) && !type.IsAbstract)
                .ToList();
        }

        private void AddComponentToSelectedGameObject(Type componentType)
        {
            // 添加组件
            // Undo.AddComponent(selectedObject, componentType);
            script.AddComponent(componentType);

            // 刷新所有编辑器窗口
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                if (window != null)
                    window.Repaint();
        }
    }
}