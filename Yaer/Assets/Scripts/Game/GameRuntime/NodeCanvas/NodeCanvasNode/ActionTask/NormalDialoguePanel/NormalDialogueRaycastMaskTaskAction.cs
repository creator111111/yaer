using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("对话框UI全屏点击遮罩启用")]
    public class NormalDialogueRaycastMaskTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<bool> IsOn;

        private NormalDialogueFormNewLogic uiFormLogic;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            this.uiFormLogic = uiFormLogic;
        }

        protected override void OnExecute()
        {
            uiFormLogic.BlockOtherInteraction(IsOn.value);
            EndAction();
        }
    }
}