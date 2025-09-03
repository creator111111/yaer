using Game.GameRuntime.Component.Move;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace DebugScene
{
    public class MoveTest2 : EntityLogic
    {
        public float speed;
        public Vector2 force;
        public Rigidbody2D rg;
        public ComponentSystemMono componentSystemMono;

        public Transform target;

        private void Awake()
        {
            componentSystemMono.OnInit();
        }

        public void Update()
        {
            componentSystemMono.OnUpdate();
        
            if (Input.GetKey(KeyCode.Q))
            {
                componentSystemMono.GetComponent<DebugMoveComponent>().MoveLeft();
            }

            if (Input.GetKey(KeyCode.E))
            {
                componentSystemMono.GetComponent<DebugMoveComponent>().MoveRight();
            }
        }

        private void FixedUpdate()
        {
            componentSystemMono.OnFixedUpdate();
        }

        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeRotation;
        // }
        //
        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeAll;
        // }
        //
        // private void OnTriggerExit2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeRotation;
        //
        // }

        // private void OnCollisionStay2D(Collision2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeAll;
        // }
    }
}