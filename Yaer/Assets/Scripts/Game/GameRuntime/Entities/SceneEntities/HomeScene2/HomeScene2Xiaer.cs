using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.GameSceneManager.Component.Story;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene2
{
    public class HomeScene2Xiaer : BaseSceneEntityLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += component =>
            {
                // 触发剧情
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene2Xiaer");
            };
        }
    }
}