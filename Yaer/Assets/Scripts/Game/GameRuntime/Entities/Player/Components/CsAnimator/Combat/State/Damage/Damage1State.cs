using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage
{
    public class Damage1State : BasePlayerDamageState
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ChangeState<CombatIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.isProtect = false;
            SetSign("IsDamaging", false);
        }
    }
}