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
        }


        public override void Update()
        {
            base.Update();

            // 获取当前动画状态
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatRunState>();
            }
        }

        public override void Exit()
        {
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
            ChangeState<RunToJumpState>();
        }

        private void StartRun(string msg)
        {
            moveComponent.SetRunSpeed();
        }

        private void AllowJump(string msg)
        {
            inputComponent.onJumpInput += Jump;
        }
    }
}