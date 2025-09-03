using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Monster.WormEgg;
using Game.GameRuntime.Entities.Monster.WormEgg.Components.Anima;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima
{
    public class WoodWormRootCsAnimator: BaseCsAnimator
    {
        protected WoodWormRootLogic wormRootLogic;
        protected override void OnInit()
        {
            base.OnInit();
            wormRootLogic = GetEntityLogic<WoodWormRootLogic>();
            RegisterRuntimeController<WoodWormRootCsRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<WoodWormRootCsRuntimeController>();
        }
    }
}