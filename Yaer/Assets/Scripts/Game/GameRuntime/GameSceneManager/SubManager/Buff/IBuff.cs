namespace Game.GameRuntime.GameSceneManager.SubManager.Buff
{
    public interface IBuff
    {
        bool IsApply { get; }
        IBuffHelper Helper { get; set; }
        void Init(IBuffManager buffManager);
        void Apply();
        void Remove();
        void Update();
    }
}