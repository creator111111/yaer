using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackPart1End : BaseNormalAttackState
    {

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<CombatIdleState>();
        }
    }
}