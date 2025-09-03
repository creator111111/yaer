using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly
{
    public class DamageFlyClsClimbUpState : BasePlayerDamageState
    {

        public override void Update()
        {
            base.Update();

            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatIdleState>();
                SetSign("IsBreakUp", false);
            }
        }

        public override void Enter()
        {
            base.Enter();
        }
        public override void Exit()
        {
            base.Exit();
            playerLogic.isProtect = false;
            SetSign("IsDamaging", false);
        }
    }
}