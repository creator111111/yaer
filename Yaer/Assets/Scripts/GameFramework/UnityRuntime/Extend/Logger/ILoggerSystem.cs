using GameFramework.CoreExtend.Base;

namespace GameFramework.CoreExtend.Systems.Logger
{
    public interface ILoggerSystem: IGFExtendSystem
    {
        void Init();
        void Close();
    }
}