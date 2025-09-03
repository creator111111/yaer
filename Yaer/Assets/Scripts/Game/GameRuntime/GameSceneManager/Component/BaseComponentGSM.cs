using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class BaseComponentGSM : MonoBehaviour, IComponentGSM
    {
        private IGameSceneManager sceneManager;
        public BaseGameSceneManager SceneManager => sceneManager as BaseGameSceneManager;
        public virtual void OnInit(IGameSceneManager manager)
        {
            sceneManager = manager;
        }
        
        public virtual void OnUpdate()
        {
        }
        
        public virtual void OnFixedUpdate()
        {
        }
        
        public virtual void OnShutdown()
        {
        }
    }
}