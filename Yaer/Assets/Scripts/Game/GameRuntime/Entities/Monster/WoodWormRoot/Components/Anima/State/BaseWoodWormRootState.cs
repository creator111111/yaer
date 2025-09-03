using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State
{
    public class BaseWoodWormRootState : BaseMonsterState
    {
        protected WoodWormRootLogic woodWormRoot;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            woodWormRoot = stateMachine.GetEntityLogic<WoodWormRootLogic>();
        }
    }
}