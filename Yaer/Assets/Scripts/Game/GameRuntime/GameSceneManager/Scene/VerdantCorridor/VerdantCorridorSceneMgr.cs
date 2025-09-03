using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using GameFramework.UnityRuntime.Entity;

namespace Game.GameRuntime.GameSceneManager.Scene.VerdantCorridor
{
    public class VerdantCorridorSceneMgr : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            nowSceneName = SceneName.VerdantCorridor;

            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(SceneName.VerdantCorridor);
        }

        public override void OnExitScene()
        {
            base.OnExitScene();
            // 特殊处理一下这个场景中新增的虫子怪,让其被对象池回收而不是被销毁
            var storyEvent = WoodWormRootBattleMgr.getInstance().wormBattleStory;
            if (storyEvent != null )
            {
                foreach(var woodObj in storyEvent.allWoodWormLogics)
                {
                    var entityComponentGM = GameManager.GetGMComponent<EntityComponentGM>();
                    if (entityComponentGM.HasEntity(woodObj.Entity.Id))
                    {
                        entityComponentGM.HideEntity(woodObj.Entity);// 移除实体
                    }
                    
                }
            }
            // 退出场景时刷新怪物死亡状态
            SceneMonsterDataMgr.getInstance().ClearAllMonsterSafeState();
        }

        public override TerrainType GetCurSceneTerrainType()
        {
            return TerrainType.GlassType;
        }
    }
}