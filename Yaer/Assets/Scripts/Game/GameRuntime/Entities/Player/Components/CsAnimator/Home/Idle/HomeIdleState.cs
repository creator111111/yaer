using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState
{
    public class HomeIdleState : BasePlayerState
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

            if (inputComponent.HasMoveInput())
            {
                // 调用 父状态机的状态
                ExitCurrentStateMachine().ChangeState<HomeWalkState>();
            }
        }
    }
}