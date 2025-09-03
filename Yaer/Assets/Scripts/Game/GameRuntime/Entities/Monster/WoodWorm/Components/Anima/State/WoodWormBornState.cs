namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormBornState : BaseWoodWormState
    {
        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            // 从虫巢中诞生时无敌
            woodWormLogic.isProtect = true;
        }
        public override void Update()
        {
            base.Update();
            
            FinishedChangeState<WoodWormIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            woodWormLogic.isProtect = false;
        }
    }
}