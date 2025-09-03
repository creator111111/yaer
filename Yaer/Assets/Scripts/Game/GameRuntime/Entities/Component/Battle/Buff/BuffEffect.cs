using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Buff
{
    // 状态效果基类
    public abstract class BuffEffect : ScriptableObject
    {
        public float duration;
        public abstract void ApplyEffect(BattleComponent target);
    }
}