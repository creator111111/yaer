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
    [Name("FightingPanel��ʾ����")]
    public class FightingPanelVisibleActionTask : ActionTask
    {
        public BBParameter<bool> Visible;

        private FightingFormLogic formLogic;

        protected override string OnInit()
        {
            // DialogDebug ɳ�е�δ��ʼ�� UIComponentGM ʱ���������� OnInit NRE
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null)
            {
                return base.OnInit();
            }

            string panelPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var uiForm = uiGm.GetUIForm(panelPath);
            if (uiForm == null)
            {
                Debug.LogError($"FightingPanelδ��");
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