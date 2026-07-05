using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState
{
    public class HomeBinkState : BasePlayerState
    {

        public override void Enter()
        {
            base.Enter();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput += InteractAciton;
            moveComponent.StopMove();
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput -= InteractAciton;
        }

        public override void Update()
        {
            base.Update();

            // 眨眼完成后播放待机
            if (IsFinished)
            {
                ChangeState<HomeIdleState>();
                return;
            }

            // 与 HomeIdleState 一致：村庄纯纵深远也切 Walk，保证 SetWalkSpeed / 脚步与 Animator 对齐
            if (inputComponent.HasMoveInput() || HasVillageExploreDepthMoveIntent())
            {
                ExitCurrentStateMachine().ChangeState<HomeWalkState>();
            }
        }
    }
}