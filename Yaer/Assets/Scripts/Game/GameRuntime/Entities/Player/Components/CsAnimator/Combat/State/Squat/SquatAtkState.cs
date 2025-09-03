using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatAtkState : BaseCombatState
    {
        public override void Enter()
        {
            base.Enter();
            var staminaValue = staminaComponent.GetCostStamina("SquatAtkState");
            staminaComponent.AddStamina(-staminaValue);
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);
            //playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput += SquatUpAction;

            playerLogic.PlayNorAtkAudio();
        }

        public override void Update()
        {
            base.Update();

            //if (Input.GetKey(KeyCode.C))
            //{
            //    ChangeState<SquatUpState>();
            //    return;
            //}
            // 如果松开按键则取消蹲下状态
            if (!inputComponent.HasSquatInput())
            {
                SquatUpAction();
                return;
            }

            FinishedChangeState<SquatStay1State>();
        }

        public override void Exit()
        {
            base.Exit();
            //playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput -= SquatUpAction;
        }

        // 起立
        protected virtual void SquatUpAction()
        {
            if (playerLogic.isEnableSquatUp)
            {
                ChangeState<SquatUpState>();
            }
            else
            {
                FinishedChangeState<SquatStay1State>();
                Debug.Log("================当前不能从蹲下到站立!!!");
            }
        }
    }
}