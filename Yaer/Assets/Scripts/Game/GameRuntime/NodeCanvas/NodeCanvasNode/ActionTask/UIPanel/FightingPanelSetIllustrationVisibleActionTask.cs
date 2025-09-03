using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Path;
using NodeCanvas.Framework;
using UnityEngine;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("UI")]
    [Name("战斗立绘显示开关")]
    public class FightingPanelSetIllustrationVisibleActionTask : ActionTask
    {
        public BBParameter<bool> visible;
        //public BBParameter<float> Duration;

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
                formLogic.UpdateBattleImageVisiable(visible.value);
            }
            EndAction();
        }
    }
}