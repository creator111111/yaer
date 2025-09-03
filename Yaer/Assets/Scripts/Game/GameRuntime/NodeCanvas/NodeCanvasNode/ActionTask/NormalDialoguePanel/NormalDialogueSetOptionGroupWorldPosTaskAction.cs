using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("对话框UI设置选项框位置对齐世界物体")]
    public class NormalDialogueSetOptionGroupWorldPosTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<Transform> targetTransform;

        private NormalDialogueFormNewLogic uiFormLogic;

        protected override void OnGetUILogic(NormalDialogueFormNewLogic uiFormLogic)
        {
            this.uiFormLogic = uiFormLogic;
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            if (uiFormLogic != null)
            {
                uiFormLogic.SetDialogueOptionsGroupPosition(targetTransform.value.position);
            }
            else
            {
                Debug.LogError("uiFormLogic is null");
            }
            EndAction();
        }
    }
}