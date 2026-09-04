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
        /// 渐入时预先 Apply Mask 小头像（Active + 脸），使头像随字幕条 CanvasGroup 同次从 0→1 显现。
        /// 不勾选则保持旧行为（首句 Statement 才 Apply）。
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

            // 方案 A：渐入只出框+头像，清空 Prefab 残留名/正文（如默认「雅尔」），首句 OnSubtitlesRequest 再填字
            if (isFadeIn)
            {
                ClearSubtitleTextsForEmptyFrame();
            }

            // 小头像在 Bottom/Mask 下，随 subtitlesCanvasGroup alpha 乘算；须在淡入前 Active，否则框出了头像空窗
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
        /// 在字幕条仍透明时把 Mask Painting 摆好；淡入过程中与对话框一起显现。
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
