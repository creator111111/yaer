using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;
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
            var playerClothesData = GameManager.GetGameSceneManager().GetArchiveData<PlayerClothesData>();
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
                //var playerClothesData = GameManager.GetGameSceneManager().GetArchiveData<PlayerClothesData>();
                //var clothesName = playerClothesData.GetClothesName(BoneName.Clothes);
                //var headwearName = playerClothesData.GetClothesName(BoneName.Headwear);
                //UpdateFace($"{clothesName}_{headwearName}_{faceType}");
                UpdateFace($"Armor_NoHeadWear_{faceType}");
            };
        }
    }
}