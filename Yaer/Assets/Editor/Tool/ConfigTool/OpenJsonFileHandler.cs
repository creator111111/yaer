using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.ConfigTool
{
    [InitializeOnLoad]
    public static class OpenJsonFileHandler
    {
        // 静态构造函数，确保项目启动时注册
        static OpenJsonFileHandler()
        {
            // 注册 Project 窗口的 GUI 绘制事件
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        // 监听 Project 窗口的GUI事件
        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            // 捕获当前的鼠标事件
            Event e = Event.current;
        
            // 检查是否是鼠标双击事件
            if (e != null && e.type == EventType.MouseDown && e.clickCount == 2 && selectionRect.Contains(e.mousePosition))
            {
                // 根据 GUID 获取资源路径
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
                // 判断是否是 JSON 文件
                if (assetPath.EndsWith("ValueConfig.json"))
                {
                    // 阻止默认打开行为
                    e.Use();

                    // 打开自定义编辑器窗口
                    ConfigTool.OpenFile(assetPath);
                }
            }
        }
    }
}