using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class BaseWoodWormState : BaseMonsterState
    {
        protected WoodWormLogic woodWormLogic;
        protected MoveComponent moveCpn;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            woodWormLogic = stateMachine.GetEntityLogic<WoodWormLogic>();
            moveCpn = woodWormLogic.componentSystem.GetComponent<MoveComponent>();
        }
    }
}