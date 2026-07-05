using Game.GameRuntime.Story.Node;
using Game.Static.Enum;

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

            // 村庄：横向须与 CombatRunState 一致用扩展意图（整队 + Raw Horizontal），否则队首非 Left/Right 时纯 A/D 进不了 Run
            bool hasHorizontal = inputComponent.LocomotionMode == PlayerLocomotionMode.Village2_5D
                ? inputComponent.HasVillageExploreHorizontalMoveIntent()
                : inputComponent.HasMoveInput();
            // 纵深 W/S 仍依赖 Town 门控，与 HomeWalkState 对齐
            if ((hasHorizontal || HasVillageExploreDepthMoveIntent())
                && GetSign("IsJumping") == false
                && GetSign("IsNormalAttacking") == false)
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