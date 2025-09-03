using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima.State;

namespace Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima
{
    public class WormEggSM: BaseStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<WormEggBreakState>("Break", "Break");
        }
    }
}