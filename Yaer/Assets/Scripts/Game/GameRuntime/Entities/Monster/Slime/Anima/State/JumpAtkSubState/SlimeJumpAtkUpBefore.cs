using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkUpBefore : BaseSlimeState
    {
        private SlimeJumpAtkSubSM sm;
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            sm = stateMachine as SlimeJumpAtkSubSM;
        }
        public override void Enter()
        {
            base.Enter();
            slime.StopMoveOnPosX();
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezeRotation;
            sm.endPos = slime.atkTargetLogic.gameObject.transform.position;
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            animationEventComponent.RegisterEvent("StopAniFrameWithSec", StopAniFrameWithSec);
        }

        public override void Update()
        {
            base.Update();
            if (monsterLogic.IsDead) { return; }
            FinishedChangeState<SlimeJumpAtkUpState>();
        }
    }
}