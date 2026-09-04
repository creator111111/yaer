#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// KenMuNi1 摆 <c>VillageDepthY_Min</c> / <c>VillageDepthY_Max</c>（0903 F_D1）。
    /// 菜单：Tools / Scene / Setup KenMuNi1 巨树纵深标尺 DepthY
    /// </summary>
    /// <remarks>
    /// 原因（0903 DepthGap）：场景无标尺时 Prefab maxY=8，楼梯落点 Y≈41 被 Clamp，与 WalkArea2 撕扯卡死。
    /// Max 须覆盖 WalkArea2 上沿（约 45.4）；Min 对齐 Prefab −20 覆盖 1 楼地面带。
    /// 替代方案：全局改 Player Prefab depthYMax——放开所有未摆标尺村场景，否决。
    /// 禁止改 VillageWalkArea2 几何。
    /// </remarks>
    public static class KenMuNi1VillageDepthYSetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup KenMuNi1 巨树纵深标尺 DepthY";
        private const string ScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity";
        private const string AutoRequestFileName = "KenMuNi1VillageDepthYSetup.request";

        private const string DepthYMinName = "VillageDepthY_Min";
        private const string DepthYMaxName = "VillageDepthY_Max";

        /// <summary>对齐 Prefab 默认 min，覆盖 1 楼 VillageWalkArea（Y≈−6）。</summary>
        private const float DepthMinWorldY = -20f;

        /// <summary>覆盖 WalkArea2 上沿约 45.4，留余量防贴边 Clamp。</summary>
        private const float DepthMaxWorldY = 46f;

        [InitializeOnLoadMethod]
        private static void AutoSetupFromRequestFile()
        {
            EditorApplication.delayCall += TryConsumeAutoSetupRequest;
        }

        private static void TryConsumeAutoSetupRequest()
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
                Debug.LogWarning("[KenMuNi1DepthY] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[KenMuNi1DepthY] 检测到 Library/" + AutoRequestFileName + "，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[KenMuNi1DepthY] 无法打开场景：" + ScenePath);
                return;
            }

            // 优先挂到含 VillageWalkArea 的根 Map（世界原点），避免命中嵌套/禁用的同名 Map
            Transform map = FindMapHostingWalkArea(scene);
            if (map == null)
            {
                map = FindNamedInScene(scene, "Map");
            }

            if (map == null)
            {
                Debug.LogError("[KenMuNi1DepthY] 场景根下未找到 Map。");
                return;
            }

            EnsureDepthMarker(map, DepthYMinName, DepthMinWorldY);
            EnsureDepthMarker(map, DepthYMaxName, DepthMaxWorldY);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[KenMuNi1DepthY] 已摆 " + DepthYMinName + "=" + DepthMinWorldY
                + " / " + DepthYMaxName + "=" + DepthMaxWorldY
                + "（父=" + map.name + "）。未改 WalkArea2。",
                map);
        }

        /// <summary>幂等：按名找或新建空物体，世界 Y 钉死；X/Z=0。</summary>
        private static void EnsureDepthMarker(Transform map, string name, float worldY)
        {
            Transform existing = null;
            for (int i = 0; i < map.childCount; i++)
            {
                Transform child = map.GetChild(i);
                if (child.name == name)
                {
                    existing = child;
                    break;
                }
            }

            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(map, false);
                int mapLayer = LayerMask.NameToLayer("Map");
                go.layer = mapLayer >= 0 ? mapLayer : 8;
            }

            go.transform.position = new Vector3(0f, worldY, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(go);
        }

        /// <summary>根下名为 Map 且直接子级含 VillageWalkArea 的 Transform。</summary>
        private static Transform FindMapHostingWalkArea(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != "Map")
                {
                    continue;
                }

                for (int i = 0; i < root.transform.childCount; i++)
                {
                    if (root.transform.GetChild(i).name == "VillageWalkArea"
                        || root.transform.GetChild(i).name == "VillageWalkArea2")
                    {
                        return root.transform;
                    }
                }
            }

            return null;
        }

        private static Transform FindNamedInScene(
            UnityEngine.SceneManagement.Scene scene, string objectName)
        {
            if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindRecursive(Transform tr, string objectName)
        {
            if (tr.name == objectName)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                Transform child = FindRecursive(tr.GetChild(i), objectName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
#endif
