using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using NodeCanvas.Framework;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    public class PlayerAutoMoveActionTask : ActionTask
    {
        public BBParameter<PlayerInputComponent.AutoInputMove> AutoInputMove;
        public BBParameter<Transform> Destination;

        private PlayerLogic player;
        private PlayerInputComponent playerInputComponent;

        protected override string OnInit()
        {
            player = GameObject.FindObjectOfType<PlayerLogic>();
            playerInputComponent = player.componentSystem.GetComponent<PlayerInputComponent>();
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            playerInputComponent.AutoMoveState = AutoInputMove.value;
        }

        protected override void OnUpdate()
        {
            float deltaX = (player.transform.position - Destination.value.position).x;
            if (Mathf.Abs(deltaX) < 0.1f)
            {
                playerInputComponent.AutoMoveState = PlayerInputComponent.AutoInputMove.None;
                EndAction();
            }
        }
    }
}

