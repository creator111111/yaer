using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;
namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitIdleState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            var staminaValue = staminaComponent.GetCostStamina("SitState");
            staminaComponent.SetRecoverSpeed(staminaValue);
            playerLogic.OnEnterSitIdleEvent?.Invoke();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSitDownInput += SitDownAction;
        }
        // 坐下
        protected virtual void SitDownAction()
        {
            float hpPercent = playerLogic.healthComponent.hp / playerLogic.healthComponent.maxHp;
            if (hpPercent < 0.25)
            {
                // 残血时直接快速起身
                ChangeState<SitUpState>();
            }
            else
            {
                ChangeState<SitUpBlinkState>();
            }
        }
        public override void Update()
        {
            base.Update();
            //if (inputComponent.GetKeyDown(KeyCode.LeftControl))
            //{
            //    float hpPercent = playerLogic.healthComponent.hp / playerLogic.healthComponent.maxHp;
            //    if (hpPercent < 0.25)
            //    {
            //        ChangeState<SitUpState>();
            //    }
            //    else
            //    {
            //        ChangeState<SitUpBlinkState>();
            //    }
            //}
        }

        public override void Exit()
        {
            base.Exit();
            staminaComponent.SetRecoverSpeed(0);
            playerLogic.OnExitSitIdleEvent?.Invoke();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSitDownInput -= SitDownAction;
        }
    }
}

