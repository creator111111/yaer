using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Attack
{
    /// <summary>
    /// 造成单次伤害的简单攻击数据
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleDamageAttackData", menuName = "ScriptableObjects/AttackData/SimpleDamageAttackData")]
    public class SimpleDamageAttackData : AttackData
    {
        public float Damage;
    }
}
