using System.IO;
using GameFramework.CoreExtend.Generic;
using GameFramework.CoreExtend.Serialiizer.Json.LitJson;
using UnityEngine;

namespace GameFramework.CoreExtend.Serialiizer.Json
{
    /// <summary>
    ///     json解析工具类型
    /// </summary>
    public enum EJsonTool
    {
        JsonUtility,
        LitJson,
        NewtonsoftJson
    }

    public class JsonSystem : BaseSingleton<JsonSystem>
    {
        /// <summary>
        ///     数据保存为Json文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="data">数据对象</param>
        /// <param name="toolType">序列化工具, 默认使用LitJson</param>
        public void Save(string fileName, object data, EJsonTool toolType)
        {
            var path = Application.persistentDataPath + "/" + fileName + ".json";
            var json = "";

            switch (toolType)
            {
                case EJsonTool.JsonUtility:
                    json = JsonUtility.ToJson(data);
                    break;
                case EJsonTool.LitJson:
                    json = JsonMapper.ToJson(data);
                    break;
            }

            File.WriteAllText(path, json);
        }

        /// <summary>
        ///     加载Json
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="toolType">序列化工具</param>
        /// <typeparam name="T">泛型</typeparam>
        /// <returns></returns>
        public T Load<T>(string fileName, EJsonTool toolType) where T : new()
        {
            var path = Application.streamingAssetsPath + "/" + fileName + ".json";

            if (!File.Exists(path)) path = Application.persistentDataPath + "/" + fileName + ".json";

            var newObj = new T();
            var json = "";
            json = File.ReadAllText(path);
            switch (toolType)
            {
                case EJsonTool.JsonUtility:
                    newObj = JsonUtility.FromJson<T>(json);
                    break;
                case EJsonTool.LitJson:
                    newObj = JsonMapper.ToObject<T>(json);
                    break;
            }

            return newObj;
        }

        public T Parse<T>(string json, EJsonTool tool)
        {
            switch (tool)
            {
                case EJsonTool.JsonUtility:
                    return JsonUtility.FromJson<T>(json);
                case EJsonTool.LitJson:
                    return JsonMapper.ToObject<T>(json);
                case EJsonTool.NewtonsoftJson:
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            
            return default;
        }

        public T Parse<T>(byte[] bytes, EJsonTool tool)
        {
            return Parse<T>(System.Text.Encoding.UTF8.GetString(bytes), tool);
        }
    }
}