using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class BaseSlimeState : BaseMonsterState
    {
        protected Slime slime;
        protected MoveComponent moveCpn;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            slime = stateMachine.GetEntityLogic<Slime>();
            moveCpn = slime.componentSystem.GetComponent<MoveComponent>();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}