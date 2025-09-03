using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State
{
    public class WoodWormRootSleepState: BaseWoodWormRootState
    {
        private HashSet<PlayerLogic> targets = new HashSet<PlayerLogic>();
        public override void Update()
        {
            base.Update();
            
            woodWormRoot.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref targets, "AwakeDetector");

            if (targets.Count > 0)
            {
                ChangeState<WoodWormRootAwakeState>();
            }
        }
    }
}