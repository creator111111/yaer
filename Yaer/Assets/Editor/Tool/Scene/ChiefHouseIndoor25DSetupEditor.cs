#if UNITY_EDITOR
using System.IO;
using Game.GameRuntime.Entities.Component.Physics;
using Game.Static.Name.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// 村长家室内划区 2.5D + 楼梯树屋化：在 <c>Village_Chief_House</c> 摆
    /// <c>VillageWalkArea</c> / DepthY 标尺 / 方案1障碍 / 可选 DepthZone。
    /// 菜单：Tools / Scene / Setup Chief House 室内划区2.5D与楼梯
    /// </summary>
    /// <remarks>
    /// 原因（0901）：脚本白名单已扩 Chief_House，但无 WalkArea 会整屋乱飞 Y。
    /// 合层「楼梯」仅美术锚点；本菜单旁挂空物体，不改合层 SR。
    /// Q7：多边形尽量窄（进门条带→楼梯→小平台），验收后可在 Scene 视图拖点微调。
    /// </remarks>
    public static class ChiefHouseIndoor25DSetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup Chief House 室内划区2.5D与楼梯";
        private const string ScenePath = "Assets/GameRes/Scenes/Village_Chief_House.unity";
        private const string AutoRequestFileName = "ChiefHouseIndoor25DSetup.request";

        private const string WalkAreaName = "VillageWalkArea";
        private const string DepthYMinName = "VillageDepthY_Min";
        private const string DepthYMaxName = "VillageDepthY_Max";
        private const string ObstaclesRootName = "VillageWalkObstacles";
        private const string OuterRailName = "Obstacle_OuterRail";
        private const string InnerRailName = "Obstacle_InnerRail";
        private const string DepthZoneName = "DepthZone_StairsUpper";

        /// <summary>合层根世界位（场景 Design 下实例）。</summary>
        private static readonly Vector3 HouseCompositeWorld = new Vector3(-13.92f, -7.7f, 0f);

        /// <summary>合层「楼梯」本地位 → 世界锚点（对齐斜面画多边形）。</summary>
        private static readonly Vector3 StairsLocal = new Vector3(5.465f, 5.495f, 0f);

        /// <summary>进门落点（须落入 WalkArea）。</summary>
        private static readonly Vector2 EnterFromVillage = new Vector2(17.42f, -3.65f);

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
                Debug.LogWarning("[ChiefHouse25D] 无法删除自动请求文件：" + ex.Message);
                return;
            }

            Debug.Log("[ChiefHouse25D] 检测到 Library/" + AutoRequestFileName + "，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[ChiefHouse25D] 无法打开场景：" + ScenePath);
                return;
            }

            Transform map = FindNamedInScene(scene, "Map");
            if (map == null)
            {
                Debug.LogError("[ChiefHouse25D] 场景根下未找到 Map。");
                return;
            }

            Vector3 stairsWorld = HouseCompositeWorld + StairsLocal;
            // 纵深标尺：覆盖落点 Y≈-3.65 到楼梯顶（sprite 半高约 5.3）
            float depthMinY = EnterFromVillage.y - 1.6f; // ≈ -5.25
            float depthMaxY = stairsWorld.y + 5.5f;      // ≈ 3.3

            EnsureDepthMarker(map, DepthYMinName, depthMinY);
            EnsureDepthMarker(map, DepthYMaxName, depthMaxY);
            EnsureWalkArea(map, stairsWorld);
            EnsureObstacles(map, stairsWorld);
            EnsureDepthZone(map, stairsWorld);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[ChiefHouse25D] 已布置 WalkArea / DepthY / 障碍 / DepthZone。" +
                "请在 Scene 视图按合层「楼梯」微调多边形与障碍边。",
                map);
        }

        /// <summary>
        /// 窄条带：进门垫 → 地面走廊 → 楼梯斜面 → 上层小平台。
        /// 点集为世界坐标（WalkArea 在 Map 原点）；验收后可拖 Polygon 点。
        /// </summary>
        private static void EnsureWalkArea(Transform map, Vector3 stairsWorld)
        {
            var go = EnsureChild(map, WalkAreaName);
            // 与 KenMuNi1 VillageWalkArea 一致：Layer=Map(8)
            int mapLayer = LayerMask.NameToLayer("Map");
            go.layer = mapLayer >= 0 ? mapLayer : 8;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var poly = go.GetComponent<PolygonCollider2D>();
            if (poly == null)
            {
                poly = go.AddComponent<PolygonCollider2D>();
            }

            poly.isTrigger = true;
            // 进门 (17.42,-3.65) 必须在内；楼梯锚约 (-8.46,-2.21)
            float sx = stairsWorld.x;
            float sy = stairsWorld.y;
            poly.pathCount = 1;
            poly.SetPath(0, new[]
            {
                new Vector2(EnterFromVillage.x + 1.1f, EnterFromVillage.y - 1.35f), // 进门右下
                new Vector2(EnterFromVillage.x + 1.1f, EnterFromVillage.y + 1.15f), // 进门右上
                new Vector2(4.0f, EnterFromVillage.y + 1.15f),                     // 走廊上沿
                new Vector2(sx + 4.5f, sy + 0.8f),                                 // 楼梯腰上
                new Vector2(sx - 2.2f, sy + 4.6f),                                 // 上层平台顶
                new Vector2(sx - 5.0f, sy + 3.6f),                                 // 上层左
                new Vector2(sx - 4.5f, sy + 1.2f),                                 // 上层底
                new Vector2(sx + 1.5f, sy - 2.0f),                                 // 楼梯腰下
                new Vector2(4.0f, EnterFromVillage.y - 1.35f),                     // 走廊下沿
            });
        }

        private static void EnsureDepthMarker(Transform map, string name, float worldY)
        {
            var go = EnsureChild(map, name);
            go.transform.position = new Vector3(0f, worldY, 0f);
        }

        private static void EnsureObstacles(Transform map, Vector3 stairsWorld)
        {
            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                Debug.LogError("[ChiefHouse25D] 缺少 Layer VillageWalkObstacle。");
                return;
            }

            var root = EnsureChild(map, ObstaclesRootName).transform;
            root.localPosition = Vector3.zero;

            // 外栏杆世界 ≈ 合层 local(5.665,4.45) → 楼梯下沿挡位
            Vector3 outerWorld = HouseCompositeWorld + new Vector3(5.665f, 4.45f, 0f);
            EnsureObstacleBox(root, OuterRailName, obstacleLayer,
                new Vector3(outerWorld.x - 0.4f, outerWorld.y - 0.3f, 0f),
                new Vector2(8.5f, 0.55f),
                zDegrees: -28f);

            // 内栏杆世界 ≈ 合层 local(6.935,7.305) → 楼梯上沿挡位
            Vector3 innerWorld = HouseCompositeWorld + new Vector3(6.935f, 7.305f, 0f);
            EnsureObstacleBox(root, InnerRailName, obstacleLayer,
                new Vector3(innerWorld.x - 0.2f, innerWorld.y - 0.2f, 0f),
                new Vector2(7.5f, 0.55f),
                zDegrees: -28f);

            // 楼梯底侧边：防从斜面外侧穿出（方案1 Cast）
            EnsureObstacleBox(root, "Obstacle_StairsSide", obstacleLayer,
                new Vector3(stairsWorld.x + 5.2f, stairsWorld.y - 1.8f, 0f),
                new Vector2(0.6f, 4.5f),
                zDegrees: 0f);
        }

        private static void EnsureObstacleBox(
            Transform parent,
            string name,
            int layer,
            Vector3 worldPos,
            Vector2 size,
            float zDegrees)
        {
            var go = EnsureChild(parent, name);
            go.layer = layer;
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, zDegrees);

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody2D>();
            }

            // 方案1：障碍只作几何查询；Static + Trigger，矩阵已 Ignore
            rb.bodyType = RigidbodyType2D.Static;
            rb.gravityScale = 0f;
            rb.simulated = true;

            var box = go.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                box = go.AddComponent<BoxCollider2D>();
            }

            box.isTrigger = true;
            box.size = size;
            box.offset = Vector2.zero;
        }

        /// <summary>
        /// P1：上层平台脚进切 Sorting → SceneObject（对齐树屋 DepthZone；Gate 本期不上）。
        /// </summary>
        private static void EnsureDepthZone(Transform map, Vector3 stairsWorld)
        {
            var go = EnsureChild(map, DepthZoneName);
            go.transform.position = new Vector3(stairsWorld.x - 2.5f, stairsWorld.y + 2.8f, 0f);

            var box = go.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                box = go.AddComponent<BoxCollider2D>();
            }

            box.isTrigger = true;
            box.size = new Vector2(8f, 5f);
            box.offset = Vector2.zero;

            var zone = go.GetComponent<VillagePlayerDepthZone>();
            if (zone == null)
            {
                zone = go.AddComponent<VillagePlayerDepthZone>();
            }

            // 序列化字段通过 SerializedObject 写，避免反射私有字段
            var so = new SerializedObject(zone);
            so.FindProperty("targetSortingLayer").stringValue = SortingLayerName.SceneObject;
            so.FindProperty("zonePriority").intValue = 0;
            so.FindProperty("lockSortingOrderInZone").boolValue = true;
            so.FindProperty("sortingOrderInZone").intValue = 2;
            so.FindProperty("requireTriggerCollider").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject EnsureChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            // 深搜：可能已被挪到别处（幂等）
            var deep = FindNamedInScene(parent.gameObject.scene, name);
            if (deep != null)
            {
                deep.SetParent(parent, true);
                return deep.gameObject;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        // 须写全名：本文件命名空间为 EditorC.Tool.Scene，裸写 Scene 会 CS0118
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
