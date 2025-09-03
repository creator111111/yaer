using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState
{
    public class HomeIdleSubSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<HomeIdleState>("IdleSubState_Idle", "Idle");
            RegisterState<HomeBinkState>("IdleSubState_Bink", "Bink");
        }

        public override void Enter()
        {
            base.Enter();

            // 默认Idle
            ChangeState<HomeBinkState>();
        }
    }
}