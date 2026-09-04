#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using Game.GameRuntime.Story.Node;
using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 「出村长家送树屋」：CSV → Prefab（雅+古壳）+ 段 A/B 间插黑幕传送；场景摆 Teleport 锚点。
    /// 菜单：Tools / Dialogue / Setup Village 出村长家送树屋
    /// </summary>
    /// <remarks>
    /// 原因（0901）：缺 CSV/Prefab 必加载失败；无黑幕传送 Action 无法同场景转场。
    /// 台本一字不改；锚句「小心一些也没什么」后插 <see cref="BlackFadeTeleportPlayerActionTask"/>。
    /// 替代方案：两段 Prefab——多一次 Trigger，报告否决。
    /// </remarks>
    public static class VillageLeaveChiefEscortDialogueSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Village 出村长家送树屋";
        private const string ShellPrefabPath = "Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab";
        private const string TargetPrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_出村长家送树屋.prefab";
        private const string CsvPath = "Assets/Dialog/Village_出村长家送树屋.csv";
        private const string GeneratedFolder = "Assets/GameRes/DialogueTrees/Generated";
        private const string AssetBaseName = "Village_出村长家送树屋";
        private const string AutoRequestFileName = "LeaveChiefEscortSetup.request";

        private const string VillageScenePath = "Assets/GameRes/Scenes/Village_KenMuNi1.unity";
        private const string TeleportName = "TeleportTo_YaerTreeHouseDoor";

        /// <summary>段 A 末句（插传送的锚）。</summary>
        private const string AnchorSubstring = "小心一些也没什么";

        /// <summary>
        /// House_Tree≈(28.32,5.45) 易出 WalkArea；落点取门前地面带（世界 Y≈-6）。
        /// Setup 会 OverlapPoint 校验，失败再试备选。
        /// </summary>
        private static readonly Vector3 PreferredTeleportWorld = new Vector3(28.5f, -6.2f, 0f);
        private static readonly Vector3 FallbackTeleportWorld = new Vector3(27.5f, -5.8f, 0f);

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
                Debug.LogWarning("[LeaveChiefEscort] 无法删除自动请求：" + ex.Message);
                return;
            }

            Debug.Log("[LeaveChiefEscort] 检测到请求文件，自动执行 Setup…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            SetupTeleportInVillageScene();
            SetupDialoguePrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LeaveChiefEscort] 完成：Prefab + TeleportTo_YaerTreeHouseDoor。");
        }

        /// <summary>在 KenMuNi1 Objects 下摆树屋门口 Walk 内落点（不改 WalkArea）。</summary>
        private static void SetupTeleportInVillageScene()
        {
            var scene = EditorSceneManager.OpenScene(VillageScenePath, OpenSceneMode.Single);
            var objects = FindNamed(scene, "Objects");
            if (objects == null)
            {
                Debug.LogError("[LeaveChiefEscort] 未找到 Objects");
                return;
            }

            var walk = FindNamed(scene, "VillageWalkArea")?.GetComponent<PolygonCollider2D>();
            Vector3 world = PreferredTeleportWorld;
            if (walk != null)
            {
                if (!walk.OverlapPoint(world))
                {
                    Debug.LogWarning("[LeaveChiefEscort] 建议落点不在 WalkArea，改试备选");
                    world = FallbackTeleportWorld;
                }

                if (!walk.OverlapPoint(world))
                {
                    Debug.LogWarning(
                        "[LeaveChiefEscort] 建议/备选均不在 WalkArea 内，仍摆建议点请手挪（禁止改多边形腾地）");
                    world = PreferredTeleportWorld;
                }
            }

            var existing = FindNamed(scene, TeleportName);
            Transform t;
            if (existing != null)
            {
                t = existing;
            }
            else
            {
                var go = new GameObject(TeleportName);
                t = go.transform;
                t.SetParent(objects, true);
            }

            t.position = world;
            EditorUtility.SetDirty(t.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            bool inside = walk != null && walk.OverlapPoint(t.position);
            Debug.Log(
                "[LeaveChiefEscort] " + TeleportName + " @ " + t.position
                + " OverlapWalkArea=" + inside,
                t);
        }

        private static void SetupDialoguePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ShellPrefabPath) == null)
            {
                Debug.LogError("[LeaveChiefEscort] 壳缺失：" + ShellPrefabPath);
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(TargetPrefabPath);
            }

            if (!AssetDatabase.CopyAsset(ShellPrefabPath, TargetPrefabPath))
            {
                Debug.LogError("[LeaveChiefEscort] CopyAsset 失败");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(TargetPrefabPath);
            try
            {
                root.name = AssetBaseName;

                // 本戏无村长句：若壳带「村长」Actor，关掉以免误显
                var chief = root.transform.Find("村长");
                if (chief != null)
                {
                    chief.gameObject.SetActive(false);
                }

                BindTwoPortraitBlackboard(root);

                if (!TryImportCsvIntoController(root, out var err))
                {
                    Debug.LogError("[LeaveChiefEscort] " + err);
                    return;
                }

                if (!InsertTeleportAfterSegmentA(root))
                {
                    Debug.LogError("[LeaveChiefEscort] 插入黑幕传送失败");
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, TargetPrefabPath);
                Debug.Log("[LeaveChiefEscort] Prefab 已写入：" + TargetPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            var ping = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);
            if (ping != null)
            {
                EditorGUIUtility.PingObject(ping);
            }
        }

        private static void BindTwoPortraitBlackboard(GameObject root)
        {
            var blackboard = root.GetComponent<Blackboard>();
            if (blackboard == null)
            {
                return;
            }

            BindCg(blackboard, root, "GoOutStoryYaerPainting");
            BindCg(blackboard, root, "GushaPainting");
            EditorUtility.SetDirty(blackboard);
        }

        private static void BindCg(Blackboard blackboard, GameObject root, string paintingName)
        {
            var painting = FindDeepChild(root.transform, paintingName);
            if (painting == null)
            {
                Debug.LogWarning("[LeaveChiefEscort] 未找到 " + paintingName, root);
                return;
            }

            var cg = painting.GetComponent<UnityEngine.CanvasGroup>();
            if (cg == null)
            {
                cg = painting.GetComponentInChildren<UnityEngine.CanvasGroup>(true);
            }

            if (cg == null)
            {
                return;
            }

            // BB 变量名 = 物体名（与 Prelude 解析一致）
            if (blackboard.GetVariable(paintingName) == null)
            {
                blackboard.AddVariable(paintingName, cg);
            }
            else
            {
                blackboard.SetVariableValue(paintingName, cg);
            }
        }

        private static bool TryImportCsvIntoController(GameObject prefabRoot, out string error)
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
                csvText = File.ReadAllText(CsvPath, Encoding.UTF8);
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

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, TargetPrefabPath);
            var refPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);

            var prelude = new DialoguePreludeOptions
            {
                FadeDialogueUI = true,
                HideFightingPanelOnStart = true,
                RestoreFightingPanelOnEnd = false,
                FadePortraitCanvasGroups = true,
                PortraitReferencePrefab = refPrefab,
                PreludeFadeDuration = 1.0f
            };

            var tree = DialogueCsvGraphBuilder.TryBuild(
                rows,
                mapping,
                startRowId: null,
                assetName: AssetBaseName,
                prelude,
                hasBodyType);
            if (tree == null)
            {
                error = "DialogueCsvGraphBuilder 建图失败。";
                return false;
            }

            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            {
                AssetDatabase.CreateFolder("Assets/GameRes/DialogueTrees", "Generated");
            }

            var assetPath = $"{GeneratedFolder}/{AssetBaseName}.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueTree>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(tree, assetPath);
            var boundTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(assetPath);

            controller.Validate();
            if (controller.behaviour == null)
            {
                error = "DialogueTree behaviour 未就绪。";
                return false;
            }

            controller.SetBoundGraphReference(boundTree);
            RebindActors(controller, prefabRoot, boundTree);
            EditorUtility.SetDirty(controller);
            return true;
        }

        private static void RebindActors(
            DialogueTreeController controller,
            GameObject root,
            DialogueTree tree)
        {
            BindActor(tree, root, "Yaer", "雅尔");
            BindActor(tree, root, "Gusha", "古莎");
            controller.SetBoundGraphReference(tree);
        }

        private static void BindActor(DialogueTree tree, GameObject root, string childName, string actorKey)
        {
            var t = root.transform.Find(childName);
            if (t == null)
            {
                Debug.LogWarning("[LeaveChiefEscort] 未找到 Actor「" + childName + "」", root);
                return;
            }

            var actor = t.GetComponent<DialogueActorEx>();
            if (actor == null)
            {
                Debug.LogWarning("[LeaveChiefEscort] 「" + childName + "」无 DialogueActorEx", root);
                return;
            }

            tree.SetActorReference(actorKey, actor);
        }

        /// <summary>段 A 末句 → 黑幕传送 → 段 B 首句。</summary>
        private static bool InsertTeleportAfterSegmentA(GameObject root)
        {
            var controller = root.GetComponent<DialogueTreeController>();
            var tree = controller != null ? controller.behaviour as DialogueTree : null;
            if (tree == null)
            {
                return false;
            }

            if (TreeAlreadyHasTeleport(tree))
            {
                Debug.Log("[LeaveChiefEscort] 图内已有黑幕传送，跳过插入。");
                return true;
            }

            StatementNodeEx anchor = null;
            foreach (var node in tree.allNodes.OfType<StatementNodeEx>())
            {
                var text = node.statement != null ? node.statement.text : null;
                if (!string.IsNullOrEmpty(text) && text.Contains(AnchorSubstring))
                {
                    anchor = node;
                    break;
                }
            }

            if (anchor == null)
            {
                Debug.LogError("[LeaveChiefEscort] 未找到锚句：" + AnchorSubstring);
                return false;
            }

            if (anchor.outConnections == null || anchor.outConnections.Count == 0)
            {
                Debug.LogError("[LeaveChiefEscort] 锚句无出边");
                return false;
            }

            var oldOut = anchor.outConnections[0];
            var nextNode = oldOut.targetNode;
            tree.RemoveConnection(oldOut);

            var teleportNode = tree.AddNode<ActionNode>(anchor.position + new Vector2(160f, 100f));
            var task = (BlackFadeTeleportPlayerActionTask)Task.Create(
                typeof(BlackFadeTeleportPlayerActionTask), tree);
            if (task.DestinationObjectName == null)
            {
                task.DestinationObjectName = new BBParameter<string>();
            }

            task.DestinationObjectName.value = TeleportName;
            if (task.SnapCameraToPlayer == null)
            {
                task.SnapCameraToPlayer = new BBParameter<bool>();
            }

            task.SnapCameraToPlayer.value = true;
            if (task.FlushVillageWalkArea == null)
            {
                task.FlushVillageWalkArea = new BBParameter<bool>();
            }

            task.FlushVillageWalkArea.value = true;
            teleportNode.action = task;

            tree.ConnectNodes(anchor, teleportNode);
            tree.ConnectNodes(teleportNode, nextNode);

            controller.SetBoundGraphReference(tree);
            EditorUtility.SetDirty(controller);
            Debug.Log("[LeaveChiefEscort] 已插入 BlackFadeTeleport → 段 B。");
            return true;
        }

        private static bool TreeAlreadyHasTeleport(DialogueTree tree)
        {
            return tree.allNodes.OfType<ActionNode>()
                .Any(n => n.action is BlackFadeTeleportPlayerActionTask);
        }

        // 须写全名：父命名空间 EditorC.Tool 下有 Scene，裸写会 CS0118
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
            if (parent.name == name)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
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
