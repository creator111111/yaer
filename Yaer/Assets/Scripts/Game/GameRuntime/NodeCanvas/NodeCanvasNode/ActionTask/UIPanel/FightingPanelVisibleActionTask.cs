using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("UI")]
    [Name("FightingPanel显示开关")]
    public class FightingPanelVisibleActionTask : ActionTask
    {
        public BBParameter<bool> Visible;

        private FightingFormLogic formLogic;

        protected override string OnInit()
        {
            string panelPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(panelPath);
            if (uiForm == null)
            {
                Debug.LogError($"FightingPanel未打开");
            }
            else
            {
                formLogic = uiForm.Logic as FightingFormLogic;
            }

            return base.OnInit();
        }

        protected override void OnExecute()
        {
            if (formLogic != null)
            {
                if (Visible.value)
                {
                    formLogic.Show();
                }
                else
                {
                    formLogic.Hide();
                }
            }
            EndAction();
        }
    }
}