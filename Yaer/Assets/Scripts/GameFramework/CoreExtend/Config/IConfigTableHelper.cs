namespace GameFramework.CoreExtend.Config
{
    public interface IConfigTableHelper
    {
        T Parse<T>(string str);
        T Parse<T>(byte[] bytes);
    }
}