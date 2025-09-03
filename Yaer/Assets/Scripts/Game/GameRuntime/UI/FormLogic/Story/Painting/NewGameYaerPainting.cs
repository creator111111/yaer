using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    public class NewGameYaerPainting : StoryFormPainting
    {
        protected override void RegisterRefreshAvatarEvent(DialogueActorEx dialogueActor)
        {
            dialogueActor.OnRefreshAvatarEvent += (roleName, faceType, sprite) =>
            {
                Debug.Log("===============初始对话雅儿的表情:" + faceType);
                UpdateFace($"Dress_Crown_{faceType}");
            };
        }
    }
}