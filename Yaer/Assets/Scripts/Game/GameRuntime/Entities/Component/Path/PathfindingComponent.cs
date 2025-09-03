using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Path
{
    public class PathfindingComponent : BaseGFComponentMono, IPathfindingComponent
    {
        [SerializeField] private float speed;
        private bool isInit;
        private bool isStart;
        private bool pause;
        private Rigidbody2D rg;
        private Vector2 target;

        public bool IsArrived { get; private set; }

        protected override void OnInit()
        {

        }

        private void FixedUpdate()
        {
            if (isStart)
            {
                if (target == Vector2.zero) return;

                if (Vector2.Distance(transform.position, target) < 0.1f)
                {
                    IsArrived = true;
                    Pause();
                }
                else
                {
                    IsArrived = false;
                    Resume();
                }

                if (!IsArrived && pause == false) rg.MovePosition(Vector2.MoveTowards(transform.position, target, speed));
            }
        }

        public void Init(Rigidbody2D rg, float speed)
        {
            isInit = true;

            this.rg = rg;
            this.speed = speed;
        }

        public void StartUp()
        {
            isStart = true;
            IsArrived = false;
        }

        public void ShutDown()
        {
            isStart = false;
            IsArrived = false;
        }

        public void SetTarget(Vector2 p)
        {
            target = p;
        }

        public void Pause()
        {
            pause = true;
        }

        public void Resume()
        {
            pause = false;
        }
    }
}