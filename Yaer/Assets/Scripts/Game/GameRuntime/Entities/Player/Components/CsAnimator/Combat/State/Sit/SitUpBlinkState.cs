using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitUpBlinkState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            FinishedChangeState<SitUpState>();
        }
    }
}