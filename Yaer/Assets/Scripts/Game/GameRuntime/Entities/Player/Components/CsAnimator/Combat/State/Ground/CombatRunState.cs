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
            // 监听按键（Idle 时这两条仍为 null，KeyDown 当帧 Parse 会打空；点按 A 的转向必须在下面同步做完）
            inputComponent.onRightInput += MoveRight;
            inputComponent.onLeftInput += MoveLeft;

            // 村庄纯 W/S：无横向意图时不给水平 Run 速度，否则纵深叠横向（0513）；Run 体态仍由 SetBool(Run) 驱动。
            bool village = inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D;
            bool villageDepthOnly = village && !inputComponent.HasVillageExploreHorizontalMoveIntent();
            if (!villageDepthOnly)
            {
                if (village)
                {
                    // 方案 B′：禁止用默认朝右灌速。Idle 丢了 Left 令后，点一下 A 会在 KeyUp 帧队列仍在时 SetRunSpeed(+X)，随后队空再也走不到 MoveLeft。
                    // 替代方案 A：先 SetRunSpeed 再 GetKey 纠正，Enter 当帧仍可能出现 +X；C 把订阅提前到 Idle 面太大；D 改默认朝左会让点 D 反。
                    ApplyVillageCombatRunEnterHorizontalFromInput();
                }
                else
                {
                    moveComponent.SetRunSpeed();
                }

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
        /// 村庄 Combat 进跑且已有横向意图：按队列里的 Left/Right（从队首往后找第一条）同步 <see cref="MoveLeft"/>/<see cref="MoveRight"/>。
        /// 队列尚未入队时（Animator 先于 Input）用 GetKey / Raw Horizontal 兜底。左右同时按下跟队列先后，不另定优先级。
        /// 不在转向前调用 <see cref="PlayerMoveComponent.SetRunSpeed"/>；MoveLeft/Right 内部会翻面写速，零速仍由 0514 补票。
        /// </summary>
        private void ApplyVillageCombatRunEnterHorizontalFromInput()
        {
            ControlInputType queued = FindFirstHorizontalCommandInQueue();
            if (queued == ControlInputType.Left)
            {
                MoveLeft(true);
                return;
            }

            if (queued == ControlInputType.Right)
            {
                MoveRight(true);
                return;
            }

            // 本帧 Input 还没入队，但 Idle 已凭 GetKey(A) 切进 Run：不能用 GetKeyDown（当帧可能已过）。
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                MoveLeft(true);
                return;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                MoveRight(true);
                return;
            }

            const float horizontalDeadZone = 0.01f;
            float axisX = Input.GetAxisRaw("Horizontal");
            if (axisX < -horizontalDeadZone)
            {
                MoveLeft(true);
                return;
            }

            if (axisX > horizontalDeadZone)
            {
                MoveRight(true);
            }
            // 仍解析不出方向：宁可不灌默认 +X，避免点 A 往右滑一下。
        }

        /// <summary>与现网 Parse 一样看队列顺序，取第一条 Left/Right（队首可能是 Jump/Interact）。</summary>
        private ControlInputType FindFirstHorizontalCommandInQueue()
        {
            for (int i = 0; i < 16; i++)
            {
                ControlInputType cmd = inputComponent.GetPlayerCurInputCmd(i);
                if (cmd == ControlInputType.None)
                {
                    break;
                }

                if (cmd == ControlInputType.Left || cmd == ControlInputType.Right)
                {
                    return cmd;
                }
            }

            return ControlInputType.None;
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