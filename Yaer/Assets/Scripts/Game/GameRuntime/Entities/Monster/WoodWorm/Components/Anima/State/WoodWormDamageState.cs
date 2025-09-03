using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormDamageState : BaseWoodWormState
    {
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            
            
        }

        public override void Update()
        {
            base.Update();
            FinishedChangeState<WoodWormIdleState>();
        }

        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            //woodWormLogic.isProtect = true;
        }

        public override void Exit()
        {
            base.Exit();
            //woodWormLogic.isProtect = false;
        }

    }
}