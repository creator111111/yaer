using Game.GameRuntime.Story.Node;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground
{
    public class CombatIdleState : CombatGroundState
    {

        float runNeedStamina;
        public override void Enter()
        {
            base.Enter();

            moveComponent.StopMove();
            SetSign("IsRunning", false);
            SetSign("IsJumping", false);
            SetSign(PlayerStateSign.Idle, true);
            var staminaValue = staminaComponent.GetCostStamina("IdleState");
            staminaComponent.SetRecoverSpeed(staminaValue);
            runNeedStamina = staminaComponent.GetCostStamina("RunState");
        }

        public override void Update()
        {
            base.Update();

            if (inputComponent.HasMoveInput() && GetSign("IsJumping") == false && GetSign("IsNormalAttacking") == false)
            {
                ChangeState<CombatRunState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            SetSign(PlayerStateSign.Idle, false);
            staminaComponent.SetRecoverSpeed(0);
        }
    }
}