using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.ExcelTool
{
    public class MainItemConfigExcelTool
    {
        [MenuItem("Editor/ExcelTool/MainItemConfig/GenerateJsonFile")]
        public static void GenerateJsonFile()
        {
            string folderPath = "Assets/ExcelConfig/MainItemConfig/"; // 替换为你的目标目录
            string outputDir = "Assets/GameRes/Config/MainItemConfig/";

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
                    string jsonPath = outputDir + "MainItemConfig.json";
                    
                    // 写入 JSON 文件
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(sheet.Value, Formatting.Indented));
                }
            }

            AssetDatabase.Refresh(); // 刷新 Unity 资源管理器
            Debug.Log("MainItemConfig Excel 转 JSON 完成" + outputDir);
        }
    }
}