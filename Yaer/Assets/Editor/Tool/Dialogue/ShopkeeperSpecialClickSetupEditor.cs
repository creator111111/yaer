#if UNITY_EDITOR
using System.IO;
using System.Text;
using Game.GameMgr.Component.Cursor;
using Game.GameRuntime.GameSceneManager.Scene.Village_Shop;
using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 0828/0829/0830 施工：老板娘 Head/Chest 热区 + 点头/点胸对白 Prefab 一键装配。
    /// 菜单：Tools / Dialogue / Setup Shopkeeper Special Click (Hotspots + Prefabs)
    /// </summary>
    /// <remarks>
    /// 原因：场景 YAML 手工挂 Collider 易错；对白 Prefab 须复制 ShopStart 最小壳再 Import CSV。
    /// 0829 方案 A：点头真源 <c>Village_ShopHead.prefab</c>；0830 方案 A：点胸真源 <c>Village_ShopChest.prefab</c>。
    /// 0830 对齐 Head：Chest 须 <c>fadeYaerPortrait=true</c>（UIAlpha+立绘淡入），否则「对话框不出现」。
    /// 勿再写 HeadClick / ChestClick 空路径。
    /// 替代方案：纯 YAML 手改场景 + 手工 NodeCanvas 粘节点——易漏 Physics2DRaycaster / Actor 绑定。
    /// </remarks>
    public static class ShopkeeperSpecialClickSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Shopkeeper Special Click (Hotspots + Prefabs)";
        private const string RebuildHeadMenuPath = "Tools/Dialogue/Rebuild Shopkeeper Head Prefab Only (Village_ShopHead)";
        private const string RebuildChestMenuPath = "Tools/Dialogue/Rebuild Shopkeeper Chest Prefab Only (Village_ShopChest)";
        private const string ScenePath = "Assets/GameRes/Scenes/Village_Shop.unity";
        private const string ShopStartPrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab";
        // 0829 方案 A：与常量 ShopkeeperHeadClickStoryName / 磁盘 Prefab 名对齐。
        private const string HeadPrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab";
        // 0830 方案 A：与常量 ShopkeeperChestClickStoryName / 磁盘 Prefab 名对齐（旧 ChestClick 空号作废）。
        private const string ChestPrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab";
        private const string HeadCsvPath = "Assets/Dialog/Village_商店点头交互.csv";
        private const string ChestCsvPath = "Assets/Dialog/Village_商店点胸交互.csv";
        private const string GeneratedFolder = "Assets/GameRes/DialogueTrees/Generated";
        private const string CompositeRootName = "商店界面合层";
        private const string PaintingNameToken = "MerchantPainting";
        private const string TriggerName = "Trigger";

        // 热区局部坐标：对齐 Face(~13.23,8.42) 与 Body Normal(~13.54,5.24)，可在 Scene Gizmo 再微调。
        private static readonly Vector2 HeadLocalPos = new Vector2(13.23f, 8.40f);
        private static readonly Vector2 HeadColliderSize = new Vector2(2.2f, 2.0f);
        private static readonly Vector2 ChestLocalPos = new Vector2(13.50f, 6.20f);
        private static readonly Vector2 ChestColliderSize = new Vector2(3.0f, 2.5f);

        [MenuItem(MenuPath)]
        public static void SetupAll()
        {
            SetupHotspotsInOpenOrLoadedScene();
            BuildSpecialStoryPrefab(
                HeadPrefabPath,
                Village_ShopSceneManager.ShopkeeperHeadClickStoryName,
                HeadCsvPath,
                includeNarrator: true,
                fadeYaerPortrait: true);
            // 0830：Chest 必须开立绘淡入 + UIAlpha 壳，否则「对话框不出现」（旧 false 会回潮）。
            BuildSpecialStoryPrefab(
                ChestPrefabPath,
                Village_ShopSceneManager.ShopkeeperChestClickStoryName,
                ChestCsvPath,
                includeNarrator: false,
                fadeYaerPortrait: true);

            // 胸部线 C6+（黑屏转树屋）本期不接；留注释挂钩，勿空接 LoadScene。
            Debug.Log(
                "[ShopSpecialSetup] Village_ShopChest 止于 C5（含壳层 Fighting→立绘→UIAlpha）。" +
                " TODO(下期): C6 黑屏转树屋 → Village_ShopChest_Treehouse / LoadScene。");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ShopSpecialSetup] 完成：热区 + Physics2DRaycaster + Head/Chest Prefab。");
        }

        /// <summary>
        /// 仅用点头 CSV 重建 <c>Village_ShopHead</c>（含 Narrator、清旧图）。
        /// 原因：0829 P0 验收文案/表情；不碰场景热区与 Chest Prefab。
        /// 可批处理：-executeMethod EditorC.Tool.Dialogue.ShopkeeperSpecialClickSetupEditor.RebuildHeadPrefabOnly
        /// </summary>
        [MenuItem(RebuildHeadMenuPath)]
        public static void RebuildHeadPrefabOnly()
        {
            BuildSpecialStoryPrefab(
                HeadPrefabPath,
                Village_ShopSceneManager.ShopkeeperHeadClickStoryName,
                HeadCsvPath,
                includeNarrator: true,
                fadeYaerPortrait: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ShopSpecialSetup] 仅重建 Head Prefab：" + HeadPrefabPath);
        }

        /// <summary>
        /// 仅用点胸 CSV 重建 <c>Village_ShopChest</c>（含壳层：Fighting→雅立绘→UIAlpha→句）。
        /// 原因：0830「对话框不出现」——纯 Statement 图无 UIAlpha；须写入 Prefab bound。
        /// 可批处理：-executeMethod EditorC.Tool.Dialogue.ShopkeeperSpecialClickSetupEditor.RebuildChestPrefabOnly
        /// </summary>
        [MenuItem(RebuildChestMenuPath)]
        public static void RebuildChestPrefabOnly()
        {
            BuildSpecialStoryPrefab(
                ChestPrefabPath,
                Village_ShopSceneManager.ShopkeeperChestClickStoryName,
                ChestCsvPath,
                includeNarrator: false,
                fadeYaerPortrait: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ShopSpecialSetup] 仅重建 Chest Prefab：" + ChestPrefabPath);
        }

        /// <summary>仅热区（场景已打开时也可单独跑）。</summary>
        [MenuItem("Tools/Dialogue/Setup Shopkeeper Hotspots Only")]
        public static void SetupHotspotsOnly()
        {
            SetupHotspotsInOpenOrLoadedScene();
            Debug.Log("[ShopSpecialSetup] 热区装配完成。");
        }

        private static void SetupHotspotsInOpenOrLoadedScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var composite = FindRootByName(CompositeRootName);
            if (composite == null)
            {
                Debug.LogError("[ShopSpecialSetup] 未找到「商店界面合层」。");
                return;
            }

            var painting = FindChildContaining(composite.transform, PaintingNameToken);
            if (painting == null)
            {
                Debug.LogError("[ShopSpecialSetup] 未找到 MerchantPainting。");
                return;
            }

            var trigger = EnsureChild(painting, TriggerName);
            EnsureHotspot(trigger, "Head", ShopkeeperBodyHotspot.HotspotKind.Head, HeadLocalPos, HeadColliderSize);
            EnsureHotspot(trigger, "Chest", ShopkeeperBodyHotspot.HotspotKind.Chest, ChestLocalPos, ChestColliderSize);

            EnsurePhysics2DRaycasterOnMainCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePhysics2DRaycasterOnMainCamera()
        {
            var camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                var cams = Object.FindObjectsOfType<Camera>();
                for (var i = 0; i < cams.Length; i++)
                {
                    if (cams[i] != null && cams[i].cullingMask != (1 << 5))
                    {
                        camGo = cams[i].gameObject;
                        break;
                    }
                }
            }

            if (camGo == null)
            {
                Debug.LogError("[ShopSpecialSetup] 找不到 Main Camera，无法挂 Physics2DRaycaster。");
                return;
            }

            if (camGo.GetComponent<Physics2DRaycaster>() == null)
            {
                Undo.AddComponent<Physics2DRaycaster>(camGo);
                Debug.Log("[ShopSpecialSetup] 已在 Main Camera 添加 Physics2DRaycaster。", camGo);
            }
        }

        private static void EnsureHotspot(
            Transform trigger,
            string childName,
            ShopkeeperBodyHotspot.HotspotKind kind,
            Vector2 localPos,
            Vector2 size)
        {
            var child = EnsureChild(trigger, childName);
            child.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            var box = child.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                box = Undo.AddComponent<BoxCollider2D>(child.gameObject);
            }

            box.isTrigger = true;
            box.size = size;
            box.offset = Vector2.zero;

            var hotspot = child.GetComponent<ShopkeeperBodyHotspot>();
            if (hotspot == null)
            {
                hotspot = Undo.AddComponent<ShopkeeperBodyHotspot>(child.gameObject);
            }

            var so = new SerializedObject(hotspot);
            so.FindProperty("hotspotKind").enumValueIndex = (int)kind;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 0830：Head/Chest 均挂 Catch，避免只手改场景；对白关热区时走 OnDisable Exit。
            EnsureCursorCatch(child.gameObject);
        }

        /// <summary>
        /// 与 Head 0829 金样一致：同物体挂 <see cref="CursorChangeTrigger"/>，Catch / Priority=1。
        /// </summary>
        private static void EnsureCursorCatch(GameObject host)
        {
            if (host == null)
            {
                return;
            }

            var cursor = host.GetComponent<CursorChangeTrigger>();
            if (cursor == null)
            {
                cursor = Undo.AddComponent<CursorChangeTrigger>(host);
            }

            var so = new SerializedObject(cursor);
            // CursorState: Normal=0, Catch=1, View=2, Chat=3
            so.FindProperty("TargetState").enumValueIndex = (int)CursorState.Catch;
            so.FindProperty("Priority").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildSpecialStoryPrefab(
            string prefabPath,
            string prefabRootName,
            string csvPath,
            bool includeNarrator,
            bool fadeYaerPortrait)
        {
            if (!File.Exists(Path.GetFullPath(csvPath)))
            {
                Debug.LogError("[ShopSpecialSetup] 缺少 CSV：" + csvPath);
                return;
            }

            // 1) 始终从 ShopStart 复制最小壳（覆盖旧产物，避免残留首进店图）
            if (!AssetDatabase.CopyAsset(ShopStartPrefabPath, prefabPath))
            {
                // 目标已存在时 CopyAsset 可能失败：先删再拷
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }

                if (!AssetDatabase.CopyAsset(ShopStartPrefabPath, prefabPath))
                {
                    Debug.LogError("[ShopSpecialSetup] 复制 Prefab 失败：" + prefabPath);
                    return;
                }
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                root.name = prefabRootName;

                // 2) 删古莎（点头/点胸台本无古莎）；保留 Yaer + Merchant
                StripGusha(root);

                if (includeNarrator)
                {
                    EnsureNarratorActor(root);
                }

                // 0829：点头线须先绑雅立绘 BB，再建「立绘淡入」前奏；否则 alpha=0 看不见大立绘。
                if (fadeYaerPortrait)
                {
                    BindYaerPortraitBlackboard(root);
                }

                // 3) CSV → DialogueTree，写入 bound graph
                if (!TryImportCsvIntoController(root, csvPath, fadeYaerPortrait, out var err))
                {
                    Debug.LogError("[ShopSpecialSetup] " + err);
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[ShopSpecialSetup] Prefab 已写入：" + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool TryImportCsvIntoController(
            GameObject prefabRoot,
            string csvPath,
            bool fadeYaerPortrait,
            out string error)
        {
            error = null;
            var controller = prefabRoot.GetComponent<DialogueTreeController>();
            if (controller == null)
            {
                error = "Prefab 无 DialogueTreeController。";
                return false;
            }

            string csvText;
            try
            {
                csvText = File.ReadAllText(csvPath, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                error = "读 CSV 失败：" + ex.Message;
                return false;
            }

            var mapping = DialogueSpeakerMapping.CreateDefaultInstance();
            if (!DialogueCsvParser.TryParse(csvText, out var rows, out var parseError, out var hasBodyType, mapping))
            {
                error = parseError;
                return false;
            }

            var assetName = Path.GetFileNameWithoutExtension(csvPath);
            // 特殊交互：对齐 Head 金样壳层 Fighting → 立绘 CGAlpha → 对话框 UIAlpha → 句。
            // 0830：点胸旧图仅 Statement →「对话框不出现」；fadeYaerPortrait 必须 true 才插立绘+框。
            // 前序「先框后立绘」A1 已作废（见 0829 先立绘后对话框时序报告）。
            var prelude = new DialoguePreludeOptions
            {
                FadeDialogueUI = true,
                // 对齐 Head：有立绘淡入时同步藏战斗条（纯句图勿开壳以外路径）。
                HideFightingPanelOnStart = fadeYaerPortrait,
                RestoreFightingPanelOnEnd = false,
                FadePortraitCanvasGroups = fadeYaerPortrait,
                PortraitReferencePrefab = fadeYaerPortrait ? prefabRoot : null,
                // T1：立绘 Duration 对齐 ShopStart=1.0；对话框 Delay0.5 / PrepareMask 在 PreludeBuilder 写死对齐 Head。
                PreludeFadeDuration = 1.0f
            };

            var tree = DialogueCsvGraphBuilder.TryBuild(
                rows,
                mapping,
                startRowId: null,
                assetName: assetName,
                prelude,
                hasBodyType);
            if (tree == null)
            {
                error = "DialogueCsvGraphBuilder 建图失败。";
                return false;
            }

            // 旁路：同步落一份 Generated .asset 便于校对
            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            {
                AssetDatabase.CreateFolder("Assets/GameRes/DialogueTrees", "Generated");
            }

            var assetPath = $"{GeneratedFolder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<DialogueTree>(assetPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(tree, assetPath);

            // 重新 Load，再绑进 Prefab（SetBoundGraphReference 会把 JSON 嵌入 bound 序列化）
            var boundTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(assetPath);
            controller.Validate();
            if (controller.behaviour == null)
            {
                error = "DialogueTree behaviour 未就绪。";
                return false;
            }

            controller.SetBoundGraphReference(boundTree);

            // 重新绑定 Actor（雅尔 / 老板娘 / 旁白）
            RebindActors(controller, prefabRoot, controller.behaviour as DialogueTree);

            EditorUtility.SetDirty(controller);
            return true;
        }

        /// <summary>
        /// 把 <c>Yaer/GoOutStoryYaerPainting</c> 的 CanvasGroup 写入 BB，并移除空壳 GushaPainting。
        /// 原因：ShopHead 从 ShopStart 拷壳后 BB 常未绑实例，CanvasGroupAlpha 拉空；alpha 默认 0 则大立绘永隐。
        /// </summary>
        private static void BindYaerPortraitBlackboard(GameObject root)
        {
            var blackboard = root.GetComponent<Blackboard>();
            if (blackboard == null)
            {
                Debug.LogWarning("[ShopSpecialSetup] Prefab 无 Blackboard，跳过雅立绘绑定。", root);
                return;
            }

            var yaer = root.transform.Find("Yaer");
            var painting = yaer != null ? yaer.Find("GoOutStoryYaerPainting") : null;
            if (painting == null)
            {
                // PrefabInstance 名可能在嵌套根上
                var all = root.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].name == "GoOutStoryYaerPainting")
                    {
                        painting = all[i];
                        break;
                    }
                }
            }

            if (painting == null)
            {
                Debug.LogWarning("[ShopSpecialSetup] 未找到 GoOutStoryYaerPainting，无法绑 BB。", root);
                return;
            }

            // 须写全名 UnityEngine.CanvasGroup，避免与 NodeCanvas.Framework.CanvasGroup 歧义（CS0104）。
            var cg = painting.GetComponent<UnityEngine.CanvasGroup>();
            if (cg == null)
            {
                Debug.LogWarning("[ShopSpecialSetup] GoOutStoryYaerPainting 无 CanvasGroup。", painting);
                return;
            }

            // 保证变量存在且指向实例（Import 淡入靠变量名解析）。
            if (blackboard.GetVariable("GoOutStoryYaerPainting") != null)
            {
                blackboard.SetVariableValue("GoOutStoryYaerPainting", cg);
            }
            else
            {
                blackboard.AddVariable("GoOutStoryYaerPainting", cg);
            }

            // P1：点头线无古莎，删空壳免误导。
            if (blackboard.GetVariable("GushaPainting") != null)
            {
                blackboard.RemoveVariable("GushaPainting");
            }

            // 保持默认 alpha=0，由前奏 CanvasGroupAlpha 拉到 1（A1）。
            cg.alpha = 0f;
            EditorUtility.SetDirty(blackboard);
            EditorUtility.SetDirty(cg);
            Debug.Log("[ShopSpecialSetup] 已绑 BB GoOutStoryYaerPainting → CanvasGroup", cg);
        }

        private static void RebindActors(DialogueTreeController controller, GameObject root, DialogueTree tree)
        {
            if (tree == null)
            {
                return;
            }

            var yaer = root.transform.Find("Yaer");
            if (yaer != null)
            {
                var actor = yaer.GetComponent<DialogueActorEx>();
                if (actor != null)
                {
                    tree.SetActorReference("雅尔", actor);
                }
            }

            var merchant = root.transform.Find("Merchant");
            if (merchant != null)
            {
                var actor = merchant.GetComponent<DialogueActorEx>();
                if (actor != null)
                {
                    tree.SetActorReference(ShopkeeperCsvDefaults.ShopkeeperActorName, actor);
                }
            }

            var narrator = root.transform.Find("Narrator");
            if (narrator != null)
            {
                var actor = narrator.GetComponent<DialogueActorEx>();
                if (actor != null)
                {
                    tree.SetActorReference("旁白", actor);
                }
            }

            controller.SetBoundGraphReference(tree);
        }

        private static void StripGusha(GameObject root)
        {
            var gusha = root.transform.Find("Gusha");
            if (gusha != null)
            {
                Object.DestroyImmediate(gusha.gameObject);
            }

            // Blackboard 上的 GushaPainting：点头线在 BindYaerPortraitBlackboard 中删除；此处仅清 GO。
        }

        private static void EnsureNarratorActor(GameObject root)
        {
            var existing = root.transform.Find("Narrator");
            Transform t;
            if (existing == null)
            {
                var go = new GameObject("Narrator", typeof(RectTransform));
                go.layer = root.layer;
                t = go.transform;
                t.SetParent(root.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100, 100);
                rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                t = existing;
            }

            var actor = t.GetComponent<DialogueActorEx>();
            if (actor == null)
            {
                actor = t.gameObject.AddComponent<DialogueActorEx>();
            }

            var so = new SerializedObject(actor);
            so.FindProperty("_name").stringValue = "旁白";
            so.FindProperty("_roleName").enumValueIndex = 0;
            so.FindProperty("_portrait").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindRootByName(string name)
        {
            var all = Object.FindObjectsOfType<Transform>();
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].parent == null && all[i].name == name)
                {
                    return all[i].gameObject;
                }

                // 合层也可能不在真正 root（少见）；再比一次 name
                if (all[i] != null && all[i].name == name)
                {
                    return all[i].gameObject;
                }
            }

            return GameObject.Find(name);
        }

        private static Transform FindChildContaining(Transform parent, string token)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c != null && c.name.IndexOf(token, System.StringComparison.Ordinal) >= 0)
                {
                    return c;
                }
            }

            return null;
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            var existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(go, "Create " + childName);
            go.transform.SetParent(parent, false);
            go.layer = parent.gameObject.layer;
            return go.transform;
        }
    }
}
#endif
