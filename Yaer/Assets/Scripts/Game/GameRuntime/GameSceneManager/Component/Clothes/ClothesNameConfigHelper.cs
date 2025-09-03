using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Static.Utility.JsonReader
{
    public class ClothesNameConfigHelper
    {
        private const string path = "Config/ClothesNameConfig";
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> nameDic;

        public void Read(string json)
        {
            // 读取json
            nameDic = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
            if (nameDic == null) Debug.LogError("无法读取json文件:" + path);
        }

        public string GetClothesName(string language, string bone, string clothes)
        {
            if (nameDic == null)
            {
                Debug.LogError("请先调用ReadJsonConfig");
                return "";
            }

            if (nameDic.ContainsKey(language) == false)
            {
                Debug.LogError("无法读取:" + language);
                return "";
            }

            if (nameDic[language].ContainsKey(bone) == false)
            {
                Debug.LogError("无法读取:" + bone);
                return "";
            }

            if (nameDic[language][bone].ContainsKey(clothes) == false)
            {
                Debug.LogError("无法读取:" + clothes);
                return "";
            }

            return nameDic[language][bone][clothes];
        }
    }
}