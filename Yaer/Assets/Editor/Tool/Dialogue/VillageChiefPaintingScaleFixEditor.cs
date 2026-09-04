#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 村长家门口/继续对话：把嵌套 <c>ChiefPainting</c> 实例 Scale 钉为 0.65（母体默认仍 0.32）。
    /// 菜单：Tools / Dialogue / Fix Village 村长立绘 Scale 0.65
    /// </summary>
    /// <remarks>
    /// 原因（0901）：门口 Scale Override 曾指向旧 RectTransform fileID 断链回落 0.32；
    /// 继续对话曾只 Nudge X 未写 Scale。用 LoadPrefabContents 写实例可生成对准现行 fileID 的 Override。
    /// 替代方案：手改 YAML fileID——易漏其它断链属性，不采用。
    /// </remarks>
    public static class VillageChiefPaintingScaleFixEditor
    {
        private const string MenuPath = "Tools/Dialogue/Fix Village 村长立绘 Scale 0.65";
        private const string DoorPrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab";
        private const string ContinuePrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab";
        private const string AutoRequestFileName = "ChiefPaintingScaleFix.request";

        [InitializeOnLoadMethod]
        private static void AutoFixFromRequestFile()
        {
            EditorApplication.delayCall += TryConsumeAutoFixRequest;
        }

        private static void TryConsumeAutoFixRequest()
        {
            var abs = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "Library", AutoRequestFileName));
            if (!File.Exists(abs))
            {
                return;
            }

            try
            {
                File.Delete(abs);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[ChiefScaleFix] 无法删除自动请求文件：" + ex.Message);
                return;
            }

            Debug.Log("[ChiefScaleFix] 检测到 Library/ChiefPaintingScaleFix.request，自动执行…");
            FixFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void FixFromMenu()
        {
            var doorOk = FixPrefabScale(DoorPrefabPath);
            var continueOk = FixPrefabScale(ContinuePrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (doorOk && continueOk)
            {
                Debug.Log("[ChiefScaleFix] 门口 + 继续 Prefab ChiefPainting Scale 已钉 0.65。");
            }
        }

        /// <summary>
        /// 打开对话 Prefab，只改 <c>ChiefPainting</c> localScale；雅/古不动；母体默认不动。
        /// </summary>
        private static bool FixPrefabScale(string prefabPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                Debug.LogError("[ChiefScaleFix] Prefab 不存在：" + prefabPath);
                return false;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var chief = FindDeepChild(root.transform, "ChiefPainting");
                if (chief == null)
                {
                    Debug.LogError("[ChiefScaleFix] 未找到 ChiefPainting：" + prefabPath);
                    return false;
                }

                var target = new Vector3(
                    VillageChiefDialoguePortraitLayout.ChiefPaintingScale,
                    VillageChiefDialoguePortraitLayout.ChiefPaintingScale,
                    VillageChiefDialoguePortraitLayout.ChiefPaintingScale);
                if (chief.localScale != target)
                {
                    chief.localScale = target;
                    EditorUtility.SetDirty(chief);
                }

                // 顺带把 Nudge X 钉回定稿，避免门口旧断链 Override 失效后脚位漂移
                var rt = chief as RectTransform;
                if (rt != null)
                {
                    var p = rt.anchoredPosition;
                    if (!Mathf.Approximately(p.x, VillageChiefDialoguePortraitLayout.ChiefPaintingPosX))
                    {
                        p.x = VillageChiefDialoguePortraitLayout.ChiefPaintingPosX;
                        rt.anchoredPosition = p;
                        EditorUtility.SetDirty(rt);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                var s = VillageChiefDialoguePortraitLayout.ChiefPaintingScale;
                Debug.Log(
                    $"[ChiefScaleFix] {prefabPath} → ChiefPainting.localScale=({s},{s},{s})",
                    chief);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == name)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindDeepChild(parent.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
