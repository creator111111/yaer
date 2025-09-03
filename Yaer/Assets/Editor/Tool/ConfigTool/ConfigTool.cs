using System.Collections.Generic;
using System.IO;
using Game.Static.Utility.JsonReader;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EditorC.Tool.ConfigTool
{
    public class ConfigTool : EditorWindow
    {
        private static ConfigTool win;
        public TextAsset file;
        private static Dictionary<string, string> jsonData = new Dictionary<string, string>();
        private static List<string> jsonKeys = new List<string>();

        [MenuItem("Editor/ConfigTool")]
        public static void OpenWindow()
        {
            if (win == null)
            {
                win = GetWindow<ConfigTool>("ConfigTool");
                win.Show();
            }

            win.Focus();
        }

        public static void OpenFile(string path)
        {
            OpenWindow();

            LoadJson(path);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("配置文件: ", GUILayout.Width(60));
                file = EditorGUILayout.ObjectField(file, typeof(TextAsset), false) as TextAsset;

                if (GUILayout.Button("读取"))
                {
                    if (file)
                    {
                        LoadJson(AssetDatabase.GetAssetPath(file));
                    }
                }

                if (GUILayout.Button("编辑"))
                {
                    string scriptPath = AssetDatabase.GetAssetPath(file);
                    // 打开脚本的第1行
                    InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 1);
                }

                if (GUILayout.Button("创建"))
                {
                    // 绝对路径
                    string newJson = EditorUtility.SaveFilePanel("创建配置文件", Application.dataPath, "NewValueConfig", "json");

                    if (!string.IsNullOrEmpty(newJson))
                    {
                        File.Create(newJson).Close();
                        File.WriteAllText(newJson, "{}");

                        LoadJson(newJson);
                        AssetDatabase.Refresh();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            
            for (int i = 0; i < jsonKeys.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField($"[{i + 1}]", GUILayout.Width(60));
                    string newKey = EditorGUILayout.TextField(jsonKeys[i]);
                    if (newKey != jsonKeys[i] && jsonData.ContainsKey(newKey) == false)
                    {
                        string v = jsonData[jsonKeys[i]];
                        jsonData.Remove(jsonKeys[i]);
                        jsonData.Add(newKey, v);
                        jsonKeys = new List<string>(jsonData.Keys);
                    }
                    EditorGUILayout.LabelField("-", GUILayout.Width(10));
                    jsonData[jsonKeys[i]] = EditorGUILayout.TextField(jsonData[jsonKeys[i]]);
                    if (GUILayout.Button("删除"))
                    {
                        jsonData.Remove(jsonKeys[i]);
                        jsonKeys = new List<string>(jsonData.Keys);
                    }

                    if (GUILayout.Button("重置"))
                    {
                        
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("添加Key", GUILayout.Width(100)))
            {
                if (jsonData.ContainsKey("") == false)
                {
                    jsonData.Add("", "");
                    jsonKeys = new List<string>(jsonData.Keys);
                }
            }
            
            if (GUILayout.Button("保存"))
            {
                ValueConfigJsonReader reader = new ValueConfigJsonReader();
                reader.Save(AssetDatabase.GetAssetPath(file), jsonData);
                AssetDatabase.Refresh();
            }

            if (file != null && !file.name.EndsWith("ValueConfig"))
            {
                file = null;
            }
        }

        private static void LoadJson(string path)
        {
            // 读取json
            OpenWindow();

            // 相对Resources路径
            path = "Config/ValueConfig/" + Path.GetFileName(path);
            path = path.Replace(".json", "");
            var textAsset = Resources.Load<TextAsset>(path);
            win.file = textAsset;

            ValueConfigJsonReader reader = new ValueConfigJsonReader();
            reader.Read(path);

            jsonData = reader.GetAllKeys();
            jsonKeys = new List<string>(jsonData.Keys);
        }
    }
}