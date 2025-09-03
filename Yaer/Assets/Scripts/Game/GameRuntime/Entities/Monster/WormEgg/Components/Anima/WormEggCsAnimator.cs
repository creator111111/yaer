using Game.GameRuntime.Entities.Component.Anima;

namespace Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima
{
    public class WormEggCsAnimator : BaseCsAnimator
    {
        
        protected WormEggLogic wormEggLogic;

        protected override void OnInit()
        {
            base.OnInit();

            wormEggLogic = GetEntityLogic<WormEggLogic>();
            RegisterRuntimeController<WormEggRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<WormEggRuntimeController>();
        }
    }
}