using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat;
using Game.Static.Enum;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground
{
    public class CombatGroundState : BaseCombatState
    {
        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            // 监听跳跃
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onJumpInput += Jump;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput += SquatAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSitDownInput += SitDownAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput += NormalAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSmashAtkInput += SmashAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onDashAtkInput += DashAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput += InteractAciton;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
            
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onJumpInput -= Jump;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSquatInput -= SquatAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSitDownInput -= SitDownAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onNormalAtkInput -= NormalAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onSmashAtkInput -= SmashAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onDashAtkInput -= DashAtkAction;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().onInteractInput -= InteractAciton;
        }

        protected virtual void Jump(bool isCheckDir=true)
        {
            var needStamina = staminaComponent.GetCostStamina("JumpState");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            if (GetSign("IsJumping") == false)
            {
                EnterSubStateMachine<CombatJumpSM>().ChangeState<JumpUpState>();
            }
        }

        // 下蹲
        protected virtual void SquatAction()
        {
            EnterSubStateMachine<SquatSM>().ChangeState<SquatDownState>();
            if (PlayerGuideMgr.getInstance().inShowSquatTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowSquatTips = false;
            }
        }
        // 坐下
        protected virtual void SitDownAction()
        {
            EnterSubStateMachine<SitSubSM>().ChangeState<SitDownState>();
            if (PlayerGuideMgr.getInstance().inShowSitTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowSitTips = false;
            }
        }

        // 普通攻击
        protected virtual void NormalAtkAction()
        {
            // 先校验体力与开关，再消费输入：避免先 Consume 导致未出招却清空指令（落地后狂按 J 无效）。
            var needStamina = staminaComponent.GetCostStamina("NorAtkState_1");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            if (!playerLogic.isEnableNorAtk) { return; }
            inputComponent.ConsumeNormalAttackInput();
            EnterSubStateMachine<NormalAttackSM>().ChangeState<NormalAttackPart1>();
            if (PlayerGuideMgr.getInstance().inShowNorAtkTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowNorAtkTips = false;
            }
            //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowNorAtkTips)
            //{
            //    // 第一次指引攻击后让按键提示消失
            //    playerLogic.showKeyTipsNode(false);
            //    FirstMeetSlimeGuideStoryMgr.getInstance().inShowNorAtkTips = false;
            //}
        }
        // 重击
        protected virtual void SmashAtkAction()
        {
            var needStamina = staminaComponent.GetCostStamina("SmashAtkState_1");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            inputComponent.ConsumeSmashAttackInput();
            EnterSubStateMachine<SmashAttackSubSM>().ChangeState<SmashAttack1State>();
            if (PlayerGuideMgr.getInstance().inShowSmashAtkTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowSmashAtkTips = false;
            }
            //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowSmallAtkTips)
            //{
            //    // 第一次指引攻击后让按键提示消失
            //    playerLogic.showKeyTipsNode(false);
            //    FirstMeetSlimeGuideStoryMgr.getInstance().inShowSmallAtkTips = false;
            //}
        }

        // 冲刺攻击
        protected virtual void DashAtkAction()
        {
            var needStamina = staminaComponent.GetCostStamina("DashAtkState");
            if (!staminaComponent.ChekcHasEnoughStamina(needStamina)) { return; }
            inputComponent.ConsumeDashAttackInput();
            ChangeState<DashAttackState>();
            if (PlayerGuideMgr.getInstance().inShowDashAtkTips)
            {
                // 第一次指引攻击后让按键提示消失
                playerLogic.ShowActionKeyTipsNode(false);
                PlayerGuideMgr.getInstance().inShowDashAtkTips = false;
            }
        }

        public void TestAnimation()
        {
            if (inputComponent.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeState<Damage1State>();
            }
            else if (inputComponent.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeState<Damage2State>();
            }
            else if (inputComponent.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeState<Dead1State>();
            }
            else if (inputComponent.GetKeyDown(KeyCode.Alpha4))
            {
                ChangeState<Dead2State>();
            }
            else if (inputComponent.GetKeyDown(KeyCode.Alpha5))
            {
                EnterSubStateMachine<FlyDeadSM>().ChangeState<FlyDeadUpState>();
            }
            else if (inputComponent.GetKeyDown(KeyCode.Alpha7))
            {
                EnterSubStateMachine<DamageFlySM>().ChangeState<DamageFlyUpState>();
            }

            if (inputComponent.GetKeyDown(KeyCode.LeftShift))
            {
                ChangeState<DashAttackState>();
            }
        }
    }
}