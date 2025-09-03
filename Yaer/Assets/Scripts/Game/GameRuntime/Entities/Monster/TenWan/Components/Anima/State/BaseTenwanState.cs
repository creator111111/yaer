using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class BaseTenWanState : BaseMonsterState
    {
        protected TenWanLogic tenWanLogic;
        protected MoveComponent moveCpn;
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            tenWanLogic = stateMachine.GetEntityLogic<TenWanLogic>();
            moveCpn = tenWanLogic.componentSystem.GetComponent<MoveComponent>();
        }
    }
}