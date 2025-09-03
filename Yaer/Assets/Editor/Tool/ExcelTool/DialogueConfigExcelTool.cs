using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.ExcelTool
{
    public class DialogueConfigExcelTool
    {
        [MenuItem("Editor/ExcelTool/DialogueConfig/GenerateJsonFile")]
        public static void GenerateJsonFile()
        {
            string folderPath = "Assets/ExcelConfig/DialogueConfig/"; // 替换为你的目标目录
            string outputDir = "Assets/GameRes/Config/DialogueConfig/";

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir); // 创建目录
            }
            
            string[] files = Directory.GetFiles(folderPath, "*.xlsx");

            foreach (string file in files)
            {
                // 读取 Excel 数据
                var sheetData = ExcelReaderFramework.ReadExcelAsDict(file, 2);

                foreach (var sheet in sheetData)
                {
                    // 读取第一行数据的storyName字段命名json文件
                    string jsonPath = "";
                    if (sheet.Value.Count > 0)
                    {
                        var firstRow = sheet.Value[0];
                        if (!string.IsNullOrEmpty(firstRow["storyName"]))
                        {
                            jsonPath = outputDir + firstRow["storyName"] + ".json";
                        }
                        else
                        {
                            Debug.LogError(sheet.Key + "表第一行数据storyName字段为空");
                        }
                    }
                
                    // 写入 JSON 文件
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(sheet.Value, Formatting.Indented));
                }
            }

            AssetDatabase.Refresh(); // 刷新 Unity 资源管理器
            Debug.Log("DialogueConfig Excel 转 JSON 完成" + outputDir);
        }
    }
}