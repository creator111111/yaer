using Game.GameMgr;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State
{
    public class BaseCombatState : BasePlayerState
    {
        /// <summary>
        /// 落地后 J/K/L 等战斗输入缓冲（秒），与 <see cref="PlayerInputComponent"/> 时间戳配合。
        /// 仅用于 FallDown 内，勿在 JumpFall 首帧调用 TryBegin（见 JumpFallState 注释）。
        /// </summary>
        protected const float LandingCombatInputBufferSeconds = 0.2f;

        protected bool TryConsumeLandingNormalAttackBuffer()
        {
            if (!inputComponent.HasRecentNormalAttackInput(LandingCombatInputBufferSeconds)) { return false; }
            return TryBeginNormalAttackFromJumpSubStateMachine();
        }

        protected bool TryConsumeLandingSmashAttackBuffer()
        {
            if (!inputComponent.HasRecentSmashAttackInput(LandingCombatInputBufferSeconds)) { return false; }
            return TryBeginSmashAttackFromJumpSubStateMachine();
        }

        protected bool TryConsumeLandingDashAttackBuffer()
        {
            if (!inputComponent.HasRecentDashAttackInput(LandingCombatInputBufferSeconds)) { return false; }
            return TryBeginDashAttackFromJumpSubStateMachine();
        }

        protected void NormalAtkActionFromJumpSubStateMachine()
        {
            TryBeginNormalAttackFromJumpSubStateMachine();
        }

        /// <summary>落地缓冲内重击：与 <see cref="CombatGroundState.SmashAtkAction"/> 等价，但从 CombatJumpSM 退出后再进 SmashAttackSubSM。</summary>
        protected void SmashAtkActionFromJumpSubStateMachine()
        {
            TryBeginSmashAttackFromJumpSubStateMachine();
        }

        /// <summary>落地缓冲内冲击攻击：与 <see cref="CombatGroundState.DashAtkAction"/> 等价，Dash 为 PlayerCombatSM 上的直接状态而非子状态机。</summary>
        protected void DashAtkActionFromJumpSubStateMachine()
        {
            TryBeginDashAttackFromJumpSubStateMachine();
        }

        /// <summary>
        /// 从 CombatJumpSM 退出并进入普攻。顺序：先 Exit 再刹停；EnterSubStateMachine 失败时回退 CombatIdle。
        /// </summary>
        private bool TryBeginNormalAttackFromJumpSubStateMachine()
        {
            var needStamina = staminaComponent.GetCostStamina("NorAtkState_1");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return false; }
            if (!playerLogic.isEnableNorAtk) { return false; }
            var combatSM = ExitCurrentStateMachine<PlayerCombatSM>();
            if (combatSM == null) { return false; }
            moveComponent.StopMove();
            var atkSm = combatSM.EnterSubStateMachine<NormalAttackSM>();
            if (atkSm == null)
            {
                combatSM.ChangeState<CombatIdleState>();
                return false;
            }
            inputComponent.ConsumeNormalAttackInput();
            atkSm.ChangeState<NormalAttackPart1>();
            if (PlayerGuideMgr.getInstance().inShowNorAtkTips)
            {
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowNorAtkTips = false;
            }
            return true;
        }

        /// <summary>落地重击：退到 PlayerCombatSM 后进 SmashAttackSubSM → SmashAttack1。</summary>
        private bool TryBeginSmashAttackFromJumpSubStateMachine()
        {
            var needStamina = staminaComponent.GetCostStamina("SmashAtkState_1");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return false; }
            var combatSM = ExitCurrentStateMachine<PlayerCombatSM>();
            if (combatSM == null) { return false; }
            moveComponent.StopMove();
            var smashSm = combatSM.EnterSubStateMachine<SmashAttackSubSM>();
            if (smashSm == null)
            {
                combatSM.ChangeState<CombatIdleState>();
                return false;
            }
            inputComponent.ConsumeSmashAttackInput();
            smashSm.ChangeState<SmashAttack1State>();
            if (PlayerGuideMgr.getInstance().inShowSmashAtkTips)
            {
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowSmashAtkTips = false;
            }
            return true;
        }

        /// <summary>落地冲击攻击：退到 PlayerCombatSM 后直接切 DashAttackState。</summary>
        private bool TryBeginDashAttackFromJumpSubStateMachine()
        {
            var needStamina = staminaComponent.GetCostStamina("DashAtkState");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return false; }
            var combatSM = ExitCurrentStateMachine<PlayerCombatSM>();
            if (combatSM == null) { return false; }
            moveComponent.StopMove();
            inputComponent.ConsumeDashAttackInput();
            combatSM.ChangeState<DashAttackState>();
            if (PlayerGuideMgr.getInstance().inShowDashAtkTips)
            {
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowDashAtkTips = false;
            }
            return true;
        }
    }
}
