using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima
{
    public class TenWanSM : BaseStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<TenWanSleepState>("Sleep", "Sleep");
            RegisterState<TenWanIdleState>("Idle", "Idle");
            RegisterState<TenWanAttackState>("Attack", "Attack");
            RegisterState<TenWanDamageState>("Damage", "Damage");
            RegisterState<TenWanDeadState>("Dead", "Dead");
            RegisterState<TenWanAwakeState>("Awake", "Awake");
        }
    }
}