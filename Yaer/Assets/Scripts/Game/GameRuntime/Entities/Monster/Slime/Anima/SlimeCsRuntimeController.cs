using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima
{
    public class SlimeCsRuntimeController : BaseCsRuntimeController
    {
        public override void Init(ICsAnimator csAnimator)
        {
            base.Init(csAnimator);
            
            RegisterMainStateMachine<SlimeStateMachine>();
        }
    }
}