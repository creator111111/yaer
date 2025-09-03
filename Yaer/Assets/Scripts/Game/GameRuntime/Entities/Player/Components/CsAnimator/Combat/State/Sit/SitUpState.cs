using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitUpState : BasePlayerState
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
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatIdleState>();
            }
        }
    }
}

