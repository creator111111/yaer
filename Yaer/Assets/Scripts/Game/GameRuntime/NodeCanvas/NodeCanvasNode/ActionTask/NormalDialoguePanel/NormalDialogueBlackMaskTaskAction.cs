using DG.Tweening;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("¶Ô»°¿òUIºÚÄ»¶¯»­")]
    public class NormalDialogueBlackMaskTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<float> StartAlpha;
        public BBParameter<float> EndAlpha;
        public BBParameter<float> Duration;
        public BBParameter<bool> EndActonOnAnimationEnd;

        private UnityEngine.CanvasGroup blackFade;
        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            blackFade = uiFormLogic.BlackFadeCanvasGroup;
        }

        protected override void OnExecute()
        {
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
                return string.Format("<i>' ºÚÄ»¶¯»­: {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration);
            }
        }
    }
}