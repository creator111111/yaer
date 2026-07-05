using System.Collections.Generic;
using Game.GameRuntime.Story.Node;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 按 <see cref="DialoguePreludeOptions"/> 在 DialogueTree 上创建开场 Action 链与可选的收尾恢复节点。
    /// 节点顺序对齐 Village_KenMuNiStart：藏战斗面板 → 立绘淡入 → 对话框 UI 淡入。
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

            // 2. 立绘 CanvasGroup 顺序淡入（ActionList）
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

            // 3. 对话框 UI 淡入
            if (options.FadeDialogueUI)
            {
                var node = CreateDialogueUiFadeNode(
                    tree,
                    options.PreludeFadeDuration,
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

        private static ActionNode CreateDialogueUiFadeNode(
            DialogueTree tree,
            float duration,
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

            if (task.EndActonOnAnimationEnd == null)
            {
                task.EndActonOnAnimationEnd = new BBParameter<bool>();
            }

            node.action = task;
            return node;
        }

        /// <summary>
        /// 创建立绘 ActionList：按参考 Prefab 变量名顺序，In Sequence 执行 CanvasGroupAlpha。
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
            actionList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInSequence;

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
                if (fadeTask.EndActionOnAnimationEnd == null)
                {
                    fadeTask.EndActionOnAnimationEnd = new BBParameter<bool>();
                }

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
