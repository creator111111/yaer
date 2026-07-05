using DG.Tweening;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("???UI????")]
    public class NormalDialogueBlackMaskTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<float> StartAlpha;
        public BBParameter<float> EndAlpha;
        public BBParameter<float> Duration;
        public BBParameter<bool> EndActonOnAnimationEnd;

        private UnityEngine.CanvasGroup blackFade;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            blackFade = FindBlackFadeCanvasGroup();
        }

        protected override void OnExecute()
        {
            if (blackFade == null)
            {
                EndAction();
                return;
            }

            blackFade.DOKill();
            blackFade.alpha = StartAlpha.value;
            blackFade.DOFade(EndAlpha.value, Duration.value).OnComplete(() =>
            {
                if (EndActonOnAnimationEnd.value)
                {
                    EndAction();
                }
            });
            if (!EndActonOnAnimationEnd.value)
            {
                EndAction();
            }
        }

        protected override string info
        {
            get
            {
                return string.Format("<i>' ????: {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration);
            }
        }
    }
}
