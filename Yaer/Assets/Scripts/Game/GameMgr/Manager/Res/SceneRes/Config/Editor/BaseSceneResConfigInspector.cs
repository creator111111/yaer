using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config.Editor
{
    [CustomEditor(typeof(BaseSceneResConfig), true)]
    public class BaseSceneResConfigInspector : UnityEditor.Editor
    {
        private BaseSceneResConfig script;
        private SerializedProperty assetInfos;
        private ReorderableList reorderableList;

        private void OnEnable()
        {
            script = (BaseSceneResConfig)serializedObject.targetObject;
            assetInfos = serializedObject.FindProperty("assetInfos");

            reorderableList = new ReorderableList(serializedObject, assetInfos, true, true, true, true);

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Asset Infos");
            };

            reorderableList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = assetInfos.GetArrayElementAtIndex(index);
                SerializedProperty assetTypeProp = element.FindPropertyRelative("assetType");
                SerializedProperty pathProp = element.FindPropertyRelative("path");

                rect.y += 2; // Add a little space
                float lineHeight = EditorGUIUtility.singleLineHeight + 2; // Add space between lines

                Rect assetTypeRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                Rect pathRect = new Rect(rect.x, rect.y + lineHeight, rect.width, EditorGUIUtility.singleLineHeight);

                EditorGUIUtility.labelWidth = 70f; // Reduce label width
                
                EditorGUI.PropertyField(assetTypeRect, assetTypeProp, new GUIContent("Asset Type"));
                EditorGUI.PropertyField(pathRect, pathProp, new GUIContent("Path"));
            };

            reorderableList.elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight * 2 + 6; // Two lines + some space
            };

            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                // Optionally set default values here, e.g., element.FindPropertyRelative("path").stringValue = "default path";
            };

            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("Warning!",
                    "Are you sure you want to delete the element?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneName"));

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}