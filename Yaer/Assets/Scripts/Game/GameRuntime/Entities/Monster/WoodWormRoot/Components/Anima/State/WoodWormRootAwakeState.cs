namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State
{
    public class WoodWormRootAwakeState : BaseWoodWormRootState
    {
        public override void Update()
        {
            base.Update();
            
            FinishedChangeState<WoodWormRootIdleState>();
        }
    }
}