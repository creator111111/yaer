using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    public class KingPainting : StoryFormPainting
    {
        private void Start()
        {
            var dialogueActor = GetComponent<DialogueActorEx>();
            dialogueActor.OnRefreshAvatarEvent += (roleName, faceType, sprite) =>
            {
                UpdateFace(faceType.ToString());
            };
        }
    }
}