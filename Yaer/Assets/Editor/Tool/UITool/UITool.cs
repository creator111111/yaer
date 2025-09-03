using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.UITool
{
    public class UITool : UnityEditor.Editor
    {
        [MenuItem("GameObject/Editor/UITool/GenerateBindUIComponent", false, 1)]
        public static void GenerateBindUIComponent()
        {
            var bindUI = new BindUI();
            bindUI.Generate(Selection.activeGameObject);
        }

        [MenuItem("GameObject/Editor/UITool/Add_ref", false, 1)]
        public static void RenameSelectedObjects()
        {
            // 获取选中的所有 GameObject
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length <= 0) return;

            // 遍历每个选中的 GameObject
            foreach (GameObject obj in selectedObjects)
            {
                // 检查是否已经添加了 _ref 后缀
                if (!obj.name.EndsWith("_ref"))
                {
                    // 添加 _ref 后缀到对象的名称
                    obj.name += "_ref";
                }
                
                // 刷新编辑器以显示名称更改
                EditorUtility.SetDirty(obj);
            }
            // 确保 Unity 编辑器界面更新
            Debug.Log("选中的 GameObject 名称已添加 _ref 后缀");
        }
    }
}