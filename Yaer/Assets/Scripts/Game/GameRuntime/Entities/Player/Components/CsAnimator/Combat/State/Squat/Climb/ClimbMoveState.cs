using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb
{
    public class ClimbMoveState : BasePlayerState
    {
        float timeCount = 0;
        float cameraActionDistance = 0.2f;// 爬行时摄像机晃动的间隔
        bool hasPlayCameraAction = false;
        public override void Enter()
        {
            base.Enter();
            inputComponent.onRightInput += MoveRight;
            inputComponent.onLeftInput += MoveLeft;
            moveComponent.SetClimbSpeed();
            var staminaValue = staminaComponent.GetCostStamina("ClimbMoveState");
            staminaComponent.SetRecoverSpeed(-staminaValue);
            stateMachine.SetSign("IsClimbMove", true);
            hasPlayCameraAction = false;
            timeCount = 0;
        }

        public override void Update()
        {
            base.Update();
            if (ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge)
            {
                timeCount += Time.deltaTime;
                if (!hasPlayCameraAction && timeCount >= cameraActionDistance)
                {
                    hasPlayCameraAction = true;
                    ForestEastTreeBridgeStoryMgr.getInstance().CameraAction();
                }
            }
            //if (inputComponent.HasMoveInput() == false)
            //{
            //    ChangeState<ClimbUpState>();
            //    return;
            //}
            // 如果松开按键则取消蹲下状态
            if (!inputComponent.HasSquatInput())
            {
                SquatUpAction();
                return;
            }

        }

        public override void Exit()
        {
            base.Exit();
            inputComponent.onRightInput -= MoveRight;
            inputComponent.onLeftInput -= MoveLeft;
            moveComponent.StopMove();
            staminaComponent.SetRecoverSpeed(0);
            stateMachine.SetSign("IsClimbMove", false);
            if (ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge)
            {
                ForestEastTreeBridgeStoryMgr.getInstance().StopCameraAction();
            }
        }


        public void MoveRight(bool isCheckDir = true)
        {
            //var needStamina = staminaComponent.GetCostStamina("ClimbMoveState");
            //if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            if (moveComponent.Direction == EDirectionType.Left)
            {
                moveComponent.SetClimbSpeed();
            }
            moveComponent.MoveRight(isCheckDir);
        }

        public void MoveLeft(bool isCheckDir = true)
        {
            //var needStamina = staminaComponent.GetCostStamina("ClimbMoveState");
            //if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            if (moveComponent.Direction == EDirectionType.Right)
            {
                moveComponent.SetClimbSpeed();
            }
            moveComponent.MoveLeft(isCheckDir);
        }

        // 起立
        protected virtual void SquatUpAction()
        {
            if (playerLogic.isEnableSquatUp)
            {
                var controller = csAnimator.CurrentCsRuntimeController as BaseCsRuntimeController;
                var subMachine = controller.mainStateMachine.Sub;
                var stateMachine = subMachine.Sub.ExitCurrentStateMachine();
                stateMachine.ChangeState<SquatUpState>();
            }
        }
    }
}