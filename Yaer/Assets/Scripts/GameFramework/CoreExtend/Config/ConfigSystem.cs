using System.Collections.Generic;
using GameFramework.CoreExtend.Resource;
using UnityEngine;

namespace GameFramework.CoreExtend.Config
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    public sealed class ConfigSystem : IConfigSystem
    {
        private IResourceSystem resourceSystem;

        private readonly Dictionary<string, ConfigTable> configDataDic;
        public int Count => configDataDic.Count;
        public ConfigSystem()
        {
            configDataDic = new Dictionary<string, ConfigTable>();
        }
        
        public void SetResourceSystem(IResourceSystem system)
        {
            resourceSystem = system;
        }

        public bool HasConfig(string configName)
        {
            return GetConfig(configName) != null;
        }

        public bool GetBool(string configName, string key)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetBool(key, default);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public bool GetBool(string configName, string key, bool defaultValue)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetBool(key, defaultValue);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public int GetInt(string configName, string key)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetInt(key, default);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public int GetInt(string configName, string key, int defaultValue)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetInt(key, defaultValue);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public float GetFloat(string configName, string key)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetFloat(key, default);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public float GetFloat(string configName, string key, float defaultValue)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetFloat(key, defaultValue);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public string GetString(string configName, string key)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetString(key, default);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public string GetString(string configName, string key, string defaultValue)
        {
            if (HasConfig(configName))
            {
                return GetConfig(configName).GetString(key, defaultValue);
            }

            Debug.LogError("未找到配置:" + configName);
            return default;
        }

        public bool AddConfig(string configName, ConfigTable configTable)
        {
            if (!configDataDic.ContainsKey(configName))
            {
                configDataDic.Add(configName, configTable);
                return true;
            }

            return false;
        }

        public void ParseConfig<T>(string configName, string json, IConfigTableHelper tableHelper)
        {
            var config = new ConfigTable(tableHelper);
            config.Parse(json);
            AddConfig(configName, config);
        }

        public void ParseConfig(string configName, byte[] bytes)
        {
        }

        public bool RemoveConfig(string configName)
        {
            if (configDataDic.ContainsKey(configName))
            {
                configDataDic.Remove(configName);
                return true;
            }

            return false;
        }

        public void RemoveAllConfigs()
        {
            configDataDic.Clear();
        }

        private ConfigTable GetConfig(string configName)
        {
            if (configDataDic.ContainsKey(configName))
            {
                return configDataDic[configName];
            }

            Debug.LogError("未找到配置:" + configName);
            return null;
        }
    }
}