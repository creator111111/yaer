using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Static.Utility.JsonReader
{
    public class ValueConfigJsonReader
    {
        private Dictionary<string, string> keys = new Dictionary<string, string>();

        public void Read(string path)
        {
            // 读取json
            var json = Resources.Load<TextAsset>(path.Replace(".json", "")).text;
            keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (keys == null) Debug.LogError("无法读取json文件:" + path);
        }

        public string GetValue(string key)
        {
            if (keys.ContainsKey(key) == false)
            {
                Debug.LogError("无法读取:" + key);
                return null;
            }

            return keys[key];
        }

        public void Save(string path, Dictionary<string, string> data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }

        public Dictionary<string, string> GetAllKeys()
        {
            return keys;
        }
    }
}