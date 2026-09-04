using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("对话UI全屏射线遮罩开关")]
    public class NormalDialogueRaycastMaskTaskAction : NormalDialoguePanelTaskAction
    {
        public BBParameter<bool> IsOn;

        protected override void OnExecute()
        {
            SetFullscreenRaycastMask(IsOn.value);
            EndAction();
        }
    }
}
