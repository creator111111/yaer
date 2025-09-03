using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Damage
{
    // 伤害修饰器配置基类（ScriptableObject）
    public abstract class DamageModifierConfig : ScriptableObject
    {
        [Header("基础设置")]
        [Tooltip("修饰器显示名称")]
        public string modifierName;
    
        [TextArea]
        public string description;
    
        [Tooltip("是否默认启用")]
        public bool enabledByDefault = true;

        // 创建对应的运行时修饰器实例
        public abstract IDamageModifier CreateModifier();

        // 可视化配置验证
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(modifierName))
            {
                modifierName = this.GetType().Name;
            }
        }
    }
}