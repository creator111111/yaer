using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State
{
    public class DashAttackState : BaseCombatState
    {
        private bool CanChangeRunState;

        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
            moveComponent.StopMove();
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);
            animationEventComponent.RegisterEvent("ChangeRunState", ChangeRunState);
            animationEventComponent.RegisterEvent("Dust1", Dust1);
            animationEventComponent.RegisterEvent("Dust2", Dust2);
            animationEventComponent.RegisterEvent("CreateAtkCollsion", CreateAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveAtkCollsion", RemoveAtkCollsion);
            CanChangeRunState = false;

            float costStamina = staminaComponent.GetCostStamina("DashAtkState");
            staminaComponent.AddStamina(-costStamina);
            // 取消重力影响并设置为不阻挡类型
            playerLogic.componentSystem.GetComponent<MoveComponent>().canGravity = false;
            playerLogic.bodyCollider.gameObject.layer = monsterCenterLayer; // 设置为怪物图层，需要穿透怪物
            moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;
            csAnimator.SetSign("IsDashAtk", true);

            playerLogic.PlayDashAtkAudio();
            playerLogic.PlayClothingAudio();
        }
        private void OnHitCollider(Collision2D collision)
        {
            var targetLogic = collision.gameObject;
            var layerName = LayerMask.LayerToName(targetLogic.layer);
            if (layerName == "Map" || layerName.StartsWith("Ground"))
            {
                Debug.Log("冲锋过程中发生碰撞");
                // 停止人物水平位移
                moveComponent.StopMove();
                playerLogic.canInStateSetPos = false;// 取消位移事件
                animationEventComponent.UnRegisterEvent("FootAlign");
            }
        }

        public override void Update()
        {
            moveComponent.ApplyAnimatedMoveSpeed();
            if (IsFinished)
            {
                ChangeState<CombatIdleState>();
            }
            if (CanChangeRunState)
            {
                if (inputComponent.HasMoveInput())
                {
                    ChangeState<CombatRunState>();
                }
            }
        }

        private void ChangeRunState(string msg)
        {
            CanChangeRunState = true;
        }

        private void Dust1(string msg)
        {
            playerLogic.DashAttackDust1?.Invoke();
        }

        private void Dust2(string msg)
        {
            playerLogic.DashAttackDust2?.Invoke();
        }

        public override void Exit()
        {
            base.Exit();
            csAnimator.SetSign("IsDashAtk", false);
            playerLogic.componentSystem.GetComponent<MoveComponent>().canGravity = true;
            playerLogic.bodyCollider.gameObject.layer = playerLayer; // 结束动作后重新设置为人物层
        }
    }
}

