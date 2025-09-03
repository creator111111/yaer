using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack
{
    public class SmashAttack1State : BasePlayerState
    {
        private bool canNextStage;

        public override void Enter()
        {
            base.Enter();
            canNextStage = false;
            moveComponent.StopMove();
            playerLogic.canInStateSetPos = true;
            playerLogic.isNoBreakState = true;
            animationEventComponent.RegisterEvent("CanNextStage", CanNextStage);
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);

            var staminaValue = staminaComponent.GetCostStamina("SmashAtkState_1");
            staminaComponent.AddStamina(-staminaValue);
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSmashAtkInput += SmashAtkAction;
            playerLogic.PlayClothingAudio();
        }

        private void SmashAtkAction()
        {
            if (canNextStage)
            {
                var needStamina = staminaComponent.GetCostStamina("SmashAtkState_2");
                if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
                ChangeState<SmashAttack2State>();
            }
        }

        public override void Update()
        {
            base.Update();
            //if (canNextStage)
            //{
            //    if (inputComponent.GetMouseDown(1))
            //    {
            //        ChangeState<SmashAttack2State>();
            //    }
            //}
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSmashAtkInput -= SmashAtkAction;
            playerLogic.isNoBreakState = false;
        }

        private void CanNextStage(string msg)
        {
            canNextStage = true;
        }
    }
}

