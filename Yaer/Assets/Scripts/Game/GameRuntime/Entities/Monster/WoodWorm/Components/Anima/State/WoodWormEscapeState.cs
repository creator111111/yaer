namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormEscapeState : BaseWoodWormState
    {
        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            animationEventComponent.RegisterEvent("MonsterEscape", MonsterEscape);
            // 逃跑时无敌
            woodWormLogic.isProtect = true;
            // 取消碰撞阻挡
            woodWormLogic.bodyCld.isTrigger = true;
            woodWormLogic.footCld.isTrigger = true;
        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                // 逃跑结束视为怪物死亡
                woodWormLogic.setHasDead(true);
                woodWormLogic.MonsterRealRemove();
            }
        }
    }
}