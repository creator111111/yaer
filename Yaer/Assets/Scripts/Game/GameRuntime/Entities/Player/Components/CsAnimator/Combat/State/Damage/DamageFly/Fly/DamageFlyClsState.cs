namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly
{
    public class DamageFlyClsState : BasePlayerDamageState
    {
        public override void Update()
        {
            base.Update();

            if (moveComponent.m_Gravity.y != 0 && moveComponent.IsGrounded)
            {
                playerLogic.FallGroundEvent?.Invoke();
                ChangeState<DamageFlyClsClimbUpState>();
            }
        }

        public override void Enter()
        {
            base.Enter();
        }
        public override void Exit()
        {
            base.Exit();
        }
    }
}