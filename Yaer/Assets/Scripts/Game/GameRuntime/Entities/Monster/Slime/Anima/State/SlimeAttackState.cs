using Game.GameRuntime.Entities.Component.Battle.Damage;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeAttackState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();

            slime.BodyRg.velocity = Vector2.zero;
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            // 设置攻击碰撞体的父节点
            var attckNode = UIUtils.findChild(monsterLogic.gameObject, "Attack1");
            if (attckNode != null)
            {
                monsterLogic.atkCollNodeParent = attckNode;
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished)
            {
                ChangeState<SlimeIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            // 进入cd
            slime.EnterAttackCd();
        }
    }
}