using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead
{
    public class FlyDeadFallState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            if (moveComponent.IsGrounded)
            {
                playerLogic.FallGroundEvent?.Invoke();
                ChangeState<FlyDeadEndState>();
            }
                
        }
    }
}