using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public interface IComponentGSM
    {
        void OnInit(IGameSceneManager manager);

        void OnUpdate();
        
        void OnFixedUpdate();

        void OnShutdown();
    }
}