using Game.GameRuntime.Entities.Component.Anima;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima
{
    public class WoodWormCsAnimator: BaseCsAnimator
    {
        protected override void OnInit()
        {
            base.OnInit();
            
            RegisterRuntimeController<WoodWormCsRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<WoodWormCsRuntimeController>();
        }
    }
}