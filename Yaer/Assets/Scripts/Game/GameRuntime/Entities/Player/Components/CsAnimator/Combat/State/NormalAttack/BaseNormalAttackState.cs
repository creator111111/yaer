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

            // jump
            if (stateMachine.GetSign("AllowJump") && Input.GetKeyDown(KeyCode.Space))
                ExitCurrentStateMachine().EnterSubStateMachine<CombatJumpSM>().ChangeState<JumpUpState>();
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.SetSign("IsNormalAtk", false);
        }

    }
}