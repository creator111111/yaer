using GameFramework.UnityRuntime.DataTable;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DataTable.AchievenmentConfig
{
    public class AchievenmentDataTableRow: DataRowBase
    {
        public int id; // 成就ID
        public int tag; // 成就界面人物图片tag
        public string name;// 成就中文名称
        public string name_en;
        public string name_jp;
        public int value; // 成就需要的数值
        public string condition; // 成就开启条件中文
        public string condition_en;
        public string condition_jp;
        public string desc; // 成就描述中文
        public string desc_en;
        public string desc_jp;
        public override int Id => id;

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // json解析一条数据
            if (userData is Dictionary<string, string> jsonData)
            {
                try
                {
                    id = int.Parse(jsonData["id"]);
                    tag = int.Parse(jsonData["tag"]);
                    name = jsonData["name"];
                    name_en = jsonData["name_en"];
                    name_jp = jsonData["name_jp"];
                    value = int.Parse(jsonData["value"]);
                    condition = jsonData["condition"];
                    condition_en = jsonData["condition_en"];
                    condition_jp = jsonData["condition_jp"];
                    desc = jsonData["desc"];
                    desc_en = jsonData["desc_en"];
                    desc_jp = jsonData["desc_jp"];
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