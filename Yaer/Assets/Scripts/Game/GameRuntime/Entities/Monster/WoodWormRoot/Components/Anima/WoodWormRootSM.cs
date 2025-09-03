using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima
{
    public class WoodWormRootSM: BaseStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);

            //RegisterState<WoodWormRootAwakeState>("Awake", "Awake");
            RegisterState<WoodWormRootIdleState>("Idle", "Idle");
            //RegisterState<WoodWormRootSleepState>("Sleep", "Sleep");
            RegisterState<WoodWormRootDeadState>("Dead", "Dead");
        }
    }
}