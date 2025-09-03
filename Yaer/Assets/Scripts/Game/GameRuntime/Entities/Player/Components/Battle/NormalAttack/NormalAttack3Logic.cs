using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Battle.SkillInfos;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Component.PhysicsDetect.HitPoint;
using Game.GameRuntime.Entities.Monster;
using Game.Static.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.Battle.NormalAttack
{
    public class NormalAttack3Logic : AttackLogic
    {
        private PlayerLogic playerLogic;
        private SimpleDamageAttackData data;

        public override void Initialize(SkillInfo info, BattleComponent attacker)
        {
            base.Initialize(info, attacker);

            data = info.data as SimpleDamageAttackData;
            playerLogic = attacker.Entity.Logic as PlayerLogic;
        }

        public override void Execute()
        {
            SkillShapeCircleData skillShapeData = (SkillShapeCircleData)info.GetSkillShapeData();
            Collider2D[] monsterClds = new Collider2D[100];
            Physics2DUtility.CircleDetectMulti(skillShapeData.center, skillShapeData.radius, ref monsterClds, "Enemy");
            if (monsterClds != null)
            {
                DamageData damageData = new DamageData()
                {
                    baseDamage = (int)data.Damage,
                    attacker = attacker,
                    attackOriginPoint = skillShapeData.center
                };
                HashSet<BaseMonster> alreadyHit = new HashSet<BaseMonster>();
                foreach (var monsterCld in monsterClds)
                {
                    if (monsterCld == null) break;

                    if (monsterCld.GetComponent<ColliderResponder>()?.GetEntityLogic() is BaseMonster monster)
                    {
                        if (alreadyHit.Contains(monster))
                        {
                            continue;
                        }

                        alreadyHit.Add(monster);
                        monster.componentSystem.GetComponent<BattleComponent>().TakeDamage(damageData);

                        // 攻击特效
                        var hitPoint = monster.componentSystem.GetComponent<HitPointComponent>()
                            .GetHitPoint(MathUtility.GetP1ToP2YProjectionPoint(skillShapeData.center, playerLogic.transform.position));
                        // playerLogic.SceneManager.PlayEffect<PlayerNormalAttackEffect>(new[] { data.effectPrefabPath }, 1, hitPoint.position);
                    }
                }
            }
        }

        public override void Cancel()
        {
        }

        public override bool CanExecute()
        {
            return true;
        }
    }
}