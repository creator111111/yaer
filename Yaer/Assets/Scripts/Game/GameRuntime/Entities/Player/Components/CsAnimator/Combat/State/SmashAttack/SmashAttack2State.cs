using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack
{
    public class SmashAttack2State : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            moveComponent.StopMove();
            playerLogic.canInStateSetPos = true;
            playerLogic.isNoBreakState = true;
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);
            var staminaValue = staminaComponent.GetCostStamina("SmashAtkState_1");
            staminaComponent.AddStamina(-staminaValue);
            // 重击二段出手动画帧比较靠后，所以使用动画帧事件播放音效
            animationEventComponent.RegisterEvent("PlayNorAtkAudioInAniFunc", PlayNorAtkAudioInAniFunc);
        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<CombatIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            playerLogic.isNoBreakState = false;
        }
    }
}

