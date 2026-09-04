#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Game.GameRuntime.GameSceneManager.Scene.Village_House;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 预置 <c>古莎动画合层</c>：写 Prefab 资产 + <b>场景拆包合层</b>（H2 真源）。
    /// 菜单：Tools / Scene / Setup Chief House 古莎动画合层预置
    /// </summary>
    /// <remarks>
    /// 原因（0901 验收排查）：此前只写 <c>Prefab/村长家合层</c>，场景 <c>Design/村长家合层</c> 是拆包 GO，
    /// 运行时 Find 不到动画 → 换人关待机后正面空白。
    /// 本菜单场景侧<strong>只</strong>在合层下增删「古莎动画合层」并可选绑 GSM 两引用；不改 WalkArea/门/其它物体。
    /// </remarks>
    public static class ChiefHouseGushaAnimStandbySetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup Chief House 古莎动画合层预置";
        private const string HouseCompositePrefabPath =
            "Assets/ArtRes/Scene/Village/Prefab/村长家合层.prefab";
        private const string AnimPrefabPath = "Assets/ArtRes/Animation/古莎动画合层.prefab";
        private const string ChiefScenePath = "Assets/GameRes/Scenes/Village_Chief_House.unity";
        private const string HouseCompositeName = "村长家合层";
        private const string StandbyName = "古莎待机";
        private const string AnimInstanceName = "古莎动画合层";
        private const string BackgroundChildName = "背景";
        private const string AutoRequestFileName = "ChiefHouseGushaAnimSetup.request";

        /// <summary>相对待机 SortingOrder 基底，使图层落在 ≈9～11（&lt; 村长 12）。</summary>
        private const int SortingOrderBoost = 8;

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
                Debug.LogWarning("[GushaAnimSetup] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[GushaAnimSetup] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            // 1) 资产侧：保持与历史一致（其它入口若挂 5cad Prefab 仍有动画）
            SetupIntoHouseCompositePrefab();

            // 2) P0：场景拆包合层才是玩时真源（H2+H8）
            SetupIntoChiefHouseScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GushaAnimSetup] 完成：Prefab 资产 + 场景 Design/村长家合层 均已预置「古莎动画合层」。");
        }

        /// <summary>只改 <c>Prefab/村长家合层</c> 资产；不打开场景。</summary>
        private static void SetupIntoHouseCompositePrefab()
        {
            var housePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HouseCompositePrefabPath);
            var animPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnimPrefabPath);
            if (housePrefab == null || animPrefab == null)
            {
                Debug.LogError(
                    "[GushaAnimSetup] Prefab 缺失：house="
                    + (housePrefab != null)
                    + " anim="
                    + (animPrefab != null));
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(HouseCompositePrefabPath);
            try
            {
                if (!TryPlaceAnimUnderComposite(root.transform, animPrefab, out var msg))
                {
                    Debug.LogError("[GushaAnimSetup][Prefab] " + msg);
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, HouseCompositePrefabPath);
                Debug.Log("[GushaAnimSetup][Prefab] " + msg, root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// 只在场景 <c>Design/村长家合层</c> 下预置动画实例 + 绑 GSM 引用。
        /// 不改 WalkArea / 楼梯门 / Objects 等其它节点。
        /// </summary>
        private static void SetupIntoChiefHouseScene()
        {
            var animPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnimPrefabPath);
            if (animPrefab == null)
            {
                Debug.LogError("[GushaAnimSetup][Scene] 缺动画 Prefab：" + AnimPrefabPath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(ChiefScenePath, OpenSceneMode.Single);
            var composite = FindNamed(scene, HouseCompositeName);
            if (composite == null)
            {
                Debug.LogError("[GushaAnimSetup][Scene] 未找到「" + HouseCompositeName + "」（勿改错合层）。");
                return;
            }

            if (!TryPlaceAnimUnderComposite(composite, animPrefab, out var msg))
            {
                Debug.LogError("[GushaAnimSetup][Scene] " + msg);
                return;
            }

            // P1：绑 SerializeField，减少运行时 Find 脆弱（仅改这两字段）
            BindGsmRefs(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GushaAnimSetup][Scene] " + msg + "（已保存场景，未动其它设置）", composite);
        }

        /// <summary>
        /// 在合层根下放置/重置「古莎动画合层」：脚位≈待机、默认关、关子「背景」、Sorting+8。
        /// </summary>
        private static bool TryPlaceAnimUnderComposite(
            Transform compositeRoot,
            GameObject animPrefab,
            out string message)
        {
            message = null;
            if (compositeRoot == null)
            {
                message = "合层 Transform 为空。";
                return false;
            }

            var standby = FindDeepChild(compositeRoot, StandbyName);
            if (standby == null)
            {
                message = "合层下未找到「" + StandbyName + "」。";
                return false;
            }

            // 幂等：已有则删再建，避免叠两份
            var existing = FindDeepChild(compositeRoot, AnimInstanceName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(animPrefab, compositeRoot);
            instance.name = AnimInstanceName;
            instance.SetActive(false);

            instance.transform.localPosition = standby.localPosition;
            instance.transform.localRotation = standby.localRotation;
            instance.transform.localScale = standby.localScale;

            DisableBackgroundChild(instance.transform);
            BoostSortingOrders(instance.transform);
            EditorUtility.SetDirty(instance);

            message = "已预置 "
                + AnimInstanceName
                + " @ local "
                + standby.localPosition
                + "（默认关，背景已关）";
            return true;
        }

        private static void BindGsmRefs(UnityEngine.SceneManagement.Scene scene)
        {
            var gsm = Object.FindObjectOfType<Village_Chief_HouseSceneManager>();
            if (gsm == null)
            {
                Debug.LogWarning("[GushaAnimSetup] 未找到 Village_Chief_HouseSceneManager，跳过引用绑定。");
                return;
            }

            var standby = FindNamed(scene, StandbyName);
            var anim = FindNamed(scene, AnimInstanceName);
            var so = new SerializedObject(gsm);
            var standbyProp = so.FindProperty("gushaStandby");
            var animProp = so.FindProperty("gushaAnimComposite");
            if (standbyProp != null && standby != null)
            {
                standbyProp.objectReferenceValue = standby.gameObject;
            }

            if (animProp != null && anim != null)
            {
                animProp.objectReferenceValue = anim.gameObject;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gsm);
            Debug.Log(
                "[GushaAnimSetup] 已绑 GSM gushaStandby="
                + (standby != null)
                + " gushaAnimComposite="
                + (anim != null));
        }

        private static void DisableBackgroundChild(Transform animRoot)
        {
            var bg = FindDeepChild(animRoot, BackgroundChildName);
            if (bg != null && bg.parent == animRoot)
            {
                bg.gameObject.SetActive(false);
                EditorUtility.SetDirty(bg.gameObject);
            }
            else
            {
                Debug.LogWarning("[GushaAnimSetup] 动画实例下未找到子「背景」，请人工确认。");
            }
        }

        private static void BoostSortingOrders(Transform animRoot)
        {
            var renderers = animRoot.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (sr.gameObject.name == BackgroundChildName)
                {
                    continue;
                }

                sr.sortingOrder += SortingOrderBoost;
                EditorUtility.SetDirty(sr);
            }
        }

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
