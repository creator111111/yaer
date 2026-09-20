#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Scene
{
    /// <summary>
    /// 村庄白天遮罩盖顶：合层兄弟实例 + 关背景 + 抬 Effect。
    /// 菜单：Tools / Scene / Setup Village 村庄遮罩盖顶
    /// </summary>
    /// <remarks>
    /// 原因（0920）：遮罩 Prefab 默认 Default/0～3，画在 Player 后面盖不住人；
    /// 须作合层兄弟挂场景，关「背景」防双影，其余抬 Effect Order≥10。
    /// 禁止嵌合层 Prefab 源、禁止 SpriteMask、禁止夜景、村长家（仅 PSD）本期跳过。
    /// 替代：手写 PrefabInstance YAML —— 子层 Sorting 覆写易漏且超长行，故用菜单幂等摆放。
    /// </remarks>
    public static class VillageOverlayMaskSetupEditor
    {
        private const string MenuPath = "Tools/Scene/Setup Village 村庄遮罩盖顶";
        private const string AutoRequestFileName = "VillageOverlayMaskSetup.request";
        private const string MaskFolder = "Assets/ArtRes/Scene/村庄遮罩";
        private const string EffectLayerName = "Effect";
        private const int OrderBoost = 10;

        private struct MaskJob
        {
            public string ScenePath;
            public string CompositeName;
            public string MaskPrefabPath;
            public string InstanceName;
        }

        private static readonly MaskJob[] Jobs =
        {
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_HomeScene1.unity",
                CompositeName = "村民家1合层",
                MaskPrefabPath = MaskFolder + "/村民家.prefab",
                InstanceName = "村庄遮罩_村民家1"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_HomeScene2.unity",
                CompositeName = "村民家2合层",
                MaskPrefabPath = MaskFolder + "/村民家2.prefab",
                InstanceName = "村庄遮罩_村民家2"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_HomeScene45.unity",
                CompositeName = "村民家3合层",
                MaskPrefabPath = MaskFolder + "/村民家3.prefab",
                InstanceName = "村庄遮罩_村民家3"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_HomeScene23.unity",
                CompositeName = "村民家4合层",
                MaskPrefabPath = MaskFolder + "/村民家4.prefab",
                InstanceName = "村庄遮罩_村民家4"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity",
                CompositeName = "肯姆尼1合层",
                MaskPrefabPath = MaskFolder + "/肯姆尼1.prefab",
                InstanceName = "村庄遮罩_肯姆尼1"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity",
                CompositeName = "肯姆尼2合层",
                MaskPrefabPath = MaskFolder + "/肯姆尼2.prefab",
                InstanceName = "村庄遮罩_肯姆尼2"
            },
            new MaskJob
            {
                ScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity",
                CompositeName = "肯姆尼3合层",
                MaskPrefabPath = MaskFolder + "/肯姆尼3.prefab",
                InstanceName = "村庄遮罩_肯姆尼3"
            },
        };

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
                Debug.LogWarning("[VillageOverlay] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[VillageOverlay] 检测到请求文件，自动执行…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            int effectId = SortingLayer.NameToID(EffectLayerName);
            if (effectId == 0 && EffectLayerName != "Default")
            {
                // NameToID 找不到时回 0；Effect 必须存在
                Debug.LogError("[VillageOverlay] SortingLayer「Effect」不存在，中止。");
                return;
            }

            string lastScene = null;
            UnityEngine.SceneManagement.Scene openScene = default;

            for (int i = 0; i < Jobs.Length; i++)
            {
                var job = Jobs[i];
                if (job.ScenePath != lastScene)
                {
                    if (!string.IsNullOrEmpty(lastScene) && openScene.IsValid())
                    {
                        EditorSceneManager.MarkSceneDirty(openScene);
                        EditorSceneManager.SaveScene(openScene);
                    }

                    openScene = EditorSceneManager.OpenScene(job.ScenePath, OpenSceneMode.Single);
                    lastScene = job.ScenePath;
                }

                SetupOneMask(openScene, job, effectId);
            }

            if (openScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(openScene);
                EditorSceneManager.SaveScene(openScene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VillageOverlay] 完成：民居 1/2/3/4 + 肯姆尼 1/2/3 遮罩已挂（村长家跳过）。");
        }

        private static void SetupOneMask(
            UnityEngine.SceneManagement.Scene scene,
            MaskJob job,
            int effectSortingLayerId)
        {
            var composite = FindNamed(scene, job.CompositeName);
            if (composite == null)
            {
                Debug.LogError("[VillageOverlay] 未找到合层：" + job.CompositeName + " @ " + job.ScenePath);
                return;
            }

            var parent = composite.parent;
            if (parent == null)
            {
                Debug.LogError("[VillageOverlay] 合层无父节点：" + job.CompositeName);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(job.MaskPrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[VillageOverlay] 缺少 Prefab：" + job.MaskPrefabPath);
                return;
            }

            // 幂等：删旧实例再摆（含旧名变体）
            var existing = FindNamed(scene, job.InstanceName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = job.InstanceName;
            // 与合层同父、同本地坐标，子层即对齐；Z 根用 0
            instance.transform.localPosition = new Vector3(
                composite.localPosition.x,
                composite.localPosition.y,
                0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);

            // 关「背景」防与合层双影
            var bg = FindChildRecursive(instance.transform, "背景");
            if (bg != null)
            {
                bg.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[VillageOverlay] 未找到子物体「背景」：" + job.InstanceName);
            }

            // 其余 SpriteRenderer → Effect，Order 相对关系保留并整体 +10；子 Z 清 0
            var renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                var sr = renderers[r];
                if (sr == null)
                {
                    continue;
                }

                // 已关的背景不抬层（Inactive 即可）
                if (bg != null && sr.transform == bg)
                {
                    continue;
                }

                sr.sortingLayerID = effectSortingLayerId;
                sr.sortingOrder = sr.sortingOrder + OrderBoost;
                sr.maskInteraction = SpriteMaskInteraction.None;

                var t = sr.transform;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 0f);
                EditorUtility.SetDirty(sr);
                EditorUtility.SetDirty(t);
            }

            EditorUtility.SetDirty(instance);
            Debug.Log(
                "[VillageOverlay] " + job.InstanceName +
                " @ " + instance.transform.localPosition +
                " parent=" + parent.name +
                " 对齐 " + job.CompositeName,
                instance);
        }

        private static Transform FindNamed(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindChildRecursive(root.transform, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform tr, string name)
        {
            if (tr.name == name)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                var c = FindChildRecursive(tr.GetChild(i), name);
                if (c != null)
                {
                    return c;
                }
            }

            return null;
        }
    }
}
#endif
