using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("NormalDialoguePanelControll")]
    [Name("?????UI??????????????")]
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