using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
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

        /// <summary>店行：驱动合层老板娘 Toggle，不走 Mask 立绘。</summary>
        public BBParameter<bool> UseShopkeeperPortrait;

        public BBParameter<ShopkeeperBodyType> ShopBody;
        public BBParameter<ShopkeeperFaceType> ShopFace;

        /// <summary>村长门口行：直通大立绘+Mask Face1～3；与 UseShopkeeperPortrait 互斥。</summary>
        public BBParameter<bool> UseChiefPortrait;

        public BBParameter<ChiefFaceType> ChiefFace;

        protected override Status OnExecute(UnityEngine.Component agent, IBlackboard bb)
        {
            var tempStatement = statement.BlackboardReplace(bb);
            var faceType = FaceType != null ? FaceType.value : DialogueFaceType.None;
            var subtitleInfo = new SubtitlesRequestInfoEx(finalActor, tempStatement, faceType, OnStatementFinish);

            if (UseShopkeeperPortrait != null && UseShopkeeperPortrait.value)
            {
                subtitleInfo.UseShopkeeperPortrait = true;
                subtitleInfo.ShopBody = ShopBody != null ? ShopBody.value : ShopkeeperBodyType.Normal;
                subtitleInfo.ShopFace = ShopFace != null ? ShopFace.value : ShopkeeperFaceType.Face1;
                subtitleInfo.FaceType = DialogueFaceType.None;
            }
            else if (UseChiefPortrait != null && UseChiefPortrait.value)
            {
                // 门口台本：CSV Face1～3 → ChiefFace；不经 MapToChiefFace（晚宴 Smile→Face3）
                subtitleInfo.UseChiefPortrait = true;
                subtitleInfo.ChiefFace = ChiefFace != null ? ChiefFace.value : ChiefFaceType.Face1;
                subtitleInfo.FaceType = DialogueFaceType.None;
            }

            DialogueTree.RequestSubtitles(subtitleInfo);
            return Status.Running;
        }

#if UNITY_EDITOR
        protected override void OnNodeGUI()
        {
            GUILayout.BeginVertical(Styles.roundedBox);
            if (UseShopkeeperPortrait != null && UseShopkeeperPortrait.value)
            {
                var body = ShopBody != null ? ShopBody.value.ToString() : "Normal";
                var face = ShopFace != null ? ShopFace.value.ToString() : "Face1";
                string info = string.Format("<i>' 店 {0}/{1}: {2} '</i>", body, face, statement.text.CapLength(30));
                GUILayout.Label(info);
            }
            else if (UseChiefPortrait != null && UseChiefPortrait.value)
            {
                var face = ChiefFace != null ? ChiefFace.value.ToString() : "Face1";
                string info = string.Format("<i>' 村 {0}: {1} '</i>", face, statement.text.CapLength(30));
                GUILayout.Label(info);
            }
            else
            {
                string info = string.Format("<i>' {0}: {1} '</i>", FaceType, statement.text.CapLength(30));
                GUILayout.Label(info);
            }
            GUILayout.EndVertical();
        }
#endif
    }
}
