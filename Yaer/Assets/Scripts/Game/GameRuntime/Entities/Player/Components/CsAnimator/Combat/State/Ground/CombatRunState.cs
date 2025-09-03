using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump;
using UnityEngine;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.Entities.Component.Move;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground
{
    public class CombatRunState : CombatGroundState
    {
        private bool moveXAndY;
        private bool moveXOrY;

        float timeCount; // 计时器
        float moveAudioPlayDistance = 0.5f; // 走路音效间隔
        public override void Enter()
        {
            base.Enter();

            // player.Transform.position = player.AnimationTsf.position;
            SetSign("IsRunning", true);
            SetSign("IsJumping", false);
            moveXOrY = false;
            moveXAndY = false;
            // var dustPos = playerLogic.GetTsf("EffectPos/ChangeDirDust").position;
            // var ef = playerLogic.SceneManager.PlayEffect<ChangeDirDustEffect>(new[] { "Effect/Player/Dust/Effect_Player_ChangeDirDust.prefab" }, 1, dustPos);
            // if (moveComponent.DirV2 == Vector2.right)
            // {
            //     ef.SetLeft();
            // }
            // else
            // {
            //     ef.SetRight();
            // }
            // 监听按键
            inputComponent.onRightInput += MoveRight;
            inputComponent.onLeftInput += MoveLeft;
            moveComponent.SetRunSpeed();
            var staminaValue = staminaComponent.GetCostStamina("RunState");
            staminaComponent.SetRecoverSpeed(-staminaValue);
            timeCount = moveAudioPlayDistance;// 第一次播放音效不需要时间间隔
        }

        public override void Update()
        {
            base.Update();

            if (IsExit) return;

            if (inputComponent.HasMoveInput() == false)
            {
                ChangeState<CombatIdleState>();
                return;
            }
            timeCount += Time.deltaTime;
            if (timeCount >= moveAudioPlayDistance)
            {
                timeCount = 0;
                playerLogic.PlayRunAudio();
            }
            //// 同时横纵移动
            //if (inputComponent.HasXYInput())
            //{
            //    moveXAndY = true;
            //    moveXOrY = false;
            //}
            //else
            //{
            //    moveXOrY = true;
            //    moveXAndY = false;
            //}
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsExit) return;
        }

        public override void Exit()
        {
            base.Exit();

            inputComponent.onRightInput -= MoveRight;
            inputComponent.onLeftInput -= MoveLeft;
            moveComponent.StopMove();
            SetSign("IsRunning", false);
        }

        protected override void Jump(bool isCheckDir = true)
        {
            var needStamina = staminaComponent.GetCostStamina("JumpState");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }

            if (GetSign("IsJumping") == false)
            {
                EnterSubStateMachine<CombatJumpSM>().ChangeState<RunToJumpState>();
            }
        }

        public void MoveRight(bool isCheckDir = true)
        {
            //var needStamina = staminaComponent.GetCostStamina("RunState");
            //if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) {
            //    moveComponent.StopMove();
            //    return; 
            //}
            if (moveComponent.Direction == EDirectionType.Left)
            {
                moveComponent.SetRunSpeed();
            }
            moveComponent.MoveRight(isCheckDir);
        }

        public void MoveLeft(bool isCheckDir = true)
        {
            //var needStamina = staminaComponent.GetCostStamina("RunState");
            //if (!staminaComponent.ChekcHasEnoughStamina(needStamina))
            //{
            //    moveComponent.StopMove();
            //    return;
            //}
            if (moveComponent.Direction == EDirectionType.Right)
            {
                moveComponent.SetRunSpeed();
            }
            moveComponent.MoveLeft(isCheckDir);
        }
    }
}