using System.Collections.Generic;

namespace Game.GameRuntime.Entities.Component.Battle.Damage
{
    // 伤害处理上下文
    public class DamageContext
    {
        public BattleComponent source;
        public BattleComponent target;
        public DamageData originalDamage;
        public DamageData modifiedDamage;
        public bool isCanceled;
        public List<IDamageModifier> ProcessingModifiers = new List<IDamageModifier>();
    }
}