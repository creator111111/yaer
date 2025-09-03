using DG.Tweening;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
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

        private UnityEngine.CanvasGroup canvasGroup;
        private Sequence seq;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            canvasGroup = uiFormLogic.dialogueUICanvasGroup;
        }

        protected override void OnExecute()
        {
            if (seq != null)
            {
                seq.Kill();
            }
            seq = DOTween.Sequence();
            canvasGroup.DOKill();
            canvasGroup.alpha = StartAlpha.value;
            if (Delay.value >= 0)
            {
                seq.AppendInterval(Delay.value);
            }
            seq.Append(canvasGroup.DOFade(EndAlpha.value, Duration.value).OnComplete(() =>
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

        protected override string info
        {
            get
            {
                return string.Format("<i>' 对话框UI透明度动画: wait {3}s, {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration, Delay);
            }
        }
    }
}