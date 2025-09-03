using GameFramework.UnityRuntime.DataTable;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DataTable.MainItem
{
    public class MonsterDataTableRow: DataRowBase
    {
        public int id; // 怪物ID
        public string cnName;// 怪物中文名称
        public string detail;
        public int hp_1; // 难度1血量
        public int hp_2;
        public int hp_3;
        public int hp_4;
        public int atk_1; // 难度1攻击力
        public int atk_2;
        public int atk_3;
        public int atk_4;
        public float atkDistance_1; // 难度1攻击间隔
        public float atkDistance_2;
        public float atkDistance_3;
        public float atkDistance_4;
        
        public override int Id => id;

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // json解析一条数据
            if (userData is Dictionary<string, string> jsonData)
            {
                try
                {
                    id = int.Parse(jsonData["id"]);
                    cnName = jsonData["cnName"];
                    detail = jsonData["detail"];
                    hp_1 = int.Parse(jsonData["hp_1"]);
                    hp_2 = int.Parse(jsonData["hp_2"]);
                    hp_3 = int.Parse(jsonData["hp_3"]);
                    hp_4 = int.Parse(jsonData["hp_4"]);
                    atk_1 = int.Parse(jsonData["atk_1"]);
                    atk_2 = int.Parse(jsonData["atk_2"]);
                    atk_3 = int.Parse(jsonData["atk_3"]);
                    atk_4 = int.Parse(jsonData["atk_4"]);
                    atkDistance_1 = float.Parse(jsonData["atkDistance_1"]);
                    atkDistance_2 = float.Parse(jsonData["atkDistance_2"]);
                    atkDistance_3 = float.Parse(jsonData["atkDistance_3"]);
                    atkDistance_4 = float.Parse(jsonData["atkDistance_4"]);
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