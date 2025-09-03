using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Buff;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Damage
{
    // 增强版伤害数据结构
    public struct DamageData
    {
        public int baseDamage;
        public Vector2 dirPos; // 伤害来源方向(1,0)表示水平向右的伤害
        public Vector3 attackOriginPoint;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public AttackType attackType;
        public AtkCollsionType atkCollsionType; // 伤害来源类型
        public ElementType elementType;
        public BattleComponent attacker;
        public List<BuffEffect> attachedEffects;
        public float breakWidth;
        public float breakHight;
        public float breakTime;
        public string atkObjName; // 本次攻击命中的碰撞体对象名称
        public string atkSkillName; // 本次攻击的技能名称
    }
}