using Game.GameRuntime.Component.Move;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormTurnState : BaseWoodWormState
    {
        private EDirectionType dir;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            
        }
        public override void Enter()
        {
            base.Enter();
            dir = moveCpn.Direction;
            moveCpn.StopMove();
        }

        public override void Update()
        {
            if (monsterLogic.IsDead) { return; }
            base.Update();
            
            FinishedChangeState<WoodWormIdleState>();
        }

        public override void Exit()
        {
            base.Exit();

            if (dir == EDirectionType.Left)
            {
                moveCpn.TurnRight();
            }
            else
            {
                moveCpn.TurnLeft();
            }
        }
    }
}