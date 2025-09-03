using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackPart1 : BaseNormalAttackState
    {
        private bool part2;

        public override void Enter()
        {
            base.Enter();

            part2 = false;
            moveComponent.StopMove();
            var staminaValue = staminaComponent.GetCostStamina("NorAtkState_1");
            staminaComponent.AddStamina(-staminaValue);
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput += NormalAtkAction;
            // 播放一次音效
            playerLogic.PlayNorAtkAudio();
        }

        private void NormalAtkAction()
        {
            part2 = true;
            
        }

        public override void Update()
        {
            base.Update();

            //if (inputComponent.GetMouseDown(0)) part2 = true;

            if (IsFinished)
            {
                if (part2)
                {
                    // 二段
                    var needStamina = staminaComponent.GetCostStamina("NorAtkState_2");
                    if (staminaComponent.ChekcHasEnoughStamina(needStamina)) {
                        ChangeState<NormalAttackPart2>();
                        return;
                    }

                }

                ChangeState<NormalAttackPart1End>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput -= NormalAtkAction;
        }


    }
}