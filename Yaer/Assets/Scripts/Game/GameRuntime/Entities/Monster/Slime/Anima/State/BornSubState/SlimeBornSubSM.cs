using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState
{
    public class SlimeBornSubSM : BaseSlimeStateMachine
    {
        private ICsAnimator csAnimator;
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<SlimeBornFallState>("BornSubState_BornFall", "BornFall");
            RegisterState<SlimeBornDownState>("BornSubState_BornDown", "BornDown");
            this.csAnimator = csAnimator;
        }

        public override void Enter()
        {
            base.Enter();
            csAnimator.SetBool("Idle", false);
        }
    }
}