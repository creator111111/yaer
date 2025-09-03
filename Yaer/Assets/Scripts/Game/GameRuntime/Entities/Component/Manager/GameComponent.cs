using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Manager
{
    public abstract class GameComponent : MonoBehaviour
    {
        public int Priority; // 组件优先级
        protected GameObject obj;

        public void Init(GameObject obj)
        {
            this.obj = obj;
        }

        public virtual void AwakeComponent()
        {
        }

        public virtual void StartComponent()
        {
        }

        public virtual void UpdateComponent()
        {
        }
    }
}