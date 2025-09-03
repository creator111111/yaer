namespace GameFramework.CoreExtend.Component.interf
{
    public interface IGFEComponent
    {
        void Init(IComponentSystem system);
        void Check();
        void OnUpdate();
        void Dispose();
    }
}