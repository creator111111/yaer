namespace Game.GameRuntime.GameSceneManager.SubManager.Buff
{
    public class BaseBuff : IBuff
    {
        protected bool prefabsIsCreated;

        protected IBuffManager BuffManager { get; private set; }
        public bool IsApply { get; private set; }
        public IBuffHelper Helper { get; set; }

        public virtual void Init(IBuffManager buffManager)
        {
            BuffManager = buffManager;
        }

        public virtual void Apply()
        {
            IsApply = true;
        }

        public virtual void Update()
        {
        }

        public virtual void Remove()
        {
            IsApply = false;
        }

        protected virtual void Reset()
        {
            IsApply = false;
        }
    }
}