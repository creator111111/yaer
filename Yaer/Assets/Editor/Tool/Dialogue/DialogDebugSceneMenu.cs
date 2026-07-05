using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 旧 GF 换场入口已废弃；DialogDebug 现为 Open Scene + Play 沙盒。
    /// </summary>
    public static class DialogDebugSceneMenu
    {
        [MenuItem("Tools/Dialogue/Enter DialogDebug Scene (Deprecated)")]
        private static void EnterDialogDebugSceneDeprecated()
        {
            EditorUtility.DisplayDialog(
                "DialogDebug（已废弃）",
                "DialogDebug 已改为解耦沙盒，不再通过 InitScene + GF 换场进入。\n\n" +
                "请使用：\n" +
                "1. Open Assets/GameRes/Scenes/DialogDebug.unity\n" +
                "2. Tools → Dialogue → Setup DialogDebug Scene（若尚未搭建）\n" +
                "3. Inspector 拖入对话 prefab → Play\n\n" +
                "日常测试无需本菜单。",
                "OK");
        }
    }
}
