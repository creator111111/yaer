using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Manager.Res.PathHelper;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Name.Clothes;
using Game.Static.Name.State;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components
{
    public class PlayerRuntimeControllerComponent: BaseGFComponentMono, IPlayerComponent
    {
        public PlayerLogic PlayerLogic { get; set; }
        protected override void OnInit()
        {
            
        }
        
        public void GetAnimatorController(Action<RuntimeAnimatorController> callBack, bool isCombatState = false)
        {
            // 根据衣服获取动画控制器
            var clothesData = PlayerLogic.sceneManager.GetArchiveData<PlayerClothesData>();
            var helper = new PlayerControllerResPathHelper()
            {
                clothes = clothesData.GetClothesName(BoneName.Clothes),
                headWear = clothesData.GetClothesName(BoneName.Headwear),
                place = isCombatState == false ? PlayerPlaceStateName.Home : PlayerPlaceStateName.Combat
            };

            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<RuntimeAnimatorController>(helper.GetPath(), callBack);
        }
    }
}