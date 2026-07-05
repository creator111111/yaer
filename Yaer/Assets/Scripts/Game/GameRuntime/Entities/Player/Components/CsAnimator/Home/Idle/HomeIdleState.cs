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

            // 横移：原 HasMoveInput；村庄纵深：TownPlayerLocomotion 门控（执行说明 §5.2，避免纯 W/S 不进 Walk）
            if (inputComponent.HasMoveInput() || HasVillageExploreDepthMoveIntent())
            {
                // 调用 父状态机的状态
                ExitCurrentStateMachine().ChangeState<HomeWalkState>();
            }
        }
    }
}