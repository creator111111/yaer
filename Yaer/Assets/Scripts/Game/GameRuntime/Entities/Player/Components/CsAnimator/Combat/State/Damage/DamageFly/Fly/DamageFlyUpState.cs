using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly
{
    public class DamageFlyUpState : BasePlayerDamageState
    {
        private float distance;
        private float height;
        private DamageFlySM sm;
        private Vector2 startPosition;
        
        private Vector2 DirV2 => playerLogic.componentSystem.GetComponent<PlayerMoveComponent>().DirV2;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            
            sm = stateMachine as DamageFlySM;
        }

        public override void Enter()
        {
            base.Enter();
            SetSign("IsBreakUp", true);// 设置目前是被击飞状态
            SetSign("IsJumping", false);
            moveComponent.StopMove();
            moveComponent.SetDamageFlySpeed();
            playerLogic.flyMoveSpeedX = moveComponent.moveSpeedX;
            playerLogic.canInStateSetPos = true;
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);

            moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;
        }

        public override void Update()
        {
            base.Update();
            
            if (moveComponent.IsMoveDown)
            {
                ChangeState<DamageFlyFallState>();
            }
        }

        private void OnHitCollider(Collision2D collision)
        {
            var targetLogic = collision.gameObject;
            var layerName = LayerMask.LayerToName(targetLogic.layer);
            if (layerName == "Map" || layerName.StartsWith("Ground"))
            {
                Debug.Log("击飞过程中发生碰撞");
                playerLogic.OnFlyHitClsEvent?.Invoke();
                ChangeState<DamageFlyClsState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            moveComponent.BodyCollider.onCollisionEnterEvent -= OnHitCollider;
        }
    }
}