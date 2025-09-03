using GameFramework.CoreExtend.Serialiizer.Json;

namespace GameFramework.CoreExtend.Config
{
    public class DefaultConfigTableHelper: IConfigTableHelper
    {
        public T Parse<T>(string str)
        {
            return JsonSystem.Instance.Parse<T>(str, EJsonTool.NewtonsoftJson);
        }

        public T Parse<T>(byte[] bytes)
        {
            return JsonSystem.Instance.Parse<T>(bytes, EJsonTool.NewtonsoftJson);
        }
    }
}