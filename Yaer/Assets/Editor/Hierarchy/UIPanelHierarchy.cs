using UnityEditor;
using UnityEngine;

namespace EditorC.Hierarchy
{
    [InitializeOnLoad]
    public class UIPanelHierarchy
    {
        static UIPanelHierarchy()
        {
            // EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj != null)
            {
                // 定义文本显示区域
                Rect buttonRect = new Rect(selectionRect.xMax - 80, selectionRect.y, 80, selectionRect.height);

                
                // 自定义文字样式
                GUIStyle textStyle = new GUIStyle(GUIStyle.none)
                {
                    normal =
                    {
                        textColor = Color.white,
                    }, // 正常状态文字为白色
                    alignment = TextAnchor.MiddleRight,  // 居中对齐
                };
                textStyle.fontSize = 11;
                
                // 判断鼠标是否悬停在该区域
                bool isHovered = buttonRect.Contains(Event.current.mousePosition);
                // 根据悬停状态修改文字颜色
                textStyle.normal.textColor = isHovered ? Color.yellow : Color.white;
                
                if (GUI.Button(buttonRect, "REF", textStyle))
                {
                    Debug.Log($"Pinged: {obj.name}");
                    EditorGUIUtility.PingObject(obj);
                }
            }
        }
    }
}