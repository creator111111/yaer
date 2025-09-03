using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor.UI
{
    public static class EditorUI
    {
        public static void DrawScriptAsset(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Enabled"));

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

        }
        
        public static void DrawBoltLabel(string text, Color color)
        {
            EditorGUILayout.LabelField(text, new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = color
                },
                fontStyle = FontStyle.Bold
            });
        }
        
        public static void DrawFoldout(string title, ref bool open, Action action)
        {
            open = EditorGUILayout.Foldout(open, title);
            if (open)
            {
                action?.Invoke();
            }
        }
        
        public static void DrawFgx()
        {
            EditorGUILayout.LabelField(new string('-', (int)EditorGUIUtility.currentViewWidth));
        }
    }
}