using Game.GameRuntime.Entities.Component.Battle.SkillInfos;

namespace Game.GameRuntime.Entities.Component.Battle.Attack
{
    public abstract class AttackLogic
    {
        protected SkillInfo info;
        protected BattleComponent attacker;

        public virtual void Initialize(SkillInfo info, BattleComponent attacker)
        {
            this.info = info;
            this.attacker = attacker;
        }

        public abstract bool CanExecute();
        public abstract void Execute();
        public abstract void Cancel();


        protected virtual void ApplyEffects(BattleComponent target)
        {
            foreach (var effect in  info.data.statusEffects)
            {
                effect.ApplyEffect(target);
            }
        }
    }
}