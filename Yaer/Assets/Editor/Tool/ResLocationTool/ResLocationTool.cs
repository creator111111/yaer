using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorC.Tool.ResLocationTool
{
    public class ResLocationTool : EditorWindow
    {
        private string cachePath;
        private SaveData saveData = new SaveData();

        private Object artFolder;
        private Object prefabFolder;
        private Object scriptFolder;
        private Object sceneFolder;
        private Object uiPrefabFolder;
        private Object excelFolder;
        private Object configFolder;
        
        
        
        [MenuItem("Editor/ResLocationTool")]
        public static void ShowWindow()
        {
            var window = GetWindow<ResLocationTool>("ResLocationTool");
            window.minSize = new Vector2(100, 100);
            
        }

        private void OnEnable()
        {
            cachePath = Application.dataPath + "/Editor/Tool/ResLocationTool/Cache.json";
            var data = Load(cachePath);
            artFolder = AssetDatabase.LoadAssetAtPath<Object>(data.artPath);
            prefabFolder = AssetDatabase.LoadAssetAtPath<Object>(data.prefabPath);
            scriptFolder = AssetDatabase.LoadAssetAtPath<Object>(data.scriptPath);
            sceneFolder = AssetDatabase.LoadAssetAtPath<Object>(data.scenePath);
            uiPrefabFolder = AssetDatabase.LoadAssetAtPath<Object>(data.uiPrefabPath);
            configFolder = AssetDatabase.LoadAssetAtPath<Object>(data.configPath);
            excelFolder = AssetDatabase.LoadAssetAtPath<Object>(data.excelPath);
        }

        private void OnDestroy()
        {
            Save(cachePath);
        }

        private void OnDisable()
        {
            Save(cachePath);
        }

        private void OnGUI()
        {
            artFolder = EditorGUILayout.ObjectField("ArtFolder",artFolder, typeof(Object), false);
            prefabFolder = EditorGUILayout.ObjectField("PrefabFolder",prefabFolder, typeof(Object), false);
            scriptFolder = EditorGUILayout.ObjectField("ScriptFolder", scriptFolder, typeof(Object), false);
            sceneFolder = EditorGUILayout.ObjectField("SceneFolder", sceneFolder, typeof(Object), false);
            uiPrefabFolder = EditorGUILayout.ObjectField("UIPrefabFolder", uiPrefabFolder, typeof(Object), false);
            configFolder = EditorGUILayout.ObjectField("ConfigFolder", configFolder, typeof(Object), false);
            excelFolder = EditorGUILayout.ObjectField("ExcelConfigFolder", excelFolder, typeof(Object), false);
        }
        
        private static void SetExpandedRecursive(EditorWindow projectWindow, Object folderObject, bool expand)
        {
            var method = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser")
                ?.GetMethod("SetExpandedRecursive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(projectWindow, new object[] { folderObject.GetInstanceID(), expand });
            }
            else
            {
                Debug.LogError("Could not find SetExpandedRecursive method.");
            }
        }
        
        private EditorWindow GetProjectWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                if (window.GetType().Name == "ProjectBrowser")
                {
                    return window;
                }
            }
            Debug.LogError("Project window not found!");
            return null;
        }

        //-----------------------------------------------------------------------------------
        // 持久化
        private void Save(string path)
        {
            saveData.artPath = AssetDatabase.GetAssetPath(artFolder);
            saveData.prefabPath = AssetDatabase.GetAssetPath(prefabFolder);
            saveData.scriptPath = AssetDatabase.GetAssetPath(scriptFolder);
            saveData.scenePath = AssetDatabase.GetAssetPath(sceneFolder);
            saveData.uiPrefabPath = AssetDatabase.GetAssetPath(uiPrefabFolder);
            saveData.configPath = AssetDatabase.GetAssetPath(configFolder);
            saveData.excelPath = AssetDatabase.GetAssetPath(excelFolder);

            // json保存
            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(path, json);
            // AssetDatabase.Refresh();
        }
        
        private static SaveData Load(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }

            return new SaveData();
        }

    }
}