using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("¶Ô»°¿òUI¹Ø±Õ")]
    public class NormalDialogueCloseTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<bool> ClearCurrentDialogueScene;
        private NormalDialogueFormNewLogic uiFormLogic;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            this.uiFormLogic = uiFormLogic;
        }

        protected override void OnExecute()
        {
            GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiFormLogic.UIForm);
            if (ClearCurrentDialogueScene.value)
            {
                uiFormLogic.ClearDialogueScene();
            }
            EndAction();
        }
    }
}