using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Component.Spawner;
using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.Entities.Monster.WoodWormRoot.Components.Anima.State
{
    public class WoodWormRootIdleState : BaseWoodWormRootState
    {
        //private PlayerLogic playerLogic;
        //private FindTargetComponent findCpn;
        //private SpawnerComponent spawnerCpn;
        //private HashSet<PlayerLogic> targets = new HashSet<PlayerLogic>();

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            //findCpn = woodWormRoot.componentSystem.GetComponent<FindTargetComponent>();
            //spawnerCpn = woodWormRoot.componentSystem.GetComponent<SpawnerComponent>();
        }

        public override void Enter()
        {
            base.Enter();

            //findCpn.FindTarget(ref targets, "SpawnDetector");
        }

        public override void Update()
        {
            base.Update();

            //if (playerLogic == null)
            //{
            //    findCpn.FindTarget(ref targets, "SpawnDetector");
            //    if (targets.Count > 0)
            //    {
            //        foreach (var p in targets)
            //        {
            //            playerLogic = p;
            //        }

            //        spawnerCpn.StartSpawn("WoodWorm");
            //    }
            //}
            //else
            //{
            //    if (!findCpn.HasTarget(playerLogic))
            //    {
            //        // 停止
            //        spawnerCpn.StopSpawn("WoodWorm");
            //        playerLogic = null;
            //    }
            //}
        }
    }
}