using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using System;

namespace Game.GameRuntime.Story.NodeCanvasExtend
{
    public class SubtitlesRequestInfoEx : SubtitlesRequestInfo
    {
        public DialogueFaceType FaceType;

        public SubtitlesRequestInfoEx(IDialogueActor actor, IStatement statement, DialogueFaceType faceType, Action callback) : base(actor, statement, callback)
        {
            FaceType = faceType;
        }
    }
}