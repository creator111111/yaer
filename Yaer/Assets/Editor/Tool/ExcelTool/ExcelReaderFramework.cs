using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExcelDataReader;

namespace EditorC.Tool.ExcelTool
{
    public static class ExcelReaderFramework
    {
        /// <summary>
        /// 读取 Excel 并返回一个字典，每个 Sheet 是一个数据块，支持从指定行开始解析
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> ReadExcelAsDict(string filePath, int startRowIndex = 0)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet();
                Dictionary<string, List<Dictionary<string, string>>> allSheets = new Dictionary<string, List<Dictionary<string, string>>>();

                foreach (DataTable table in result.Tables)
                {
                    List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();

                    if (table.Rows.Count <= startRowIndex) continue; // 确保起始行有效

                    // 读取表头（从 startRowIndex 开始）
                    List<string> headers = new List<string>();
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        headers.Add(table.Rows[startRowIndex][col]?.ToString()?.Trim() ?? $"Column{col}");
                    }

                    // 读取数据 
                    for (int row = startRowIndex + 1; row < table.Rows.Count; row++)
                    {
                        Dictionary<string, string> rowData = new Dictionary<string, string>();
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            rowData[headers[col]] = table.Rows[row][col]?.ToString() ?? "";
                        }

                        dataList.Add(rowData);
                    }

                    // 存入字典（Sheet 名称 -> 数据）
                    allSheets[table.TableName] = dataList;
                }

                return allSheets;
            }
        }

        /// <summary>
        /// 读取 Excel 并返回一个字典，每个 Sheet 是一个数据块（List<List<string>>）
        /// </summary>
        public static Dictionary<string, List<List<string>>> ReadExcelAsRawData(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet();
                Dictionary<string, List<List<string>>> allSheets = new Dictionary<string, List<List<string>>>();

                foreach (DataTable table in result.Tables)
                {
                    List<List<string>> sheetData = new List<List<string>>();

                    for (int row = 0; row < table.Rows.Count; row++)
                    {
                        List<string> rowData = new List<string>();
                        for (int col = 0; col < table.Columns.Count; col++)
                        {
                            rowData.Add(table.Rows[row][col]?.ToString() ?? "");
                        }

                        sheetData.Add(rowData);
                    }

                    allSheets[table.TableName] = sheetData;
                }

                return allSheets;
            }
        }

        /// <summary>
        /// 获取指定 Sheet 的数据
        /// </summary>
        public static List<Dictionary<string, string>> GetSheetData(string filePath, string sheetName)
        {
            var sheets = ReadExcelAsDict(filePath);
            if (sheets.TryGetValue(sheetName, out var data))
            {
                return data;
            }

            return new List<Dictionary<string, string>>();
        }

        /// <summary>
        /// 获取所有行数据（List<List<string>>），默认返回第一个 Sheet
        /// </summary>
        public static List<List<string>> GetAllRows(string filePath, string sheetName = null)
        {
            var allSheets = ReadExcelAsRawData(filePath);
            if (sheetName == null) sheetName = allSheets.Keys.FirstOrDefault();
            return allSheets.TryGetValue(sheetName, out var rows) ? rows : new List<List<string>>();
        }

        /// <summary>
        /// 获取指定行数据（List<string>）
        /// </summary>
        public static List<string> GetRow(string filePath, int rowIndex, string sheetName = null)
        {
            var rows = GetAllRows(filePath, sheetName);
            return rowIndex >= 0 && rowIndex < rows.Count ? rows[rowIndex] : new List<string>();
        }

        /// <summary>
        /// 获取所有列数据（List<List<string>>）
        /// </summary>
        public static List<List<string>> GetAllColumns(string filePath, string sheetName = null)
        {
            var rows = GetAllRows(filePath, sheetName);
            if (rows.Count == 0) return new List<List<string>>();

            int columnCount = rows.Max(r => r.Count);
            List<List<string>> columns = new List<List<string>>();

            for (int col = 0; col < columnCount; col++)
            {
                List<string> columnData = rows.Select(row => row.Count > col ? row[col] : "").ToList();
                columns.Add(columnData);
            }

            return columns;
        }

        /// <summary>
        /// 获取指定列数据（List<string>）
        /// </summary>
        public static List<string> GetColumn(string filePath, int columnIndex, string sheetName = null)
        {
            var columns = GetAllColumns(filePath, sheetName);
            return columnIndex >= 0 && columnIndex < columns.Count ? columns[columnIndex] : new List<string>();
        }

        /// <summary>
        /// 获取指定单元格数据（string）
        /// </summary>
        public static string GetCell(string filePath, int rowIndex, int columnIndex, string sheetName = null)
        {
            var rows = GetAllRows(filePath, sheetName);
            return (rowIndex >= 0 && rowIndex < rows.Count && columnIndex >= 0 && columnIndex < rows[rowIndex].Count)
                ? rows[rowIndex][columnIndex]
                : "";
        }

        /// <summary>
        /// 将字符串转换为指定类型
        /// </summary>
        private static object ConvertValue(string value, Type type)
        {
            if (string.IsNullOrEmpty(value)) return type.IsValueType ? Activator.CreateInstance(type) : null;
            if (type == typeof(int)) return int.TryParse(value, out var i) ? i : 0;
            if (type == typeof(float)) return float.TryParse(value, out var f) ? f : 0f;
            if (type == typeof(double)) return double.TryParse(value, out var d) ? d : 0.0;
            if (type == typeof(bool)) return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (type == typeof(string)) return value;
            return Convert.ChangeType(value, type);
        }
    }
}