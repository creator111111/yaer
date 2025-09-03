using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima
{
    public abstract class BaseSlimeStateMachine : BaseStateMachine
    {
        protected Slime slime;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);

            slime = csAnimator.GetEntityLogic<Slime>();
        }
    }
}