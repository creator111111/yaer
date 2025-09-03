using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle.SkillInfos;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Attack
{
    public class AttackHandler
    {
        private BattleComponent attacker;
        private Dictionary<string, AttackLogic> attackLogicMap = new Dictionary<string, AttackLogic>();
        private Dictionary<string, SkillInfo> skillMap = new Dictionary<string, SkillInfo>();
        private AttackLogic currentAttack;

        public AttackHandler(BattleComponent attacker)
        {
            this.attacker = attacker;
        }

        public void RegisterAttack(SkillInfo info)
        {
            if (skillMap.ContainsKey(info.skillName))
            {
                Debug.LogError("SkillInfo重复注册 " + info.name);
                return;
            }
            
            
            var logic = info.data.CreateLogic();
            logic?.Initialize(info, attacker);
            attackLogicMap.Add(info.skillName, logic);
            skillMap.Add(info.skillName, info);
        }

        public void PerformAttack(string attackName)
        {
            if (attackLogicMap.TryGetValue(attackName, out var attack))
            {
                if (attack.CanExecute())
                {
                    currentAttack = attack;
                    attack.Execute();
                }
            }
            else
            {
                Debug.LogError(GetType().Name + "没有注册攻击: " + attackName);
            }
        }

        public void CancelAttack()
        {
            currentAttack?.Cancel();
        }

        public void PerformDamage(int damage)
        {
        }

        public T GetAttackLogic<T>(string skillName) where T : class
        {
            attackLogicMap.TryGetValue(skillName, out var logic);
            return logic as T;
        }
    }
}