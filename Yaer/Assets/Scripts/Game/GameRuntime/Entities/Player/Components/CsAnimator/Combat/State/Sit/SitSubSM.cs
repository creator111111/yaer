using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitSubSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            RegisterState<SitDownState>("SitDown", "SitDown");
            RegisterState<SitIdleStartState>("SitIdleStart", "SitIdleStart");
            RegisterState<SitIdleState>("SitIdle", "SitIdle");
            RegisterState<SitUpState>("SitUp", "SitUp");
            RegisterState<SitUpBlinkState>("SitUpBlink", "SitUpBlink");
        }

        public override void Enter()
        {
            base.Enter();

            SetSign("IsSitting", true);
        }

        public override void Exit()
        {
            base.Exit();

            SetSign("IsSitting", false);
        }
    }
}

