using System.Collections.Generic;
using GameFramework.CoreExtend.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GameFramework.CoreExtend.Config
{
    public class ConfigTable
    {
        [JsonProperty] public string Name { get; set; }
        public int Count => kvs.Count;

        [JsonIgnore] private IConfigTableHelper tableHelper;

        [JsonProperty] private Dictionary<string, ValuePro> kvs;

        public ConfigTable(IConfigTableHelper helper)
        {
            kvs = new Dictionary<string, ValuePro>();
            tableHelper = helper;
        }

        public void Add(string key, ValuePro valuePro)
        {
            kvs.Add(key, valuePro);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (HasKey(key))
            {
                return kvs[key].BoolValue;
            }

            Debug.LogWarning(Name + "未找到配置:" + key + ",使用默认值:" + defaultValue);
            Add(key, new ValuePro(defaultValue, default, default, default));
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            if (HasKey(key))
            {
                return kvs[key].IntValue;
            }

            Debug.LogWarning(Name + "未找到配置:" + key + ",使用默认值:" + defaultValue);
            Add(key, new ValuePro(default, defaultValue, default, default));
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
            if (HasKey(key))
            {
                return kvs[key].FloatValue;
            }

            Debug.LogWarning(Name + "未找到配置:" + key + ",使用默认值:" + defaultValue);
            Add(key, new ValuePro(default, default, defaultValue, default));
            return defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            if (HasKey(key))
            {
                return kvs[key].StringValue;
            }

            Debug.LogWarning(Name + "未找到配置:" + key + ",使用默认值:" + defaultValue);
            Add(key, new ValuePro(default, default, default, defaultValue));
            return defaultValue;
        }

        public void Remove(string key)
        {
            kvs.Remove(key);
        }

        public void Clear()
        {
            kvs.Clear();
        }

        public bool HasKey(string key) => kvs.ContainsKey(key);

        public void SetHelper(IConfigTableHelper tableHelper) => this.tableHelper = tableHelper;

        public void Parse(string str)
        {
            var table = tableHelper.Parse<ConfigTable>(str);
            kvs = table.kvs;
        }

        public void Parse(byte[] bytes)
        {
            var table = tableHelper.Parse<ConfigTable>(bytes);
            kvs = table.kvs;
        }

        
    }
}