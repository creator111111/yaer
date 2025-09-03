using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.MainNPC;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.Story.ForestSceneFirstEnter;
using Game.Static.Path;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    public class HomeDoorStoryTriggerLogic : BaseSceneEntityLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            componentSystem.GetComponent<InteractiveComponent>().onEnterInteractiveEvent += component =>
            {
                if (SceneManager.GetArchiveData<ForestSceneData>().homeDoorStoryComplete == false)
                {
                    SceneManager.GetModule<StoryComponentGSM>().TriggerStory("ForestSceneLaiFlyStory");
                }
            };
        }
    }
}