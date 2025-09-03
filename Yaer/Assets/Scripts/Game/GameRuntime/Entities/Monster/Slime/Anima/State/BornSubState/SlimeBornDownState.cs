using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState
{
    public class SlimeBornDownState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();

            slime.BodyRg.velocity = Vector2.zero;
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<SlimeIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            slime.isFallDownAtk = false;
            slime.isProtect = false;
        }
    }
}