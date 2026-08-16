using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State
{
    public class FallDownRunState : BaseJumpState
    {
        private Transform shadowTsf;

        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            SetSign("IsJumpEndBefore", true);
            moveComponent.StopMove();
            // playerLogic.SceneManager.SetCinemachineFollow(playerLogic.transform);
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("StartRun", StartRun);
            animationEventComponent.RegisterEvent("AllowJump", AllowJump);

            moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;

            playerLogic.PlayJumpDownInGround();
            playerLogic.PlayClothingAudio();
            // 同 FallDownIdle：落地跑动缓冲阶段也需能接 J/K/L。
            inputComponent.onNormalAtkInput += OnFallDownNormalAttack;
            inputComponent.onSmashAtkInput += OnFallDownSmashAttack;
            inputComponent.onDashAtkInput += OnFallDownDashAttack;
        }


        public override void Update()
        {
            base.Update();
            if (TryConsumeLandingNormalAttackBuffer()) { return; }
            if (TryConsumeLandingSmashAttackBuffer()) { return; }
            if (TryConsumeLandingDashAttackBuffer()) { return; }

            // 获取当前动画状态
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatRunState>();
            }
        }

        public override void Exit()
        {
            inputComponent.onNormalAtkInput -= OnFallDownNormalAttack;
            inputComponent.onSmashAtkInput -= OnFallDownSmashAttack;
            inputComponent.onDashAtkInput -= OnFallDownDashAttack;
            base.Exit();

            SetSign("IsJumping", false);
            SetSign("IsJumpEndBefore", false);
            if (playerLogic.canInStateSetPos) playerLogic.SetPos(csAnimator.GetAnimationPos());
            //playerLogic.componentSystem.GetComponent<PlayerAnimaCameraTrackComponent>().SetMainCameraFollowRoot();
            inputComponent.onJumpInput -= Jump;

            moveComponent.BodyCollider.onCollisionEnterEvent -= OnHitCollider;
        }

        private void Jump(bool isCheckDir = true)
        {
            if (!playerLogic.isEnableJump) { return; }
            ChangeState<RunToJumpState>();
        }

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

        /// <summary>
        /// 动画事件名保留（与落地衔接跑姿的剪辑一致），但<strong>不能</strong>在这里调用 <see cref="PlayerMoveComponent.SetRunSpeed"/>。
        /// 落地缓冲阶段 Enter 已 <see cref="PlayerMoveComponent.StopMove"/>，若在缓冲动画中途写入跑动速度，会与“落地刹停”冲突，
        /// 角色会在缓冲未结束前持续横向移动（玩家体感为无限滑行）；真正进入跑步由 <see cref="CombatRunState"/> 的 Enter 统一 SetRunSpeed。
        /// 需要衔接跑时仅依赖 Animator 过渡，物理速度在 FallDown 结束切到 CombatRunState 后再给。
        /// </summary>
        private void StartRun(string msg)
        {
        }

        private void AllowJump(string msg)
        {
            inputComponent.onJumpInput += Jump;
        }
    }
}