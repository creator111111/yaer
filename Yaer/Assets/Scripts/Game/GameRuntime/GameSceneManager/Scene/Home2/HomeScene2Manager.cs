using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.SceneEntities.HomeScene2;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Name.Res;
using Game.Static.Path;
using Game.Static.Path.Sound;

namespace Game.GameRuntime.GameSceneManager.Scene.Home2
{
    public class HomeScene2Manager : BaseGameSceneManager
    {
        private HomeScene2Data sceneData;

        public override void OnInit()
        {
            base.OnInit();

            // 当前场景
            nowSceneName = SceneName.HomeScene2;

            // 保存数据中更新场景物体
            sceneData = GetArchiveData<HomeScene2Data>();

            if (GetArchiveData<SelectClothesSceneData>().exitTimes > 0)
            {
                // 换装完成二楼夏尔消失
                GetSceneEntityLogic<HomeScene2Xiaer>().gameObject.SetActive(false);
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

            // 第一次从换装完成
            var isFromSelectClonthesSceneEnter = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName == SceneName.SelectClothesScene;
            if (sceneData.firstCompleteChangeClothes && isFromSelectClonthesSceneEnter)
            {
                GetModule<StoryComponentGSM>().TriggerStory("ChangeClothesSceneExit");
                sceneData.firstCompleteChangeClothes = false;
                // 记录成就
                AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.FirstOutfitChange, 1);
            }
            if (isFromSelectClonthesSceneEnter)
            {
                GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫内BGM.ogg", true);
            }
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