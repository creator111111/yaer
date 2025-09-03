using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.SubManager.Buff
{
    public class BaseBuffHelper : MonoBehaviour, IBuffHelper
    {
        [SerializeField] private bool apply;

        public void Init()
        {
        }

        public virtual void Apply()
        {
            apply = true;
        }

        public virtual void Remove()
        {
            apply = false;
        }
    }
}