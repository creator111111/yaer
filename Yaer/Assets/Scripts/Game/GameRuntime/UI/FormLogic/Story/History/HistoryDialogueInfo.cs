using Game.Static.Enum.Dialogue;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story.Base.Control
{
    public class HistoryDialogueInfo
    {
        public DialogueRoleName roleName;
        public DialogueFaceType faceType;
        public string content;

        public HistoryDialogueInfo(DialogueRoleName roleName, DialogueFaceType faceType, string content)
        {
            this.roleName = roleName;
            this.faceType = faceType;
            this.content = content;
        }
    }
}