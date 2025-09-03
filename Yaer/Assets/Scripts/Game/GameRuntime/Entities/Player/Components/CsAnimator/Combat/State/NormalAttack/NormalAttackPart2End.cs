using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackPart2End : BaseNormalAttackState
    {
        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<CombatIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}