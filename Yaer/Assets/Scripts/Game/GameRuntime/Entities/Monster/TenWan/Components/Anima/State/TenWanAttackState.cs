using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Monster.TenWan.Components.Battle;
using Game.GameRuntime.Entities.Monster.WoodWorm;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanAttackState : BaseTenWanState
    {
        public override void Enter()
        {
            base.Enter();
            
            // 一次动画只造成一次伤害
            tenWanLogic.componentSystem.GetComponent<BattleComponent>().GetAttackLogic<TenWanAttackLogic>("Attack").Reset();
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            animationEventComponent.RegisterEvent("PlayAudioSfx", PlayAudioSfx);
            // 设置攻击碰撞体的父节点
            var attckNode = UIUtils.findChild(monsterLogic.gameObject, "Attack");
            if (attckNode != null)
            {
                monsterLogic.atkCollNodeParent = attckNode;
            }

        }

        public override void Update()
        {
            if (monsterLogic.IsDead) { return; }
            base.Update();

            tenWanLogic.componentSystem.GetComponent<BattleComponent>().PerformAttack("Attack");
            FinishedChangeState<TenWanIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            // 攻击状态结束后进入CD
            tenWanLogic.EnterAttackCd();
        }
    }
}