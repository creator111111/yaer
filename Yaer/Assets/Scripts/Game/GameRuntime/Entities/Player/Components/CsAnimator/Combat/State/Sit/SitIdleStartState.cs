using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;


namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitIdleStartState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            var staminaValue = staminaComponent.GetCostStamina("SitState");
            staminaComponent.SetRecoverSpeed(staminaValue);
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSitDownInput += SitDownAction;
        }
        // ×øÏÂ
        protected virtual void SitDownAction()
        {
            ChangeState<SitUpState>();
        }
        public override void Update()
        {
            base.Update();
            //if (inputComponent.GetKeyDown(KeyCode.LeftControl))
            //{
            //    ChangeState<SitUpState>();
            //}
            FinishedChangeState<SitIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            staminaComponent.SetRecoverSpeed(0);
        }
    }
}
