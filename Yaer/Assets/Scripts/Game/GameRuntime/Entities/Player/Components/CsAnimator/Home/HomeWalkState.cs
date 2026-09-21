using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.IdleSubState;
using Game.Static.Enum;
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

            // 村庄：进 Walk 当帧就对齐脸和横向输入（订阅可能还没吃到这一下 A/D）
            SyncVillageFacingToHorizontalInput();

            timeCount = 0.1f;// 第一次播放音效不需要时间间隔
            //animationEventComponent.RegisterEvent("PlayAudioSfx", PlayAudioSfx);
        }

        public override void Update()
        {
            base.Update();

            // 按住横键但队首不是 Left/Right（或进门时没有新的 KeyDown）时，HasMoveInput 为假，
            // Town 仍会按横向符号位移。这里不能退回 Idle，否则脸和速度再次脱节。
            if (!inputComponent.HasMoveInput()
                && !inputComponent.HasVillageExploreHorizontalMoveIntent()
                && !HasVillageExploreDepthMoveIntent())
            {
                EnterSubStateMachine<HomeIdleSubSM>().ChangeState<HomeBinkState>();
            }

            // 走路过程中持续对齐：只在 Enter 补一次，按住 A 跨场景 / 队首被占时仍会脸朝右、人往左
            SyncVillageFacingToHorizontalInput();

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

        /// <summary>
        /// 村庄走路：脸跟 <see cref="PlayerInputComponent.GetVillageExploreHorizontalSign"/> 走。
        /// 与 Town 写 vx 用同一个符号，避免脸朝右、速度朝左（倒着走）。
        /// 符号为 0（纯 W/S）不转身。
        /// </summary>
        /// <remarks>
        /// 原因（0920）：Enter 只补一次，且 Resolve 优先看队列，Town 优先看轴，两边可能相反。
        /// 替代：只改落点朝向 —— 按住 A 进门没有新的 KeyDown，仍然不转。
        /// </remarks>
        private void SyncVillageFacingToHorizontalInput()
        {
            if (inputComponent.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            float sx = inputComponent.GetVillageExploreHorizontalSign();
            if (sx < -0.01f && moveComponent.Direction != EDirectionType.Left)
            {
                moveComponent.MoveLeft(true);
            }
            else if (sx > 0.01f && moveComponent.Direction != EDirectionType.Right)
            {
                moveComponent.MoveRight(true);
            }
        }
    }
}
