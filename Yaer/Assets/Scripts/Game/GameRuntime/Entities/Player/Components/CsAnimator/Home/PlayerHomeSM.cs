using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Home
{
    public class PlayerHomeSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<HomeWalkState>("Walk", "Walk");
            
            RegisterSubStateMachine<HomeIdleSubSM>("IdleSubState", "IdleSubState");
        }

        public override void Enter()
        {
            base.Enter();

            EnterSubStateMachine<HomeIdleSubSM>();
        }
    }
}