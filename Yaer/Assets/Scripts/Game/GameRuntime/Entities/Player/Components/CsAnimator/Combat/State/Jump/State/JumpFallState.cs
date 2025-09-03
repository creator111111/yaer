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