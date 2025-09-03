using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Monster.Slime;
using GameFramework.CoreExtend.Component;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormAttackState: BaseWoodWormState
    {
        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            // 设置攻击碰撞体的父节点
            var attckNode = UIUtils.findChild(monsterLogic.gameObject, "WoodWormAttack");
            if (attckNode != null)
            {
                monsterLogic.atkCollNodeParent = attckNode;
            }
        }
        public override void Update()
        {
            base.Update();
            
            FinishedChangeState<WoodWormIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            // 攻击状态结束后进入CD
            woodWormLogic.EnterAttackCd();
        }
    }
}