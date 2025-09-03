using System;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using GameFramework.CoreExtend.Serialiizer.Binary;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.ExcelTool
{
    public static class ExcelGenerateBinary
    {
        private static string EXCEL_FILE_PATH = Application.dataPath + "/Config/DialogueInfo/";
        private static string BINARY_FILE_PATH = Application.streamingAssetsPath + "/Data/DialogueData/";

        #region 对话信息类转换

        [MenuItem("Editor/ExcelTool/GenerateBinary/GenerateDialogueInfo")]
        public static void DialogueInfoExcelToBinaryFile()
        {
            // 获取文件夹
            DirectoryInfo directoriesInfo = Directory.CreateDirectory(EXCEL_FILE_PATH);
            // 获取文件
            FileInfo[] files = directoriesInfo.GetFiles();
            // 储存Excel表容器
            DataTableCollection tables;
        
            // 获取文件夹中的所有对话文件路径
            string[] filePaths = Directory.GetFiles(BINARY_FILE_PATH);
            // 删除每个文件
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }

            // 遍历所有Excel文件
            for (int i = 0; i < files.Length; i++)
            {
                // 去除非excel文件
                if (files[i].Extension == ".xlsx" || files[i].Extension == ".xls")
                {
                    // 打开文件
                    using (FileStream fileStream = files[i].Open(FileMode.Open, FileAccess.Read))
                    {
                        // 读取Excel
                        IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                        // 读取所有表
                        var a = excelDataReader.AsDataSet();
                        tables = excelDataReader.AsDataSet().Tables;
                        fileStream.Close();
                    }
                
                    // 遍历Excel内所有表
                    foreach (DataTable table in tables)
                    {
                        // 生成二进制文件
                        GenerateBinaryFile(table);
                    }
                }
            }

            Debug.Log($"Finish! => Files in {BinarySystem.BINARYFILE_PATH}");
        }

        #endregion

        public static void GenerateBinaryFile(DataTable table)
        {
            // 创建文件夹
            if (!Directory.Exists(BINARY_FILE_PATH))
            {
                Directory.CreateDirectory(BINARY_FILE_PATH);
            }
        
            // 获取表名第1行第列
            string name = table.Rows[0][1].ToString();
        
            // 创建文件
            using (FileStream fs = File.Create(BINARY_FILE_PATH + name + ".dli"))
            {
                int row = 3;
                try
                {
                    // 存真实数据的行数
                    fs.Write(BitConverter.GetBytes(table.Rows.Count - 3), 0, sizeof(int));
                    // 遍历每一行,第3行才是真实数据
                    for (; row < table.Rows.Count; row++)
                    {
                        // 1 isSelect
                        fs.Write(BitConverter.GetBytes(bool.Parse(table.Rows[row][0].ToString())), 0, sizeof(bool));

                        // 2 id
                        fs.Write(BitConverter.GetBytes(int.Parse(table.Rows[row][1].ToString())), 0, sizeof(int));

                        // 3 roleName 人物名字先存字符串长度
                        string roleName = table.Rows[row][2].ToString();
                        fs.Write(BitConverter.GetBytes(roleName.Length), 0, sizeof(int));
                        fs.Write(Encoding.UTF8.GetBytes(roleName), 0, roleName.Length);

                        // faceType
                        string faceType = table.Rows[row][3].ToString();
                        fs.Write(BitConverter.GetBytes(faceType.Length), 0, sizeof(int));
                        fs.Write(Encoding.UTF8.GetBytes(faceType), 0, faceType.Length);

                        // content
                        string content = table.Rows[row][4].ToString();
                        // 中文内容先转字符数组再读取长度
                        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                        fs.Write(BitConverter.GetBytes(contentBytes.Length), 0, sizeof(int));
                        fs.Write(contentBytes, 0, contentBytes.Length);

                        // 6 gotoId
                        fs.Write(BitConverter.GetBytes(int.Parse(table.Rows[row][5].ToString())), 0, sizeof(int));

                        // 7 result
                        string result = table.Rows[row][6].ToString();
                        byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                        fs.Write(BitConverter.GetBytes(resultBytes.Length), 0, sizeof(int));
                        fs.Write(resultBytes, 0, resultBytes.Length);

                        // eventName
                        string eventName = table.Rows[row][7].ToString();
                        fs.Write(BitConverter.GetBytes(eventName.Length), 0, sizeof(int));
                        fs.Write(Encoding.UTF8.GetBytes(eventName), 0, eventName.Length);
                    }

                    fs.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError( $"{table.TableName}.dli的第{row}行出错");
                    Debug.LogError(e);
                    throw;
                }
            }

            AssetDatabase.Refresh();
        }
    }
}