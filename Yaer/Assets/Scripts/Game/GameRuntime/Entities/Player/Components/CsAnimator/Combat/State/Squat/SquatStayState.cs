using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatStayState : BaseCombatState
    {
        public override void Enter()
        {
            base.Enter();
            moveComponent.StopMove();
            var staminaValue = staminaComponent.GetCostStamina("SquatState");
            staminaComponent.SetRecoverSpeed(staminaValue);

            // 蹲下状态修改为持续按键触发
            //playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput += SquatUpAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput += NormalAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput += InteractAciton;
        }

        public override void Update()
        {
            base.Update();
            if (inputComponent.HasMoveInput())
            {
                EnterSubStateMachine<ClimbSM>().ChangeState<ClimbDownState>();
                return;
            }
            // 如果松开按键则取消蹲下状态
            if (!inputComponent.HasSquatInput())
            {
                SquatUpAction();
                return;
            }
            //else if (Input.GetMouseButtonDown(0))
            //{
            //    ChangeState<SquatAtkState>();
            //    return;
            //}
            //else if (Input.GetKey(KeyCode.C))
            //{
            //    ChangeState<SquatUpState>();
            //}
            //else if (IsTest)
            //{
            //    if (Input.GetKeyDown(KeyCode.Alpha1))
            //    {
            //        ChangeState<SquatDamageState>();
            //    }
            //    else if (Input.GetKeyDown(KeyCode.Alpha2))
            //    {
            //        ChangeState<SquatDeadState>();
            //    }
            //}
        }

        public override void Exit()
        {
            base.Exit();
            staminaComponent.SetRecoverSpeed(0);
            //playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput -= SquatUpAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput -= NormalAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput -= InteractAciton;
        }

        // 起立
        protected virtual void SquatUpAction()
        {
            if (playerLogic.isEnableSquatUp)
            {
                ChangeState<SquatUpState>();
            }
        }

        // 普通攻击
        protected virtual void NormalAtkAction()
        {
            var needStamina = staminaComponent.GetCostStamina("SquatAtkState");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            if (playerLogic.isEnableNorAtk)
            {
                ChangeState<SquatAtkState>();
            }
            if (PlayerGuideMgr.getInstance().inShowNorAtkTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowNorAtkTips = false;
            }
        }
    }
}

