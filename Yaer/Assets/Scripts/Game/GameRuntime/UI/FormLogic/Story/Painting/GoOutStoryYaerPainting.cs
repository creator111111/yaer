using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    public class GoOutStoryYaerPainting : StoryFormPainting
    {
        public GameObject armorNone;
        public GameObject armorHead;
        public GameObject armorCrown;
        protected override void SetDefaultPainting()
        {
            // DialogDebug 等沙盒场景无 GameSceneManager，使用默认立绘避免 NRE
            var gsm = GameManager.GetGameSceneManager();
            if (gsm == null)
            {
                UpdateFace("Armor_NoHeadWear_Smile");
                if (armorHead != null)
                {
                    armorHead.SetActive(false);
                }

                if (armorCrown != null)
                {
                    armorCrown.SetActive(false);
                }

                return;
            }

            var playerClothesData = gsm.GetArchiveData<PlayerClothesData>();
            //var clothesName = playerClothesData.GetClothesName(BoneName.Clothes);
            //var headwearName = playerClothesData.GetClothesName(BoneName.Headwear);
            //UpdateFace($"{clothesName}_{headwearName}_Smile");
            //UpdateClothes(playerClothesData.GetClothesName(BoneName.Clothes));
            UpdateFace("Armor_NoHeadWear_Smile");
            var headWear = playerClothesData.GetClothesName(BoneName.Headwear);
            var hasCrown = headWear == ClothesName.HeadWear.Crown;
            var hasArmorHead = headWear == ClothesName.HeadWear.ArmorHead;
            armorHead.SetActive(hasArmorHead);
            armorCrown.SetActive(hasCrown);
        }

        protected override void RegisterRefreshAvatarEvent(DialogueActorEx dialogueActor)
        {
            dialogueActor.OnRefreshAvatarEvent += (roleName, faceType, sprite) =>
            {
                UpdateFace(ResolveGoOutFaceKey(faceType));
            };
        }

        /// <summary>
        /// GoOut 立绘集文件名形如 Armor_NoHeadWear_Smile；CSV/图里常用 Normal，但集内无 Normal 键，回退 Smile 避免说话时全隐藏。
        /// </summary>
        private static string ResolveGoOutFaceKey(DialogueFaceType faceType)
        {
            if (faceType == DialogueFaceType.Normal)
            {
                return "Armor_NoHeadWear_Smile";
            }

            return $"Armor_NoHeadWear_{faceType}";
        }
    }
}