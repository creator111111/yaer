using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public static class DialogueConfigJsonParser
    {
        /// <summary>
        /// 读取指定 JSON 文件并返回 List<Dictionary<string, string>>
        /// </summary>
        public static List<Dictionary<string, string>> ReadJsonFile(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError("❌ JSON 文件不存在：" + jsonPath);
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                return JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonText);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 解析 JSON 失败: {jsonPath} \n错误信息: {ex.Message}");
                return null;
            }
        }
    }
}