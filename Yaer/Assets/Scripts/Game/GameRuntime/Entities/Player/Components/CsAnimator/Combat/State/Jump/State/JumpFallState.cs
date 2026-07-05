using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State
{
    public class JumpFallState : BaseJumpState
    {
        public override void Enter()
        {
            base.Enter();
            SetSign("IsJumpFall", true);
            moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;
        }
        public override void Update()
        {
            base.Update();

            if (moveComponent.IsGrounded)
            {
                moveComponent.OnJumpDown?.Invoke();
                // 禁止在此调用 TryConsumeLandingNormalAttackBuffer / TryBegin：在父状态机 Update 仍递归到本 JumpFall 时
                // ExitCurrentStateMachine 会拆掉 CombatJumpSM 并立刻挂 NormalAttackSM，易导致当帧层级与 Animator 不一致，
                // 或 EnterSubStateMachine 失败时 PlayerCombatSM 无 sub 悬空（表现为卡在 JumpFall、无法移动）。
                // 落地缓冲由 FallDownIdle/FallDownRun 的缓冲 + onNormalAtk 处理。
                if (inputComponent.HasMoveInput())
                {
                    ChangeState<FallDownRunState>();
                }
                else
                {
                    ChangeState<FallDownIdleState>();
                }
                    
            }
        }

        public override void Exit()
        {
            base.Exit();
            SetSign("IsJumpFall", false);
            moveComponent.BodyCollider.onCollisionEnterEvent -= OnHitCollider;
            // 落地灰尘
            // var ef = playerLogic.SceneManager.PlayEffect<JumpDownDustEffect>(new[] { "Effect/Player/Dust/Effect_Player_JumpDownDust.prefab" }, 1,
            //     playerLogic.GetTsf("EffectPos/JumpDownDust").position);
            // ef.SetSrSortLayer(csAnimator.animaSr.sortingLayerName, csAnimator.animaSr.sortingLayerID - 10);
        }


    }
}