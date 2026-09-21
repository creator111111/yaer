using DG.Tweening;
using Game.GameRuntime.UI.FormLogic.Story;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Enum.Dialogue;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("对话框UI透明度动画")]
    public class NormalDialogueUIAlphaAnimationTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<float> StartAlpha;
        public BBParameter<float> EndAlpha;
        public BBParameter<float> Duration;
        public BBParameter<float> Delay;
        public BBParameter<bool> EndActonOnAnimationEnd;

        /// <summary>
        /// 渐入时是否预先 Apply Mask 小头像（与框同拍）。
        /// 村庄产品是空框：默认不勾，第一句才出脸。只有显式 true 才允许在出字之前亮脸。
        /// 不要在淡入里无条件 Apply。
        /// </summary>
        public BBParameter<bool> PrepareMaskAvatarOnFadeIn;

        public BBParameter<DialogueRoleName> MaskAvatarRole;
        public BBParameter<DialogueFaceType> MaskAvatarFace;

        private UnityEngine.CanvasGroup canvasGroup;
        private Sequence seq;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            canvasGroup = GetDialogueUICanvasGroup();
        }

        protected override void OnExecute()
        {
            if (canvasGroup == null)
            {
                EndAction();
                return;
            }

            if (seq != null)
            {
                seq.Kill();
            }
            seq = DOTween.Sequence();
            canvasGroup.DOKill();

            float startA = StartAlpha != null ? StartAlpha.value : 0f;
            float endA = EndAlpha != null ? EndAlpha.value : 1f;
            bool isFadeIn = endA > startA;

            // 渐入时若 subtitlesGroup 仍 Inactive，DOFade 在幕后跑完 → 首句 Active 时硬切。
            if (isFadeIn && !canvasGroup.gameObject.activeSelf)
            {
                canvasGroup.gameObject.SetActive(true);
            }

            // 方案 A：渐入只出空框，清空 Prefab 残留名/正文（如默认「雅尔」），首句 OnSubtitlesRequest 再填字
            if (isFadeIn)
            {
                ClearSubtitleTextsForEmptyFrame();
                // 字幕条重新打开时，子物体保持上次的亮/灭。先藏光上一场留下的小头像。
                // 预亮分支仍保留：以后真要「框和头像一起出」的图，显式把开关设为 true。
                // 替代方案：只在 OnDialogueStarted 藏一次——没有淡入节点的图靠它；
                // 有淡入的图在 SetActive 之后仍可能把旧脸带出来，所以这里必须再藏一次。
                HideMaskAvatarsForEmptyFrame();
            }

            // 小头像在 Bottom/Mask 下，随 subtitlesCanvasGroup alpha 乘算。
            // 只有显式预亮才在淡入前 Active；村庄产品不能在这里无条件 Apply。
            if (isFadeIn && PrepareMaskAvatarOnFadeIn != null && PrepareMaskAvatarOnFadeIn.value)
            {
                PrepareMaskAvatarForFadeIn();
            }

            canvasGroup.alpha = startA;
            if (Delay.value >= 0)
            {
                seq.AppendInterval(Delay.value);
            }
            seq.Append(canvasGroup.DOFade(endA, Duration.value).OnComplete(() =>
            {
                if (EndActonOnAnimationEnd.value)
                {
                    EndAction();
                }
            }));
            if (!EndActonOnAnimationEnd.value)
            {
                EndAction();
            }
        }

        /// <summary>
        /// 渐入前清空名字与正文，避免露出 Prefab 默认「雅尔」等残留字。
        /// 首句 Statement → OnSubtitlesRequest 会再写入正式台词与演员名。
        /// </summary>
        void ClearSubtitleTextsForEmptyFrame()
        {
            var dialogueUi = FormLogic != null ? FormLogic.DialogueUI : SandboxDialogueUI;
            if (dialogueUi == null)
            {
                return;
            }

            if (dialogueUi.actorName != null)
            {
                dialogueUi.actorName.text = string.Empty;
            }

            if (dialogueUi.actorSpeech != null)
            {
                dialogueUi.actorSpeech.text = string.Empty;
            }
        }

        /// <summary>
        /// 淡入前藏掉 Mask 小头像。村庄产品是空框，不能在这里无条件 Apply。
        /// 预亮开关为 true 时，紧接着的 <see cref="PrepareMaskAvatarForFadeIn"/> 会再亮指定脸。
        /// </summary>
        void HideMaskAvatarsForEmptyFrame()
        {
            var presenter = canvasGroup.GetComponentInChildren<DialogueMaskAvatarPresenter>(true);
            if (presenter == null)
            {
                return;
            }

            presenter.HideAllMaskAvatars();
        }

        /// <summary>
        /// 在字幕条仍透明时把 Mask Painting 摆好；淡入过程中与对话框一起显现。
        /// 仅当 <see cref="PrepareMaskAvatarOnFadeIn"/> 为 true 时调用。
        /// </summary>
        void PrepareMaskAvatarForFadeIn()
        {
            var presenter = canvasGroup.GetComponentInChildren<DialogueMaskAvatarPresenter>(true);
            if (presenter == null)
            {
                return;
            }

            var role = MaskAvatarRole != null ? MaskAvatarRole.value : DialogueRoleName.None;
            var face = MaskAvatarFace != null ? MaskAvatarFace.value : DialogueFaceType.None;
            if (role == DialogueRoleName.None)
            {
                return;
            }

            presenter.Apply(role, face);
        }

        protected override string info
        {
            get
            {
                return string.Format("<i>' 对话框UI透明度动画: wait {3}s, {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration, Delay);
            }
        }
    }
}
