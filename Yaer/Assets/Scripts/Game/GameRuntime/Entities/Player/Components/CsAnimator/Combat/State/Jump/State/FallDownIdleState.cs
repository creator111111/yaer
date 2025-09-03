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

        private void AllowJump(string msg)
        {
            inputComponent.onJumpInput += Jump;
        }
    }
}