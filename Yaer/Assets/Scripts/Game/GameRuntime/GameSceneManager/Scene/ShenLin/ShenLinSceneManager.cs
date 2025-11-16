using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using System;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.ShenLin
{
    public class ShenLinSceneManager : BaseGameSceneManager
    {
        private ShenLinSceneData sceneData;
        
        public override void OnInit()
        {            
            base.OnInit();

            nowSceneName = SceneName.ShenLin;

            // 记录位置
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.HomeToJingLingVillage2);
            sceneData = GetArchiveData<ShenLinSceneData>();
        }

        public override void OnEnterScene()
        {            
            base.OnEnterScene();
        }

        public override void OnExitScene()
        {            
            base.OnExitScene();
            // 退出场景时刷新怪物死亡状态
            SceneMonsterDataMgr.getInstance().ClearAllMonsterSafeState();
        }

        public override TerrainType GetCurSceneTerrainType()
        {            
            return TerrainType.GlassType;
        }
    }
}