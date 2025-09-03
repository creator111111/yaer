using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.GameRuntime.Entities.Component.Battle.Damage
{
    // 伤害处理阶段枚举
    public enum DamageProcessPhase
    {
        PreProcess,   // 伤害生效前
        Resistance,   // 抗性计算
        Mitigation,   // 伤害减免
        Finalization, // 最终处理
        PostProcess   // 伤害生效后
    }

    // 可配置的伤害处理组件
    public class DamageHandler
    {
        protected BattleComponent ownerBattleComponent;
        private Dictionary<Type, IDamageModifier> modifierCache = new Dictionary<Type, IDamageModifier>();
        public event Action<DamageData> onApplyFinalDamage; // 伤害生效后
        public event Action<DamageData> onApplyStatusEffects;
        public event Action<DamageData> onPlayImpactEffects;
        

        public DamageHandler(BattleComponent ownerBattleComponent)
        {
            this.ownerBattleComponent = ownerBattleComponent;
        }

        public T GetModifier<T>() where T : IDamageModifier
        {
            if (modifierCache.TryGetValue(typeof(T), out var modifier))
            {
                return (T)modifier;
            }

            return default;
        }
        
        public void RegisterModifier(DamageModifierConfig modifier)
        {
            var modifierInstance = modifier.CreateModifier();
            modifierInstance.Initialize(ownerBattleComponent);
            modifierCache.Add(modifier.GetType(), modifierInstance);
        }

        /// <summary>
        /// 触发伤害
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(DamageData damage)
        {
            var context = new DamageContext
            {
                source = damage.attacker,
                target = ownerBattleComponent,
                originalDamage = damage,
                modifiedDamage = damage
            };

            ProcessDamage(context);
        }

        /// <summary>
        /// 处理伤害流程
        /// </summary>
        /// <param name="context"></param>
        private void ProcessDamage(DamageContext context)
        {
            // 责任链处理流程
            ExecutePhase(DamageProcessPhase.PreProcess, context);
            if (context.isCanceled) return;

            ExecutePhase(DamageProcessPhase.Resistance, context);
            ExecutePhase(DamageProcessPhase.Mitigation, context);
            ExecutePhase(DamageProcessPhase.Finalization, context);

            ApplyFinalDamage(context.modifiedDamage); // 应用数值
            PlayImpactEffects(context.modifiedDamage); // 播放效果
            ApplyStatusEffects(context.modifiedDamage); // 应用状态

            ExecutePhase(DamageProcessPhase.PostProcess, context);
        }

        /// <summary>
        /// 执行每个流程的逻辑
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="context"></param>
        private void ExecutePhase(DamageProcessPhase phase, DamageContext context)
        {
            if (modifierCache.Count > 0)
            {
                foreach (var modifier in modifierCache.Values
                             .Where(m => m.Phase == phase)
                             .OrderBy(m => m.Priority))
                {
                    modifier.ProcessDamage(context);
                    if (context.isCanceled) break;
                }
            }
        }

        private void ApplyFinalDamage(DamageData finalDamage) => onApplyFinalDamage?.Invoke(finalDamage);

        private void PlayImpactEffects(DamageData finalDamage) => onPlayImpactEffects?.Invoke(finalDamage);

        private void ApplyStatusEffects(DamageData finalDamage) => onApplyStatusEffects?.Invoke(finalDamage);
    }
}