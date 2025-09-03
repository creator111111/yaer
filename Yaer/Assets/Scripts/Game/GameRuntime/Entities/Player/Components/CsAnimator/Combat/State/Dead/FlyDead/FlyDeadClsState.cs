using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead
{
    public class FlyDeadClsState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();

            moveComponent.StopMove();
        }

        public override void Update()
        {
            base.Update();

            if (moveComponent.m_Gravity.y != 0 && moveComponent.IsGrounded)
            {
                playerLogic.FallGroundEvent?.Invoke();
                ChangeState<FlyDeadClsEndState>();
            }
        }
    }
}