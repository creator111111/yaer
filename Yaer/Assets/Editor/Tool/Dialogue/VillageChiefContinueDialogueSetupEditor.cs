#if UNITY_EDITOR
using System.IO;
using System.Text;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using Game.GameRuntime.UI.FormLogic.Story.Painting.Editor;
using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 「村长家继续对话」成品 Prefab：复用门口初次对话 Setup 流水线，仅改目标路径 / CSV / Asset 名。
    /// 菜单：Tools / Dialogue / Setup Village 村长家继续对话 Prefab
    /// </summary>
    /// <remarks>
    /// 原因（0901）：续聊 CSV + Generated 已有，Prefab 未落盘 → Trigger 必「加载资源失败」。
    /// 壳优先用门口成品（已含三人立绘）；若门口壳缺失则回退 KenMuNiStart。
    /// 替代方案：手工 Copy YAML 改名——易漏 Actor / BB / CSV 图内容，不采用。
    /// </remarks>
    public static class VillageChiefContinueDialogueSetupEditor
    {
        private const string MenuPath = "Tools/Dialogue/Setup Village 村长家继续对话 Prefab";

        /// <summary>优先壳：门口成品（三人立绘已齐）。</summary>
        private const string PreferredShellPrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab";

        /// <summary>回退壳：进村开场（与门口 Setup 同源）。</summary>
        private const string FallbackShellPrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab";

        private const string TargetPrefabPath =
            "Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab";
        private const string CsvPath = "Assets/Dialog/Village_村长家继续对话.csv";
        private const string GeneratedFolder = "Assets/GameRes/DialogueTrees/Generated";
        private const string AssetBaseName = "Village_村长家继续对话";

        // DialogueRoleName：None=0 … Chief=11（只能追加末尾）
        private const int ChiefRoleEnumIndex = (int)DialogueRoleName.Chief;

        /// <summary>
        /// Agent / 批处理：在工程根写入 <c>Library/ChiefContinueSetup.request</c> 后，
        /// 已打开的 Unity 编译完成会自动跑一次 Setup。
        /// </summary>
        private const string AutoSetupRequestFileName = "ChiefContinueSetup.request";

        [InitializeOnLoadMethod]
        private static void AutoSetupFromRequestFile()
        {
            EditorApplication.delayCall += TryConsumeAutoSetupRequest;
        }

        private static void TryConsumeAutoSetupRequest()
        {
            // 相对路径依赖进程 CWD 不可靠；以 Application.dataPath 锚到工程根
            var abs = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "Library", AutoSetupRequestFileName));
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
                Debug.LogWarning("[ChiefContinueSetup] 无法删除自动请求文件：" + ex.Message);
                return;
            }

            Debug.Log("[ChiefContinueSetup] 检测到 Library/ChiefContinueSetup.request，自动执行 Setup…");
            SetupFromMenu();
        }

        [MenuItem(MenuPath)]
        public static void SetupFromMenu()
        {
            // 1) UI 大立绘（替换 SR）
            var chiefPaintingPrefab = ChiefPaintingSetupEditor.CreateOrUpdatePrefab();
            if (chiefPaintingPrefab == null)
            {
                Debug.LogError("[ChiefContinueSetup] ChiefPainting UI 化失败。");
                return;
            }

            var shellPath = ResolveShellPrefabPath();
            if (shellPath == null)
            {
                Debug.LogError(
                    "[ChiefContinueSetup] 壳 Prefab 不存在（门口成品与 KenMuNiStart 均缺失）。");
                return;
            }

            // 2) 拷壳 → 目标路径
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(TargetPrefabPath);
            }

            if (!AssetDatabase.CopyAsset(shellPath, TargetPrefabPath))
            {
                Debug.LogError($"[ChiefContinueSetup] CopyAsset 失败：{shellPath} → {TargetPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(TargetPrefabPath);
            try
            {
                root.name = AssetBaseName;

                // 3) 嵌村长 Actor + ChiefPainting；绑三路 BB；alpha=0 待前奏
                EnsureChiefActorAndPainting(root, chiefPaintingPrefab);
                BindThreePortraitBlackboard(root);
                NudgePortraitLayout(root);

                // 4) CSV → 图（Fighting 藏 → 三立绘淡入 → 对话框）
                if (!TryImportCsvIntoController(root, out var err))
                {
                    Debug.LogError("[ChiefContinueSetup] " + err);
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, TargetPrefabPath);
                Debug.Log($"[ChiefContinueSetup] Prefab 已写入：{TargetPrefabPath}（壳={shellPath}）");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var ping = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);
            if (ping != null)
            {
                EditorGUIUtility.PingObject(ping);
            }
        }

        /// <summary>优先门口成品，否则 KenMuNiStart；都没有返回 null。</summary>
        private static string ResolveShellPrefabPath()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PreferredShellPrefabPath) != null)
            {
                return PreferredShellPrefabPath;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(FallbackShellPrefabPath) != null)
            {
                return FallbackShellPrefabPath;
            }

            return null;
        }

        /// <summary>创建「村长」Actor（RoleName.Chief）并嵌套 UI ChiefPainting。</summary>
        private static void EnsureChiefActorAndPainting(GameObject root, GameObject chiefPaintingPrefab)
        {
            var existing = root.transform.Find("村长");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var actorGo = new GameObject("村长", typeof(RectTransform));
            actorGo.layer = root.layer;
            var actorRt = actorGo.GetComponent<RectTransform>();
            actorRt.SetParent(root.transform, false);
            actorRt.anchorMin = actorRt.anchorMax = actorRt.pivot = new Vector2(0.5f, 0.5f);
            actorRt.sizeDelta = new Vector2(100f, 100f);
            actorRt.anchoredPosition = Vector2.zero;

            var actor = actorGo.AddComponent<DialogueActorEx>();
            var so = new SerializedObject(actor);
            so.FindProperty("_name").stringValue = ChiefCsvDefaults.ChiefActorName;
            so.FindProperty("_roleName").enumValueIndex = ChiefRoleEnumIndex;
            so.FindProperty("_portrait").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();

            var paintingInstance = (GameObject)PrefabUtility.InstantiatePrefab(chiefPaintingPrefab, actorRt);
            paintingInstance.name = "ChiefPainting";
            paintingInstance.SetActive(true);

            var painting = paintingInstance.GetComponent<ChiefMaskPainting>();
            if (painting != null)
            {
                painting.EditorResetDefaultActiveState();
            }

            var cg = paintingInstance.GetComponent<UnityEngine.CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        /// <summary>BB 绑三路 CanvasGroup，供 Prelude 解析变量名并淡入。</summary>
        private static void BindThreePortraitBlackboard(GameObject root)
        {
            var blackboard = root.GetComponent<Blackboard>();
            if (blackboard == null)
            {
                Debug.LogWarning("[ChiefContinueSetup] Prefab 无 Blackboard。", root);
                return;
            }

            BindCg(blackboard, root, "GoOutStoryYaerPainting");
            BindCg(blackboard, root, "GushaPainting");
            BindCg(blackboard, root, "ChiefPainting");
            EditorUtility.SetDirty(blackboard);
        }

        private static void BindCg(Blackboard blackboard, GameObject root, string paintingName)
        {
            var painting = FindDeepChild(root.transform, paintingName);
            if (painting == null)
            {
                Debug.LogWarning($"[ChiefContinueSetup] 未找到 {paintingName}，跳过 BB。", root);
                return;
            }

            var cg = painting.GetComponent<UnityEngine.CanvasGroup>();
            if (cg == null)
            {
                Debug.LogWarning($"[ChiefContinueSetup] {paintingName} 无 CanvasGroup。", painting);
                return;
            }

            cg.alpha = 0f;

            if (blackboard.GetVariable(paintingName) != null)
            {
                blackboard.SetVariableValue(paintingName, cg);
            }
            else
            {
                blackboard.AddVariable(paintingName, cg);
            }

            EditorUtility.SetDirty(cg);
            Debug.Log($"[ChiefContinueSetup] BB {paintingName} → CanvasGroup", cg);
        }

        /// <summary>
        /// 三人占位对齐门口定稿（与 Door Setup 同源 <see cref="VillageChiefDialoguePortraitLayout"/>）。
        /// <para>原因（0901）：旧 Nudge 写死雅 X=-380，且未写 Actor「村长」位姿 → 续聊与门口站位不一致。</para>
        /// </summary>
        private static void NudgePortraitLayout(GameObject root)
        {
            VillageChiefDialoguePortraitLayout.ApplyToDialogueRoot(root);
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

            // 先落盘一次，使 PortraitReferencePrefab 能读到含 ChiefPainting 的 BB
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, TargetPrefabPath);
            var refPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);

            var prelude = new DialoguePreludeOptions
            {
                FadeDialogueUI = true,
                HideFightingPanelOnStart = true,
                RestoreFightingPanelOnEnd = false,
                FadePortraitCanvasGroups = true,
                PortraitReferencePrefab = refPrefab,
                PreludeFadeDuration = 1.0f,
                // 0902：对齐门口空框；禁止 Setup 回潮预亮（与 Door Setup 一致）
                PrepareMaskAvatarOnFadeIn = false
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
                error = "DialogueCsvGraphBuilder 建图失败（见 Console）。";
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
            if (tree == null)
            {
                return;
            }

            BindActor(tree, root, "Yaer", "雅尔");
            BindActor(tree, root, "Gusha", "古莎");
            BindActor(tree, root, "村长", ChiefCsvDefaults.ChiefActorName);

            controller.SetBoundGraphReference(tree);
        }

        private static void BindActor(DialogueTree tree, GameObject root, string childName, string actorKey)
        {
            var t = root.transform.Find(childName);
            if (t == null)
            {
                Debug.LogWarning($"[ChiefContinueSetup] 未找到 Actor 子物体「{childName}」。", root);
                return;
            }

            var actor = t.GetComponent<DialogueActorEx>();
            if (actor == null)
            {
                Debug.LogWarning($"[ChiefContinueSetup] 「{childName}」无 DialogueActorEx。", t);
                return;
            }

            tree.SetActorReference(actorKey, actor);
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
