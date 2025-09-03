using Game.GameRuntime.Entities.Component.Anima;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima
{
    public class SlimeCsAnimator : BaseCsAnimator
    {
        protected override void OnInit()
        {
            base.OnInit();
            
            RegisterRuntimeController<SlimeCsRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<SlimeCsRuntimeController>();
        }
    }
}