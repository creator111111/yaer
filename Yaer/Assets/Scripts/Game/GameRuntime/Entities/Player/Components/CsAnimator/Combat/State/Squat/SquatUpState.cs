using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatUpState : BaseCombatState
    {

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<CombatIdleState>();
        }
    }
}