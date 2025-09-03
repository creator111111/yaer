using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Player;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanSleepState : BaseTenWanState
    {
        private FindTargetComponent findTargetComponent;
        private HashSet<PlayerLogic> playerLogics = new HashSet<PlayerLogic>();

        public override void Enter()
        {
            base.Enter();

            if (findTargetComponent is null)
            {
                findTargetComponent = tenWanLogic.componentSystem.GetComponent<FindTargetComponent>();
                if (findTargetComponent is null)
                {
                    Debug.LogError("找不到FindTargetComponent --" + GetType().Name);
                }
            }
            
            playerLogics.Clear();
        }

        public override void Update()
        {
            base.Update();

            if (findTargetComponent != null)
            {
                // 检测到玩家
                findTargetComponent.FindTarget<PlayerLogic>(ref playerLogics, "AwakeDetector");
                if (playerLogics.Count > 0)
                {
                    ChangeState<TenWanAwakeState>();
                }
            }
        }
    }
}