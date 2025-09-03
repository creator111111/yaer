using Game.GameRuntime.Entities.Component.Battle.Attack;
using UnityEditor;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Damage.Modifier
{
    // 元素抗性配置（具体修饰器配置）
    [CreateAssetMenu(menuName = "Damage Modifiers/Element Resistance")]
    public class ElementResistanceConfig : DamageModifierConfig
    {
        [Header("抗性设置")] public ElementType elementType;

        [Range(0, 1)] public float resistancePercentage = 0.3f;

        [Tooltip("是否对过量伤害有额外抗性")] public bool hasOverflowProtection;

        public override IDamageModifier CreateModifier()
        {
            return new ElementResistanceModifier(this);
        }

        // 自定义属性抽屉
#if UNITY_EDITOR
        [CustomEditor(typeof(ElementResistanceConfig))]
        public class ElementResistanceEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var config = target as ElementResistanceConfig;
                if (config.hasOverflowProtection)
                {
                    EditorGUILayout.HelpBox("过量保护生效时，超过当前生命值的伤害将额外减免50%", MessageType.Info);
                }
            }
        }
#endif
    }

}