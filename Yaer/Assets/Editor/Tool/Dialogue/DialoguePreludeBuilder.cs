using System.Collections.Generic;
using Game.GameRuntime.Story.Node;
using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 按 <see cref="DialoguePreludeOptions"/> 在 DialogueTree 上创建开场 Action 链与可选的收尾恢复节点。
    /// 节点顺序对齐 Village_KenMuNiStart 分层显现：藏战斗面板 → 立绘淡入 → 对话框 UI 淡入。
    /// </summary>
    public static class DialoguePreludeBuilder
    {
        private const float BaseX = 200f;
        private const float BaseY = 100f;
        private const float PreludeSpacing = 120f;

        /// <summary>
        /// 创建前奏链与可选的 restore 节点。前奏为空时直接返回 true 且 out 参数均为 null。
        /// </summary>
        /// <param name="tree">目标 DialogueTree。</param>
        /// <param name="options">前奏配置（须已通过 <see cref="DialoguePreludeOptions.Validate"/>）。</param>
        /// <param name="entryNode">前奏链首节点（primeNode）。</param>
        /// <param name="tailNode">前奏链尾节点，用于连接 CSV 入口。</param>
        /// <param name="restoreNode">结束时恢复战斗面板节点；未勾选 restore 时为 null。</param>
        public static bool TryCreatePreludeNodes(
            DialogueTree tree,
            DialoguePreludeOptions options,
            out ActionNode entryNode,
            out ActionNode tailNode,
            out ActionNode restoreNode)
        {
            entryNode = null;
            tailNode = null;
            restoreNode = null;

            if (options == null || options.IsEmpty)
            {
                return true;
            }

            var preludeStepCount = CountPreludeChainSteps(options);
            var preludeIndex = 0;
            ActionNode previous = null;

            // 1. 藏战斗面板
            if (options.HideFightingPanelOnStart)
            {
                var node = CreateFightingPanelNode(tree, visible: false, preludeStepCount, ref preludeIndex);
                LinkPreludeStep(ref entryNode, ref tailNode, ref previous, node);
            }

            // 2. 立绘 CanvasGroup 并行淡入（产品：大立绘先于对话框）
            if (options.FadePortraitCanvasGroups)
            {
                if (!DialoguePortraitReferenceResolver.TryResolveCanvasGroupVariableNames(
                        options.PortraitReferencePrefab,
                        out var canvasGroupNames,
                        out var resolveError))
                {
                    Debug.LogError($"[DialoguePreludeBuilder] {resolveError}");
                    return false;
                }

                var node = CreatePortraitFadeNode(
                    tree,
                    canvasGroupNames,
                    options.PreludeFadeDuration,
                    preludeStepCount,
                    ref preludeIndex);
                LinkPreludeStep(ref entryNode, ref tailNode, ref previous, node);
            }

            // 3. 对话框 UI 淡入（是否预亮 Mask 由 options.PrepareMaskAvatarOnFadeIn 决定）
            if (options.FadeDialogueUI)
            {
                var node = CreateDialogueUiFadeNode(
                    tree,
                    options.PreludeFadeDuration,
                    options.PrepareMaskAvatarOnFadeIn,
                    preludeStepCount,
                    ref preludeIndex);
                LinkPreludeStep(ref entryNode, ref tailNode, ref previous, node);
            }

            // 收尾：恢复战斗面板（独立节点，由 GraphBuilder 将各叶子连入）
            if (options.RestoreFightingPanelOnEnd)
            {
                restoreNode = CreateFightingPanelNode(
                    tree,
                    visible: true,
                    preludeStepCount,
                    ref preludeIndex,
                    isEpilogue: true);
            }

            return true;
        }

        /// <summary>统计前奏链内节点数（不含 epilogue restore）。</summary>
        private static int CountPreludeChainSteps(DialoguePreludeOptions options)
        {
            var count = 0;
            if (options.HideFightingPanelOnStart)
            {
                count++;
            }

            if (options.FadePortraitCanvasGroups)
            {
                count++;
            }

            if (options.FadeDialogueUI)
            {
                count++;
            }

            return count;
        }

        /// <summary>前奏链内顺序连线并维护 entry/tail。</summary>
        private static void LinkPreludeStep(
            ref ActionNode entryNode,
            ref ActionNode tailNode,
            ref ActionNode previous,
            ActionNode current)
        {
            if (entryNode == null)
            {
                entryNode = current;
            }

            if (previous != null)
            {
                previous.graph.ConnectNodes(previous, current);
            }

            tailNode = current;
            previous = current;
        }

        /// <summary>
        /// 计算前奏节点坐标：位于对白区域上方，链内自上而下 Y 递增。
        /// </summary>
        private static Vector2 GetPreludePosition(int preludeStepCount, int stepIndex, bool isEpilogue)
        {
            if (isEpilogue)
            {
                // epilogue 由 GraphBuilder 按 CSV 行数另行定位时可覆盖；此处给占位坐标
                return new Vector2(BaseX, BaseY + preludeStepCount * PreludeSpacing + 200f);
            }

            var preludeBaseY = BaseY - preludeStepCount * PreludeSpacing;
            return new Vector2(BaseX, preludeBaseY + stepIndex * PreludeSpacing);
        }

        private static ActionNode CreateFightingPanelNode(
            DialogueTree tree,
            bool visible,
            int preludeStepCount,
            ref int preludeIndex,
            bool isEpilogue = false)
        {
            var position = GetPreludePosition(preludeStepCount, preludeIndex, isEpilogue);
            if (!isEpilogue)
            {
                preludeIndex++;
            }

            var node = tree.AddNode<ActionNode>(position);
            var task = (FightingPanelVisibleActionTask)Task.Create(typeof(FightingPanelVisibleActionTask), tree);
            if (task.Visible == null)
            {
                task.Visible = new BBParameter<bool>();
            }

            task.Visible.value = visible;
            node.action = task;
            return node;
        }

        /// <param name="prepareMaskAvatarOnFadeIn">
        /// true：框淡入前 Apply Mask（KenMuNiStart / Shop 同拍）；
        /// false：空框，等首句 Statement 再出头像（门口三人戏产品）。
        /// </param>
        private static ActionNode CreateDialogueUiFadeNode(
            DialogueTree tree,
            float duration,
            bool prepareMaskAvatarOnFadeIn,
            int preludeStepCount,
            ref int preludeIndex)
        {
            var position = GetPreludePosition(preludeStepCount, preludeIndex, isEpilogue: false);
            preludeIndex++;

            var node = tree.AddNode<ActionNode>(position);
            var task = (NormalDialogueUIAlphaAnimationTaskAction)Task.Create(
                typeof(NormalDialogueUIAlphaAnimationTaskAction),
                tree);

            EnsureFloatParam(ref task.StartAlpha, 0f);
            EnsureFloatParam(ref task.EndAlpha, 1f);
            EnsureFloatParam(ref task.Duration, duration);
            if (task.Delay == null)
            {
                task.Delay = new BBParameter<float>();
            }

            // 对齐 Village_ShopHead 金样：Delay=0.5 空拍后再出框（立绘先于对话框观感）。
            task.Delay.value = 0.5f;

            // 须等淡入结束再进后续节点，否则 Statement 会抢跑、分层观感被抹掉
            if (task.EndActonOnAnimationEnd == null)
            {
                task.EndActonOnAnimationEnd = new BBParameter<bool>();
            }

            task.EndActonOnAnimationEnd.value = true;

            // 是否预亮：默认 true（ShopHead / KenMuNi）；门口须传 false，禁止硬写 true 回潮
            if (task.PrepareMaskAvatarOnFadeIn == null)
            {
                task.PrepareMaskAvatarOnFadeIn = new BBParameter<bool>();
            }

            task.PrepareMaskAvatarOnFadeIn.value = prepareMaskAvatarOnFadeIn;

            if (task.MaskAvatarRole == null)
            {
                task.MaskAvatarRole = new BBParameter<DialogueRoleName>();
            }

            // Head 金样：Role=Yaer(1)；Face=Smug(3)。未勾预亮时不会 Apply，仅作 BB 占位
            task.MaskAvatarRole.value = DialogueRoleName.Yaer;

            if (task.MaskAvatarFace == null)
            {
                task.MaskAvatarFace = new BBParameter<DialogueFaceType>();
            }

            task.MaskAvatarFace.value = DialogueFaceType.Smug;

            node.action = task;
            return node;
        }

        /// <summary>
        /// 创建立绘 ActionList：按参考 Prefab 变量名，并行 CanvasGroupAlpha，并阻塞至淡入结束。
        /// 方案 A：仅写 canvasGroup.name，不复制 Blackboard 引用。
        /// </summary>
        private static ActionNode CreatePortraitFadeNode(
            DialogueTree tree,
            IReadOnlyList<string> canvasGroupVariableNames,
            float duration,
            int preludeStepCount,
            ref int preludeIndex)
        {
            var position = GetPreludePosition(preludeStepCount, preludeIndex, isEpilogue: false);
            preludeIndex++;

            var node = tree.AddNode<ActionNode>(position);
            var actionList = (ActionList)Task.Create(typeof(ActionList), tree);
            // 两立绘同拍并行淡入（与 Village_KenMuNiStart 一致）；若需依次出场可改 Sequence
            actionList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;

            foreach (var variableName in canvasGroupVariableNames)
            {
                var fadeTask = (CanvasGroupAlphaActionTask)Task.Create(typeof(CanvasGroupAlphaActionTask), tree);
                if (fadeTask.canvasGroup == null)
                {
                    fadeTask.canvasGroup = new BBParameter<UnityEngine.CanvasGroup>();
                }

                // 仅绑定变量名字符串；合并进含同名 Blackboard 的 Prefab 后生效
                fadeTask.canvasGroup.name = variableName;
                EnsureFloatParam(ref fadeTask.StartAlpha, 0f);
                EnsureFloatParam(ref fadeTask.EndAlpha, 1f);
                EnsureFloatParam(ref fadeTask.Duration, duration);
                EnsureFloatParam(ref fadeTask.Delay, 0f);
                if (fadeTask.EndActionOnAnimationEnd == null)
                {
                    fadeTask.EndActionOnAnimationEnd = new BBParameter<bool>();
                }

                fadeTask.EndActionOnAnimationEnd.value = true;

                actionList.AddAction(fadeTask);
            }

            node.action = actionList;
            return node;
        }

        private static void EnsureFloatParam(ref BBParameter<float> param, float value)
        {
            if (param == null)
            {
                param = new BBParameter<float>();
            }

            param.value = value;
        }
    }
}
