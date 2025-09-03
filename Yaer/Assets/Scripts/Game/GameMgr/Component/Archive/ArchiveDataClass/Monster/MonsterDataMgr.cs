using Game.DataTable.MainItem;
using Game.GameRuntime.Entities.Monster;
using Game.Static.Enum;
using GameFramework.DataTable;
using System;
using System.Collections.Generic;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class MonsterDataMgr : object
    {
        public static MonsterDataMgr instance = null;
        public static MonsterDataMgr getInstance()
        {
            if (instance == null)
            {
                instance = new MonsterDataMgr();
            }
            return instance;
        }
        #region 属性

        private static IDataTable<MonsterDataTableRow> table;

        #endregion

        #region 初始化

        public void Init()
        {
            GameManager.GetGMComponent<ResComponentGM>().LoadConfig<MonsterDataTableRow>("Assets/GameRes/Config/MonsterConfig/MonsterConfig.json", rows => table = rows);
        }

        #endregion
        // 获取某个怪物的最大HP
        public int getMonsterHp(int id)
        {
            var hp_1 = table.GetDataRow(condition: row => row.id == id).hp_1;
            var hp_2 = table.GetDataRow(condition: row => row.id == id).hp_2;
            var hp_3 = table.GetDataRow(condition: row => row.id == id).hp_3;
            var hp_4 = table.GetDataRow(condition: row => row.id == id).hp_4;
            var hpValueList = new Dictionary<EGameHard, int>() {
                { EGameHard.Easy, hp_1 }, { EGameHard.Normal, hp_2 },
                { EGameHard.Hard, hp_3 }, { EGameHard.Hardest, hp_4 },
            };
            var hardCompont = GameManager.GetGMComponent<HardComponentGM>();
            var gameHard = hardCompont.Hard;
            if (!hpValueList.ContainsKey(gameHard)) { return 0; }
            return hpValueList[gameHard];
        }
        // 获取某个怪物的攻击力
        public int getMonsterAtkValue(int id)
        {
            var atk_1 = table.GetDataRow(condition: row => row.id == id).atk_1;
            var atk_2 = table.GetDataRow(condition: row => row.id == id).atk_2;
            var atk_3 = table.GetDataRow(condition: row => row.id == id).atk_3;
            var atk_4 = table.GetDataRow(condition: row => row.id == id).atk_4;
            var hpValueList = new Dictionary<EGameHard, int>() {
                { EGameHard.Easy, atk_1 }, { EGameHard.Normal, atk_2 },
                { EGameHard.Hard, atk_3 }, { EGameHard.Hardest, atk_4 },
            };
            var gameHard = GameManager.GetGMComponent<HardComponentGM>().Hard;
            if (!hpValueList.ContainsKey(gameHard)) { return 0; }
            return hpValueList[gameHard];
        }
        // 获取某个怪物的最大HP
        public float getMonsterAtkDistance(int id)
        {
            var atkDistance_1 = table.GetDataRow(condition: row => row.id == id).atkDistance_1;
            var atkDistance_2 = table.GetDataRow(condition: row => row.id == id).atkDistance_2;
            var atkDistance_3 = table.GetDataRow(condition: row => row.id == id).atkDistance_3;
            var atkDistance_4 = table.GetDataRow(condition: row => row.id == id).atkDistance_4;
            var hpValueList = new Dictionary<EGameHard, float>() {
                { EGameHard.Easy, atkDistance_1 }, { EGameHard.Normal, atkDistance_2 },
                { EGameHard.Hard, atkDistance_3 }, { EGameHard.Hardest, atkDistance_4 },
            };
            var gameHard = GameManager.GetGMComponent<HardComponentGM>().Hard;
            if (!hpValueList.ContainsKey(gameHard)) { return 0; }
            return hpValueList[gameHard];
        }

        // 获取某个怪物攻击的真正伤害
        public int getMonsterAtkRealValue(BaseMonster monsterLogic, string atkTypeName)
        {
            // 如果怪物有不同的攻击，在这里对不同攻击造成的伤害进行判断
            // ...
            return monsterLogic.atkValue;
        }
    }
}