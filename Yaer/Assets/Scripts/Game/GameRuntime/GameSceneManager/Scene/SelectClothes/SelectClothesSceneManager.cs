using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Name.Res;
using Game.Static.Path;

namespace Game.GameRuntime.GameSceneManager.Scene.SelectClothes
{
    public class SelectClothesSceneManager : BaseGameSceneManager
    {
        private int exitTimes;
        private SelectClothesSceneData savedData;

        public override void OnInit()
        {
            base.OnInit();
            
            nowSceneName = SceneName.SelectClothesScene;
        }

        protected override void OnInitAddModules()
        {
            base.OnInitAddModules();
            
            AddModule<SelectClothesComponentGSM>();
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();
            
            // 第一次进入
            if (GetArchiveData<SelectClothesSceneData>().exitTimes == 0)
            {
                // 显示对话框
                GetModule<StoryComponentGSM>().TriggerStory("ChangeClothesSceneEnter");
            }
            else
            {
                // 打开ui
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SelectClothesPanel"), EUIGroup.Bottom, new OpenFormArgs());
            }
        }

        public override void OnExitScene()
        {
            base.OnExitScene();
            
            GetArchiveData<SelectClothesSceneData>().exitTimes++;
        }

        public override void initAllSceneMonster()
        {

        }
    }
}