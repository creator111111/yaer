using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class BaseNormalAttackState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            moveComponent.StopMove();
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);
            stateMachine.SetSign("IsNormalAtk", true);
        }



        public override void Update()
        {
            base.Update();

            // jump（村内 isEnableJump=false；直接读 Space 的旁路也必须守门，避免绕过输入队列）
            if (playerLogic.isEnableJump
                && stateMachine.GetSign("AllowJump")
                && Input.GetKeyDown(KeyCode.Space))
                ExitCurrentStateMachine().EnterSubStateMachine<CombatJumpSM>().ChangeState<JumpUpState>();
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.SetSign("IsNormalAtk", false);
        }

    }
}