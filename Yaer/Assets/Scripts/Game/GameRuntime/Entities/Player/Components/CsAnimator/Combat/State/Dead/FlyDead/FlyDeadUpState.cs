using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead
{
    public class FlyDeadUpState : BasePlayerState
    {
        public override void Update()
        {
            base.Update();

            if (moveComponent.IsMoveDown)
            {
                ChangeState<FlyDeadFallState>();
            }
        }

        public override void Enter()
        {
            base.Enter();
            moveComponent.StopMove();
            moveComponent.SetDamageFlySpeed();
            playerLogic.canInStateSetPos = true;
            animationEventComponent.RegisterEvent("FootAlign", FootAlign);

            moveComponent.BodyCollider.onCollisionEnterEvent += OnHitCollider;
        }

        private void OnHitCollider(Collision2D collision)
        {
            Debug.Log("击飞过程中发生碰撞");
            playerLogic.OnFlyHitClsEvent?.Invoke();
            ChangeState<FlyDeadClsState>();
        }

        public override void Exit()
        {
            base.Exit();
            moveComponent.BodyCollider.onCollisionEnterEvent -= OnHitCollider;
        }
    }
}