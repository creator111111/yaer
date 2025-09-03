using Game.Entry;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.MVC;
using Game.Static.Name.Res;

namespace Game.GameRuntime.GameSceneManager.Scene.Start
{
    public class StartSceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();
            
            nowSceneName = SceneName.StartScene;
            
        }

        public override void initAllSceneMonster()
        {

        }
    }
}