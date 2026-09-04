using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("对话UI设置选项组位置到世界坐标")]
    public class NormalDialogueSetOptionGroupWorldPosTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<Transform> targetTransform;

        protected override void OnExecute()
        {
            if (targetTransform.value != null)
            {
                SetDialogueOptionsGroupWorldPosition(targetTransform.value.position);
            }

            EndAction();
        }
    }
}
