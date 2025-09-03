using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.PureMVC.Base;
using Game.GameMgr.Manager.Effect;
using Game.GameMgr.Manager.Res;
using Game.GameMgr.Manager.Res.PathHelper;
using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Fighting;
using Game.Static.MVC;
using Game.Static.Name.Clothes;
using Game.Static.Name.State;
using GameFramework.CoreExtend.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player
{
    public class PlayerProxy : BaseProxy
    {
        private IGameSceneManager sceneManager;
        private PlayerLogic playerLogic;
        private bool isCombatState;
        public Action<RuntimeAnimatorController> onControllerChange;

        private Dictionary<string, ValuePro> runtimeConfig = new Dictionary<string, ValuePro>();
        private Dictionary<string, ValuePro> staticConfig = new Dictionary<string, ValuePro>();


        public void SetIsCombat(bool value)
        {
            isCombatState = value;

            // 更新衣服
            ChangeAnimatorController();
        }

        public void SetSceneManager(IGameSceneManager manager) => sceneManager = manager;

        /// <summary>
        /// 更改状态机
        /// </summary>
        public void ChangeAnimatorController()
        {
            // 根据衣服获取动画控制器
            var clothesData = GameManager.GetGMComponent<PlayerDataComponentGM>().GetClothesData();
            var helper = new PlayerControllerResPathHelper()
            {
                clothes = clothesData.GetClothesName(BoneName.Clothes),
                headWear = clothesData.GetClothesName(BoneName.Headwear),
                place = isCombatState == false ? PlayerPlaceStateName.Home : PlayerPlaceStateName.Combat
            };

            GameManager.GetGMComponent<ResComponentGM>()
                .LoadAsset<RuntimeAnimatorController>(helper.GetPath(), asset => { onControllerChange?.Invoke(asset); });
        }

        // public void RefreshCameraFollow(Transform tar)
        // {
        //     sceneManager.GetModule<CameraComponentGSM>().SetFollow(tar);
        // }

        public void UpdateFightingPanel()
        {
            SendNotification(NotificationName.UI.UPDATE_FIGHTING_PANEL);
        }

        public T GetEffectPrefabs<T>(params string[] keys) where T : IEffectComponent
        {
            return GameManager.GetManager<IEffectManager>().CreateEffect<T>(keys);
        }

        public ValuePro GetRuntimeConfig(string key)
        {
            if (runtimeConfig.ContainsKey(key)) return runtimeConfig[key];

            Debug.LogError("没有找到配置");
            return default;
        }

        public ValuePro GetStaticConfig(string key)
        {
            if (staticConfig.ContainsKey(key)) return staticConfig[key];

            Debug.LogError("没有找到配置");
            return default;
        }

        public PlayerStateValue GetPlayerStateValue()
        {
            return new PlayerStateValue
            {
                // hp = playerLogic.Hp,
                // hpMax = playerLogic.HpMax,
                // mp = playerLogic.Mp,
                // mpMax = playerLogic.MpMax
            };
        }

        public void LoadConfig()
        {
        }
    }
}