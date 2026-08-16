using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump;
using Game.Static.Enum;
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

        /// <summary>
        /// 与 <see cref="TownPlayerLocomotion"/> 中 <c>walkAnimatorDeadZone</c> 同量级；用于「横向目标速度是否已建立」判定（执行文档 0514）。
        /// </summary>
        private const float VillageCombatRunHorizontalDeadZone = 0.12f;
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
            // 村庄纯 W/S：在确认无横向意图（含队首外左/右、Raw Horizontal）时不给水平 Run 速度，否则纵深会叠横向；Run 体态仍由 SetBool(Run) 驱动
            bool villageDepthOnly = inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D
                                    && !inputComponent.HasVillageExploreHorizontalMoveIntent();
            if (!villageDepthOnly)
            {
                moveComponent.SetRunSpeed();
                // 纯纵深不进「真跑」体力曲线，避免村庄探索按 W/S 仍吃 Run 消耗
                var staminaValue = staminaComponent.GetCostStamina("RunState");
                staminaComponent.SetRecoverSpeed(-staminaValue);
            }

            timeCount = moveAudioPlayDistance;// 第一次播放音效不需要时间间隔
        }

        public override void Update()
        {
            base.Update();

            if (IsExit) return;

            // 仅在「本帧仍按住 W/S（Raw Vertical）且确认无横向意图」时清 X。
            // 若用 HasVillageExploreDepthMoveIntent（含松键后 depthVelocity 惯性），会在惯性滑行数帧内仍每帧清 X，
            // 再叠加上 Horizontal Raw 偶发为 0 → 表现为贴台沿能上下不能「向前」；与 TownPlayerLocomotion 用 Raw Vertical 门控纵深一致。
            if (inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D
                && !inputComponent.HasVillageExploreHorizontalMoveIntent()
                && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
            {
                moveComponent.StopMoveInX();
            }

            // 与 HomeWalkState 对称：村庄下横向意图用扩展判定，避免队首非 Left/Right 时误退 Idle
            bool hasHorizontalIntent = inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D
                ? inputComponent.HasVillageExploreHorizontalMoveIntent()
                : inputComponent.HasMoveInput();
            if (!hasHorizontalIntent && !HasVillageExploreDepthMoveIntent())
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
            if (!playerLogic.isEnableJump) { return; }
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

            // 纯 W/S 入 Run 时 Enter 的 villageDepthOnly 会跳过 SetRunSpeed，moveSpeedX 仍为 0；面朝右再按 D 时 TurnRight 同向早退不写速 → 零速补票（0514），对齐 HomeWalkState.Enter 无条件 SetWalkSpeed 的语义。
            EnsureVillageCombatRunHorizontalSpeedIfStale();

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

            EnsureVillageCombatRunHorizontalSpeedIfStale();

            moveComponent.MoveLeft(isCheckDir);
        }

        /// <summary>
        /// 仅村庄 Combat Run：在 <see cref="MoveComponent.TurnRight"/>/<see cref="MoveComponent.TurnLeft"/> 同向早退前，
        /// 若横向目标速度仍接近 0 则补一次 <see cref="PlayerMoveComponent.SetRunSpeed"/>，避免「先 WS 再同向 AD 无位移」。
        /// <para><b>替代方案</b>：改 MoveComponent.Turn* 全局早退语义影响战斗全场景，故收敛在本状态（文档 P2 不推荐）。</para>
        /// </summary>
        private void EnsureVillageCombatRunHorizontalSpeedIfStale()
        {
            if (inputComponent.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            if (Mathf.Abs(moveComponent.moveSpeedX) > VillageCombatRunHorizontalDeadZone)
            {
                return;
            }

            moveComponent.SetRunSpeed();
        }
    }
}