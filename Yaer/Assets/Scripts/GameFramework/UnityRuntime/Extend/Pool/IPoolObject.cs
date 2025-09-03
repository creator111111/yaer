namespace GameFramework.UnityRuntimeExtend.Pool
{
    /// <summary>
    ///     使用对象池的对象要继承该接口
    /// </summary>
    public interface IPoolObject
    {
        void OnGet();
        void OnPush();
    }
}