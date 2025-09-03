using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.NodeCanvasExtend
{
    [Name("SayEx")]
    [Description("Make the selected Dialogue Actor talk. You can make the text more dynamic by using variable names in square brackets\ne.g. [myVarName] or [Global/myVarName]")]
    public class StatementNodeEx : NodeCanvas.DialogueTrees.StatementNode
    {
        public BBParameter<DialogueFaceType> FaceType;

        protected override Status OnExecute(UnityEngine.Component agent, IBlackboard bb)
        {
            var tempStatement = statement.BlackboardReplace(bb);
            DialogueTree.RequestSubtitles(new SubtitlesRequestInfoEx(finalActor, tempStatement, FaceType.value, OnStatementFinish));
            return Status.Running;
        }

#if UNITY_EDITOR
        protected override void OnNodeGUI()
        {
            GUILayout.BeginVertical(Styles.roundedBox);
            string info = string.Format("<i>' {0}: {1} '</i>", FaceType, statement.text.CapLength(30));
            GUILayout.Label(info);
            GUILayout.EndVertical();
        }
#endif
    }
}