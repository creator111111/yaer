using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Battle.SkillInfos;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle
{
    public class BattleComponent : BaseGFComponentEntity, IBattleComponent
    {
        private AttackHandler attackHandler;
        private DamageHandler damageHandler;

        [SerializeField] private Transform skillInfoRoot;
        [SerializeField] private List<SkillInfo> skillInfoList = new List<SkillInfo>();

        /// <summary>
        /// 记录上一次释放技能的时间
        /// </summary>
        private Dictionary<string, float> SkillCDTimer = new Dictionary<string, float>();

        public event Action<DamageData> OnApplyFinalDamage
        {
            add => damageHandler.onApplyFinalDamage += value;
            remove => damageHandler.onApplyFinalDamage -= value;
        }

        public event Action<DamageData> OnApplyStatusEffects
        {
            add => damageHandler.onApplyStatusEffects += value;
            remove => damageHandler.onApplyStatusEffects -= value;
        }
        
        public event Action<DamageData> OnPlayImpactEffects
        {
            add => damageHandler.onPlayImpactEffects += value;
            remove => damageHandler.onPlayImpactEffects -= value;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (skillInfoRoot != null)
            {
                skillInfoList = skillInfoRoot.GetComponentsInChildren<SkillInfo>().ToList();
            }
        }
#endif

        protected override void OnInit()
        {
            // 使用默认处理
            attackHandler = new AttackHandler(this);
            damageHandler = new DamageHandler(this);

            foreach (var info in skillInfoList)
            {
                RegisterAttack(info);
            }
        }

        public override void Check()
        {
            base.Check();

            if (attackHandler == null)
            {
                Debug.LogError("AttackHandler为空 " + GetType().Name, gameObject);
            }

            if (damageHandler == null)
            {
                Debug.LogError("DamageHandler为空 " + GetType().Name, gameObject);
            }
        }

        // --------------------------------------------------------------------------------

        public void RegisterAttack(SkillInfo info) => attackHandler.RegisterAttack(info);

        public bool PerformAttack(string skillName)
        {
            float currentTime = Time.time;
            if (!SkillInCDTime(skillName))
            {
                SkillCDTimer[skillName] = currentTime;
                attackHandler.PerformAttack(skillName);
                return true;
            }
            return false;
        }

        private float GetCoolDownTime(string skillName)
        {
            var skillInfo = skillInfoList.Find((x) => x.skillName == skillName);
            return skillInfo == null ? float.MaxValue : skillInfo.data.cooldown;
        }

        /// <summary>
        /// 技能是否正在冷却
        /// </summary>
        /// <returns></returns>
        public bool SkillInCDTime(string skillName)
        {
            float currentTime = Time.time;
            float cd = GetCoolDownTime(skillName);
            if (!SkillCDTimer.TryGetValue(skillName, out var lastTime))
            {
                lastTime = 0;
            }
            return currentTime - lastTime < cd;
        }

        public void CancelAttack() => attackHandler.CancelAttack();


        // --------------------------------------------------------------------------------

        public void TakeDamage(DamageData damage) => damageHandler.TakeDamage(damage);

        //-----------------------------------------------------------------------------------

        public T GetAttackLogic<T>(string skillName) where T : class => attackHandler.GetAttackLogic<T>(skillName);
    }
}