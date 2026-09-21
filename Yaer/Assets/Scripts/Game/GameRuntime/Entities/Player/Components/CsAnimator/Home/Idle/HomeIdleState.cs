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

            // 横移：队首 Left/Right，或村庄里按住 A/D（进门无新 KeyDown 时队是空的）
            // 纵深：TownPlayerLocomotion 门控（避免纯 W/S 不进 Walk）
            if (inputComponent.HasMoveInput()
                || inputComponent.HasVillageExploreHorizontalMoveIntent()
                || HasVillageExploreDepthMoveIntent())
            {
                // 调用 父状态机的状态
                ExitCurrentStateMachine().ChangeState<HomeWalkState>();
            }
        }
    }
}