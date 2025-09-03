using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackPart2 : BaseNormalAttackState
    {
        private bool part3;

        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            part3 = false;
            var staminaValue = staminaComponent.GetCostStamina("NorAtkState_2");
            staminaComponent.AddStamina(-staminaValue);
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput += NormalAtkAction;
            // 播放一次音效
            playerLogic.PlayNorAtkAudio();
        }

        public override void Update()
        {
            base.Update();

            //if (inputComponent.GetMouseDown(0)) part3 = true;

            if (IsFinished)
            {
                if (part3)
                {
                    // 三段
                    var needStamina = staminaComponent.GetCostStamina("NorAtkState_3");
                    if (staminaComponent.ChekcHasEnoughStamina(needStamina)) {
                        ChangeState<NormalAttackPart3>();
                        return;
                    }
                }

                ChangeState<NormalAttackPart2End>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput -= NormalAtkAction;
        }

        private void NormalAtkAction()
        {
            part3 = true;
        }
    }
}