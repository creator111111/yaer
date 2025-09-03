using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect
{
    public abstract class BaseGroundChecker : MonoBehaviour
    {
        [SerializeField]
        protected Transform Root;
        [SerializeField]
        public LayerMask GroundLayerMask;

        public void Init(Transform root)
        {
            Root = root;
        }

        public abstract bool GroundCheck();

        protected virtual void OnDrawGizmosSelected()
        {

        }
    }
}

