using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeWoundState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();

            slime.BodyRg.velocity = Vector2.zero;
            slime.LookAtTarget();
            //slime.isProtect = true;
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ChangeState<SlimeIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            //slime.isProtect = false;
        }
    }
}