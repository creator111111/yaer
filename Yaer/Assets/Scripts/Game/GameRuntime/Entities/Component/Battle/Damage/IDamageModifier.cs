namespace Game.GameRuntime.Entities.Component.Battle.Damage
{
    // 对应的运行时修饰器接口增强版
    public interface IDamageModifier
    {
        DamageProcessPhase Phase { get; }
        int Priority { get; }
    
        // 是否激活（支持运行时动态启用/禁用）
        bool IsActive { get; set; }

        // 初始化方法（用于获取宿主信息）
        void Initialize(IBattleComponent owner);

        void ProcessDamage(DamageContext context);
        
        // 更新方法（适用于持续生效的修饰器）
        void UpdateModifier(float deltaTime);
    
        // 销毁时的清理逻辑
        void Dispose();
    }
}