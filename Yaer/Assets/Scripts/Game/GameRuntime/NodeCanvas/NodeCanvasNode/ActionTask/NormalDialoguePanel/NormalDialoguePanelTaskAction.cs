using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using UnityEngine;
using GameFramework.UnityRuntime.UI;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;

namespace Game.GameRuntime.Story.Node
{
    public abstract class NormalDialoguePanelTaskAction : ActionTask
    {
        protected override string OnInit()
        {
            string panelPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(panelPath);
            if (uiForm == null)
            {
                Debug.LogError($"NormalDialogueNewPanelÎ´´ò¿ª");
            }
            else
            {
                OnGetUILogic(uiForm.Logic as NormalDialogueFormNewLogic);
            }
            return base.OnInit();
        }

        protected virtual void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {

        }
    }
}