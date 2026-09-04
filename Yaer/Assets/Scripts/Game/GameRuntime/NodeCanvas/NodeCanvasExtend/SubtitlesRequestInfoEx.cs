using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story.Painting;
using Game.Static.Enum.Dialogue;
using NodeCanvas.DialogueTrees;
using System;

namespace Game.GameRuntime.Story.NodeCanvasExtend
{
    public class SubtitlesRequestInfoEx : SubtitlesRequestInfo
    {
        public DialogueFaceType FaceType;

        /// <summary>店行：切合层老板娘 Body/Face 子物体。</summary>
        public bool UseShopkeeperPortrait;

        public ShopkeeperBodyType ShopBody;
        public ShopkeeperFaceType ShopFace;

        /// <summary>村长门口行：直通 ChiefFace Face1～3（不经 DialogueFaceType / 晚宴 F2）。</summary>
        public bool UseChiefPortrait;

        public ChiefFaceType ChiefFace;

        public SubtitlesRequestInfoEx(
            IDialogueActor actor,
            IStatement statement,
            DialogueFaceType faceType,
            Action callback) : base(actor, statement, callback)
        {
            FaceType = faceType;
            UseShopkeeperPortrait = false;
            ShopBody = ShopkeeperBodyType.Normal;
            ShopFace = ShopkeeperFaceType.Face1;
            UseChiefPortrait = false;
            ChiefFace = ChiefFaceType.Face1;
        }
    }
}
