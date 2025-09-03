using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Move;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State
{
    public class JumpUpState : BaseJumpState
    {

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
        }

        public override void Enter()
        {
            base.Enter();

            // 跳跃灰尘
            // var effect = playerLogic.SceneManager.PlayEffect<JumpUpDustEffect>(new[] { "Effect/Player/Dust/Effect_Player_JumpUpDust.prefab" }, 1,
            //     playerLogic.GetTsf("EffectPos/JumpUpDust").position);
            // effect.SetSrSortLayer(csAnimator.animaSr.sortingLayerName, csAnimator.animaSr.sortingLayerID - 10);


            // if (moveComponent.DirV2 == Vector2.right)
            // {
            //     effect.SetLeft();
            // }
            // else
            // {
            //     effect.SetRight();
            // }
            // 落地状态中如果当前指令有左右则执行对应方法
            var curInputCmd = inputComponent.GetPlayerCurInputCmd(0);
            if (curInputCmd == Static.Enum.ControlInputType.Right) { moveComponent.MoveRight(); }
            if (curInputCmd == Static.Enum.ControlInputType.Left) { moveComponent.MoveLeft(); }
            SetSign("IsJumping", true);
            SetSign("IsJumpUp", true);

            //playerLogic.componentSystem.GetComponent<PlayerAnimaCameraTrackComponent>().SetMainCameraFollowTrack();
            playerLogic.canInStateSetPos = true;
            animationEventComponent.RegisterEvent("JumpAddSpeed", JumpAddSpeed);
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            moveComponent.StopMove();
            var staminaValue = staminaComponent.GetCostStamina("JumpState");
            staminaComponent.AddStamina(-staminaValue);
            //moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;
        }

        public override void Update()
        {
            base.Update();
            if (playerLogic.hasInStoryEventState) {
                moveComponent.StopMove();
                ChangeState<JumpFallState>();
                return;
            }
            if (moveComponent.IsMoveDown) ChangeState<JumpFallState>();
        }

        private void JumpAddSpeed(string msg)
        {
            moveComponent.SetJumpSpeed();
        }

        public override void Exit()
        {
            base.Exit();
            SetSign("IsJumpUp", false);
            //moveComponent.BodyCollider.onCollisionEnterEvent -= OnHitCollider;
        }
    }
}