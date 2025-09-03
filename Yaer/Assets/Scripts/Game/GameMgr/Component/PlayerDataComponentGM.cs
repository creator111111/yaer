using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Base;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Manager.Res;
using Game.GameMgr.Manager.Res.UI.interf;
using Game.Static.Enum.Goods;
using Game.Static.Enum.Map;
using Game.Static.Name.Clothes;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class PlayerDataComponentGM : BaseComponentGM
    {
        private ArchiveComponentGM archiveComponentGM;

        public override void OnInit()
        {
            base.OnInit();

            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
        }

        public PlayerClothesData GetClothesData() => archiveComponentGM.GetData<PlayerClothesData>();

        #region 地图数据相关

        public bool UnlockPlace(string place)
        {
            return archiveComponentGM.GetData<PlayerMapData>().AddUnlockPlace(place);
        }

        public void SetNowPlace(string roadOrPlace)
        {
            archiveComponentGM.GetData<PlayerMapData>().SetNowPlace(roadOrPlace);
        }

        public bool UnlockRoad(string road)
        {
            return archiveComponentGM.GetData<PlayerMapData>().AddUnlockRoad(road);
        }

        #endregion

        #region 加载或保存游戏回调

        private void OnSaveGame()
        {
        }

        private void OnLoadGame()
        {
        }

        public void InitNewGameData()
        {
            // 初始化默认衣服数据
            var clothesData = archiveComponentGM.GetData<PlayerClothesData>();
            clothesData.AddClothes(BoneName.Weapon, ClothesName.Weapon.NoWeapon);
            clothesData.AddClothes(BoneName.Headwear, ClothesName.HeadWear.Crown);
            clothesData.AddClothes(BoneName.Bangs, ClothesName.Bangs.HasCrownBangs);
            clothesData.AddClothes(BoneName.Face, ClothesName.Face.Smile);
            clothesData.AddClothes(BoneName.Clothes, ClothesName.Clothes.Dress);
            clothesData.AddClothes(BoneName.Bra, ClothesName.Bra.DefaultBra);
            clothesData.AddClothes(BoneName.Hand, ClothesName.Hand.DressNoWeapon);
            clothesData.AddClothes(BoneName.Underwear, ClothesName.Underwear.DefaultUnderwear);
            clothesData.AddClothes(BoneName.Trousers, ClothesName.Trousers.DressTrousers);
            clothesData.AddClothes(BoneName.Shoes, ClothesName.Shoes.DressShoes);
            clothesData.AddClothes(BoneName.Shadow, ClothesName.Shadow.DressNoAiLinSword);

            // 初始化场景数据
            archiveComponentGM.GetData<PlayerMapData>().SetNowPlace(PlaceName.Home);
        }

        #endregion
    }
}