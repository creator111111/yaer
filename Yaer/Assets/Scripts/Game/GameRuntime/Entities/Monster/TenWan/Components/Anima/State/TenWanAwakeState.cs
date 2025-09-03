namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanAwakeState : BaseTenWanState
    {

        public override void Update()
        {
            base.Update();

            FinishedChangeState<TenWanIdleState>();
        }
    }
}