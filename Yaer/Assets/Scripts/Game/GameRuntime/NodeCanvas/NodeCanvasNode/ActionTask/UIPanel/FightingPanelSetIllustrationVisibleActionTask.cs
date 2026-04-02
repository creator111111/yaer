using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Path;
using NodeCanvas.Framework;
using UnityEngine;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("UI")]
    [Name("ս��������ʾ����")]
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
                // �� visible Ϊ true ʱ�������������������ʾս�������ǰ���³�����ʾ
                // �� visible Ϊ false ʱ��ʼ������������ʱ�ر�����
                bool finalVisible = false;

                if (visible.value)
                {
                    var settingManager = GameManager.GetManager<SettingManager>();
                    if (settingManager != null)
                    {
                        var configData = settingManager.LoadSetting<SettingsConfigData>();
                        if (configData != null)
                        {
                            finalVisible = configData.showBattleImage;
                        }
                    }
                }

                formLogic.UpdateBattleImageVisiable(finalVisible);
            }
            EndAction();
        }
    }
}