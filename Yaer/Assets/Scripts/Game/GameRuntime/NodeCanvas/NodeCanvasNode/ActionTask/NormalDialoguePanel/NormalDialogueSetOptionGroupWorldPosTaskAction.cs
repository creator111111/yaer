using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("�Ի���UI����ѡ���λ�ö�����������")]
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
