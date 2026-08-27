using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.GameRuntime.Story.Node;
using Game.GameRuntime.Story.Node;
using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 将校验后的 <see cref="DialogueRow"/> 列表构建为 NodeCanvas <see cref="DialogueTree"/> 资产数据。
    /// 使用 Graph 官方 AddNode / ConnectNodes API；对白节点统一为项目扩展的 StatementNodeEx。
    /// </summary>
    public static class DialogueCsvGraphBuilder
    {
        private const float BaseX = 200f;
        private const float BaseY = 100f;
        private const float RowSpacing = 120f;

        // NodeCanvas 的 Node/DTNode 并非 UnityEngine.Object，不能用 SerializedObject，需反射写私有字段。
        private static readonly FieldInfo ActorNameField =
            typeof(DTNode).GetField("_actorName", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ActorParameterIdField =
            typeof(DTNode).GetField("_actorParameterID", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo AvailableChoicesField =
            typeof(MultipleChoiceNode).GetField("availableChoices", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// 构建 DialogueTree（阶段 1 默认：无前奏）。失败时返回 null 并在 Console 输出错误。
        /// </summary>
        public static DialogueTree TryBuild(
            IReadOnlyList<DialogueRow> rows,
            DialogueSpeakerMapping mapping,
            int? startRowId,
            string assetName)
        {
            return TryBuild(rows, mapping, startRowId, assetName, DialoguePreludeOptions.CreateDefault(), false);
        }

        /// <summary>
        /// 构建 DialogueTree，可选插入开场前奏 Action 链。前奏为空时与四参数重载结果一致。
        /// </summary>
        /// <param name="preludeOptions">前奏配置；null 视为 <see cref="DialoguePreludeOptions.CreateDefault"/>。</param>
        public static DialogueTree TryBuild(
            IReadOnlyList<DialogueRow> rows,
            DialogueSpeakerMapping mapping,
            int? startRowId,
            string assetName,
            DialoguePreludeOptions preludeOptions,
            bool hasBodyTypeColumn = false)
        {
            preludeOptions ??= DialoguePreludeOptions.CreateDefault();

            if (!preludeOptions.IsEmpty && !preludeOptions.Validate(out var preludeValidationError))
            {
                Debug.LogError($"[DialogueCsvGraphBuilder] {preludeValidationError}");
                return null;
            }

            if (rows == null || rows.Count == 0)
            {
                Debug.LogError("[DialogueCsvGraphBuilder] 行数据为空，无法建图。");
                return null;
            }

            if (mapping == null)
            {
                Debug.LogError("[DialogueCsvGraphBuilder] Speaker 映射未配置。");
                return null;
            }

            var tree = ScriptableObject.CreateInstance<DialogueTree>();
            tree.name = string.IsNullOrWhiteSpace(assetName) ? "GeneratedDialogueTree" : assetName;

            // Step0（可选）：开场前奏 Action 链
            ActionNode preludeEntry = null;
            ActionNode preludeTail = null;
            ActionNode restoreNode = null;
            if (!preludeOptions.IsEmpty)
            {
                if (!DialoguePreludeBuilder.TryCreatePreludeNodes(
                        tree,
                        preludeOptions,
                        out preludeEntry,
                        out preludeTail,
                        out restoreNode))
                {
                    UnityEngine.Object.DestroyImmediate(tree);
                    return null;
                }
            }

            // Step3：收集 Actor 参数（去重）
            if (!TrySetupActorParameters(tree, rows, mapping, out var speakerErrors))
            {
                foreach (var err in speakerErrors)
                {
                    Debug.LogError($"[DialogueCsvGraphBuilder] {err}");
                }

                UnityEngine.Object.DestroyImmediate(tree);
                return null;
            }

            // Step4：店行 Body/Face 继承表（按 CSV 行序）
            var shopPortraitMap = BuildShopkeeperPortraitMap(
                rows,
                mapping,
                hasBodyTypeColumn || rowsHaveBodyTypeColumn(rows));

            // Step4b：第一轮 — 创建节点
            var nodeMap = new Dictionary<int, Node>();
            // Anim 行：入边接到 Action，出边从 Statement 接出
            var animStatementMap = new Dictionary<int, Node>();
            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var position = new Vector2(BaseX, BaseY + index * RowSpacing);
                Node node;

                if (DialogueCsvParser.IsAnimType(row.type))
                {
                    // Play → Statement：Extra=动画键（BB 名），Text=字幕
                    var playNode = CreatePlayUiAnimatorNode(tree, row, position);
                    if (playNode == null)
                    {
                        UnityEngine.Object.DestroyImmediate(tree);
                        return null;
                    }

                    var statementPos = position + new Vector2(220f, 0f);
                    var statementNode = CreateStatementNode(tree, row, mapping, position, shopPortraitMap);
                    if (statementNode == null)
                    {
                        UnityEngine.Object.DestroyImmediate(tree);
                        return null;
                    }

                    tree.ConnectNodes(playNode, statementNode);
                    nodeMap[row.id] = playNode;
                    animStatementMap[row.id] = statementNode;
                    continue;
                }

                if (DialogueCsvParser.IsDialogueType(row.type))
                {
                    node = CreateStatementNode(tree, row, mapping, position, shopPortraitMap);
                }
                else if (DialogueCsvParser.IsChoiceType(row.type))
                {
                    node = CreateChoiceNode(tree, row, mapping, position);
                }
                else
                {
                    Debug.LogError($"[DialogueCsvGraphBuilder] 未支持的 Type：{row.type}（ID {row.id}）");
                    UnityEngine.Object.DestroyImmediate(tree);
                    return null;
                }

                if (node == null)
                {
                    UnityEngine.Object.DestroyImmediate(tree);
                    return null;
                }

                nodeMap[row.id] = node;
            }

            // Step5：第二轮 — 连线
            foreach (var row in rows)
            {
                if (!nodeMap.TryGetValue(row.id, out var sourceNode))
                {
                    continue;
                }

                // Anim：出边从字幕 Statement 出发，保证先播完动画再点继续
                if (animStatementMap.TryGetValue(row.id, out var animStatement))
                {
                    sourceNode = animStatement;
                }

                var nextIds = DialogueCsvParser.SplitNextTargets(row.next);
                if (nextIds.Count == 0)
                {
                    continue;
                }

                if (DialogueCsvParser.IsChoiceType(row.type))
                {
                    for (var branchIndex = 0; branchIndex < nextIds.Count; branchIndex++)
                    {
                        var nextTarget = nextIds[branchIndex];

                        // END 分支：挂「恢复战斗面板」叶子 Action，与手改 Prefab 方案 A 一致
                        if (nextTarget == DialogueCsvParser.EndBranchSentinel)
                        {
                            var endNode = CreateChoiceEndBranchNode(tree, row, branchIndex);
                            tree.ConnectNodes(sourceNode, endNode, sourceIndex: branchIndex);
                            continue;
                        }

                        if (!nodeMap.TryGetValue(nextTarget, out var targetNode))
                        {
                            Debug.LogError(
                                $"[DialogueCsvGraphBuilder] Choice ID {row.id} 无法连接至 ID {nextTarget}。");
                            UnityEngine.Object.DestroyImmediate(tree);
                            return null;
                        }

                        tree.ConnectNodes(sourceNode, targetNode, sourceIndex: branchIndex);
                    }
                }
                else
                {
                    if (nextIds.Count > 1)
                    {
                        Debug.LogWarning(
                            $"[DialogueCsvGraphBuilder] ID {row.id} 的 Next 含多个目标，仅连接第一个：{nextIds[0]}");
                    }

                    if (nodeMap.TryGetValue(nextIds[0], out var targetNode))
                    {
                        tree.ConnectNodes(sourceNode, targetNode);
                    }
                }
            }

            var csvEntryId = startRowId ?? rows.Min(r => r.id);
            if (!nodeMap.TryGetValue(csvEntryId, out var csvEntryNode))
            {
                Debug.LogError($"[DialogueCsvGraphBuilder] 起始 ID {csvEntryId} 不存在于节点映射中。");
                UnityEngine.Object.DestroyImmediate(tree);
                return null;
            }

            // Step5b（可选）：前奏尾 → CSV 入口；各叶子 → restore
            if (preludeTail != null)
            {
                tree.ConnectNodes(preludeTail, csvEntryNode);
            }

            if (restoreNode != null)
            {
                restoreNode.position = new Vector2(BaseX, BaseY + rows.Count * RowSpacing + RowSpacing);
                foreach (var leaf in FindLeafNodes(tree, restoreNode))
                {
                    tree.ConnectNodes(leaf, restoreNode);
                }
            }

            // Step6：primeNode — 有前奏时为前奏首节点，否则为 CSV 入口（与阶段 1 一致）
            tree.primeNode = preludeEntry != null ? preludeEntry : csvEntryNode;

            if (!preludeOptions.IsEmpty && preludeOptions.FadePortraitCanvasGroups)
            {
                Debug.LogWarning(
                    "[DialogueCsvGraphBuilder] 已生成立绘淡入节点（变量名来自参考 Prefab）。" +
                    "须将本图合并进含同名 Blackboard 的 Prefab 后，立绘淡入方可在实机生效。");
            }

            return tree;
        }

        /// <summary>找出所有无出边节点（对话结束叶子），排除 restore 节点自身。</summary>
        private static IEnumerable<Node> FindLeafNodes(DialogueTree tree, Node excludeNode)
        {
            foreach (var node in tree.allNodes)
            {
                if (node == excludeNode)
                {
                    continue;
                }

                if (node.outConnections == null || node.outConnections.Count == 0)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// 按 Speaker 列收集映射后的 Actor 名，写入 tree.actorParameters。
        /// </summary>
        private static bool TrySetupActorParameters(
            DialogueTree tree,
            IReadOnlyList<DialogueRow> rows,
            DialogueSpeakerMapping mapping,
            out List<string> errors)
        {
            errors = new List<string>();
            var actorNames = new HashSet<string>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.speaker))
                {
                    continue;
                }

                if (!mapping.TryResolve(row.speaker, out var actorName))
                {
                    errors.Add($"Speaker「{row.speaker}」（ID {row.id}）未在映射表中找到，导入已中止。");
                    continue;
                }

                actorNames.Add(actorName);
            }

            if (errors.Count > 0)
            {
                return false;
            }

            foreach (var actorName in actorNames.OrderBy(n => n, StringComparer.Ordinal))
            {
                if (tree.GetParameterByName(actorName) == null)
                {
                    tree.actorParameters.Add(new DialogueTree.ActorParameter(actorName));
                }
            }

            return true;
        }

        private static StatementNodeEx CreateStatementNode(
            DialogueTree tree,
            DialogueRow row,
            DialogueSpeakerMapping mapping,
            Vector2 position,
            Dictionary<int, (ShopkeeperBodyType body, ShopkeeperFaceType face)> shopPortraitMap)
        {
            if (!mapping.TryResolve(row.speaker, out var actorName))
            {
                Debug.LogError($"[DialogueCsvGraphBuilder] ID {row.id} Speaker 映射失败：{row.speaker}");
                return null;
            }

            var node = tree.AddNode<StatementNodeEx>(position);
            node.statement.text = row.text ?? string.Empty;

            if (ShopkeeperCsvDefaults.IsShopkeeperActor(actorName))
            {
                if (!shopPortraitMap.TryGetValue(row.id, out var portrait))
                {
                    portrait = (ShopkeeperBodyType.Normal, ShopkeeperFaceType.Face1);
                }

                if (node.UseShopkeeperPortrait == null)
                {
                    node.UseShopkeeperPortrait = new BBParameter<bool>();
                }

                if (node.ShopBody == null)
                {
                    node.ShopBody = new BBParameter<ShopkeeperBodyType>();
                }

                if (node.ShopFace == null)
                {
                    node.ShopFace = new BBParameter<ShopkeeperFaceType>();
                }

                node.UseShopkeeperPortrait.value = true;
                node.ShopBody.value = portrait.body;
                node.ShopFace.value = portrait.face;

                if (node.FaceType == null)
                {
                    node.FaceType = new BBParameter<DialogueFaceType>();
                }

                node.FaceType.value = DialogueFaceType.None;
            }
            else
            {
                // FaceType：CSV 第 7 列或按 Actor 名默认（雅尔→Smile，其它→Normal）
                if (!DialogueFaceTypeCsvDefaults.TryResolve(row.faceType, actorName, out var resolvedFace))
                {
                    Debug.LogError(
                        $"[DialogueCsvGraphBuilder] ID {row.id} FaceType「{row.faceType}」无法解析为 DialogueFaceType。");
                    return null;
                }

                if (node.FaceType == null)
                {
                    node.FaceType = new BBParameter<DialogueFaceType>();
                }

                node.FaceType.value = resolvedFace;

                if (node.UseShopkeeperPortrait == null)
                {
                    node.UseShopkeeperPortrait = new BBParameter<bool>();
                }

                node.UseShopkeeperPortrait.value = false;
            }

            SetNodeActor(node, tree, actorName);
            return node;
        }

        /// <summary>按 CSV 行序累计店行 Body/Face；空列继承上一句。</summary>
        private static Dictionary<int, (ShopkeeperBodyType body, ShopkeeperFaceType face)> BuildShopkeeperPortraitMap(
            IReadOnlyList<DialogueRow> rows,
            DialogueSpeakerMapping mapping,
            bool hasBodyTypeColumn)
        {
            var result = new Dictionary<int, (ShopkeeperBodyType, ShopkeeperFaceType)>();
            var body = ShopkeeperBodyType.Normal;
            var face = ShopkeeperFaceType.Face1;

            foreach (var row in rows)
            {
                if (!DialogueCsvParser.IsDialogueType(row.type) && !DialogueCsvParser.IsAnimType(row.type))
                {
                    continue;
                }

                if (!ShopkeeperCsvDefaults.IsShopkeeperRow(row, mapping))
                {
                    continue;
                }

                if (!ShopkeeperCsvDefaults.ApplyFaceInheritance(row.faceType, ref face))
                {
                    Debug.LogWarning(
                        $"[DialogueCsvGraphBuilder] ID {row.id} 店行 Face 继承失败，保持 {face}。");
                }

                if (hasBodyTypeColumn)
                {
                    if (!ShopkeeperCsvDefaults.ApplyBodyInheritance(row.bodyType, ref body))
                    {
                        Debug.LogWarning(
                            $"[DialogueCsvGraphBuilder] ID {row.id} 店行 Body 继承失败，保持 {body}。");
                    }
                }
                else
                {
                    body = ShopkeeperBodyType.Normal;
                }

                result[row.id] = (body, face);
            }

            return result;
        }

        private static bool rowsHaveBodyTypeColumn(IReadOnlyList<DialogueRow> rows)
        {
            // Import 窗口经 Parser 传出 hasBodyTypeColumn 更准；此处兜底：任一行 bodyType 非空视为有列。
            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row.bodyType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Anim 行：生成 PlayUiAnimator Action。BB 变量名 = Extra（如 Anim_Gusha）；不序列化场景引用。
        /// </summary>
        private static ActionNode CreatePlayUiAnimatorNode(DialogueTree tree, DialogueRow row, Vector2 position)
        {
            var animKey = row.extra?.Trim();
            if (string.IsNullOrEmpty(animKey))
            {
                Debug.LogError($"[DialogueCsvGraphBuilder] Anim ID {row.id} Extra 为空。");
                return null;
            }

            var node = tree.AddNode<ActionNode>(position);
            // 与 PreludeBuilder 一致：用 Task.Create 挂到 Graph，保证序列化/Owner 正确。
            var playTask = (PlayUiAnimatorActionTask)Task.Create(typeof(PlayUiAnimatorActionTask), tree);
            playTask.animator = new BBParameter<Animator> { name = animKey };
            playTask.fallbackObjectName = new BBParameter<string> { value = animKey };
            playTask.stateName = new BBParameter<string> { value = "Play" };
            playTask.waitUntilFinish = new BBParameter<bool> { value = true };
            playTask.hideWhenFinished = new BBParameter<bool> { value = true };
            node.action = playTask;
            return node;
        }

        /// <summary>
        /// 创建 MultipleChoiceNode，按 Extra 列填充 availableChoices（私有列表，用反射）。
        /// </summary>
        private static MultipleChoiceNode CreateChoiceNode(
            DialogueTree tree,
            DialogueRow row,
            DialogueSpeakerMapping mapping,
            Vector2 position)
        {
            var node = tree.AddNode<MultipleChoiceNode>(position);

            if (!string.IsNullOrWhiteSpace(row.text))
            {
                node.comments = row.text;
            }

            if (!string.IsNullOrWhiteSpace(row.speaker))
            {
                if (!mapping.TryResolve(row.speaker, out var actorName))
                {
                    Debug.LogError($"[DialogueCsvGraphBuilder] Choice ID {row.id} Speaker 映射失败：{row.speaker}");
                    return null;
                }

                SetNodeActor(node, tree, actorName);
            }

            var choiceTexts = DialogueCsvParser.SplitPipeList(row.extra);
            SetChoiceTexts(node, choiceTexts);
            return node;
        }

        /// <summary>
        /// 通过反射写入 DTNode 私有字段，与 NodeCanvas 序列化结构一致。
        /// </summary>
        private static void SetNodeActor(DTNode node, DialogueTree tree, string actorParameterName)
        {
            var param = tree.GetParameterByName(actorParameterName);
            ActorNameField.SetValue(node, actorParameterName);
            ActorParameterIdField.SetValue(node, param != null ? param.ID : string.Empty);
        }

        /// <summary>
        /// Choice 的 END 分支：恢复战斗面板后无出边，<see cref="DialogueTree.Continue"/> 即 Success 结束。
        /// 替代方案：不连任何节点仅靠 Continue 越界 Stop——但 MultipleChoice 要求至少一条出边，故用叶子 Action。
        /// </summary>
        private static ActionNode CreateChoiceEndBranchNode(DialogueTree tree, DialogueRow row, int branchIndex)
        {
            var y = BaseY + (row.id + branchIndex) * RowSpacing + RowSpacing;
            var endNode = tree.AddNode<ActionNode>(new Vector2(BaseX + 80f, y));
            endNode.action = new FightingPanelVisibleActionTask
            {
                Visible = new BBParameter<bool> { value = true }
            };
            return endNode;
        }

        /// <summary>
        /// 扩容并写入 MultipleChoiceNode.availableChoices 各选项的中文文案。
        /// </summary>
        private static void SetChoiceTexts(MultipleChoiceNode node, IReadOnlyList<string> choiceTexts)
        {
            var choices = (List<MultipleChoiceNode.Choice>)AvailableChoicesField.GetValue(node);
            choices.Clear();

            foreach (var text in choiceTexts)
            {
                choices.Add(new MultipleChoiceNode.Choice(new Statement(text ?? string.Empty)));
            }
        }
    }
}
