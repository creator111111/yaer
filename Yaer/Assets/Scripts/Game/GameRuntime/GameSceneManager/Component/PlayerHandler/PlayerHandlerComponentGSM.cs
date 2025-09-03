using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class PlayerHandlerComponentGSM : BaseComponentGSM
    {
        [SerializeField] private string playerPrefabsPath = "Assets/GameRes/Prefabs/Entity/Player/Player.prefab";
        private EntityComponentGM entityComponentGM;
        private PlayerLogic playerLogic;

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            entityComponentGM = GameManager.GetGMComponent<EntityComponentGM>();
        }

        public void CreatePlayer(Action<PlayerLogic> callback = null)
        {
            // 获取配置

            // 创建玩家
            entityComponentGM.ShowPlayerEntity<PlayerLogic>(playerPrefabsPath, 0, SceneManager, logic =>
            {
                playerLogic = logic;
                callback?.Invoke(playerLogic);
            });
        }

        public bool UnlockPlace(string place) => SceneManager.GetArchiveData<PlayerMapData>().AddUnlockPlace(place);
        public void SetNowPlace(string place) => SceneManager.GetArchiveData<PlayerMapData>().SetNowPlace(place);

        public bool UnlockRoad(string road) => SceneManager.GetArchiveData<PlayerMapData>().AddUnlockRoad(road);

        public override void OnShutdown()
        {
            base.OnShutdown();

            if (playerLogic != null)
            {
                entityComponentGM.HideEntity(playerLogic.Entity);
            }
        }
    }
}