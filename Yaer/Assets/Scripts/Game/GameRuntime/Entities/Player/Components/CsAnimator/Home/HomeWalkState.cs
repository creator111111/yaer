using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Home
{
    public class HomeWalkState : BasePlayerState
    {
        float timeCount; // 计时器
        float walkAudioPlayDistance = 0.5f; // 走路音效间隔
        public override void Enter()
        {
            base.Enter();

            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput += InteractAciton;
            // 监听按键
            inputComponent.onRightInput += moveComponent.MoveRight;
            inputComponent.onLeftInput += moveComponent.MoveLeft;
            // 使用走路的速度
            moveComponent.SetWalkSpeed();
            timeCount = 0.1f;// 第一次播放音效不需要时间间隔
            //animationEventComponent.RegisterEvent("PlayAudioSfx", PlayAudioSfx);
        }

        public override void Update()
        {
            base.Update();

            // 无横移且无村庄纵深意图时才回 Idle/Bink，避免按住 W/S 时被误判为静止
            if (!inputComponent.HasMoveInput() && !HasVillageExploreDepthMoveIntent())
            {
                EnterSubStateMachine<HomeIdleSubSM>().ChangeState<HomeBinkState>();
            }

            timeCount += Time.deltaTime;
            if (timeCount >= walkAudioPlayDistance)
            {
                timeCount = 0;
                playerLogic.PlayRunAudio();
            }
        }

        public override void Exit()
        {
            base.Exit();

            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput -= InteractAciton;
            // 取消监听按键
            inputComponent.onRightInput -= moveComponent.MoveRight;
            inputComponent.onLeftInput -= moveComponent.MoveLeft;

            playerLogic.PlayRunAudio(false);
        }
    }
}