#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.SceneEntities.HomeScene2;
using Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// 巨树 2 楼 WalkArea2 内摆 Hp/Mp×3 宝箱（Box.prefab + VillageKenMuNi1HpMpBox）。
    /// 菜单：Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3
    /// </summary>
    /// <remarks>
    /// 原因（0901）：KenMuNi1 无宝箱实体；须仿 West 挂村脚本+独立存档，禁止改 WalkArea2 形状。
    /// 替代方案：手改 YAML PrefabInstance——易漏 sceneObjs / 组件引用，故用菜单幂等摆放。
    /// </remarks>
    public static class KenMuNi1Tree2fHpMpBoxSetupEditor
    {
        private const string MenuPath =
            "Tools/Scene/Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3";
        private const string VillageScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity";
        private const string BoxPrefabPath = "Assets/Prefabs/Box.prefab";
        private const string AutoRequestFileName = "KenMuNi1Tree2fHpMpBoxSetup.request";
        private const string BoxName = "Tree2fHpMpBox";

        /// <summary>报告建议：落点东侧，错开 ExitFrom_HomeSceneChief2f。</summary>
        private static readonly Vector3 PreferredWorldPos = new Vector3(-152f, 41.2f, 0f);

        /// <summary>备选西侧平台。</summary>
        private static readonly Vector3 FallbackWorldPos = new Vector3(-165f, 40.8f, 0f);

        /// <summary>
        /// 0903 V1：抬过合层树干同层 Order0，避免 Game 看不见（附近合层最高约 35）。
        /// 替代方案：换专用 SortingLayer——需改 Project Settings，本期否决。
        /// </summary>
        private const int VisibleAboveTreeSortingOrder = 50;

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
                Debug.LogWarning("[Tree2fBox] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[Tree2fBox] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            SetupTree2fHpMpBox();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tree2fBox] 完成：Objects/" + BoxName + "（WalkArea2 内，不改多边形）。");
        }

        private static void SetupTree2fHpMpBox()
        {
            var scene = EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);

            var boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoxPrefabPath);
            if (boxPrefab == null)
            {
                Debug.LogError("[Tree2fBox] 缺少 Box.prefab：" + BoxPrefabPath);
                return;
            }

            var objects = FindNamed(scene, "Objects");
            if (objects == null)
            {
                Debug.LogError("[Tree2fBox] 未找到 Objects");
                return;
            }

            // 幂等：删旧箱再摆
            var existing = FindNamed(scene, BoxName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var walkArea2 = FindNamed(scene, "VillageWalkArea2")?.GetComponent<PolygonCollider2D>();
            if (walkArea2 == null)
            {
                Debug.LogError("[Tree2fBox] 未找到 VillageWalkArea2 PolygonCollider2D");
                return;
            }

            // 硬禁止：本工具绝不写 walkArea2.points / offset / pathCount
            Vector3 worldPos = PreferredWorldPos;
            if (!walkArea2.OverlapPoint(worldPos))
            {
                Debug.LogWarning(
                    "[Tree2fBox] 建议坐标不在 WalkArea2 内，改试备选 " + FallbackWorldPos);
                worldPos = FallbackWorldPos;
            }

            if (!walkArea2.OverlapPoint(worldPos))
            {
                Debug.LogError(
                    "[Tree2fBox] 建议/备选坐标均不在 WalkArea2 内，请手挪 "
                    + BoxName
                    + "（禁止改 WalkArea2 多边形腾地）。");
                // 仍摆在建议点，便于人工微调
                worldPos = PreferredWorldPos;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(boxPrefab, objects);
            instance.name = BoxName;
            instance.transform.position = worldPos;
            instance.SetActive(true);

            // 0903 V1：实例覆写 SortingOrder，防合层树干盖住（勿改 Box.prefab 全局默认）
            ApplyVisibleSortingOrder(instance);

            // Prefab 自带 HomeScene2Box：去掉并挂村脚本（对齐 West 场景做法）
            var homeBox = instance.GetComponent<HomeScene2Box>();
            if (homeBox != null)
            {
                Object.DestroyImmediate(homeBox);
            }

            // 防误挂：若曾挂西境脚本也清掉
            var westType = System.Type.GetType(
                "Game.GameRuntime.Entities.SceneEntities.WestRappRoad.WestRappRoadHpMpBox, Assembly-CSharp");
            if (westType != null)
            {
                var west = instance.GetComponent(westType);
                if (west != null)
                {
                    Object.DestroyImmediate(west);
                }
            }

            var villageBox = instance.GetComponent<VillageKenMuNi1HpMpBox>();
            if (villageBox == null)
            {
                villageBox = instance.AddComponent<VillageKenMuNi1HpMpBox>();
            }

            // 接线：Animator / SFX / ComponentSystem（OnInit 也会补，编辑器侧先填齐）
            villageBox.animator = instance.GetComponent<Animator>();
            villageBox.componentSystem = instance.GetComponent<ComponentSystemMono>();
            var sfx = instance.GetComponentInChildren<SoundToggleComponent>(true);
            villageBox.soundSfxCpn = sfx;

            var so = new SerializedObject(villageBox);
            so.FindProperty("useStoryOnOpen").boolValue = false;
            so.FindProperty("storyName").stringValue = string.Empty;
            so.FindProperty("hpBallCount").intValue = 3;
            so.FindProperty("mpBallCount").intValue = 3;
            so.FindProperty("enableDebugLog").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 登记 sceneObjs（运行时会重扫 objRoot，仍写磁盘便于 Inspector 可见）
            RegisterSceneObj(instance);

            EditorUtility.SetDirty(instance);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            bool inside = walkArea2.OverlapPoint(instance.transform.position);
            Debug.Log(
                "[Tree2fBox] 已摆 "
                + BoxName
                + " @ "
                + instance.transform.position
                + " OverlapWalkArea2="
                + inside
                + " SortingOrder="
                + VisibleAboveTreeSortingOrder
                + "（WalkArea2 点集未改）",
                instance);
        }

        /// <summary>
        /// 抬实例 SpriteRenderer.sortingOrder，写入 Prefab 覆写（不改源 Prefab）。
        /// 原因（0903 H1）：Default/Order0 易被巨树合层同层挡住。
        /// </summary>
        private static void ApplyVisibleSortingOrder(GameObject boxRoot)
        {
            var sr = boxRoot.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = boxRoot.GetComponentInChildren<SpriteRenderer>(true);
            }

            if (sr == null)
            {
                Debug.LogWarning("[Tree2fBox] 无 SpriteRenderer，跳过 SortingOrder");
                return;
            }

            sr.sortingOrder = VisibleAboveTreeSortingOrder;
            EditorUtility.SetDirty(sr);
        }

        private static void RegisterSceneObj(GameObject boxRoot)
        {
            var sceneEntity = boxRoot.GetComponent<SceneEntity>();
            if (sceneEntity == null)
            {
                Debug.LogError("[Tree2fBox] Box 无 SceneEntity");
                return;
            }

            var gsm = Object.FindObjectOfType<Village_KenMuNiSceneManager>();
            if (gsm == null)
            {
                Debug.LogWarning("[Tree2fBox] 未找到 Village_KenMuNiSceneManager，跳过 sceneObjs 手写");
                return;
            }

            var entityGsm = gsm.GetComponent<SceneEntityComponentGSM>();
            if (entityGsm == null)
            {
                // 可能挂在子节点
                entityGsm = gsm.GetComponentInChildren<SceneEntityComponentGSM>(true);
            }

            if (entityGsm == null)
            {
                Debug.LogWarning("[Tree2fBox] 未找到 SceneEntityComponentGSM");
                return;
            }

            var gsmSo = new SerializedObject(entityGsm);
            var listProp = gsmSo.FindProperty("sceneObjs");
            if (listProp == null || !listProp.isArray)
            {
                return;
            }

            for (int i = 0; i < listProp.arraySize; i++)
            {
                if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == sceneEntity)
                {
                    return; // 已登记
                }
            }

            listProp.arraySize++;
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = sceneEntity;
            gsmSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(entityGsm);
        }

        // 须写全名：本文件命名空间为 EditorC.Tool.Scene，裸写 Scene 会 CS0118
        private static Transform FindNamed(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var t = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(x => x.name == name);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
#endif
