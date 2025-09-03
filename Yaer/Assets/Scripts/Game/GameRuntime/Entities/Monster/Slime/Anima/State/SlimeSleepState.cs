using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState;
using Game.GameRuntime.Entities.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeSleepState : BaseSlimeState
    {
        private HashSet<PlayerLogic> moveTarget = new HashSet<PlayerLogic>();
        private FindTargetComponent findTargetCpn;
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            findTargetCpn = slime.componentSystem.GetComponent<FindTargetComponent>();
        }

        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            moveCpn.canGravity = false;
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            animationEventComponent.RegisterEvent("UpdateMonsterPos", UpdateMonsterPos);
            return;
            //slime.FootCld.isTrigger = true;
        }


        public override void Update()
        {
            base.Update();
            findTargetCpn.FindTarget(ref moveTarget, "BornTriggerArea");
            if (moveTarget.Count > 0)
            {
                EnterSubStateMachine<SlimeBornSubSM>().ChangeState<SlimeBornFallState>();
                //ChangeState<SlimeIdleState>();
            }
            //if (slime.IsTriggerBorn()) EnterSubStateMachine<SlimeBornSubSM>().ChangeState<SlimeBornFallState>();
        }
    }
}