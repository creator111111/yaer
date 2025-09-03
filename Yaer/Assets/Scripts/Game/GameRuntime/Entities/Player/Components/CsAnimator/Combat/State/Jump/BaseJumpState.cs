using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump
{
    public class BaseJumpState : BaseCombatState
    {
        protected CombatJumpSM sm;
        

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            
            sm = stateMachine as CombatJumpSM;
            
        }

        public override void Enter()
        {
            base.Enter();
            playerLogic.canInStateSetPos = true;
        }

        public override void Exit()
        {
            base.Exit();
        }

        public void OnHitCollider(Collision2D collision)
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 collisionNormal = contact.normal;
            // 只判断水平方向上的碰撞
            if (Mathf.Abs(collisionNormal.x) > Mathf.Abs(collisionNormal.y))
            {
                // 跳跃过程碰撞到物体则设置速度为0
                moveComponent.moveSpeedX = 0;
                // 在空中碰到东西则设置脚步跟随方法失效
                playerLogic.canInStateSetPos = false;
            }
            
        }
    }
}