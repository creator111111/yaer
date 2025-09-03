using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb
{
    public class ClimbUpState : BasePlayerState
    {
        public override void Update()
        {
            base.Update();

            if (inputComponent.HasMoveInput())
            {
                ChangeState<ClimbMoveState>();
            }
            else
            if (IsFinished) ExitCurrentStateMachine().ChangeState<SquatStay2State>();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}