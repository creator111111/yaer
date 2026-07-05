using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State
{
    public class FallDownIdleState : BaseJumpState
    {
        private Transform shadowTsf;

        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            SetSign("IsJumpEndBefore", true);
            moveComponent.StopMove();
            // 落地缓冲仍属于 CombatJumpSM，地面 CombatGroundState 已 Exit，J/K/L 回调未挂载；此处补订阅才能立刻响应。
            inputComponent.onNormalAtkInput += OnFallDownNormalAttack;
            inputComponent.onSmashAtkInput += OnFallDownSmashAttack;
            inputComponent.onDashAtkInput += OnFallDownDashAttack;
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("AllowJump", AllowJump);
            // playerLogic.SceneManager.SetCinemachineFollow(playerLogic.transform);
            playerLogic.PlayJumpDownInGround();
            playerLogic.PlayClothingAudio();
        }

        public override void Update()
        {
            base.Update();
            //moveComponent.ApplyAnimatedMoveSpeed();
            // 优先消费空中按下的时间缓冲（J→K→L 顺序，避免同帧多键时重复进招）。
            if (TryConsumeLandingNormalAttackBuffer()) { return; }
            if (TryConsumeLandingSmashAttackBuffer()) { return; }
            if (TryConsumeLandingDashAttackBuffer()) { return; }
            // 获取当前动画状态
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatIdleState>();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //moveComponent.ApplyAnimatedMoveSpeed();
        }

        public override void Exit()
        {
            inputComponent.onNormalAtkInput -= OnFallDownNormalAttack;
            inputComponent.onSmashAtkInput -= OnFallDownSmashAttack;
            inputComponent.onDashAtkInput -= OnFallDownDashAttack;
            base.Exit();
            SetSign("IsJumpEndBefore", false);
            SetSign("IsJumping", false);
            moveComponent.StopMove();
            if (playerLogic.canInStateSetPos) playerLogic.SetPos(csAnimator.GetAnimationPos());
            //playerLogic.componentSystem.GetComponent<PlayerAnimaCameraTrackComponent>().SetMainCameraFollowRoot();
            inputComponent.onJumpInput -= Jump;
        }

        private void Jump(bool isCheckDir=true)
        {
            ChangeState<JumpUpState>();
        }

        /// <summary>落地缓冲期普攻：退到 PlayerCombatSM 后进 NormalAttack，与 <see cref="NormalAtkActionFromJumpSubStateMachine"/> 一致。</summary>
        private void OnFallDownNormalAttack()
        {
            NormalAtkActionFromJumpSubStateMachine();
        }

        private void OnFallDownSmashAttack()
        {
            SmashAtkActionFromJumpSubStateMachine();
        }

        private void OnFallDownDashAttack()
        {
            DashAtkActionFromJumpSubStateMachine();
        }

        private void AllowJump(string msg)
        {
            inputComponent.onJumpInput += Jump;
        }
    }
}