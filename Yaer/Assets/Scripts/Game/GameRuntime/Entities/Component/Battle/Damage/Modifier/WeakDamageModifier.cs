namespace Game.GameRuntime.Entities.Component.Battle.Damage.Modifier
{
    public class WeakDamageModifier: IDamageModifier
    {
        public DamageProcessPhase Phase { get; }
        public int Priority { get; }
        public bool IsActive { get; set; }
        
        public void Initialize(IBattleComponent owner)
        {
        }

        public void ProcessDamage(DamageContext context)
        {
        }

        public void UpdateModifier(float deltaTime)
        {
        }

        public void Dispose()
        {
        }
    }
}