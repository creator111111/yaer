using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima
{
    public class WormEggRuntimeController: BaseCsRuntimeController
    {
        public override void Init(ICsAnimator csAnimator)
        {
            base.Init(csAnimator);
            
            RegisterMainStateMachine<WormEggSM>();
        }
    }
}