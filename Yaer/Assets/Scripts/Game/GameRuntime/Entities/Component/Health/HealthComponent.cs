using System;
using GameFramework.UnityRuntimeExtend.Component;

namespace Game.GameRuntime.Entities.Component.Health
{
    public class HealthComponent : BaseGFComponentMono
    {
        public float hp;
        public float maxHp;

        public bool IsDead => hp <= 0;

        public event Action<float> onHpChange;
        public event Action onHpIsZero;

        protected override void OnInit()
        {
        }

        public override void OnUpdate()
        {
        }

        public void SetData(float hp, float maxHp)
        {
            bool changed = hp != this.hp;
            this.hp = hp;
            this.maxHp = maxHp;
            if (changed)
            {
                onHpChange?.Invoke(hp);
            }
        }

        public bool IsMax => hp >= maxHp;

        public void AddHp(float value)
        {
            if ((hp += value) > maxHp) hp = maxHp;
            onHpChange?.Invoke(hp);
            if (hp <= 0) onHpIsZero?.Invoke();
        }

        public void TakeDamage(float value)
        {
            hp -= value;
            onHpChange?.Invoke(hp);
            if (hp <= 0) onHpIsZero?.Invoke();
        }
    }
}