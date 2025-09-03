using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima
{
    public class WoodWormSM: BaseStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<WoodWormBornState>("Born", "Born");
            RegisterState<WoodWormBounceState>("Boundce", "Boundce");
            RegisterState<WoodWormDamageState>("Damage", "Damage");
            RegisterState<WoodWormDeadState>("Dead", "Dead");
            RegisterState<WoodWormIdleState>("Idle", "Idle");
            RegisterState<WoodWormMoveState>("Move", "Move");
            RegisterState<WoodWormTurnState>("Turn", "Turn");
            RegisterState<WoodWormEscapeState>("Escape", "Escape");
            RegisterState<WoodWormAttackState>("Attack", "Attack");
        }
    }
}