using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb
{
    public class ClimbDownState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            inputComponent.onRightInput += moveComponent.MoveRight;
            inputComponent.onLeftInput += moveComponent.MoveLeft;
            stateMachine.SetSign("IsClimbMove", true);
        }

        public override void Update()
        {
            base.Update();

            if (inputComponent.HasMoveInput() == false)
            {
                ExitCurrentStateMachine().ChangeState<SquatStay2State>();
                return;
            }
            moveComponent.SetClimbSpeed();
            FinishedChangeState<ClimbMoveState>();
        }

        public override void Exit()
        {
            base.Exit();
            inputComponent.onRightInput -= moveComponent.MoveRight;
            inputComponent.onLeftInput -= moveComponent.MoveLeft;
            stateMachine.SetSign("IsClimbMove", false);
        }
    }
}