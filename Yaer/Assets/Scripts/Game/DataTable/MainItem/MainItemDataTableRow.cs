using System.Collections.Generic;
using GameFramework.UnityRuntime.DataTable;
using UnityEngine;

namespace Game.DataTable.MainItem
{
    public class MainItemDataTableRow: DataRowBase
    {
        public int id;
        public string name;
        public string cnName;
        public string detail;
        public string detail_en;
        public string detail_jp;
        public int itemType;
        public override int Id => id;

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // json解析一条数据
            if (userData is Dictionary<string, string> jsonData)
            {
                try
                {
                    id = int.Parse(jsonData["id"]);
                    name = jsonData["name"];
                    cnName = jsonData["cnName"];
                    detail = jsonData["detail"];
                    detail_en = jsonData["detail_en"];
                    detail_jp = jsonData["detail_jp"];
                    itemType = int.Parse(jsonData["itemType"]);
                }
                catch
                {
                    Debug.LogError("对话数据解析错误");
                    return false;
                } 
            }

            return true;
        }
    }
}