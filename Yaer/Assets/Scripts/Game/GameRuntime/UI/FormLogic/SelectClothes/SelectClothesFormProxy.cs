using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum.Goods;
using Game.Static.Name.Clothes;

namespace Game.GameRuntime.UI.FormLogic.SelectClothes
{
    public class SelectClothesFormProxy : BaseFormProxy
    {
        private SelectClothesComponentGSM component;
        
        public Action<Dictionary<string, string>> onUpdateClothesNamesForBones;

        public override void OnEnter()
        {
            base.OnEnter();
            
            component = GameManager.GetGameSceneManager().GetModule<SelectClothesComponentGSM>();
        }

        /// <summary>
        /// 获取模版上的衣服
        /// </summary>
        /// <param name="boneName"></param>
        public void GetAllClothesNamesForBones(string boneName)
        {
            var data = component.GetAllClothesNamesForBones(boneName);
            
            // 判断是否获取了武器
            if (boneName == BoneName.Weapon && GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>().HasMainItem(EMainItemName.AiLinSword.ToString()) == false)
            {
                data.Remove(ClothesName.Weapon.AiLinSword);
            }
            
            onUpdateClothesNamesForBones?.Invoke(data);
        }

        /// <summary>
        /// 获取已经穿上的
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetWearingClothesData()
        {
            return component.GetWearingClothesData();
        }

        /// <summary>
        /// 换装
        /// </summary>
        /// <param name="boneName"></param>
        /// <param name="clothesName"></param>
        public void ChangingClothes(string boneName, string clothesName)
        {
            component.ChangingClothes(boneName, clothesName);
            CorrectingClothingMatching();
        }

        /// <summary>
        ///     根据衣服获取对应的动画的Keys
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void SaveWearingClothes()
        {
            component.SaveWearingClothes();
        }
        
         /// <summary>
        ///     修正搭配组合
        /// </summary>
        private void CorrectingClothingMatching()
        {
            var wearingClothesDic = GetWearingClothesData();

            // 修正刘海
            switch (wearingClothesDic[BoneName.Headwear])
            {
                case ClothesName.HeadWear.NoHeadWear:
                    component.ChangingClothes(BoneName.Bangs, ClothesName.Bangs.NoCrownBangs);
                    break;
                case ClothesName.HeadWear.ArmorHead:
                    component.ChangingClothes(BoneName.Bangs, ClothesName.Bangs.NoCrownBangs);
                    break;
                case ClothesName.HeadWear.Crown:
                    component.ChangingClothes(BoneName.Bangs, ClothesName.Bangs.HasCrownBangs);
                    break;
            }

            // 修正手
            var isWeapon = wearingClothesDic[BoneName.Weapon] != ClothesName.Weapon.NoWeapon;

            if (isWeapon)
                switch (wearingClothesDic[BoneName.Clothes])
                {
                    case ClothesName.Clothes.NoClothes:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.NoClothesHasWeapon);
                        break;
                    case ClothesName.Clothes.Dress:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.DressHasWeapon);
                        break;
                    case ClothesName.Clothes.Armor:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.ArmorHasWeapon);
                        break;
                }
            else
                // 没有武器
                switch (wearingClothesDic[BoneName.Clothes])
                {
                    case ClothesName.Clothes.NoClothes:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.NoClothesNoWeapon);
                        break;
                    case ClothesName.Clothes.Dress:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.DressNoWeapon);
                        break;
                    case ClothesName.Clothes.Armor:
                        component.ChangingClothes(BoneName.Hand, ClothesName.Hand.ArmorNoWeapon);
                        break;
                }

            // 修正影子
            var weapon = wearingClothesDic[BoneName.Weapon] != ClothesName.Weapon.NoWeapon;
            if (weapon)
                switch (wearingClothesDic[BoneName.Clothes])
                {
                    case ClothesName.Clothes.NoClothes:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.NoClothesHasAiLinSword);
                        break;
                    case ClothesName.Clothes.Dress:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.DressHasAiLinSword);
                        break;
                    case ClothesName.Clothes.Armor:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.ArmorHasAiLinSword);
                        break;
                }
            else
                switch (wearingClothesDic[BoneName.Clothes])
                {
                    case ClothesName.Clothes.NoClothes:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.NoClothesNoAiLinSword);
                        break;
                    case ClothesName.Clothes.Dress:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.DressNoAiLinSword);
                        break;
                    case ClothesName.Clothes.Armor:
                        component.ChangingClothes(BoneName.Shadow, ClothesName.Shadow.ArmorNoAiLinSword);
                        break;
                }

            // 修正表情
            if (wearingClothesDic[BoneName.Clothes] == ClothesName.Clothes.NoClothes ||
                wearingClothesDic[BoneName.Trousers] == ClothesName.Trousers.NoTrousers)
                component.ChangingClothes(BoneName.Face, ClothesName.Face.Shy);
            else
                component.ChangingClothes(BoneName.Face, ClothesName.Face.Smile);

            // 修正内衣
            if (wearingClothesDic[BoneName.Clothes] == ClothesName.Clothes.NoClothes)
            {
                component.ChangingClothes(BoneName.Underwear, ClothesName.Underwear.DefaultUnderwear);
                component.ChangingClothes(BoneName.Bra, ClothesName.Bra.DefaultBra);
            }
            else
            {
                component.ChangingClothes(BoneName.Underwear, ClothesName.Underwear.NoUnderwear);
                component.ChangingClothes(BoneName.Bra, ClothesName.Bra.NoBra);
            }

            // 修正内裤
            if (wearingClothesDic[BoneName.Trousers] == ClothesName.Trousers.NoTrousers)
            {
                component.ChangingClothes(BoneName.Underwear, ClothesName.Underwear.DefaultUnderwear);
            }
            else if (wearingClothesDic[BoneName.Trousers] == ClothesName.Trousers.DressTrousers)
            {
                component.ChangingClothes(BoneName.Underwear, ClothesName.Underwear.DefaultUnderwear);
            }
            else if (wearingClothesDic[BoneName.Trousers] == ClothesName.Trousers.ArmorTrousers)
            {
                component.ChangingClothes(BoneName.Underwear, ClothesName.Underwear.NoUnderwear);
            }
        }
    }
}