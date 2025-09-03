using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Damage.Modifier
{
    [CreateAssetMenu(menuName = "Damage Modifiers/Weak Damage Modifier")]
    public class WeakDamageModifierConfig: DamageModifierConfig
    {
        public override IDamageModifier CreateModifier()
        {
            return new WeakDamageModifier();
        }
    }
}