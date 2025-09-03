using System;
using System.Collections.Generic;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass
{
    // 主存档数据，整合所有数据类，并包含版本号字段
    [ES3Serializable]
    public class MasterGameData
    {
        // 数据版本号，便于升级和迁移
        public int version = ArchiveComponentGM.CurrentDataVersion;
        public Dictionary<string, object> data = new Dictionary<string, object>();

        /// <summary>
        /// 获取值，如果 key 不存在则返回默认值
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
        }

        public bool TrySetValue<T>(string key, T value)
        {
            if (HasField(key))
            {
                return false;
            }

            data[key] = value;
            return true;
        }

        /// <summary>
        /// 删除字段
        /// </summary>
        public void RemoveField(string key)
        {
            if (data.ContainsKey(key))
                data.Remove(key);
        }

        /// <summary>
        /// 检查字段是否存在
        /// </summary>
        public bool HasField(string key)
        {
            return data.ContainsKey(key);
        }
    }
}