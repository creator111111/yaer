using Game.GameMgr;
using Game.GameMgr.Component.UI;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("关闭对话UI")]
    public class NormalDialogueCloseTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<bool> ClearCurrentDialogueScene;

        protected override void OnExecute()
        {
            if (SandboxMode || FormLogic == null)
            {
                EndAction();
                return;
            }

            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm != null)
            {
                uiGm.CloseUIForm(FormLogic.UIForm);
            }

            if (ClearCurrentDialogueScene.value)
            {
                FormLogic.ClearDialogueScene();
            }

            EndAction();
        }
    }
}
