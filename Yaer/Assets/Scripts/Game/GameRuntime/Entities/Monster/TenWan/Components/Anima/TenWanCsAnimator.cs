using Game.GameRuntime.Entities.Component.Anima;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima
{
    public class TenWanCsAnimator : BaseCsAnimator
    {

        protected override void OnInit()
        {
            base.OnInit();

            RegisterRuntimeController<TenWanCsRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<TenWanCsRuntimeController>();
        }
    }
}