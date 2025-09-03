using Game.GameMgr;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.CoreExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State
{
    public class AttackBossMogutState : BaseCombatState
    {
        float timeCount; // 计时器
        float moveAudioPlayDistance = 0.5f; // 走路音效间隔
        public override void Enter()
        {
            base.Enter();
            //animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);
            animationEventComponent.RegisterEvent("StopMove", StopMove);
            playerLogic.canInStateSetPos = false;
            moveComponent = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            timeCount = moveAudioPlayDistance;
        }

        public override void Update()
        {
            base.Update();
            timeCount += Time.deltaTime;
            if (timeCount >= moveAudioPlayDistance && moveComponent.moveSpeedX > 0)
            {
                timeCount = 0;
                playerLogic.PlayRunAudio();
            }

            if (IsFinished)
            {
                moveComponent.StopMove();
            }
            FinishedChangeState<CombatIdleState>();
        }
    }
}