using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.Entities.SceneEntities.HomeScene1;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using Game.Static.Path.Sound;

namespace Game.GameRuntime.GameSceneManager.Scene.Home1
{
    public class HomeScene1Manager : BaseGameSceneManager
    {
        private HomeScene1Data sceneData;

        public override void OnInit()
        {
            base.OnInit();

            nowSceneName = SceneName.HomeScene1;
            sceneData = GetArchiveData<HomeScene1Data>();

            GetModule<MapControlComponentGSM>().SetSceneUnlockCondition(null, CheckRightSceneUnlock);
            // 保存当前地图位置
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.Home);

            // 没有换装完成不显示雅尔
            if (GetArchiveData<SelectClothesSceneData>().exitTimes == 0)
            {
                GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<HomeScene1Xiaer>().gameObject.SetActive(false);
            }

            // 没有音乐播放时播放场景音乐
            if (!GameManager.GetGMComponent<SoundComponentGM>().IsPlayingBGM)
            {
                GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫内BGM.ogg", true);
            }
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            if (sceneData.firstEnter && GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName == SceneName.NewGameScene)
            {
                GetModule<StoryComponentGSM>().TriggerStory("HomeScene1FirstEnter");
                //GetModule<StoryComponentGSM>().TriggerStory(StoryPrefabPath.GetPath("HomeScene1FirstEnter"));
                sceneData.firstEnter = false;
            }
        }

        private bool CheckRightSceneUnlock()
        {
            return sceneData.getMap;
        }

        public override TerrainType GetCurSceneTerrainType()
        {
            return TerrainType.IndoorType;
        }

        public override void initAllSceneMonster()
        {

        }
    }
}