using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// CSV 导入工具「开场前奏」勾选配置。
    /// 全部为 false 时 <see cref="IsEmpty"/> 为 true，建图器走阶段 1 原路径，保证向后兼容。
    /// </summary>
    public sealed class DialoguePreludeOptions
    {
        /// <summary>插入对话框 UI 透明度 0→1 动画（<see cref="NormalDialogueUIAlphaAnimationTaskAction"/>）。</summary>
        public bool FadeDialogueUI { get; set; }

        /// <summary>入口插入隐藏战斗面板（FightingPanel Visible=false）。</summary>
        public bool HideFightingPanelOnStart { get; set; }

        /// <summary>
        /// 在图末尾叶子节点之后插入恢复战斗面板（Visible=true）。
        /// 仅当 <see cref="HideFightingPanelOnStart"/> 为 true 时有效。
        /// </summary>
        public bool RestoreFightingPanelOnEnd { get; set; }

        /// <summary>插入立绘 CanvasGroup 顺序淡入（ActionList + CanvasGroupAlphaActionTask）。</summary>
        public bool FadePortraitCanvasGroups { get; set; }

        /// <summary>
        /// 立绘 Blackboard 变量名来源 Prefab（须含 <see cref="NodeCanvas.DialogueTrees.DialogueTreeController"/>）。
        /// 仅当 <see cref="FadePortraitCanvasGroups"/> 为 true 时必填。
        /// </summary>
        public GameObject PortraitReferencePrefab { get; set; }

        /// <summary>淡入时长（秒），与 Village_KenMuNiStart 分层显现默认 1.0 一致。</summary>
        public float PreludeFadeDuration { get; set; } = 1.0f;

        /// <summary>
        /// 是否未启用任何前奏/收尾选项。为 true 时 <see cref="DialogueCsvGraphBuilder"/> 不得进入前奏分支。
        /// </summary>
        public bool IsEmpty =>
            !FadeDialogueUI
            && !HideFightingPanelOnStart
            && !RestoreFightingPanelOnEnd
            && !FadePortraitCanvasGroups;

        /// <summary>创建全 false 默认配置，与阶段 1 行为一致。</summary>
        public static DialoguePreludeOptions CreateDefault()
        {
            return new DialoguePreludeOptions();
        }

        /// <summary>
        /// 校验勾选组合是否合法。失败时 <paramref name="error"/> 为窗口可展示的文案。
        /// </summary>
        public bool Validate(out string error)
        {
            if (RestoreFightingPanelOnEnd && !HideFightingPanelOnStart)
            {
                error = "「结束时恢复战斗面板」须同时勾选「开始时隐藏战斗面板」。";
                return false;
            }

            if (FadePortraitCanvasGroups && PortraitReferencePrefab == null)
            {
                error = "已勾选「立绘淡入」但未指定参考 Prefab，无法解析 Blackboard 中的 CanvasGroup 变量名。";
                return false;
            }

            error = null;
            return true;
        }
    }
}
