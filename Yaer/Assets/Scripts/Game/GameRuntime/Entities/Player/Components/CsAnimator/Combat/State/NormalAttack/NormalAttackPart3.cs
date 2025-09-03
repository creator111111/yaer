using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackPart3 : BaseNormalAttackState
    {
        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            var staminaValue = staminaComponent.GetCostStamina("NorAtkState_3");
            staminaComponent.AddStamina(-staminaValue);
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            // 播放一次音效
            playerLogic.PlayNorAtkAudio();
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<CombatIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}