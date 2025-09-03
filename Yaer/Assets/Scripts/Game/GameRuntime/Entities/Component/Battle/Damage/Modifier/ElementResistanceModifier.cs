namespace Game.GameRuntime.Entities.Component.Battle.Damage.Modifier
{
    // 对应的运行时修饰器实现
    public class ElementResistanceModifier : IDamageModifier
    {
        public DamageProcessPhase Phase => DamageProcessPhase.Resistance;
        public int Priority => 150;
        public bool IsActive { get; set; }

        private readonly ElementResistanceConfig _config;

        public ElementResistanceModifier(ElementResistanceConfig config)
        {
            _config = config;
            IsActive = config.enabledByDefault;
        }

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