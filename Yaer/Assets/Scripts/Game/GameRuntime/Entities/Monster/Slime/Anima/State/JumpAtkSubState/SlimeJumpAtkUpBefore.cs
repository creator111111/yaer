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
            // JumpAtk 升空前解冻 Y（Idle 可能冻了 FreezePositionY）；升空允许不同轴，落地再 Snap 到 CombatAxisY
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezeRotation;
            // 0913 A′：只跟目标 X；Y 用场景战斗轴，避免玩家跳时 endPos.y 抬高跳攻
            sm.endPos = ResolveJumpAtkEndPos(slime.atkTargetLogic != null
                ? slime.atkTargetLogic.gameObject.transform
                : null);
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