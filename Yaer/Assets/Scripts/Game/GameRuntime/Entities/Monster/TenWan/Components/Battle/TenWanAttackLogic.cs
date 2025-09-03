using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Battle.SkillInfos;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Player;
using Game.Static.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Battle
{
    public class TenWanAttackLogic: AttackLogic
    {
        private SimpleDamageAttackData data;
        private HashSet<PlayerLogic> alreadyAttack = new HashSet<PlayerLogic>();
        
        public override void Initialize(SkillInfo info, BattleComponent attacker)
        {
            base.Initialize(info, attacker);
            
            data = info.data as SimpleDamageAttackData;
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override void Execute()
        {
            return;
            //SkillShapeCircleData skillShapeData = (SkillShapeCircleData)info.GetSkillShapeData();
            //var targetClds = Physics2DUtility.CircleDetectMulti(skillShapeData.center, skillShapeData.radius);
            //if (targetClds != null)
            //{
            //    DamageData damageData = new DamageData()
            //    {
            //        baseDamage = (int)data.Damage,
            //        attacker = attacker,
            //        attackOriginPoint = skillShapeData.center
            //    };
            //    foreach (var cld in targetClds)
            //    {
            //        if (cld == null) break;

            //        if (cld.GetComponent<ColliderResponder>()?.GetEntityLogic() is PlayerLogic playerLogic)
            //        {
            //            // 一次攻击动画只造成一次伤害
            //            if (alreadyAttack.Contains(playerLogic))
            //            {
            //                continue;
            //            }

            //            alreadyAttack.Add(playerLogic);
            //            playerLogic.componentSystem.GetComponent<BattleComponent>().TakeDamage(damageData);
            //            Debug.Log("对player造成伤害");
            //        }
            //    }
            //}
        }

        public override void Cancel()
        {
        }
        
        public void Reset()
        {
            alreadyAttack.Clear();
        }
    }
}