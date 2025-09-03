using Game.GameRuntime.Component.Move;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace DebugScene
{
    public class MoveTest : EntityLogic
    {
        public float speed;
        public Vector2 force;
        public Rigidbody2D rg;
        public ComponentSystemMono componentSystemMono;

        private void Awake()
        {
            componentSystemMono.OnInit();
        }

        public void Update()
        {
            componentSystemMono.OnUpdate();

            if (Input.GetKey(KeyCode.A))
            {
                componentSystemMono.GetComponent<DebugMoveComponent>().MoveLeft();
            }

            if (Input.GetKey(KeyCode.D))
            {
                componentSystemMono.GetComponent<DebugMoveComponent>().MoveRight();
            }
        }
    
        private void FixedUpdate()
        {
            componentSystemMono.OnFixedUpdate();
        }

        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeAll;
        // }
        //
        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeRotation;
        // }
        //
        // private void OnTriggerExit2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeRotation;
        //
        // }

        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeAll;
        // }
        //
        // private void OnTriggerStay2D(Collider2D other)
        // {
        //     rg.constraints = RigidbodyConstraints2D.FreezeAll;
        // }
    }
}