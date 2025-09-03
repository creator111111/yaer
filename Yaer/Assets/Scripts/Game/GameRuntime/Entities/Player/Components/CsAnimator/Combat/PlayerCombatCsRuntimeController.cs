using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat
{
    public class PlayerCombatCsRuntimeController : BaseCsRuntimeController
    {
        public override void Init(ICsAnimator csAnimator)
        {
            base.Init(csAnimator);
            
            RegisterMainStateMachine<PlayerCombatSM>();
        }
    }
}