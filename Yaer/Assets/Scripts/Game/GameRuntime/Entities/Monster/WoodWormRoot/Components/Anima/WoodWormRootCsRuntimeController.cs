using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima
{
    public class WoodWormRootCsRuntimeController: BaseCsRuntimeController
    {
        public override void Init(ICsAnimator csAnimator)
        {
            base.Init(csAnimator);
            
            RegisterMainStateMachine<WoodWormRootSM>();
        }
    }
}