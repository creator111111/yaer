namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanDamageState : BaseTenWanState
    {
        public override void Enter()
        {
            base.Enter();
            //tenWanLogic.isProtect = true;
        }
        public override void Update()
        {
            base.Update();

            FinishedChangeState<TenWanIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            //tenWanLogic.isProtect = false;
        }
    }
}