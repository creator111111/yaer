using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatDamageState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            moveComponent.StopMove();
            playerLogic.isProtect = true;
        }
        public override void Update()
        {
            base.Update();

            FinishedChangeState<SquatStay2State>();
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.isProtect = false;
        }
    }
}